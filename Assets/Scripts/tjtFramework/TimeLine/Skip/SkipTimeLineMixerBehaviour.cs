using System.Collections;
using System.Collections.Generic;
using tjtFramework.Utiliy;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace tjtFramework.TimeLine
{
    public class SkipTimeLineMixerBehaviour : PlayableBehaviour
    {
        public string trackId;
        public PlayableDirector director;
        public ISkipTimeLineHandler handler;
        public List<TimelineClip> clips = new();

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if(!Application.isPlaying)
            {
                return;
            }

            int inputCount = playable.GetInputCount();
            if (inputCount == 0) return;

            for (int i = 0; i < inputCount; i++)
            {
                var inputPlayable = (ScriptPlayable<SkipTimeLineBehaviour>)playable.GetInput(i);
                if (!inputPlayable.IsValid()) continue;

                var behaviour = inputPlayable.GetBehaviour();
                if (behaviour == null) continue;

                behaviour.trackId = trackId;
                behaviour.director = director;
                behaviour.startTime = clips[i].start;
                behaviour.endTime = clips[i].end;

                if(handler.ShouldSkip(trackId, behaviour.clipId))
                {
                    behaviour.SetCanSkip();
                }
            }
        }
    }
}

