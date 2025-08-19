using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace tjtFramework.TimeLine
{
    public class SkipTimeLineBehaviour : PlayableBehaviour
    {
        public string trackId;
        public string clipId;
        public PlayableDirector director;

        /// <summary>
        /// 绝对开始时间
        /// </summary>
        public double startTime;
        /// <summary>
        /// 绝对结束时间
        /// </summary>
        public double endTime;

        private bool canSkip = false;
        private bool hasSkipped = false;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (canSkip && !hasSkipped)
            {
                hasSkipped = true;
                canSkip = false;
                if(director != null)
                {
                    director.time = endTime;
                    director.Evaluate();
                    Debug.Log($"[SkipTimeLine]跳过{trackId}/{clipId}");
                }
            }
        }

        public void SetCanSkip()
        {
            canSkip = true;
        }
    }
}

