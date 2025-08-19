using UnityEngine.Playables;

namespace tjtFramework.TimeLine
{
    public interface ISkipTimeLineHandler
    {
        public bool ShouldSkip(string trackId, string clipId);
    }
}

