﻿using EasyLua.Lexer;
using UnityEngine;
using XLua;
using Object = System.Object;

namespace EasyLua {
    [LuaCallCSharp]
    public class EasyBehaviour : MonoBehaviour {
        private EasyLuaEnv mEnv;

        [SerializeField, HideInInspector]
        private TextAsset mLuaCode;

        [SerializeField, HideInInspector]
        private EasyLuaParam[] mLuaParams;
        
        public LuaTable GetLuaInstance() {
            if (mEnv == null) {
                InitLuaEnv();
            }

            return mEnv?.Table;
        }

        public EasyLuaParam[] GetLuaParams() {
            return mLuaParams;
        }

        public void SetLuaParams(EasyLuaParam[] @params) {
            mLuaParams = @params;
        }

        public TextAsset LuaCode {
            set { mLuaCode = value; }
            get { return mLuaCode; }
        }

        public bool IsInitiated() {
            return mEnv != null;
        }

        public string GetClassName() {
            if (mEnv == null) {
                InitLuaEnv();
            }

            return mEnv?.GetClassName();
        }

        // attach a class instance to this component
        public void AttachLuaClass(string className) {
            DestroyEnv();
            mEnv = new EasyLuaEnv(className);
            SetUnityFields();
            CallLuaFun("Awake");
        }

        [System.Obsolete]
        public void AddLuaScript(string script) {
            DestroyEnv();
            mEnv = new EasyLuaEnv(script, null);
            SetUnityFields();
        }

        //push a object to lua environment
        public void PushFieldToLua(string key, Object value) {
            if (mEnv == null) {
                Debug.LogError("EasyLuaEnv not initiated");
                return;
            }

            mEnv.SetField(key, value);
        }

        private void InitLuaEnv() {
            if (mLuaCode == null) {
                return;
            }

            var invalidCode = string.IsNullOrWhiteSpace(mLuaCode.text)
                              || string.IsNullOrWhiteSpace(mLuaCode.name);

            if (invalidCode) {
                return;
            }

            mEnv = new EasyLuaEnv(EasyLuaLexer.TrimExtension(mLuaCode.name));
            SetUnityFields();
            PushParams();
        }

        private void PushParams() {
            if (mEnv == null || mLuaParams == null) {
                return;
            }

            for (int i = 0; i < mLuaParams.Length; i++) {
                mEnv.PushParam(mLuaParams[i]);
            }
        }

        private void SetUnityFields() {
            mEnv.SetField("gameObject", gameObject);
            mEnv.SetField("transform", transform);
            mEnv.SetField("this", this);
        }

        protected void CallLuaFun(string funName, params Object[] args) {
            if (mEnv != null) {
                mEnv.CallLuaFun(funName, args);
            }
        }

        protected virtual void Awake() {
            if (mEnv == null) {
                InitLuaEnv();
            }
        }

        protected virtual void OnDestroy() {
            DestroyEnv();
        }

        private void DestroyEnv() {
            CallLuaFun("OnDestroy");
            mEnv?.Dispose();
            mEnv = null;
        }
    }
}