using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tjtFramework.Singleton;
using UnityEngine.Timeline;
using tjtFramework.Utiliy;
using UnityEngine.Playables;
using System.Linq;
using tjtFramework.PublicMono;
using static PlasticPipe.Server.MonitorStats;

namespace tjtFramework.TimeLine
{
    /// <summary>
    /// 可以选择的轨道数据
    /// </summary>
    public class SelectTimeLineControlDataItem
    {
        public string name;
        public int index;
        public TrackAsset trackAsset;
    }

    /// <summary>
    /// 选择轨道的控制项
    /// </summary>
    public class SelectTimeLineControlItem
    {
        public string selectTrackId;
        public string selectClipId;
        public List<SelectTimeLineControlDataItem> controlTracklist = new();
    }

    public class SelectTimeLineSystem : MonoSingleton<SelectTimeLineSystem>
    {
        public ISelectTimeLineHandler handler;
        public PlayableDirector director;

        private List<SelectTimeLineControlItem> selectTimeLineControlItems = new();

        private bool needRebuild;

        public void OnClipEnter(string trackId, string clipId)
        {
            if (handler == null)
            {
                return;
            }

            string chosen = handler.OnSelectTimeline(trackId, clipId);
            if(string.IsNullOrEmpty(chosen))
            {
                return;
            }

            Debug.Log($"[SelectTimeLineSystem] trackId={trackId}, clipId={clipId}, chosen={chosen}");

            var controlItem = GetControlItemInList(trackId, clipId);
            foreach(var dataItem in controlItem.controlTracklist)
            {
                if(dataItem.name == chosen)
                {
                    //dataItem.trackAsset.muted = false;
                }
                else
                {
                    //dataItem.trackAsset.muted = true;
                    SetTrackMute(dataItem.trackAsset);
                }
            }
        }

        private IEnumerator DelayRebuildGraph()
        {
            yield return null; // 等待一帧，确保不在 Evaluate 内部
            double t0 = director.time;
            director.RebuildGraph();
            director.time = t0;
            director.Evaluate();
            director.Play();
        }

        public void SetTrackMute(TrackAsset track)
        {
            if (director == null || track == null) return;

            var timeline = director.playableAsset as TimelineAsset;
            if (timeline == null) return;

            var graph = director.playableGraph;
            if (!graph.IsValid()) return;

            var root = graph.GetRootPlayable(0);

            int trackIndex = 0;
            foreach (var output in timeline.outputs)
            {
                if (output.sourceObject == track)
                {
                    // 找到对应 root 的输入
                    if (trackIndex < root.GetInputCount())
                    {
                        var input = root.GetInput(trackIndex);

                        root.DisconnectInput(trackIndex);
                    }
                    break;
                }
                trackIndex++;
            }
        }


        public void RegisterTrack(string trackId,string clipId,string name,int index,TrackAsset trackAsset)
        {
            var controlItem = GetControlItemInList(trackId, clipId);
            if (controlItem != null)
            {
                controlItem.controlTracklist.Add(new SelectTimeLineControlDataItem
                {
                    name = name,
                    index = index,
                    trackAsset = trackAsset
                });
            }
            else
            {
                var dataItem = new SelectTimeLineControlDataItem
                {
                    name = name,
                    index = index,
                    trackAsset = trackAsset
                };
                var dataItemList = new List<SelectTimeLineControlDataItem>();
                dataItemList.Add(dataItem);
                var newControlItem = new SelectTimeLineControlItem
                {
                    selectTrackId = trackId,
                    selectClipId = clipId,
                    controlTracklist = dataItemList
                };
                selectTimeLineControlItems.Add(newControlItem);
            }
        }

        /// <summary>
        /// 根据相对索引获取轨道
        /// </summary>
        /// <param name="playableDirector"></param>
        /// <param name="selectTrack"></param>
        /// <param name="relativeIndex"></param>
        /// <returns></returns>
        public TrackAsset GetTrackByRelativeIndex(PlayableDirector playableDirector, SelectTimeLineTrack selectTrack, int relativeIndex)
        {
            var allTracks = playableDirector.playableAsset as TimelineAsset;
            if (allTracks == null) 
                return null;

            var tracks = allTracks.GetOutputTracks().ToList();

            // 找到当前 SelectTrack 在 tracks 中的位置
            var currentIndex = tracks.IndexOf(selectTrack);
            if(currentIndex < 0) 
                return null;

            var targetIndex = currentIndex + relativeIndex;
            if(targetIndex < 0 || targetIndex >= tracks.Count) 
                return null;

            return tracks[targetIndex];
        }

        private SelectTimeLineControlItem GetControlItemInList(string trackId, string clipId)
        {
            if(selectTimeLineControlItems.IsNullOrEmpty())
            {
                return null;
            }

            foreach(var item in  selectTimeLineControlItems)
            {
                if(item.selectTrackId == trackId && item.selectClipId == clipId)
                {
                    return item;
                }
            }

            return null;
        }

        public void Release()
        {
            if(!selectTimeLineControlItems.IsNullOrEmpty())
            {
                foreach(var controlItem in selectTimeLineControlItems)
                {
                    var dataItemList = controlItem.controlTracklist;
                    foreach(var dataItem in dataItemList)
                    {
                        dataItem.trackAsset.muted = false;
                    }
                }
            }
            selectTimeLineControlItems.Clear();
            handler = null;
        }

        public override void Destroy()
        {
            base.Destroy();
            Release();
        }
    }
}

