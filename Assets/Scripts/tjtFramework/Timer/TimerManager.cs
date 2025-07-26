using System.Collections;
using System.Collections.Generic;
using tjtFramework.PublicMono;
using tjtFramework.Singleton;
using UnityEngine;
using UnityEngine.Events;

namespace tjtFramework.Timer
{
    public class TimerManager : MonoSingleton<TimerManager>
    {
        //当前要创建的计时器id
        private int TIMER_ID = 0;
        //存储计时器
        private Dictionary<int, TimerItem> timerDic = new Dictionary<int, TimerItem>();
        //存储不受Time.timeScale影响的计时器
        private Dictionary<int, TimerItem> realTimerDic = new Dictionary<int, TimerItem>();
        //待移除列表
        private List<int> delList = new List<int>();

        private Coroutine timer;
        private Coroutine realTimer;

        //计时器检查间隔，单位为秒
        private const float intervalTime = 0.1f;//100毫秒

        public void Begin()
        {
            timer = MonoManager.Instance.StartCoroutine(StartTiming(false, timerDic));
            realTimer = MonoManager.Instance.StartCoroutine(StartTiming(true, realTimerDic));
            Debug.Log("计时器管理器启动");
        }

        private WaitForSeconds waitForSeconds = new WaitForSeconds(intervalTime);
        private WaitForSecondsRealtime waitForSecondsRealtime = new WaitForSecondsRealtime(intervalTime);

        IEnumerator StartTiming(bool isReal, Dictionary<int, TimerItem> timer_dic)
        {
            while (true)
            {
                // 根据 isReal 选择延迟等待时间
                yield return isReal ? waitForSecondsRealtime : waitForSeconds;

                //取一个快照，防止在循环中对字典值的修改导致的报错
                List<TimerItem> timers_snapshot = new List<TimerItem>(timer_dic.Values);

                foreach (TimerItem item in timers_snapshot)
                {
                    if (!item.isRuning)
                        continue;
                    if (item.callBack != null)
                    {
                        item.intervalTime -= intervalTime;//减时间
                        if (item.intervalTime <= 0f)
                        {
                            item.callBack.Invoke();//执行回调
                            item.intervalTime = item.isRandom ? item.GenerateRandomInterval() : item.maxIntervalTime;//重置间隔
                        }
                    }

                    if (item.allTime < 0f)//无限时间
                        continue;

                    item.allTime -= intervalTime;
                    if (item.allTime <= 0f)
                    {
                        item.overCallBack?.Invoke();
                        delList.Add(item.id);
                    }
                }

                for (int i = 0; i < delList.Count; i++)
                {
                    if (timer_dic.TryGetValue(delList[i], out TimerItem item))
                    {
                        item.ClearRef();
                        timer_dic.Remove(delList[i]);
                    }
                }
                delList.Clear();
            }
        }

        public void Stop()
        {
            MonoManager.Instance.StopCoroutine(timer);
            MonoManager.Instance.StopCoroutine(realTimer);
        }

        /// <summary>
        /// 创建计时器 1秒=1000毫秒 all_time小于0为无限
        /// </summary>
        /// <param name="all_time"></param>
        /// <param name="over_callback"></param>
        /// <param name="interval_time"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public int CreateTimer(bool isReal, bool isRandom, float all_time, UnityAction over_callback = null, float interval_time = 0f,
            UnityAction callback = null, float min_interval = 0, float max_interval = 0)
        {
            int id = ++TIMER_ID;
            TimerItem timer = new TimerItem();
            timer.Init(id, all_time, over_callback, interval_time, callback, isRandom, min_interval, max_interval);

            if (!isReal)
                timerDic.Add(id, timer);
            else
                realTimerDic.Add(id, timer);

            Debug.Log($"创建计时器 id:{id}");

            return id;
        }

        /// <summary>
        /// 移除计时器
        /// </summary>
        /// <param name="id"></param>
        public void RemoveTimer(int id)
        {
            if (timerDic.ContainsKey(id))
            {
                timerDic.Remove(id);
            }
            else if (realTimerDic.ContainsKey(id))
            {
                realTimerDic.Remove(id);
            }
        }

        /// <summary>
        /// 重置计时器
        /// </summary>
        /// <param name="id"></param>
        public void ResetTimer(int id)
        {
            if (timerDic.ContainsKey(id))
            {
                timerDic[id].ResetTimer();
            }
            else if (realTimerDic.ContainsKey(id))
            {
                realTimerDic[id].ResetTimer();
            }
        }

        /// <summary>
        /// 开启计时器
        /// </summary>
        /// <param name="id"></param>
        public void StartTimer(int id)
        {
            if (timerDic.ContainsKey(id))
            {
                timerDic[id].isRuning = true;
            }
            else if (realTimerDic.ContainsKey(id))
            {
                realTimerDic[id].isRuning = true;
            }
        }

        /// <summary>
        /// 关闭计时器
        /// </summary>
        /// <param name="id"></param>
        public void CloseTimer(int id)
        {
            if (timerDic.ContainsKey(id))
            {
                timerDic[id].isRuning = false;
            }
            else if (realTimerDic.ContainsKey(id))
            {
                realTimerDic[id].isRuning = false;
            }
        }

    }
}

