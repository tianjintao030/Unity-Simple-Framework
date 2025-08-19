using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace tjtFramework.TimeLine
{
    [TrackColor(0.8f, 0.4f, 0.3f)]
    [TrackClipType(typeof(SkipTimeLineClip))]
    public class SkipTimeLineTrack : TrackAsset
    {
        public string trackId;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<SkipTimeLineMixerBehaviour>.Create(graph, inputCount);
            var mixerBehaviour = mixer.GetBehaviour();

            var director = go.GetComponent<PlayableDirector>();
            var handler = go.GetComponent<ISkipTimeLineHandler>();

            mixerBehaviour.trackId = trackId;
            mixerBehaviour.director = director;
            mixerBehaviour.handler = handler;
            mixerBehaviour.clips = GetClips().ToList();

            return mixer;
        }
    }
}

