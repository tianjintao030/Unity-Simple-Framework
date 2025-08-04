using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tjtFramework.Singleton;
using System;
using tjtFramework.PublicMono;
using UnityEngine.Events;

namespace tjtFramework.Input
{
    /// <summary>
    /// 缓存输入类型枚举
    /// </summary>
    [SerializeField]
    public enum InputType
    {
        jump,
        attack
    }

    /// <summary>
    /// 输入缓存队列
    /// </summary>
    public class InputBuffer : MonoSingleton<InputBuffer>
    {
        [SerializeField]
        public class BuffItem
        {
            public float bufferTime = 0.2f;
            public float timer;
        }

        private Dictionary<InputType, Queue<BuffItem>> queueBuffDic = new();
        private int queueMaxCount = 10;

        public override void Init()
        {
            base.Init();
            queueBuffDic.Clear();
            MonoManager.Instance.AddUpdateListener(UpdateBufferQueue);
        }

        public override void Destroy()
        {
            base.Destroy();
            queueBuffDic.Clear();
            MonoManager.Instance.RemoveUpdateListener(UpdateBufferQueue);
        }

        /// <summary>
        /// 输入缓存入队
        /// </summary>
        /// <param name="inputType"></param>
        /// <param name="bufferTime"></param>
        public void EnqueueInputBuffer(InputType inputType, float bufferTime = 0.3f)
        {
            if(!queueBuffDic.ContainsKey(inputType))
            {
                queueBuffDic[inputType] = new Queue<BuffItem>();
            }

            if(queueBuffDic[inputType].Count >= queueMaxCount)
            {
                queueBuffDic[inputType].Dequeue();
            }

            queueBuffDic[inputType].Enqueue(new BuffItem
            {
                bufferTime = bufferTime,
                timer = bufferTime,
            });

            Debug.Log($"添加输入缓存{inputType}");
        }

        /// <summary>
        /// 消耗输入缓存
        /// </summary>
        /// <param name="inputType"></param>
        /// <param name="calback"></param>
        /// <returns></returns>
        public bool ConsumeInputBuffer(InputType inputType, UnityAction calback = null)
        {
            if(queueBuffDic.TryGetValue(inputType, out Queue<BuffItem> queue))
            {
                if(queue.Count <= 0)
                {
                    Debug.LogError($"要消耗的输入缓存{inputType}队列为空");
                    return false;
                }

                queue.Dequeue();
                calback?.Invoke();
                return true;
            }

            Debug.Log($"未找到类型为{inputType}的输入缓存队列");
            return false;
        }

        /// <summary>
        /// 是否有输入缓存
        /// </summary>
        /// <param name="inputType"></param>
        /// <returns></returns>
        public bool IsHaveInputBuffer(InputType inputType)
        {
            if(queueBuffDic.ContainsKey(inputType) && queueBuffDic[inputType].Count > 0)
            {
                return true;
            }
            return false;
        }


        public void ClearAllInputBuffer()
        {
            queueBuffDic.Clear();
        }

        public void ClearInputBuffer(InputType inputType)
        {
            if(queueBuffDic.ContainsKey(inputType))
            {
                queueBuffDic[inputType].Clear();
            }
        }

        private void UpdateBufferQueue()
        {
            if(queueBuffDic.Count <= 0)
            {
                return;
            }

            // 启用按时清楚缓存，则取消注释该部分
            //foreach(var queue in queueBuffDic.Values)
            //{
            //    while(queue.Count > 0)
            //    {
            //        var buffItem = queue.Peek();
            //        buffItem.timer -= Time.deltaTime;

            //        if (buffItem.timer <= 0)
            //        {
            //            Debug.Log($"清除输入缓存");
            //            queue.Dequeue();
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //}
        }
    }

}

