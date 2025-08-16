using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace tjtFramework.TimeLine
{
    public class EventTimeLineMixerBehaviour : PlayableBehaviour
    {
        public IEventTimeLineHandler handler;
        public string trackId;
        public List<TimelineClip> clips;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);

            int inputCount = playable.GetInputCount();
            if (inputCount == 0) return;

            for (int i = 0; i < inputCount; i++)
            {
                var inputPlayable = (ScriptPlayable<EventTimeLineBehaviour>)playable.GetInput(i);
                if (!inputPlayable.IsValid()) continue;

                var behaviour = inputPlayable.GetBehaviour();
                if (behaviour == null) continue;

                // 对应 clip 的时间
                var clip = clips[i];
                behaviour.SetContent(handler, trackId, 0, clip.duration);
            }
        }
    }
}

