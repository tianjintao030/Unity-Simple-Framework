using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.Pool
{
    public class ClassObjectPool<T> where T : class, new()
    {
        /// <summary>
        /// 对象栈
        /// </summary>
        protected Stack<T> pool = new();
        /// <summary>
        /// 最大对象数, <= 0 不限个数
        /// </summary>
        protected int maxCount;

        public ClassObjectPool(int maxCount = 30)
        {
            this.maxCount = maxCount;
            for(int i = 0; i < maxCount; i++)
            {
                pool.Push(new T());
            }
        }

        public T Spawn()
        {
            if(pool.Count > 0)
            {
                return pool.Pop();
            }
            else
            {
                return new T();
            }
        }

        public void Release(T obj)
        {
            if(obj == null)
            {
                Debug.LogError($"回收类失败,obj == null");
                return;
            }
            pool.Push(obj);
        }
    }
}

