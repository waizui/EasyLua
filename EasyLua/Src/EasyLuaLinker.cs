using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace EasyLua.Lexer {

    // used for linking multiple lua scripts into a (or several) single large lua script
    public class EasyLuaLinker {

        private StringBuilder mSb = new StringBuilder();

        private List<string> mNoEasyLuaScripts = new List<string>();

        public void AddScript(string script) {
            Assert.IsFalse(string.IsNullOrEmpty(script));

            try {
                var lexer = new EasyLuaLexer(script);
                var className = lexer.GetLuaClassName();
                var regCmd = $"RegClass({className},'{className}')";
                var baseClass = lexer.GetBaseClassName();
                if (!string.IsNullOrWhiteSpace(baseClass)) {
                    regCmd = $"RegClass({className},'{className}','{baseClass}')";
                }
                AppendScript(script, regCmd);
            } catch (EasyLuaSyntaxError e) {
                mNoEasyLuaScripts.Add(script);
            } catch (Exception e) {
                Debug.LogError(e.Message);
                Debug.LogError(script);
                throw;
            }
        }

        private void AppendScript(string content, string cmd = null) {
            mSb.AppendLine();
            mSb.AppendLine(content);
            if (cmd != null) {
                mSb.AppendLine(cmd);
            }
        }

        public void Clear() {
            mSb.Clear();
        }

        public string[] GetLinkedScripts() {
            var arr = new string[mNoEasyLuaScripts.Count + 1];
            var maxIndex = arr.Length - 1;
            for (int i = 0; i < maxIndex; i++) {
                arr[i] = mNoEasyLuaScripts[i];
            }
            arr[maxIndex] = mSb.ToString();

            return arr;
        }
    }

}