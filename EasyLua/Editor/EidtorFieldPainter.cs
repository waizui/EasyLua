using System;
using UnityEditor;
using UnityEngine;

namespace EasyLua.Editor {
    public class EditorFieldPainter {
        public static bool Draw(EasyLuaParam para) {
            var changed = false;
            if (para.IsInt()) {
                changed = DrawNumber(para);
            } else if (para.IsColor()) {
                changed = DrawColor(para);
            } else if (para.IsVector3()) {
                changed = DrawVector3(para);
            } else if (para.IsFloat()) {
                changed = DrawFloat(para);
            } else if (para.IsString()) {
                changed = DrawString(para);
            } else if (para.IsBool()) {
                changed = DrawBool(para);
            } else if (para.IsArray()) {
                changed = DrawArray(para);
            } else {
                changed = DrawObject(para);
            }

            return changed;
        }


        private static bool DrawNumber(EasyLuaParam para) {
            var prev = para.Int;
            var newVal = EditorGUILayout.LongField(para.name, prev);
            if (prev != newVal) {
                para.Int = newVal;
                return true;
            }

            return false;
        }


        private static bool DrawFloat(EasyLuaParam para) {
            var prev = para.Float;
            var newVal = EditorGUILayout.FloatField(para.name, prev);
            if (!Mathf.Approximately(prev, newVal)) {
                para.Float = newVal;
                return true;
            }

            return false;
        }


        private static bool DrawColor(EasyLuaParam para) {
            var prev = para.Color;
            var newVal = EditorGUILayout.ColorField(para.name, prev);
            if (prev != newVal) {
                para.Color = newVal;
                return true;
            }

            return false;
        }

        private static bool DrawVector3(EasyLuaParam para) {
            var prev = para.Vector3;
            var newVal = EditorGUILayout.Vector3Field(para.name, prev);
            if (prev != newVal) {
                para.Vector3 = newVal;
                return true;
            }

            return false;
        }

        private static bool DrawBool(EasyLuaParam para) {
            var prev = para.Bool;
            var newVal = EditorGUILayout.Toggle(para.name, prev);
            if (prev != newVal) {
                para.Bool = newVal;
                return true;
            }

            return false;
        }

        private static bool DrawString(EasyLuaParam para) {
            var prev = para.String;
            var newVal = EditorGUILayout.TextField(para.name, prev);
            if (prev != newVal) {
                para.String = newVal;
                return true;
            }

            return false;
        }

        private static bool DrawObject(EasyLuaParam para) {
            var prev = para.UnityObject;
            var t = GetSystemType(para.TypeName);
            if (t.BaseType == typeof(Enum)) {
                return DrawEnum(para, t);
            }

            var newVal = EditorGUILayout.ObjectField(para.name, prev, t, true);
            if (prev != newVal) {
                para.UnityObject = newVal;
                return true;
            }

            return false;
        }

        private static bool DrawEnum(EasyLuaParam param, Type enumType) {
            var nameList = Enum.GetNames(enumType);
            var valueList = Enum.GetValues(enumType);
            int[] intList = new int[valueList.Length];
            for (int k = 0; k < valueList.Length; k++) {
                intList[k] = (int) valueList.GetValue(k);
            }

            var prev = param.EnumVal;
            var newVal = EditorGUILayout.IntPopup(param.name, param.EnumVal, nameList, intList);
            if (prev != newVal) {
                param.EnumVal = newVal;
                return true;
            }

            return false;
        }

        private static bool DrawArray(EasyLuaParam para) {
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
                var elementChanged = Draw(subPara);
                if (elementChanged) {
                    changed = true;
                }
            }

            return changed;
        }

        private static Type GetSystemType(string typeName) {
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