using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace tjtFramework.TimeLine
{
    public class SkipTimeLineClip : PlayableAsset, ITimelineClipAsset
    {
        public string clipId;
        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SkipTimeLineBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.clipId = clipId;
            return playable;
        }
    }
}

