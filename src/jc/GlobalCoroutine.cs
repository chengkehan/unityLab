using UnityEngine;
using System.Collections;

namespace JC
{
    public class GlobalCoroutine
    {
        private static GameObject go = null;

        private static MonoBehaviour mono = null;

        public static void Start(IEnumerator coroutine)
        {
            if (go == null)
            {
                go = new GameObject();
                go.name = "__JC_GloablCoroutine__";
                GameObject.DontDestroyOnLoad(go);

                mono = go.AddComponent<MonoBehaviour>();
            }
            mono.StartCoroutine(coroutine);
        }
    }
}