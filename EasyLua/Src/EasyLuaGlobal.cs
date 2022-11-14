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

        public static EasyLuaGlobal Get(bool create = true) {
            if (instance) {
                return instance;
            }

            if (!create) {
                return null;
            }

            var obj = new GameObject("EasyLuaGlobal");
            DontDestroyOnLoad(obj);
            instance = obj.AddComponent<EasyLuaGlobal>();
            return instance;
        }

        public static bool Delete() {
            if (instance) {
                instance.mEnvTable.Dispose();
                instance.mEnvTable = null;

                // var c = @"
                // local util = require 'xlua.util'
                // util.print_func_ref_by_csharp()";
                // instance.mLuaEnv.DoString(c);
                instance.mLuaEnv.Tick();
                // caching luaFun causes "reference not eliminated" error when trying to  dispose 
                //instance.mLuaEnv.Dispose();
                instance.mLuaEnv = null;
                DestroyImmediate(instance.gameObject);

                instance = null;
                return true;
            }

            return false;
        }

        private LuaEnv mLuaEnv;
        private LuaTable mEnvTable;

        public string LoadClass(string script) {
            return LoadLua(script);
        }

        public void BulkLoadClass(IEnumerable<string> loader) {
            Assert.IsNotNull(loader);
            foreach (var script in loader) {
                LoadLua(script);
            }
        }

        // create a class instance in global environment
        public LuaTable NewClass(string className, params object[] paras) {
            return NewClassImpl(className, paras);
        }

        // directly execute lua script
        public void ExecuteString(string content) {
            mLuaEnv.DoString(content);
        }


        private void Awake() {
            InitLuaEnv();
        }

        private void Start() {
            StartCoroutine(CoTickLuaEnv());
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
                //regular lua script , execute directly
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
            // properly set chunk name so emmylua debugger can work
            mLuaEnv.DoString(script, className);
            var regCmd = $"RegClass({className},'{className}')";
            var baseClass = lexer.GetBaseClassName();
            if (!string.IsNullOrWhiteSpace(baseClass)) {
                regCmd = $"RegClass({className},'{className}','{baseClass}')";
            }

            mLuaEnv.DoString(regCmd);
        }

        private LuaTable NewClassImpl(string className, params object[] paras) {
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