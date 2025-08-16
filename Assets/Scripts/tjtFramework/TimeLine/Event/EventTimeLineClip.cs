using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace tjtFramework.TimeLine
{
    [SerializeField]
    public class EventTimeLineClip : PlayableAsset, ITimelineClipAsset
    {
        [Header("片段id")]
        public string clipId;

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<EventTimeLineBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.clipId = clipId;
            return playable;
        }
    }
}

