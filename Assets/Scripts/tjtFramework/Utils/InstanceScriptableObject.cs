using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InstanceScriptableObject<T> : ScriptableObject where T :InstanceScriptableObject<T>
{
    protected abstract string ResourcePath { get; }

    private static T instance;
    public static T Instance
    {
        get
        {
#if UNITY_EDITOR
            // 优先查找 Editor 中加载的资源，避免多份
            if (instance == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{typeof(T).Name}");
                if (guids.Length > 0)
                {
                    string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    instance = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);
                }
            }
#endif
            if (instance == null)
            {
                // 正式运行时从 Resources 加载
                T temp = ScriptableObject.CreateInstance<T>();
                string path = temp.ResourcePath;
                UnityEngine.Object.DestroyImmediate(temp);

                instance = Resources.Load<T>(path);
                if (instance == null)
                {
                    Debug.LogError($"[InstanceScriptableObject] 无法找到资源：{path}");
                }
            }
            return instance;
        }
    }
}
