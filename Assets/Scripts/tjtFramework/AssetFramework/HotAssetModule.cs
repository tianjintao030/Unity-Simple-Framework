using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using tjtFramework.Utiliy;
using UnityEngine;
using UnityEngine.Networking;

namespace tjtFramework.AssetFramework
{
    /// <summary>
    /// 热更资源模块
    /// </summary>
    public class HotAssetModule
    {
        /// <summary>
        /// 当前下载的资源模块类型
        /// </summary>
        private BundleModule currentBundleModule;

        /// <summary>
        /// 服务器热更资源清单路径
        /// </summary>
        private string serverHotAssetManifestPath;
        /// <summary>
        /// 本地热更资源清单路径
        /// </summary>
        private string localHotAssetManifestPath;

        private HotAssetManifest serverHotAssetManifest;
        private HotAssetManifest localHotAssetManifest;

        /// <summary>
        /// 热更资源保存路径
        /// </summary>
        private string hotAssetsSavePath { 
            get
            {
                return BuildBundleSetting.Instance.GetHotAssetsPath(currentBundleModule);
            } 
        }

        /// <summary>
        /// 需要下载的资源列表
        /// </summary>
        private List<HotFileInfo> needDownLoadAssetList = new();

        /// <summary>
        /// 需要下载的资源大小(M)
        /// </summary>
        public float maxNeedDownLoadAssetSizeM;
        /// <summary>
        /// 当前已下载的资源大小(M)
        /// </summary>
        public float currentDownLoadAssetSizeM;

        /// <summary>
        /// 所有热更的资源
        /// </summary>
        private List<HotFileInfo> allHotAssetList = new();

        /// <summary>
        /// 多线程资源下载器
        /// </summary>
        private AssetDownLoader assetDownLoader;

        /// <summary>
        /// 下载AssetBundle配置文件回调
        /// </summary>
        private Action<string> onDownloadAssetBundleConfig;
        /// <summary>
        /// 下载AssetBundle文件回调
        /// </summary>
        private Action<string> onDownloadAssetBundle;
        /// <summary>
        /// 模块AssetBundle全部下载完成回调
        /// </summary>
        private Action<BundleModule> onDownloadFinsh;

        private MonoBehaviour mono;

        public HotAssetModule(BundleModule bundleModule, MonoBehaviour mono)
        {
            currentBundleModule = bundleModule;
            this.mono = mono;
        }

        /// <summary>
        /// 开始热更资源
        /// </summary>
        public void StarHotAssets(string gameVersion,
                                  Action startDownloadCallback,
                                  Action<BundleModule> finshCallback,
                                  bool needCheckResourceVersion = true)
        {
            onDownloadFinsh += finshCallback;

            if(needCheckResourceVersion)
            {
                CheckResourceVersion((isHot, size) =>
                {
                    if (isHot)
                    {
                        Debug.Log("需要热更");
                        StartDownLoadHotAssets(startDownloadCallback);
                    }
                    else
                    {
                        Debug.Log("无需热更");
                        onDownloadFinsh?.Invoke(currentBundleModule);
                    }
                });
            }
        }

        /// <summary>
        /// 设置下载线程个数
        /// </summary>
        public void SetDownloadThreadCount(int count)
        {
            if(assetDownLoader != null)
            {
                assetDownLoader.maxDownloadThreadCount = count;
            }
        }

        /// <summary>
        /// 下载热更资源
        /// </summary>
        private void StartDownLoadHotAssets(Action startDownLoadCallback)
        {
            List<HotFileInfo> downLoadList = new();
            for(int i = 0; i < needDownLoadAssetList.Count; i++)
            {
                var hotFile = needDownLoadAssetList[i];
                if(hotFile.abName.Contains("bundleconfig"))
                {
                    // 优先下载配置文件
                    downLoadList.Insert(0, hotFile);
                }
                else
                {
                    downLoadList.Add(hotFile);
                }
            }
            needDownLoadAssetList = downLoadList;

            // 按下载顺序存入下载队列
            Queue<HotFileInfo> downLoadQueue = new();
            for(int i = 0;i < needDownLoadAssetList.Count;i++)
            {
                downLoadQueue.Enqueue(needDownLoadAssetList[i]);
            }

            var queue = new StringBuilder();
            queue.Append("下载队列:");
            foreach(var asset in needDownLoadAssetList)
            {
                queue.Append(asset.abName);
                queue.Append(',');
            }
            Debug.Log(queue.ToString());

            // 通过多线程资源下载器下载
            assetDownLoader = new AssetDownLoader(this, downLoadQueue, serverHotAssetManifest.downLoadUrl, hotAssetsSavePath, 
                                                    OnDownLoadAssetSuccess, OnDownLoadAssetFailed, OnDownLoadAllFinsh);
            startDownLoadCallback?.Invoke();
            assetDownLoader.StartThreadsDownLoadQueue();
        }

        /// <summary>
        /// 检查资源版本
        /// </summary>
        /// <param name="checkVersionCallback">bool:是否要更新,float:资源包大小</param>
        public void CheckResourceVersion(Action<bool, float> checkVersionCallback)
        {
            serverHotAssetManifestPath = BuildBundleSetting.Instance.GetServerHotAssetsManifestPath(currentBundleModule);
            localHotAssetManifestPath = BuildBundleSetting.Instance.GetLocalHotAssetsManifestPath(currentBundleModule);

            if(mono == null)
            {
                return;
            }
            mono.StartCoroutine(DownloadHotAssetManifest(() =>
            {
                if(IsModuleNeedHot())
                {
                    var serverHotPatch = serverHotAssetManifest.patchList[serverHotAssetManifest.patchList.Count - 1];
                    if(CalculateNeedHotAssetList(serverHotPatch))
                    {
                        checkVersionCallback?.Invoke(true, maxNeedDownLoadAssetSizeM);
                    }
                    else
                    {
                        checkVersionCallback?.Invoke(false, 0);
                    }
                }
                else
                {
                    checkVersionCallback?.Invoke(false, 0);
                }
            }));
        }

