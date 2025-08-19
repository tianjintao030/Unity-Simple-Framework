using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.TimeLine
{
    public class SkipTimeLineSample : MonoBehaviour, ISkipTimeLineHandler
    {
        public bool ShouldSkip(string trackId, string clipId)
        {
            if(trackId == "track1" && clipId == "clip1")
            {
                return true;
            }

            return false;
        }
    }
}

