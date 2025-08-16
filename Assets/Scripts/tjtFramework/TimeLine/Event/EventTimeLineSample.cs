using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace tjtFramework.TimeLine
{
    public class EventTimeLineSample : MonoBehaviour, IEventTimeLineHandler
    {
        public string trackId_1;
        public string trackId_2;
        public string clipId_1;
        public string clipId_2;

        public void OnTimelineEventEnter(string trackId, string clipId)
        {
            if(trackId == trackId_1 && clipId == clipId_1)
            {
                Debug.Log($"执行轨道{trackId},片段{clipId}开始事件");
            }
            if (trackId == trackId_1 && clipId == clipId_2)
            {
                Debug.Log($"执行轨道{trackId},片段{clipId}开始事件");
            }
            if (trackId == trackId_2 && clipId == clipId_1)
            {
                Debug.Log($"执行轨道{trackId},片段{clipId}开始事件");
            }
            if (trackId == trackId_2 && clipId == clipId_2)
            {
                Debug.Log($"执行轨道{trackId},片段{clipId}开始事件");
            }
        }

        public void OnTimelineEventExit(string trackId, string clipId)
        {
            if (trackId == trackId_1 && clipId == clipId_1)
            {
                Debug.Log($"执行轨道{trackId},片段{clipId}结束事件");
            }
            if (trackId == trackId_1 && clipId == clipId_2)
            {
                Debug.Log($"执行轨道{trackId},片段{clipId}结束事件");
            }
            if (trackId == trackId_2 && clipId == clipId_1)
            {
                Debug.Log($"执行轨道{trackId},片段{clipId}结束事件");
            }
            if (trackId == trackId_2 && clipId == clipId_2)
            {
                Debug.Log($"执行轨道{trackId},片段{clipId}结束事件");
            }
        }
    }
}