        /// <summary>
        /// 热更资源是否存在
        /// </summary>
        public bool IsHotAssetExist(string bundleName)
        {
            if (!allHotAssetList.IsNullOrEmpty())
            {
                foreach (var item in allHotAssetList)
                {
                    if(item.abName == bundleName)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 下载资源热更清单
        /// </summary>
        private IEnumerator DownloadHotAssetManifest(Action downLoadManifestCallback)
        {
            var url = $"{BuildBundleSetting.Instance.downLoadUrl}/HotAsset/{BuildBundleSetting.Instance.buildTarget}/" +
                $"{GameAppInfoSetting.Instance.GameVersion}/{currentBundleModule}HotAssetManifest.json";
            using(var webRequest = UnityWebRequest.Get(url))
            {
                webRequest.timeout = 30;
                Debug.Log($"请求{url}下载");
                yield return webRequest.SendWebRequest();

                if(webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"请求{url}失败:{webRequest.error}");
                }
                else
                {
                    try
                    {
                        Debug.Log($"请求{url}成功,模块{currentBundleModule},txt:{webRequest.downloadHandler.text}");
                        FileUtility.WriteFile(serverHotAssetManifestPath, webRequest.downloadHandler.data);

                        serverHotAssetManifest = JsonConvert.DeserializeObject<HotAssetManifest>(webRequest.downloadHandler.text);
                    }
                    catch(Exception e)
                    {
                        Debug.LogError($"请求{url}下载服务器清单失败，错误:{e}");
                    }
                }

                downLoadManifestCallback?.Invoke();
            }
        }

        /// <summary>
        /// 检测该模块的资源是否需要热更
        /// </summary>
        private bool IsModuleNeedHot()
        {
            if(serverHotAssetManifest == null)
            {
                return false;
            }

            if(!File.Exists(localHotAssetManifestPath))
            {
                return true;
            }

            var localManifest = JsonConvert.DeserializeObject<HotAssetManifest>(File.ReadAllText(localHotAssetManifestPath));
            if(localManifest.patchList.IsNullOrEmpty() && !serverHotAssetManifest.patchList.IsNullOrEmpty())
            {
                return true;
            }

            var localLastHotPatch = localManifest.patchList[localManifest.patchList.Count - 1];
            var serverLastHotPatch = serverHotAssetManifest.patchList[serverHotAssetManifest.patchList.Count - 1];
            if(localLastHotPatch != null && serverLastHotPatch != null)
            {
                if(localLastHotPatch.resourceVersion != serverLastHotPatch.resourceVersion)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if(serverLastHotPatch != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 计算需要下载的热更资源列表
        /// </summary>
        private bool CalculateNeedHotAssetList(HotAssetPatch serverAssetPath)
        {
            if (!Directory.Exists(hotAssetsSavePath))
            {
                Directory.CreateDirectory(hotAssetsSavePath);
            }

            if(!serverAssetPath.hotFileList.IsNullOrEmpty())
            {
                needDownLoadAssetList.Clear();
                maxNeedDownLoadAssetSizeM = 0;
                foreach(var item in serverAssetPath.hotFileList)
                {
                    // 获取该文件在本地的路径
                    var localFilePath = $"{hotAssetsSavePath}/{item.abName}";

                    allHotAssetList.Add(item);

                    // 若本地不存在该文件或者本地文件和服务器文件md5不一致，则需热更
                    if(!File.Exists(localFilePath) || MD5.GetMd5FromFile(localFilePath) != item.md5)
                    {
                        needDownLoadAssetList.Add(item);
                        maxNeedDownLoadAssetSizeM += (item.size / 1024f);
                    }
                }
            }

            Debug.Log($"需要下载{maxNeedDownLoadAssetSizeM}M");
            return needDownLoadAssetList.Count > 0;
        }


        public void OnDownLoadAssetSuccess(HotFileInfo hotFileInfo)
        {
            if (hotFileInfo.abName.Contains("bundleconfig"))
            {
                onDownloadAssetBundleConfig?.Invoke(hotFileInfo.abName.Replace(".ab",""));
                Debug.Log($"下载配置文件{hotFileInfo.abName}成功");
            }
            else
            {
                onDownloadAssetBundle?.Invoke(hotFileInfo.abName.Replace(".ab", ""));
                Debug.Log($"下载文件{hotFileInfo.abName}成功");
            }
        }

        public void OnDownLoadAssetFailed(HotFileInfo hotFileInfo)
        {
            Debug.Log($"下载文件{hotFileInfo.abName}失败");
        }

        public void OnDownLoadAllFinsh()
        {
            var localDir = Path.GetDirectoryName(localHotAssetManifestPath);
            if (!Directory.Exists(localDir))
            {
                Directory.CreateDirectory(localDir);
            }

            if (File.Exists(localHotAssetManifestPath))
            {
                File.Delete(localHotAssetManifestPath);
            }

            // 把服务端热更清单文件放到本地位置
            File.Copy(serverHotAssetManifestPath, localHotAssetManifestPath, true);

            onDownloadFinsh?.Invoke(currentBundleModule);
        }

        public void UpdateDownloader()
        {
            if(assetDownLoader != null)
            {
                assetDownLoader.OnMainThreadUpdate();
            }
        }
    }
}

