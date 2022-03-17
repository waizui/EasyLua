using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using EasyLua.Lexer;
using XLua;

namespace EasyLua {
    public class EasyLuaGlobal : MonoBehaviour {
        private static EasyLuaGlobal instance = null;

        public static EasyLuaGlobal Get() {
            if (instance) {
                return instance;
            }

            var obj = new GameObject("EasyLuaGlobal");
            DontDestroyOnLoad(obj);
            instance = obj.AddComponent<EasyLuaGlobal>();
            return instance;
        }

        public static void Delete() {
            if (instance) {
#if UNITY_EDITOR
                instance.mLuaEnv.DoString("Util.PrintRef()");
#endif
                instance.mLuaEnv.Dispose();
                DestroyImmediate(instance);
                instance = null;
            }
        }

        private LuaEnv mLuaEnv;
        private LuaTable mEnvTable;

        private void Awake() {
            InitLuaEnv();
        }

        private void InitLuaEnv() {
            mLuaEnv = new LuaEnv();

#if UNITY_EDITOR
            OpenDebug();
#endif
            LuaTable meta = mLuaEnv.NewTable();
            meta.Set("__index", mLuaEnv.Global);
            mEnvTable = mLuaEnv.NewTable();
            mEnvTable.SetMetaTable(meta);

            mLuaEnv.DoString(GetLanguageLua(), "language.lua");
            meta.Dispose();
        }

        private void OpenDebug() {
            try {
                string configPath = Path.Combine(Application.dataPath + "/../", "luaDebugConfig.txt");
                if (File.Exists(configPath)) {
                    string chunk = File.ReadAllText(configPath);
                    mLuaEnv.DoString(chunk);
                }
            } catch {
                // ignored
            }
        }

        private void Start() {
            StartCoroutine(CoTickLuaEnv());
        }

        private IEnumerator CoTickLuaEnv() {
            while (true) {
                yield return new WaitForSeconds(1);
                mLuaEnv?.Tick();
            }
        }

        private string GetLanguageLua() {
            var preScript = Resources.Load<TextAsset>("language.lua");
            return preScript?.text;
        }

        private string LoadLua(string script) {
            if (string.IsNullOrWhiteSpace(script)) {
                Debug.LogError("easy lua load error : script empty");
                return null;
            }

            try {
                var lexer = new EasyLuaLexer(script);
                PushClass(lexer);
                return lexer.GetBaseClassName();
            } catch (EasyLuaSyntaxError e) {
                //普通lua脚本 直接执行
                mLuaEnv.DoString(script);
                return null;
            } catch (Exception e) {
                Debug.LogError(e.Message);
                Debug.LogError(script);
                throw;
            }
        }

        private void PushClass(EasyLuaLexer lexer) {
            var script = lexer.GetScript();
            var className = lexer.GetLuaClassName();
            // chunk 要设置好 emmylua断点才能工作
            mLuaEnv.DoString(script, className);
            var regCmd = $"RegClass({className},'{className}')";
            var baseClass = lexer.GetBaseClassName();
            if (!string.IsNullOrWhiteSpace(baseClass)) {
                regCmd = $"RegClass({className},'{className}','{baseClass}')";
            }

            mLuaEnv.DoString(regCmd);
        }

        public string LoadClass(string script) {
            return LoadLua(script);
        }

        public void BulkLoadClass(IEnumerable<string> loader) {
            Assert.IsNotNull(loader);
            foreach (var script in loader) {
                LoadLua(script);
            }
        }

        public LuaTable NewClass(string className, params object[] paras) {
            var hasArgs = (paras != null && paras.Length != 0);
            if (hasArgs) {
                var args = mLuaEnv.DoString("local paraTable={} return paraTable");
                var table = args[0] as LuaTable;
                for (int i = 0; i < paras.Length; i++) {
                    table.Set(i, paras[i]);
                }
            }

            var newChunk = $"return NewClass('{className}')";
            if (hasArgs) {
                newChunk = $"return NewClass('{className}',table.unpack(paraTable))";
            }

            var ret = mLuaEnv.DoString(newChunk);
            var t = ret[0] as LuaTable;
            mLuaEnv.DoString("paraTable=nil");
            return t;
        }
    }
}