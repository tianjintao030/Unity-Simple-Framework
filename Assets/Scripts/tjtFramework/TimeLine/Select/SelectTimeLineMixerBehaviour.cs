using System.Collections;
using System.Collections.Generic;
using tjtFramework.Utiliy;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace tjtFramework.TimeLine
{
    public class SelectTimeLineMixerBehaviour : PlayableBehaviour
    {
        public SelectTimeLineTrack selectTrack;
        public string trackId;
        public List<TimelineClip> clips;
        public PlayableDirector playableDirector;
        public ISelectTimeLineHandler handler;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if(!Application.isPlaying) 
                return;

            int inputCount = playable.GetInputCount();
            if (inputCount == 0) 
                return;

            SelectTimeLineSystem.Instance.handler = handler;
            SelectTimeLineSystem.Instance.director = playableDirector;

            for (int i = 0; i < inputCount; i++)
            {
                var inputPlayable = (ScriptPlayable<SelectTimeLineBehaviour>)playable.GetInput(i);
                if (!inputPlayable.IsValid()) continue;

                var behaviour = inputPlayable.GetBehaviour();
                if (behaviour == null) continue;

                behaviour.trackId = trackId;

                if(!behaviour.trackList.IsNullOrEmpty())
                {
                    for(int j = 0;j < behaviour.trackList.Count;j++)
                    {
                        var trackItem = behaviour.trackList[j];
                        var trackAsset = SelectTimeLineSystem.Instance.GetTrackByRelativeIndex(
                            playableDirector, selectTrack, trackItem.index);
                        if(trackAsset != null)
                        {
                            SelectTimeLineSystem.Instance.RegisterTrack(
                                trackId, behaviour.clipId, trackItem.name, trackItem.index, trackAsset);
                        }
                    }
                }
            }
        }
    }
}

