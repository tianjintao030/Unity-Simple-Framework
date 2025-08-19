using Codice.Client.BaseCommands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace tjtFramework.TimeLine
{
    public class SelectTimeLineBehaviour : PlayableBehaviour
    {
        public string trackId;
        public string clipId;
        public List<TrackSelecterItem> trackList = new();
        private bool hasCalled = false;

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if(!Application.isPlaying)
            {
                return;
            }

            if(string.IsNullOrEmpty(trackId) || string.IsNullOrEmpty(clipId))
            {
                return;
            }

            if(!hasCalled)
            {
                hasCalled = true;
                SelectTimeLineSystem.Instance?.OnClipEnter(trackId, clipId);
            }
        }
    }
}

