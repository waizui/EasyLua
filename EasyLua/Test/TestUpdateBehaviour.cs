using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EasyLua {
    public class TestUpdateBehaviour : EasyBehaviour {
        protected override void Awake() {
            base.Awake();
            CallLuaFun("Awake");
        }

        private void Start() {
            CallLuaFun("Start");
        }

        private void OnEnable() {
            CallLuaFun("OnEnable");
        }

        private void Update() {
            CallLuaFun("Update");
        }
    }
}