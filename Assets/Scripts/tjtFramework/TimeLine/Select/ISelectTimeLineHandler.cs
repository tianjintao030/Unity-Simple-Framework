using System.Collections;
using System.Collections.Generic;
using tjtFramework.TimeLine;
using UnityEngine;
using UnityEngine.Playables;

namespace tjtFramework.TimeLine
{
    public interface ISelectTimeLineHandler
    { 
        /// <summary>
        /// Timeline 进入一个 Select Clip 时调用
        /// </summary>
        /// <param name="trackId">所在轨道ID</param>
        /// <param name="clipId">clip ID</param>
        /// <returns>要选择的轨道名字，默认返回""</returns>
        public string OnSelectTimeline(string trackId, string clipId);
    }
}

