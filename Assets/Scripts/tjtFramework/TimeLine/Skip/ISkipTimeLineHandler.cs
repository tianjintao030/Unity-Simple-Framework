using UnityEngine.Playables;

namespace tjtFramework.TimeLine
{
    public interface ISkipTimeLineHandler
    {
        public void OnSkipTimeline(string trackId, string clipId);
    }
}

