using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.AssetFramework
{
    /// <summary>
    /// 多线程资源加载器
    /// </summary>
    public class AssetDownLoader
    {
        private string downLoadUrl;
        private string hotAssetsSavePath;
        private HotAssetModule hotAssetModule;
        private Queue<HotFileInfo> downLoadQueue = new();

        private Action<HotFileInfo> downLoadSuccessAction;
        private Action<HotFileInfo> downLoadFailureAction;
        private Action downLoadAllFinshAction;

        /// <summary>
        /// 最大下载线程数
        /// </summary>
        public int maxDownloadThreadCount = 3;

        /// <summary>
        /// 当前所有正在下载的线程
        /// </summary>
        private List<AssetDownLoadThread> allCurrentDownLoadingThreads = new();

        /// <summary>
        /// 资源下载事件队列
        /// </summary>
        private Queue<AssetDownLoadEventHandler> assetDownLoadEventHandlers = new();

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="hotAssetModule">热更模块</param>
        /// <param name="downLoadQueue">下载队列</param>
        /// <param name="downLoadUrl">下载资源地址</param>
        /// <param name="hotAssetsSavePath">资源保存路径</param>
        /// <param name="downLoadSuccess">下载文件成功回调</param>
        /// <param name="downLoadFailed">下载失败回调</param>
        /// <param name="allDownLoadFinsh">下载全部完成回调</param>
        public AssetDownLoader(HotAssetModule hotAssetModule, Queue<HotFileInfo> downLoadQueue, 
                                string downLoadUrl, string hotAssetsSavePath,
                                Action<HotFileInfo> downLoadSuccess, Action<HotFileInfo> downLoadFailed,
                                Action allDownLoadFinsh) 
        {
            this.hotAssetModule = hotAssetModule;
            this.downLoadUrl = downLoadUrl;
            this.downLoadQueue = downLoadQueue;
            this.hotAssetsSavePath = hotAssetsSavePath;
            downLoadSuccessAction = downLoadSuccess;
            downLoadFailureAction = downLoadFailed;
            downLoadAllFinshAction = allDownLoadFinsh;
        }


        public void StartThreadsDownLoadQueue()
        {
            // 多线程下载
            for(int i = 0; i < maxDownloadThreadCount; i++)
            {
                if(downLoadQueue.Count > 0)
                {
                    var hotFileInfo = downLoadQueue.Dequeue();
                    var thread = new AssetDownLoadThread(hotAssetModule, hotFileInfo, downLoadUrl, hotAssetsSavePath);
                    thread.StartDownLoad(DownLoadFileSuccess, DownLoadFileFailed);
                    allCurrentDownLoadingThreads.Add(thread);

                    Debug.Log($"开启一个下载线程下载{hotFileInfo.abName}");
                }
            }
        }

        /// <summary>
        /// 开始下载下一个AssetBundle
        /// </summary>
        private void StartDownLoadNextBundle()
        {
            if(allCurrentDownLoadingThreads.Count > maxDownloadThreadCount)
            {
                return;
            }

            if(downLoadQueue.Count > 0)
            {
                StartThreadsDownLoadQueue();

                if(allCurrentDownLoadingThreads.Count < maxDownloadThreadCount)
                {
                    // 计算当前待机的线程
                    var idleThreadCount = maxDownloadThreadCount - allCurrentDownLoadingThreads.Count;
                    for(int i = 0;i < idleThreadCount; i++)
                    {
                        if(downLoadQueue.Count > 0)
                        {
                            StartThreadsDownLoadQueue();
                        }
                    }
                }
            }
            else
            {
                // 没有了正在使用的下载线程，说明所有都下载完成了
                if(allCurrentDownLoadingThreads.Count == 0)
                {
                    downLoadAllFinshAction?.Invoke();
                }
            }
        }

        private void DownLoadFileSuccess(AssetDownLoadThread downLoadThread, HotFileInfo hotFileInfo)
        {
            if(allCurrentDownLoadingThreads.Contains(downLoadThread))
            {
                allCurrentDownLoadingThreads.Remove(downLoadThread);
            }

            EnqueueAssetDownLoadEventInMainThread(new AssetDownLoadEventHandler { hotFileInfo = hotFileInfo, downLoadAction = downLoadSuccessAction });
            StartDownLoadNextBundle();
        }

        private void DownLoadFileFailed(AssetDownLoadThread downLoadThread, HotFileInfo hotFileInfo)
        {
            if (allCurrentDownLoadingThreads.Contains(downLoadThread))
            {
                allCurrentDownLoadingThreads.Remove(downLoadThread);
            }

            EnqueueAssetDownLoadEventInMainThread(new AssetDownLoadEventHandler { hotFileInfo = hotFileInfo, downLoadAction = downLoadFailureAction });
            StartDownLoadNextBundle();
        }

        /// <summary>
        /// 在主线程中加入事件
        /// </summary>
        private void EnqueueAssetDownLoadEventInMainThread(AssetDownLoadEventHandler downLoadEventHandler)
        {
            // 因为文件在子线程中下载，所以事件回调在子线程中调用
            // 要把回调放回主线程调用
            // 使用lock防止子线程异步同时传入多个入队
            lock(assetDownLoadEventHandlers)
            {
                assetDownLoadEventHandlers.Enqueue(downLoadEventHandler);
            }
        }

        public void OnMainThreadUpdate()
        {
            if(assetDownLoadEventHandlers.Count > 0)
            {
                var eventHandler = assetDownLoadEventHandlers.Dequeue();
                eventHandler.downLoadAction?.Invoke(eventHandler.hotFileInfo);
            }
        }
    }

    public class AssetDownLoadEventHandler
    {
        public HotFileInfo hotFileInfo;
        public Action<HotFileInfo> downLoadAction;
    }
}

