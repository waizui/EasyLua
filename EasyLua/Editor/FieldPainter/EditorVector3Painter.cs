using UnityEditor;
using UnityEngine;

namespace EasyLua.Editor {

    [CustomFieldPainter("UnityEngine.Vector3")]
    public class EditorVector3Painter : EditorBasicFieldPainter {
        public override bool Draw(EasyLuaParam para) {
            return DrawVector3(para);
        }

        private bool DrawVector3(EasyLuaParam para) {
            var val = para.ValueObject;
            if (!(val is Vector3)) {
                val = Vector3.zero;
            }

            var prev = (Vector3)val;
            var newVal = EditorGUILayout.Vector3Field(para.name, prev);
            if (prev != newVal) {
                para.ValueObject = newVal;
                return true;
            }

            return false;
        }
    }

}