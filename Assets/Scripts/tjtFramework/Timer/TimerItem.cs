using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace tjtFramework.Timer
{
    public class TimerItem : MonoBehaviour
    {
        //唯一id
        public int id;
        /// <summary>
        /// 计时结束时调用回调
        /// </summary>
        public UnityAction overCallBack;
        /// <summary>
        /// 间隔时间调用回调
        /// </summary>
        public UnityAction callBack;
        /// <summary>
        /// 计时器总记录时间(毫秒)
        /// </summary>
        public float allTime;
        //一开始的总时间，用于计时器重置
        public float maxAllTime;
        /// <summary>
        /// 执行回调的时间间隔
        /// </summary>
        public float intervalTime;
        /// <summary>
        /// 一开始的时间间隔，用于计时器重置
        /// </summary>
        public float maxIntervalTime;
        /// <summary>
        /// 是否正在计时
        /// </summary>
        public bool isRuning = false;
        /// <summary>
        /// 是否随机回调间隔
        /// </summary>
        public bool isRandom = false;
        /// <summary>
        /// 最小时间间隔（可选）
        /// </summary>
        public float minInterval;
        /// <summary>
        /// 最大时间间隔（可选）
        /// </summary>
        public float maxInterval;
        /// <summary>
        /// 初始化计时器
        /// </summary>
        /// <param name="id"></param>
        /// <param name="all_time">总时间</param>
        /// <param name="over_callback">计时结束回调</param>
        /// <param name="interval_time">间隔时间</param>
        /// <param name="callback">间隔回调</param>
        public void Init(int id, float all_time, UnityAction over_callback, float interval_time, UnityAction callback,
            bool is_random = false, float min_interval = 0, float max_interval = 0)
        {
            this.id = id;
            maxAllTime = allTime = all_time;
            overCallBack = over_callback;
            callBack = callback;
            isRandom = is_random;
            minInterval = min_interval;
            maxInterval = max_interval;

            if (isRandom)
                intervalTime = GenerateRandomInterval();
            else
                maxIntervalTime = intervalTime = interval_time;

            isRuning = true;
        }

        /// <summary>
        /// 清除相关引用
        /// </summary>
        public void ClearRef()
        {
            overCallBack = null;
            callBack = null;
        }

        public void ResetTimer()
        {
            allTime = maxAllTime;
            intervalTime = maxIntervalTime;
            isRuning = true;
        }

        public void OverCallBack()
        {
            overCallBack?.Invoke();
        }

        public void CallBack()
        {
            callBack?.Invoke();
        }

        public float GenerateRandomInterval()
        {
            return Random.Range(minInterval, maxInterval);
        }

    }
}
