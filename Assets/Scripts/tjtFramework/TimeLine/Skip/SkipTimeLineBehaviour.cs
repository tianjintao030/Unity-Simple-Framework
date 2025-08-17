using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace tjtFramework.TimeLine
{
    public class SkipTimeLineBehaviour : PlayableBehaviour
    {
        public string trackId;
        public string clipId;

        private bool hasSkipped = false;
    }
}

