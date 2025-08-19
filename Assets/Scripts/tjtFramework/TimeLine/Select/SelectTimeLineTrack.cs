using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace tjtFramework.TimeLine
{
    [TrackColor(0.1f, 0.3f, 0.3f)]
    [TrackClipType(typeof(SelectTimeLineClip))]
    public class SelectTimeLineTrack : TrackAsset
    {
        public string trackId;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<SelectTimeLineMixerBehaviour>.Create(graph, inputCount);
            var mixerBehaviour = mixer.GetBehaviour();

            var director = go.GetComponent<PlayableDirector>();
            var handler = go.GetComponent<ISelectTimeLineHandler>();

            mixerBehaviour.selectTrack = this;
            mixerBehaviour.trackId = trackId;
            mixerBehaviour.clips = GetClips().ToList();
            mixerBehaviour.playableDirector = director;
            mixerBehaviour.handler = handler;
            
            return mixer;
        }
    }
}

