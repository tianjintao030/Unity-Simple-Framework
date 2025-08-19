using UnityEngine;
using UnityEngine.Playables;

namespace tjtFramework.TimeLine
{
    public class EventTimeLineBehaviour : PlayableBehaviour
    {
        private string trackId;
        public string clipId;

        private bool hasEntered = false;
        private bool hasExited = false;

        private double startTime;
        private double endTime;

        private IEventTimeLineHandler handler;

        private bool isInited = false;

        /// <summary>
        /// 初始化事件Timeline行为
        /// </summary>
        public void SetContent(IEventTimeLineHandler hander, string trackId, double startClipTime, double endClipTime)
        {
            if(isInited)
            {
                return;
            }

            this.trackId = trackId;
            this.startTime = startClipTime;
            this.endTime = endClipTime;
            this.handler = hander;
            isInited = true;
        }

        public void TriggerEvent(double currentTime)
        {
            if (!isInited || handler == null)
            {
                return;
            }

            // 进入事件
            if (!hasEntered && currentTime >= startTime && currentTime <= endTime)
            {
                hasEntered = true;
                handler.OnTimelineEventEnter(trackId, clipId);
                Debug.Log($"Timeline EvnetClip Enter: {trackId}:{clipId}");
            }

            // 退出事件
            if (!hasExited && Mathf.Abs((float)(endTime - currentTime)) <= 0.01f)
            {
                hasExited = true;
                handler.OnTimelineEventExit(trackId, clipId);
                Debug.Log($"Timeline EvnetClip Exit: {trackId}:{clipId}");
            }
        }

        public void ResetState()
        {
            hasEntered = false;
            hasExited = false;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            base.ProcessFrame(playable, info, playerData);

            var currentTime = playable.GetTime();
            TriggerEvent(currentTime);
        }

        public override void OnGraphStart(Playable playable)
        {
            ResetState();
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            ResetState();
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            ResetState();
        }
    }
}

