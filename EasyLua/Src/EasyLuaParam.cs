using System;
using UnityEngine;
using Object = System.Object;

namespace EasyLua {
    [System.Serializable]
    public class EasyLuaParam {
        [NonSerialized]
        private string mTypeName;

        public string LowerTypeName {
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

        public string RawTypeName {
            get {
                return mTypeName;
            }
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

        [SerializeField]
        private int mType;


        public EasyLuaParam() {
            ClearValue();
        }

        public EasyLuaParamType ParamType {
            get {
                return (EasyLuaParamType)mType;
            }
        }

        public long Int {
            get { return (intVal); }
            set {
                ClearValue(EasyLuaParamType.Int);
                intVal = value;
            }
        }

        public float Float {
            get { return (floatVal); }
            set {
                ClearValue(EasyLuaParamType.Float);
                floatVal = value;
            }
        }

        public bool Bool {
            get { return boolVal == 1; }
            set {
                ClearValue(EasyLuaParamType.Boolean);
                boolVal = value ? 1 : -1;
            }
        }

        public int BoolValue {
            get { return boolVal; }
        }

        public string String {
            get { return stringVal; }
            set {
                ClearValue(EasyLuaParamType.String);
                stringVal = value;
            }
        }

        public EasyLuaParam[] Array {
            get { return arrObj; }
            set {
                ClearValue(EasyLuaParamType.Array);
                arrObj = value;
            }
        }

        public UnityEngine.Object UnityObject {
            get { return unityObj; }
            set {
                ClearValue(EasyLuaParamType.UnityObject);
                unityObj = value;
            }
        }

        public Object ValueObject {
            get { return valObj; }
            set {
                ClearValue(EasyLuaParamType.Ref);
                valObj = value;
            }
        }

        public int EnumVal {
            get { return enumVal; }
            set {
                ClearValue(EasyLuaParamType.Enum);
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
            target.mType = mType;
        }

        private void ClearValue(EasyLuaParamType type = EasyLuaParamType.None) {
            valObj = null;
            unityObj = null;
            intVal = 0;
            floatVal = 0;
            boolVal = 0;
            stringVal = null;
            arrObj = null;
            enumVal = -1;
            mType = (int)type;
        }


        public bool IsArray() {
            if (string.IsNullOrEmpty(mTypeName)) {
                return false;
            }

            return mTypeName.EndsWith("[]");
        }

        public System.Object Cast() {
            if (ParamType == EasyLuaParamType.None) {
                return TryCast();
            }

            switch (ParamType) {
                case EasyLuaParamType.Int:
                    return Int;
                case EasyLuaParamType.Float:
                    return Float;
                case EasyLuaParamType.Boolean:
                    return Bool;
                case EasyLuaParamType.Array:
                    return CastArray();
                case EasyLuaParamType.String:
                    return String;
                case EasyLuaParamType.Enum:
                    return EnumVal;
                case EasyLuaParamType.Ref:
                    return ValueObject;
                case EasyLuaParamType.UnityObject:
                    if (UnityObject != null) {
                        if (UnityObject is EasyBehaviour) {
                            var luaClass = (UnityObject as EasyBehaviour)?.GetLuaInstance();
                            return luaClass;
                        }

                        return UnityObject;
                    }
                    return null;
                default:
                    return TryCast();
            }
        }

        public System.Object TryCast() {
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


            var arr = CastArray();
            if (arr != null) {
                return arr;
            }

            if (EnumVal != -1) {
                return EnumVal;
            }

            return null;
        }

        private System.Object CastArray() {
            if (Array != null) {
                if (Array.Length == 0) {
                    return null;
                }

                var arr = new object[Array.Length];
                for (int i = 0; i < Array.Length; i++) {
                    var para = Array[i];
                    if (para != null) {
                        arr[i] = para.Cast();
                    }
                }

                return arr;
            }

            return null;
        }

    }
}