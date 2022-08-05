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
        /// load all lua scripts at runtime
        /// </summary>
        /// <returns></returns>
        IEnumerator CoLoadClass() {
            TextAsset[] assets = GetLuaFiles();
            for (int i = 0 ; i < assets.Length ; i++) {
                try {
                    EasyLuaGlobal.Get().LoadClass(assets[i].text);
                } catch (Exception e) {
                    Debug.LogError("error whild loading lua scripts" + assets[i].name);
                    throw;
                }

                yield return new WaitForEndOfFrame();
            }

            // after all lua scripts are loaded the EasyLua can work properly
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