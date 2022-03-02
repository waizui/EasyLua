# Easylua

easylua 是一个依赖于xlua的lua运行模块，目的是用最符合unity使用习惯的方式来写lua代码。


基本的脚本写法与c#几乎相同

```Lua
  ---@class LuaBehaviour:LuaObject
  ---@field private gameObject UnityEngine.GameObject
  ---@field private transform UnityEngine.Transform
  LuaBehaviour = {}

  function LuaBehaviour:Awake()
     print("hellow world")
  end
```

* 支持编辑器拖拽字段引用，与Mono脚本使用方法一致

* 支持基于继承的类型系统

* 配合emmylua插件实现语法分析
