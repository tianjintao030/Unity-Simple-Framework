using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.Singleton
{
    /// <summary>
    /// 不继承MonoBehaiavor的单例的基类
    /// 要求必须有一个空参构造函数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NoMonoSingleton<T> where T : class, new()
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                    instance = new T();
                return instance;
            }
        }
    }
}


