using System;
using System.Linq.Expressions;
using UnityEngine;
using Object = System.Object;

namespace EasyLua {
    [System.Serializable]
    public class EasyLuaParam {
        [NonSerialized]
        private string mTypeName;

        private string LowerTypeName {
            get { return mTypeName?.ToLower(); }
        }

        public string TypeName {
            get {
                if (IsArray()) {
                    return mTypeName?.Replace("[]", "");
                }

                return mTypeName;
            }
            set { mTypeName = value; }
        }

        [NonSerialized]
        public bool arryFold;

        // not null
        [SerializeField]
        public string name;

        [SerializeField]
        private long intVal;

        [SerializeField]
        private float floatVal;

        [SerializeField]
        private int boolVal;

        [SerializeField]
        private string stringVal;

        [SerializeField]
        private int enumVal;

        // nullable
        [SerializeField]
        private UnityEngine.Object unityObj;

        [SerializeReference]
        private System.Object valObj; // 用来序列化一些内置类型

        [SerializeReference]
        private EasyLuaParam[] arrObj;

        public EasyLuaParam() {
            ClearValue();
        }

        public long Int {
            get { return (intVal); }
            set {
                ClearValue();
                intVal = value;
            }
        }

        public float Float {
            get { return (floatVal); }
            set {
                ClearValue();
                floatVal = value;
            }
        }

        public bool Bool {
            get { return boolVal == 1; }
            set {
                ClearValue();
                boolVal = value ? 1 : -1;
            }
        }

        public int BoolValue {
            get { return boolVal; }
        }

        public string String {
            get { return stringVal; }
            set {
                ClearValue();
                stringVal = value;
            }
        }

        public EasyLuaParam[] Array {
            get { return arrObj; }
            set {
                ClearValue();
                arrObj = value;
            }
        }

        public Color Color {
            get {
                if (valObj is Color) {
                    return (Color) valObj;
                }

                return Color.clear;
            }
            set {
                ClearValue();
                valObj = value;
            }
        }

        public Vector3 Vector3 {
            get {
                if (valObj is Vector3) {
                    return (Vector3) valObj;
                }

                return Vector3.zero;
            }
            set {
                ClearValue();
                valObj = value;
            }
        }

        public UnityEngine.Object UnityObject {
            get { return unityObj; }
            set {
                ClearValue();
                unityObj = value;
            }
        }

        public Object ValueObject {
            get { return valObj; }
        }

        public int EnumVal {
            get { return enumVal; }
            set {
                ClearValue();
                enumVal = value;
            }
        }

        public void CopyTo(EasyLuaParam target) {
            if (target == null) {
                return;
            }

            target.valObj = valObj;
            target.unityObj = unityObj;
            target.intVal = intVal;
            target.floatVal = floatVal;
            target.boolVal = boolVal;
            target.stringVal = stringVal;
            target.arrObj = arrObj;
            target.arryFold = arryFold;
            target.enumVal = enumVal;
        }

        private void ClearValue() {
            valObj = null;
            unityObj = null;
            intVal = 0;
            floatVal = 0;
            boolVal = 0;
            stringVal = null;
            arrObj = null;
            enumVal = -1;
        }

        public bool IsInt() {
            return LowerTypeName == "number" || LowerTypeName == "system.int32";
        }

        public bool IsFloat() {
            return LowerTypeName == "float" || LowerTypeName == "system.single";
        }

        public bool IsColor() {
            return LowerTypeName == "color" || LowerTypeName == "unityengine.color";
        }

        public bool IsVector3() {
            return LowerTypeName == "vector3" || LowerTypeName == "unityengine.vector3";
        }

        public bool IsBool() {
            return LowerTypeName == "bool" || LowerTypeName == "system.boolean";
        }

        public bool IsString() {
            return LowerTypeName == "string" || LowerTypeName == "system.string";
        }

        public bool IsArray() {
            if (string.IsNullOrEmpty(mTypeName)) {
                return false;
            }

            return mTypeName.EndsWith("[]");
        }

        public System.Object Cast() {
            if (UnityObject != null) {
                if (UnityObject is EasyBehaviour) {
                    var luaClass = (UnityObject as EasyBehaviour)?.GetLuaInstance();
                    return luaClass;
                }

                return UnityObject;
            }

            if (ValueObject != null) {
                return ValueObject;
            }

            if (Int != 0) {
                return Int;
            }

            if (Float != 0) {
                return Float;
            }

            if (BoolValue != 0) {
                return boolVal;
            }

            if (!string.IsNullOrWhiteSpace(String)) {
                return String;
            }

            if (Array != null) {
                var arr = new object[Array.Length];
                for (int i = 0; i < Array.Length; i++) {
                    var para = Array[i];
                    if (para != null) {
                        arr[i] = para.Cast();
                    }
                }

                return arr;
            }

            Debug.LogError("error : EasyLuaParam not cast properly" + TypeName);
            return null;
        }
    }
}