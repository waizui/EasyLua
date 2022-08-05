# EasyLua tutorial

## Create a lua script

Right click a folder then click <b>Create->LuaClass</b> to create a lua script and give it a name,for instance "ExampleCode",

## Write some lua code

Open the lua script in your code editor, you will see:
```Lua
    ---@class ExampleCode
    ExampleCode= {}
```

* First of all , a class declaration must on the top of the script. It starts with "---@class" and followed with class name.
If there are multiple classe declararions in one single script , you need to use EasyLua built-in function <b>RegClass</b>
to Register Classes other than the first one.

```Lua
    RegClass(class,className,superClassName)
```
```Lua
    ---@class ExampleCode
    ExampleCode= {}

    ---@class OtherClass
    OtherClass= {}
    RegClass(OtherClass,"OtherClass","LuaObject")
```

* And you can specify the super class of a class ,the syntax is "class:superClass". all classes implictly inherit from LuaObject.

```Lua
    ---@class ExampleCode:LuaObject
    ExampleCode= {}

    ---@class OtherClass
    OtherClass= {}
    RegClass(OtherClass,"OtherClass","LuaObject")
```


* Declare fields just below the class declaration,the field declaration must starts with "---" and then followed with "@field"
 and continue with a modifier then the field name ,and finally, the type of the field. The field can be a C# type or a lua type.
  
  A modifier could be "public" or "private". if it's public , it will be displayed in inspector of EasyBehaviour.

  For example "---@field public obj UnityEngine.GameObject"

```Lua
    ---@class ExampleCode
    ---@field public obj UnityEngine.GameObject
    ---@field private luaNumber number
    ExampleCode= {}
```
![exampleCode](./exampleCode.png)


* After declaration,you can write funcions of the script. and don't forget drag references in script's inspector.

```Lua
    ---@class ExampleCode
    ---@field public obj UnityEngine.GameObject
    ---@field private luaNumber number
    ExampleCode= {}

    function ExampleCode:Awake()
        print("obj Name is:" self.obj.name)
    end  
```

* I don't recommend to use EasyBehaviour directly , just inherit it and make your own behaviour class, that could be more versatile.
for example:

```CSharp
    public class LuaBehaviour : EasyBehaviour {
        public void CallLua(string funName, params object[] args) {
            CallLuaFun(funName, args);
        }

        protected override void Awake() {
            base.Awake();
            CallLuaFun("Awake");
        }

        protected virtual void Start() {
            CallLuaFun("Start");
        }

        protected virtual void OnEnable() {
            CallLuaFun("OnEnable");
        }

        protected virtual void OnDisable() {
            CallLuaFun("OnDisable");
        }

        protected virtual void OnDestory() {
            CallLuaFun("OnDestory");
        }
    }
```



