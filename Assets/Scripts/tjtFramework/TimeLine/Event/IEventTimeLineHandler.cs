using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.TimeLine
{
    [SerializeField]
    public interface IEventTimeLineHandler
    {
        public void OnTimelineEventEnter(string trackId, string clipId);
        public void OnTimelineEventExit(string trackId, string clipId);
    }
}

