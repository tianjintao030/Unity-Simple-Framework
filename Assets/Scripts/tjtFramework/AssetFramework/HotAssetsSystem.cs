using System;
using System.Collections;
using System.Collections.Generic;
using tjtFramework.GameSystem;
using UnityEngine;

namespace tjtFramework.AssetFramework
{
    public class HotAssetsSystem : GameSystemBase<HotAssetsSystem>
    {
        private string gameVersion;
        private int maxDownloadThreadCount;

        /// <summary>
        /// 所有热更资源模块们
        /// </summary>
        private Dictionary<BundleModule, HotAssetModule> allHotAssetModuleDic = new();
        public int hotAssetCount { get { return allHotAssetModuleDic.Count; } }
        /// <summary>
        /// 正在下载的热更资源模块们
        /// </summary>
        private Dictionary<BundleModule, HotAssetModule> downloadingHotAssetModuleDic = new();
        /// <summary>
        /// 等待下载的队列
        /// </summary>
        private Queue<WaitDownloadModule> waitDownloadModules = new();

        public override bool needUpdate => false;

        public override void OnInit()
        {
            base.OnInit();
            MarkReady();
        }

        #region 对外接口
        public void CheckAssetsVersion(BundleModule bundleModule, Action<bool, float> callback)
        {
            var hotAssetModule = GetOrNewBundleModule(bundleModule);
            hotAssetModule.CheckResourceVersion(callback);
        }

        public HotAssetModule GetHotBundleModule(BundleModule bundleModule)
        {
            if (allHotAssetModuleDic.ContainsKey(bundleModule))
            {
                return allHotAssetModuleDic[bundleModule];
            }
            return null;
        }

        public void HotAsset(BundleModule bundleModule, Action<BundleModule> startHotCallback, Action<BundleModule> finshHotCallback, Action<BundleModule> waitHotCallback, bool checkVersion = true)
        {
            if(BuildBundleSetting.Instance.bundleHotEnum == BundleHotEnum.No_Hot)
            {
                finshHotCallback?.Invoke(bundleModule);
                return;
            }

            gameVersion = GameAppInfoSetting.Instance.GameVersion;
            maxDownloadThreadCount = BuildBundleSetting.Instance.maxDownLoadThreadCount;
            var hotAssetModule = GetOrNewBundleModule(bundleModule);

            // 判断是否有闲置的下载线程
            if(downloadingHotAssetModuleDic.Count < maxDownloadThreadCount)
            {
                if (!downloadingHotAssetModuleDic.ContainsKey(bundleModule))
                {
                    downloadingHotAssetModuleDic.Add(bundleModule , hotAssetModule);
                }

                hotAssetModule.StarHotAssets(gameVersion, 
                () =>
                {
                    MultithreadedBalancing();
                    startHotCallback?.Invoke(bundleModule);
                }, 
                (module) => OnDownLoadModuleFinsh(module), checkVersion);
            }
            else
            {
                waitHotCallback?.Invoke(bundleModule);

                waitDownloadModules.Enqueue(new WaitDownloadModule
                {
                    bundleModule = bundleModule,
                    startHotCallback = startHotCallback,
                    finshHotCallback = finshHotCallback
                });
            }
        }

        public void OnMainThreadUpdate()
        {
            if(downloadingHotAssetModuleDic.Count > 0)
            {
                foreach(var hotAssetModule in downloadingHotAssetModuleDic.Values)
                {
                    hotAssetModule.UpdateDownloader();
                }
            }
        }
        #endregion

        /// <summary>
        /// 获取或创建后获取HotAssetModule
        /// </summary>
        private HotAssetModule GetOrNewBundleModule(BundleModule bundleModule)
        {
            if (allHotAssetModuleDic.ContainsKey(bundleModule))
            {
                return allHotAssetModuleDic[bundleModule];
            }
            else
            {
                var hotAssetModule = new HotAssetModule(bundleModule, null);
                allHotAssetModuleDic.Add(bundleModule, hotAssetModule);
                return allHotAssetModuleDic[bundleModule];
            }
        }

        /// <summary>
        /// 下载模块完成回调
        /// </summary>
        private void OnDownLoadModuleFinsh(BundleModule bundleModule)
        {
            // 将下载完成的模块从正在下载字典中移除
            if (allHotAssetModuleDic.TryGetValue(bundleModule, out var hotAssetModule))
            {
                if (downloadingHotAssetModuleDic.ContainsKey(bundleModule))
                {
                    downloadingHotAssetModuleDic.Remove(bundleModule);
                }
            }

            // 查看是否有在等待下载线程的模块
            if (waitDownloadModules.Count > 0)
            {
                var waitDownloadModule = waitDownloadModules.Dequeue();
                HotAsset(bundleModule, waitDownloadModule.startHotCallback, waitDownloadModule.finshHotCallback, null);
            }
            else
            {
                // 在无等待下载线程模块且已经有空闲下来的线程时，需要处理负载均衡
                // 将空闲线程分配给正在下载的模块，以提高下载速度
                MultithreadedBalancing();
            }
        }

        /// <summary>
        /// 多线程负载均衡
        /// </summary>
        private void MultithreadedBalancing()
        {
            var downloadingModuleCount = downloadingHotAssetModuleDic.Count;
            var threadCountFloat = (float)maxDownloadThreadCount / downloadingModuleCount;

            var mainThreadCount = 0;
            var balancedThreadCount = Mathf.FloorToInt(threadCountFloat);

            if (Mathf.FloorToInt(threadCountFloat) < threadCountFloat)
            {
                mainThreadCount = Mathf.CeilToInt(threadCountFloat);
                balancedThreadCount = Mathf .FloorToInt(threadCountFloat);
            }

            var i = 0;
            foreach(var hotAsstModule in downloadingHotAssetModuleDic.Values)
            {
                if(i == 0 && mainThreadCount != 0)
                {
                    hotAsstModule.SetDownloadThreadCount(mainThreadCount);
                }
                else
                {
                    hotAsstModule.SetDownloadThreadCount(balancedThreadCount);
                }
                i++;
            }
        }
    }


    public class WaitDownloadModule
    {
        public BundleModule bundleModule;
        public Action<BundleModule> startHotCallback;
        public Action<BundleModule> finshHotCallback;
        public Action<BundleModule, float> progressCallback;
    }
}

