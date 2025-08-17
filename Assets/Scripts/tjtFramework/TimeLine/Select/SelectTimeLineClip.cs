using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace tjtFramework.TimeLine
{
    [System.Serializable]
    public class TrackSelecterItem
    {
        public string name;
        public int index;
    }

    public class SelectTimeLineClip : PlayableAsset, ITimelineClipAsset
    {
        public string clipId;

        public List<TrackSelecterItem> trackList = new();

        public ClipCaps clipCaps => ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SelectTimeLineBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.clipId = clipId;
            behaviour.trackList = trackList;
            return playable;
        }
    }

}
