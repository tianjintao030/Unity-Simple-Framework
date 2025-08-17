using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.TimeLine
{
    public class SelectTimeLineSample : MonoBehaviour, ISelectTimeLineHandler
    {
        public string trackId_1;
        public string clipId_1;
        public bool isCube;

        public string OnSelectTimeline(string trackId, string clipId)
        {
            if(trackId == trackId_1 &&  clipId == clipId_1)
            {
                if(isCube)
                {
                    return "Cube";
                }
                else
                {
                    return "Sphere";
                }
            }

            return "";
        }
    }
}

