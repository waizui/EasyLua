using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EasyLua {
    public static class EditorCreateLua {
        [MenuItem("Assets/Create/LuaClass", false, 0)]
        private static void CreateLuaFile() {
            var selection = Selection.activeObject;
            if (!selection) {
                Debug.LogError("Select folder first!");
                return;
            }

            var path = AssetDatabase.GetAssetPath(selection);
            if (!Directory.Exists(path)) {
                path = Path.GetDirectoryName(path);
            }

            var name = EditorInputDialog.Show("Info", "Please input class name", "");
            if (string.IsNullOrWhiteSpace(name)) {
                return;
            }

            var text = CreateLuaScript(name.Trim());
            var luaPath = Path.Combine(path, name + ".lua.txt");
            File.WriteAllText(luaPath, text, Encoding.UTF8);
            AssetDatabase.Refresh();
        }

        private static string CreateLuaScript(string ClassName) {
            var sb = new StringBuilder();
            sb.Append($"---@class {ClassName}");
            sb.Append(Environment.NewLine);
            sb.Append(ClassName + " = {}");
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }
    }
}