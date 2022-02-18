# Easylua

easylua 是一个依赖于xlua的lua运行模块，目的是用最符合unity使用习惯的方式来写lua代码。


基本的脚本写法与c#几乎相同

  ---@class LuaBehaviour:LuaObject
  ---@field private gameObject UnityEngine.GameObject
  ---@field private transform UnityEngine.Transform
  LuaBehaviour = {}

  function LuaBehaviour:Awake()
     print("hellow world")
  end
