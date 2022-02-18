using System.Collections.Generic;
using System.IO;
using System.Linq;
using AmplifyShaderEditor;
using EasyLua.Lexer;
using UnityEngine;
using UnityEditor;

namespace EasyLua.Editor {
    [CustomEditor(typeof(EasyBehaviour), true)]
    public class EditorEasyBehaviour : UnityEditor.Editor {
        private EasyBehaviour mLua;
        private FileSystemWatcher mWatcher = new FileSystemWatcher();

        protected void OnEnable() {
            mLua = target as EasyBehaviour;
            if (mLua.LuaCode != null) {
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
            EditorGUILayout.LabelField("LUA CLASS:" + cls?.GetClassName());
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
            if (paras==null) {
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

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var script = (TextAsset) AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
            var lexer = new EasyLuaLexer(script.text);

            var baseClass = lexer.GetBaseClassName();
            if (!string.IsNullOrWhiteSpace(baseClass)) {
                var baseFields = ParseClassFields(baseClass);
                paras.AddRange(baseFields);
            }

            var fields = lexer.GetScriptFields();
            paras.AddRange(fields);

            return paras;
        }

        private EasyLuaParam[] UpdateParams() {
            var fields = ParseScriptFields();
            if (fields==null) {
                return null;
            }
            var newParas = fields.Select((f) => new EasyLuaParam() {
                TypeName = f.GetFieldType(),
                name = f.GetFieldName()
            }).ToArray();
            // 去重
            newParas = GetParamsDic(newParas).Values.ToArray();

            var origin = mLua.GetLuaParams();
            if (origin != null && origin.Length > 0) {
                var dic = GetParamsDic(origin);
                for (int i = 0; i < newParas.Length; i++) {
                    var para = newParas[i];
                    if (dic.ContainsKey(para.name)) {
                        CopyParam(para, dic[para.name]);
                    }
                }
            }

            mLua.SetLuaParams(newParas);
            AssetDatabase.SaveAssets();
            return newParas;
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