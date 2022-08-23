using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EasyLua.Editor {
    public class EditorFieldPainter {

        private static Dictionary<string, EditorBasicFieldPainter> sPainters = new Dictionary<string, EditorBasicFieldPainter>();
        private static EditorBasicFieldPainter sDefaultPainter = new EditorBasicFieldPainter();

        public static bool Draw(EasyLuaParam para) {
            var painter = TryGetPainter(para);
            if (painter != null) {
                return painter.Draw(para);
            }
            Debug.LogError("no painter found");
            return false;

        }


        private static EditorBasicFieldPainter TryGetPainter(EasyLuaParam param) {
            var rawType = param.RawTypeName;
            if (sPainters.ContainsKey(rawType)) {
                return sPainters[rawType];
            }

            var ass = Assembly.GetExecutingAssembly();
            var painter = FindPainter(param, ass);
            if (painter != null) {
                sPainters[rawType] = painter;
                return painter;
            }

            sPainters[rawType] = sDefaultPainter;
            return sDefaultPainter;
        }


        private static EditorBasicFieldPainter FindPainter(EasyLuaParam param, Assembly ass) {
            Type t = typeof(EditorBasicFieldPainter);
            var rawType = param.RawTypeName;
            var types = ass.GetTypes();
            for (int i = 0; i < types.Length; i++) {
                var curType = types[i];
                if (!curType.IsSubclassOf(t)) {
                    continue;
                }
                var attr = curType.GetCustomAttribute<EasyLua.Editor.CustomFieldPainterAttribute>();
                if (attr == null) {
                    Debug.LogError("should using 'CustomFieldPainter' attribute for custom field Painting");
                    continue;
                }

                var field = attr.GetHandledFieldType();
                if (!string.IsNullOrWhiteSpace(field) && field == rawType) {
                    var painter = (EditorBasicFieldPainter)Activator.CreateInstance(curType);
                    return painter;
                }
            }

            return null;
        }


    }
}