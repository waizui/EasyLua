using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
using Object = System.Object;

namespace EasyLua {
    [CSharpCallLua]
    [LuaCallCSharp]
    public delegate void LuaFun(LuaTable t, params Object[] args);
}