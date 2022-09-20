using System.Collections.Generic;
using System.IO;
using System.Linq;
using EasyLua.Lexer;
using UnityEngine;
using UnityEditor;

namespace EasyLua.Editor {
    [CustomEditor(typeof(EasyBehaviour), true)]
    public class EditorEasyBehaviour : UnityEditor.Editor {
        private EasyBehaviour mLua;
        private FileSystemWatcher mWatcher = new FileSystemWatcher();

        protected virtual void OnEnable() {
            mLua = target as EasyBehaviour;
            if (mLua && mLua.LuaCode) {
                AddWatcher();
            }
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (Event.current.type == EventType.DragExited) {
                EditorUtility.SetDirty(mLua);
            }

            serializedObject.Update();
            if (Application.isPlaying) {
                DrawClassName();
            }

            var code = DrawCodeField();
            if (code == null) {
                return;
            }

            DrawScriptFields();
        }

        private void DrawClassName() {
            var cls = serializedObject.targetObject as EasyBehaviour;
            if (cls == null || !cls.IsInitiated()) {
                return;
            }
            EditorGUILayout.LabelField("LUA CLASS:" + cls.GetClassName());
        }

        private TextAsset DrawCodeField() {
            TextAsset code = EditorGUILayout.ObjectField("Lua Script", mLua.LuaCode, typeof(TextAsset), true) as TextAsset;
            if (code != mLua.LuaCode) {
                mLua.LuaCode = code;
                AddWatcher();
                EditorUtility.SetDirty(mLua);
            }

            return code;
        }


        private void AddWatcher() {
            if (mLua.LuaCode == null) {
                return;
            }

            var path = GetAssetFullName(mLua.LuaCode);
            string folderPath = Path.GetDirectoryName(path);

            mWatcher.Path = folderPath;
            mWatcher.Filter = Path.GetFileName(path);
            mWatcher.Changed -= OnChanged;
            mWatcher.Changed += OnChanged;
            mWatcher.EnableRaisingEvents = true;

            OnChanged(mWatcher, new FileSystemEventArgs(WatcherChangeTypes.Changed, folderPath, path));
        }

        private string GetAssetFullName(Object o) {
            var assetPath = AssetDatabase.GetAssetPath(o);
            if (!string.IsNullOrEmpty(assetPath)) {
                var dataPath = Application.dataPath;
                return Path.Combine(dataPath.Substring(0, dataPath.Length - 6), assetPath);
            }

            return null;
        }


        private void OnChanged(object source, FileSystemEventArgs e) {
            EditorApplication.delayCall -= DelayPaint;
            EditorApplication.delayCall += DelayPaint;
        }

        private void DelayPaint() {
            UpdateParams();
            Repaint();
        }

        private void DrawScriptFields() {
            var paras = mLua.GetLuaParams();
            if (paras == null) {
                return;
            }

            for (int i = 0; i < paras.Length; i++) {
                var para = paras[i];
                if (!string.IsNullOrEmpty(para.TypeName)) {
                    var dirty = EditorFieldPainter.Draw(para);
                    if (dirty) {
                        EditorUtility.SetDirty(mLua);
                    }
                }
            }
        }

        private List<FieldToken> ParseScriptFields() {
            if (!mLua) {
                return null;
            }

            var code = mLua.LuaCode.text;
            var lexer = new EasyLuaLexer(code);
            if (lexer.GetLuaClassName() != EasyLuaLexer.TrimExtension(mLua.LuaCode.name)) {
                Debug.LogError("error: class name not same as lua file name");
                return new List<FieldToken>();
            }

            var baseName = lexer.GetBaseClassName();
            var baseFields = ParseClassFields(baseName);
            var fields = lexer.GetScriptFields();
            baseFields.AddRange(fields);
            return baseFields;
        }

        private List<FieldToken> ParseClassFields(string className) {
            var paras = new List<FieldToken>();
            if (string.IsNullOrWhiteSpace(className)) {
                return paras;
            }

            var fileName = className + " t:TextAsset";
            var guids = AssetDatabase.FindAssets(fileName);
            if (guids == null || guids.Length == 0) {
                return paras;
            }

            string path = null;

            for (int i = 0; i < guids.Length; i++) {
                var p = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (p != null && p.EndsWith("lua.txt")) {
                    // find raw lua file name same as class name
                    var rawName = Path.GetFileNameWithoutExtension(p).Split('.')[0];
                    if (rawName != className) {
                        continue;
                    }
                    path = p;
                    break;
                }
            }

            if (path == null) {
                return paras;
            }


            var script = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
            var lexer = new EasyLuaLexer(script.text);

            var baseClass = lexer.GetBaseClassName();
            if (!string.IsNullOrWhiteSpace(baseClass)) {
                if (baseClass != className) {
                    var baseFields = ParseClassFields(baseClass);
                    paras.AddRange(baseFields);
                } else {
                    Debug.LogError($"{className} has incorrect base class");
                }
            }

            var fields = lexer.GetScriptFields();
            paras.AddRange(fields);

            return paras;
        }

        private EasyLuaParam[] UpdateParams() {
            var fields = ParseScriptFields();
            if (fields == null) {
                return null;
            }

            var newParas = fields.Select((f) => new EasyLuaParam()
            {
                TypeName = f.GetFieldType(),
                name = f.GetFieldName()
            }).ToArray();
            newParas = GetParamsDic(newParas).Values.ToArray();
            var origin = mLua.GetLuaParams();
            var dirty = false;
            var equal = !IsParamsCountEqual(origin, newParas);
            if (!equal) {
                dirty = true;
            }

            if (origin != null && origin.Length > 0) {
                var originDic = GetParamsDic(origin);
                for (int i = 0; i < newParas.Length; i++) {
                    var para = newParas[i];
                    if (originDic.ContainsKey(para.name)) {
                        CopyParam(para, originDic[para.name]);
                    } else {
                        // if one new param not included in origin it's changed
                        dirty = true;
                    }
                }
            }


            mLua.SetLuaParams(newParas);
            if (dirty) {
                EditorUtility.SetDirty(mLua);
            }
            return newParas;
        }

        private bool IsParamsCountEqual(EasyLuaParam[] origin, EasyLuaParam[] target) {
            if (origin == null && target == null) {
                return true;
            }

            if (origin == null) {
                if (target.Length == 0) {
                    return true;
                }
                return false;
            }

            if (target == null) {
                if (origin.Length == 0) {
                    return true;
                }
                return false;
            }

            return origin.Length != target.Length;
        }

        private Dictionary<string, EasyLuaParam> GetParamsDic(EasyLuaParam[] @params) {
            var dic = new Dictionary<string, EasyLuaParam>();
            if (@params != null && @params.Length > 0) {
                for (int i = 0; i < @params.Length; i++) {
                    var para = @params[i];
                    if (!dic.ContainsKey(para.name)) {
                        dic.Add(para.name, para);
                    }
                }
            }

            return dic;
        }

        private void CopyParam(EasyLuaParam origin, EasyLuaParam newParam) {
            newParam.CopyTo(origin);
        }
    }
}