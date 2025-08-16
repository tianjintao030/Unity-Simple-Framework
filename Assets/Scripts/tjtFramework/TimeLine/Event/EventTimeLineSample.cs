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
                Debug.Log($"ִ�й��{trackId},Ƭ��{clipId}��ʼ�¼�");
            }
            if (trackId == trackId_1 && clipId == clipId_2)
            {
                Debug.Log($"ִ�й��{trackId},Ƭ��{clipId}��ʼ�¼�");
            }
            if (trackId == trackId_2 && clipId == clipId_1)
            {
                Debug.Log($"ִ�й��{trackId},Ƭ��{clipId}��ʼ�¼�");
            }
            if (trackId == trackId_2 && clipId == clipId_2)
            {
                Debug.Log($"ִ�й��{trackId},Ƭ��{clipId}��ʼ�¼�");
            }
        }

        public void OnTimelineEventExit(string trackId, string clipId)
        {
            if (trackId == trackId_1 && clipId == clipId_1)
            {
                Debug.Log($"ִ�й��{trackId},Ƭ��{clipId}�����¼�");
            }
            if (trackId == trackId_1 && clipId == clipId_2)
            {
                Debug.Log($"ִ�й��{trackId},Ƭ��{clipId}�����¼�");
            }
            if (trackId == trackId_2 && clipId == clipId_1)
            {
                Debug.Log($"ִ�й��{trackId},Ƭ��{clipId}�����¼�");
            }
            if (trackId == trackId_2 && clipId == clipId_2)
            {
                Debug.Log($"ִ�й��{trackId},Ƭ��{clipId}�����¼�");
            }
        }
    }
}

