using System;
using System.IO;
using EasyLua.Lexer;
using UnityEditor;
using UnityEngine;

namespace EasyLua.Editor {

    // attribute for custom field painter
    public class CustomFieldPainterAttribute : Attribute {
        private string mFieldType;
        public CustomFieldPainterAttribute(string fieldType) {
            mFieldType = fieldType;
        }

        public string GetHandledFieldType() {
            return mFieldType;
        }

    }

    public class EditorBasicFieldPainter {
        public virtual bool Draw(EasyLuaParam para) {
            var changed = false;
            if (IsInt(para)) {
                changed = DrawNumber(para);
            } else if (IsFloat(para)) {
                changed = DrawFloat(para);
            } else if (IsString(para)) {
                changed = DrawString(para);
            } else if (IsBool(para)) {
                changed = DrawBool(para);
            } else if (IsArray(para)) {
                changed = DrawArray(para);
            } else {
                changed = DrawObject(para);
            }

            return changed;
        }

        private bool IsInt(EasyLuaParam para) {
            var name = para.LowerTypeName;
            return name == "number" || name == "system.int32";
        }

        private bool DrawNumber(EasyLuaParam para) {
            var prev = para.Int;
            var newVal = EditorGUILayout.LongField(para.name, prev);
            if (prev != newVal) {
                para.Int = newVal;
                return true;
            }

            return false;
        }


        private bool IsFloat(EasyLuaParam para) {
            var name = para.LowerTypeName;
            return name == "float" || name == "system.single";
        }

        private bool DrawFloat(EasyLuaParam para) {
            var prev = para.Float;
            var newVal = EditorGUILayout.FloatField(para.name, prev);
            if (!Mathf.Approximately(prev, newVal)) {
                para.Float = newVal;
                return true;
            }

            return false;
        }

        public bool IsBool(EasyLuaParam para) {
            var name = para.LowerTypeName;
            return name == "bool" || name == "system.boolean";
        }

        private bool DrawBool(EasyLuaParam para) {
            var prev = para.Bool;
            var newVal = EditorGUILayout.Toggle(para.name, prev);
            if (prev != newVal) {
                para.Bool = newVal;
                return true;
            }

            return false;
        }

        public bool IsString(EasyLuaParam para) {
            var name = para.LowerTypeName;
            return name == "string" || name == "system.string";
        }

        private bool DrawString(EasyLuaParam para) {
            var prev = para.String;
            var newVal = EditorGUILayout.TextField(para.name, prev);
            if (prev != newVal) {
                para.String = newVal;
                return true;
            }

            return false;
        }

        private bool DrawObject(EasyLuaParam para) {
            var prev = para.UnityObject;
            var t = GetSystemType(para.TypeName);
            if (t.BaseType == typeof(Enum)) {
                return DrawEnum(para, t);
            }


            var newVal = EditorGUILayout.ObjectField(para.name, prev, t, true);
            if (prev != newVal) {
                var behaviour = newVal as EasyBehaviour;
                if (behaviour != null) {
                    // if there are multiple luaEnv ,find same name
                    var curBehaviour = FindLuaClassByName(behaviour, para.TypeName);
                    para.UnityObject = curBehaviour;
                } else {
                    para.UnityObject = newVal;
                }
                return true;
            }
            return false;
        }

        private EasyBehaviour FindLuaClassByName(EasyBehaviour behaviour, string typeName) {
            var components = behaviour.GetComponents<EasyBehaviour>();
            for (int i = 0; i < components.Length; i++) {
                var curBehaviour = components[i];
                var code = curBehaviour.LuaCode;
                if (!code) {
                    continue;
                }

                var className = EasyLuaLexer.TrimExtension(code.name);
                if (className == typeName) {
                    return curBehaviour;
                }
            }

            // has not been found in siblings search the parent
            for (int i = 0; i < components.Length; i++) {
                var curBehaviour = components[i];
                var code = curBehaviour.LuaCode;
                if (!code) {
                    continue;
                }

                var className = EasyLuaLexer.TrimExtension(code.name);
                if (IsSubClassOf(className, typeName)) {
                    return curBehaviour;
                }
            }

            return null;
        }

        private bool IsSubClassOf(string luaClass, string targetClass) {
            var fileName = luaClass + " t:TextAsset";
            var guids = AssetDatabase.FindAssets(fileName);
            if (guids == null || guids.Length == 0) {
                return false;
            }

            string path = null;

            for (int i = 0; i < guids.Length; i++) {
                var p = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (p != null && p.EndsWith("lua.txt")) {
                    // find raw lua file name same as class name
                    var rawName = Path.GetFileNameWithoutExtension(p).Split('.')[0];
                    if (rawName != luaClass) {
                        continue;
                    }
                    path = p;
                    break;
                }
            }

            if (path == null) {
                return false;
            }


            var script = (TextAsset)AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
            var lexer = new EasyLuaLexer(script.text);

            var baseClass = lexer.GetBaseClassName();
            if (!string.IsNullOrWhiteSpace(baseClass)) {
                // not found keep searching
                if (baseClass != targetClass) {
                    return IsSubClassOf(baseClass, targetClass);
                }

                return true;
            }

            return false;
        }

        private bool DrawEnum(EasyLuaParam param, Type enumType) {
            var nameList = Enum.GetNames(enumType);
            var valueList = Enum.GetValues(enumType);
            int[] intList = new int[valueList.Length];
            for (int k = 0; k < valueList.Length; k++) {
                intList[k] = (int)valueList.GetValue(k);
            }

            var prev = param.EnumVal;
            var newVal = EditorGUILayout.IntPopup(param.name, param.EnumVal, nameList, intList);
            if (prev != newVal) {
                param.EnumVal = newVal;
                return true;
            }

            return false;
        }

        public bool IsArray(EasyLuaParam para) {
            return para.IsArray();
        }

        private bool DrawArray(EasyLuaParam para) {
            para.arryFold = EditorGUILayout.Foldout(para.arryFold, para.name);
            if (!para.arryFold) {
                return false;
            }

            var arr = para.Array;
            var valid = arr != null && arr.Length > 0;
            var oldSize = valid ? arr.Length : 0;
            var newSize = EditorGUILayout.IntField("Size", oldSize);

            var changed = false;
            if (oldSize != newSize) {
                if (newSize == 0) {
                    para.Array = null;
                    return true;
                }

                changed = true;
                var oldArr = para.Array;
                var newArr = new EasyLuaParam[newSize];
                var copySize = Mathf.Min(oldSize, newSize);
                if (oldArr != null) {
                    Array.Copy(oldArr, newArr, copySize);
                }

                para.Array = newArr;
            }

            if (newSize == 0) {
                return false;
            }

            for (int i = 0; i < para.Array.Length; i++) {
                var subPara = para.Array[i];
                if (subPara == null) {
                    changed = true;
                    subPara = new EasyLuaParam();
                    para.Array[i] = subPara;
                }

                subPara.TypeName = para.TypeName;
                subPara.name = $"element{i}";
                var elementChanged = EditorFieldPainter.Draw(subPara);
                if (elementChanged) {
                    changed = true;
                }
            }

            return changed;
        }

        private Type GetSystemType(string typeName) {
            Type type = Type.GetType(typeName);
            if (type != null) {
                return type;
            }

            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies()) {
                type = ass.GetType(typeName);
                if (type != null) {
                    return type;
                }
            }

            return typeof(EasyBehaviour);
        }

    }

}