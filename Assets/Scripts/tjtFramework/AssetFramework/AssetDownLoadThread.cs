using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace tjtFramework.AssetFramework
{
    /// <summary>
    /// 资源下载线程
    /// </summary>
    public class AssetDownLoadThread
    {
        private HotAssetModule hotAssetModule;
        private HotFileInfo hotFileInfo;
        private string downLoadUrl;
        private string fileSavePath;

        private Action<AssetDownLoadThread, HotFileInfo> onDownLoadSuccess;
        private Action<AssetDownLoadThread, HotFileInfo> onDownLoadFailed;

        private float downLoadSizeByte;

        private const int maxTryDownLoadCount = 3;
        private int currentTryDownLoadCount;

        public AssetDownLoadThread(HotAssetModule hotAssetModule, HotFileInfo hotFileInfo,
                                    string downLoadUrl, string fileSavePath)
        {
            this.hotAssetModule = hotAssetModule;
            this.hotFileInfo = hotFileInfo;
            this.downLoadUrl = $"{downLoadUrl}/{hotFileInfo.abName}";
            this.fileSavePath = $"{fileSavePath}/{hotFileInfo.abName}";

            currentTryDownLoadCount = 0;
        }


        public void StartDownLoad(Action<AssetDownLoadThread, HotFileInfo> downLoadSuccessCallback,
                                    Action<AssetDownLoadThread, HotFileInfo> downLoadFailedCallback)
        {
            onDownLoadSuccess = downLoadSuccessCallback;
            onDownLoadFailed = downLoadFailedCallback;

            // 子线程中下载
            Task.Run(() =>
            {
                try
                {
                    var request = WebRequest.Create(downLoadUrl) as HttpWebRequest;
                    request.Method = "GET";
                    
                    using (var response = request.GetResponse() as HttpWebResponse)
                    using (var fileStream = File.Create(fileSavePath))
                    using (var stream = response.GetResponseStream())
                    {
                        // 从字节流中读取字节，使用buffer字节数组作为读取容器
                        var buffer = new byte[512];
                        var size = 0;

                        while ((size = stream.Read(buffer,0 ,buffer.Length)) > 0)
                        {
                            fileStream.Write(buffer, 0, size);

                            // 1mb=1024kb,1kb=1024byte
                            downLoadSizeByte += size;
                            hotAssetModule.currentDownLoadAssetSizeM += (size / 1024) / 1024;
                        }

                        onDownLoadSuccess?.Invoke(this, hotFileInfo);

                        if (downLoadSizeByte <= 0)
                        {
                            Debug.Log($"下载了空文件{downLoadUrl},将其删除");
                            if (File.Exists(fileSavePath))
                            {
                                File.Delete(fileSavePath);
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError($"下载{hotFileInfo.abName}失败，{e}");

                    if(currentTryDownLoadCount >= maxTryDownLoadCount)
                    {
                        onDownLoadFailed?.Invoke(this, hotFileInfo);
                    }
                    else
                    {
                        Debug.LogError($"尝试重新下载{hotFileInfo.abName}...");
                        currentTryDownLoadCount++;
                        StartDownLoad(onDownLoadSuccess, onDownLoadFailed);
                    }
                }
            });
        }
    }
}

