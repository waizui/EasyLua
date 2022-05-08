using System;
using System.Collections;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace EasyLua {
    public class TestLoad : MonoBehaviour {
#if UNITY_EDITOR

        public GameObject testObject;

        private void Awake() {
            StartCoroutine(CoLoadClass());
        }

        private void Start() {
        }

        /// <summary>
        /// 分帧加载示例 也可以用来接入异步加载 
        /// </summary>
        /// <returns></returns>
        IEnumerator CoLoadClass() {
            // 所有lua代码加载后才能初始化lua环境
            var assets = GetLuaFiles();
            for (int i = 0 ; i < assets.Length ; i++) {
                try {
                    EasyLuaGlobal.Get().LoadClass(assets[i].text);
                } catch (Exception e) {
                    Debug.LogError("载入lua代码文件时错误" + assets[i].name);
                    throw;
                }

                yield return new WaitForEndOfFrame();
            }

            var obj = GameObject.Instantiate(testObject, transform);
            obj.SetActive(true);
        }

        private TextAsset[] GetLuaFiles() {
            var folder = new string[] { "Assets/EasyLua/Examples/Code" };
            var assets = AssetDatabase.FindAssets("t:TextAsset", folder);
            var codes = assets.Select((g) => {
                var path = AssetDatabase.GUIDToAssetPath(g);
                return AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            });
            return codes.ToArray();
        }
#endif
    }
}