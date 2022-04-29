using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;

namespace EasyLua {
    public class EasyLuaEnv : IDisposable {
        private LuaTable mTable;

        public LuaTable Table {
            get {
                if (mTable != null) {
                    return mTable;
                }

                return mTable;
            }
        }

        private string mLualassName;
        private string mFileName;

        public EasyLuaEnv(string script, string fileName) {
            Assert.IsFalse(string.IsNullOrWhiteSpace(script));
            mLualassName = LoadClass(script);
            mFileName = fileName;
        }

        public EasyLuaEnv(string className) {
            Assert.IsFalse(string.IsNullOrWhiteSpace(className));
            mLualassName = className;
            InitClass(className);
        }

        private void InitClass(string className) {
            var env = EasyLuaGlobal.Get();
            mTable = env.NewClass(className);
        }

        private string LoadClass(string script) {
            var env = EasyLuaGlobal.Get();
            var cls = env.LoadClass(script);
            env.NewClass(cls);
            return cls;
        }


        public string GetClassName() {
            return mLualassName;
        }

        public void SetField<TKey, TValue>(TKey key, TValue value) {
            mTable.Set(key, value);
        }

        public void PushParam(EasyLuaParam parameters) {
            if (parameters == null) {
                Debug.LogError("error: try push null parameter to lua");
                return;
            }

            var table = Table;
            var fieldName = parameters.name;
            if (parameters.UnityObject != null) {
                if (parameters.UnityObject is EasyBehaviour) {
                    var luaClass = (parameters.UnityObject as EasyBehaviour)?.GetLuaInstance();
                    table.Set(fieldName, luaClass);
                    return;
                }

                table.Set(fieldName, parameters.UnityObject);
                return;
            }

            if (parameters.ValueObject != null) {
                table.Set(fieldName, parameters.ValueObject);
                return;
            }

            if (parameters.Int != 0) {
                table.Set(fieldName, parameters.Int);
                return;
            }

            if (parameters.Float != 0) {
                table.Set(fieldName, parameters.Float);
                return;
            }

            if (parameters.BoolValue != 0) {
                table.Set(fieldName, parameters.Bool);
                return;
            }

            if (!string.IsNullOrWhiteSpace(parameters.String)) {
                table.Set(fieldName, parameters.String);
                return;
            }

            if (parameters.Array != null && parameters.Array.Length != 0) {
                var arr = new object[parameters.Array.Length];
                for (int i = 0; i < parameters.Array.Length; i++) {
                    arr[i] = CastParam(parameters.Array[i]);
                }

                table.Set(fieldName, arr);
                return;
            }

            if (parameters.EnumVal != -1) {
                table.Set(fieldName, parameters.EnumVal);
            }
        }

        private System.Object CastParam(EasyLuaParam param) {
            if (param == null) {
                return null;
            }

            return param.Cast();
        }

        private Dictionary<string, LuaFun> mFunCaches = new Dictionary<string, LuaFun>();

        // TODO: 添加泛型实现
        public void CallLuaFun(string fun, params System.Object[] args) {
            LuaFun luaFun = null;
            if (mFunCaches.TryGetValue(fun, out luaFun)) {
                if (luaFun != null) {
                    luaFun(mTable, args);
                }
            } else {
                mTable.Get(fun, out luaFun);
                if (luaFun != null) {
                    luaFun(mTable, args);
                }

                mFunCaches.Add(fun, luaFun);
            }
        }

        public void Dispose() {
            mFunCaches.Clear();
            mFunCaches = null;
            mTable.Dispose();
            mTable = null;
        }
    }
}