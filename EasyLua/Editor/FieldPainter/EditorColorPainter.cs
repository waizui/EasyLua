using UnityEditor;
using UnityEngine;

namespace EasyLua.Editor {

    [CustomFieldPainter("UnityEngine.Color")]
    public class EditorColorPainter : EditorBasicFieldPainter {
        public override bool Draw(EasyLuaParam para) {
            return DrawColor(para);
        }

        private bool DrawColor(EasyLuaParam para) {
            var val = para.ValueObject;
            if (!(val is Color)) {
                val = Color.white;
            }

            var prev = (Color)val;
            var newVal = EditorGUILayout.ColorField(para.name, prev);
            if (prev != newVal) {
                para.ValueObject = newVal;
                return true;
            }

            return false;
        }

    }

}