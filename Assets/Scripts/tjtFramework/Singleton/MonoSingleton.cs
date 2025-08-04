using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.Singleton
{
    /// <summary>
    /// 继承MonoBehaviour的单例对象的基类（无需手动挂载）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static bool applicationIsQuit = false;
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (applicationIsQuit)
                {
                    return _instance;
                }

                if (_instance == null && Application.isPlaying)
                {
                    //在场景中根据类型查找引用
                    _instance = FindObjectOfType<T>();
                    if (_instance == null)
                    {
                        //在调用Instance时自动创建其游戏对象
                        _instance = new GameObject("Singleton of " + typeof(T)).AddComponent<T>();
                        DontDestroyOnLoad(_instance);
                    }
                    if (_instance == null)
                    {
                        Debug.LogError("Failed to create instance of" + typeof(T));
                    }
                }
                return _instance;
            }
        }
        protected void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            Init();
        }
        public virtual void Init()
        { }

        public void OnDestroy()
        {
            Destroy();
            applicationIsQuit = true;
        }

        public virtual void Destroy() 
        { }
    }
}



