using tjtFramework.Utiliy;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;
using System.Collections.Generic;

namespace tjtFramework.TimeLine
{
    [TrackColor(0.2f, 0.7f, 1f)]
    [TrackClipType(typeof(EventTimeLineClip))]
    [TrackBindingType(typeof(GameObject))]
    public class EventTimeLineTrack : TrackAsset
    {
        [Header("轨道id")]
        public string trackId;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<EventTimeLineMixerBehaviour>.Create(graph, inputCount);
            var mixerBehaviour = mixer.GetBehaviour();

            var director = go.GetComponent<PlayableDirector>();
            var boundObj = director.GetGenericBinding(this) as GameObject;
            var handler = boundObj?.GetComponent<IEventTimeLineHandler>();

            mixerBehaviour.handler = handler;
            mixerBehaviour.trackId = trackId;
            mixerBehaviour.clips = GetClips().ToList();

            return mixer;
        }
    }
}

