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
        /// Timeline ����һ�� Select Clip ʱ����
        /// </summary>
        /// <param name="trackId">���ڹ��ID</param>
        /// <param name="clipId">clip ID</param>
        /// <returns>Ҫѡ��Ĺ�����֣�Ĭ�Ϸ���""</returns>
        public string OnSelectTimeline(string trackId, string clipId);
    }
}

