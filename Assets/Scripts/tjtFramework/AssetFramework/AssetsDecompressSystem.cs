using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using tjtFramework.GameSystem;
using tjtFramework.PublicMono;
using tjtFramework.Utiliy;
using UnityEngine;
using UnityEngine.Networking;

namespace tjtFramework.AssetFramework
{
    public class AssetsDecompressSystem : GameSystemBase<AssetsDecompressSystem>
    {
        /// <summary>
        /// 需要解压的资源大小
        /// </summary>
        private float totalSizeM;
        /// <summary>
        /// 已经解压的大小
        /// </summary>
        private float alreadyDecompressSizeM;
        /// <summary>
        /// 是否已开始解压
        /// </summary>
        private bool isStartDecompress;
        public bool IsStartDecompress => isStartDecompress;

        /// <summary>
        /// 需要解压的文件列表
        /// </summary>
        private List<string> needDecompressFileList = new();

        /// <summary>
        /// 资源内嵌路径
        /// </summary>
        private string streamingAssetsBundlePath;
        /// <summary>
        /// 资源解压路径
        /// </summary>
        private string assetsDecompressPath;

        public override bool needUpdate => false;

        public override void OnInit()
        {
            base.OnInit();
            alreadyDecompressSizeM = 0f;
            totalSizeM = 0f;
            MarkReady();
        }

        /// <summary>
        /// 开始解压内嵌文件
        /// </summary>
        public void StartDecompressBuiltinFile(BundleModule bundleModule, Action decompressFinshCallback)
        {
            if (CheckNeedDecompressFile(bundleModule))
            {
                MonoManager.Instance.StartCoroutine(UnpackToPersistentDataPath(bundleModule, decompressFinshCallback));
            }
            else
            {
                isStartDecompress = false;
                Debug.Log($"不需要解压{bundleModule}资源");
                decompressFinshCallback?.Invoke();
            }
        }

        public float GetDecompressProgress()
        {
            return Mathf.Clamp01(alreadyDecompressSizeM / totalSizeM);
        }

        /// <summary>
        /// 计算需要解压的文件
        /// </summary>
        private bool CheckNeedDecompressFile(BundleModule bundleModule)
        {
            streamingAssetsBundlePath = BuildBundleSetting.Instance.GetBuiltinAssetsPath(bundleModule);
            assetsDecompressPath = BuildBundleSetting.Instance.GetDecompressAssetsPath(bundleModule);
            needDecompressFileList.Clear();

            alreadyDecompressSizeM = 0f;
            totalSizeM = 0f;

            if(!Directory.Exists(assetsDecompressPath))
            {
                Directory.CreateDirectory(assetsDecompressPath);
            }

            var textAsset = Resources.Load<TextAsset>($"BuiltinAssetsInfos/{bundleModule}info");
            if(textAsset != null)
            {
                var builtinAssetsInfoList = JsonConvert.DeserializeObject<List<BuildInAssetBundleInfo>>(textAsset.text);
                foreach(var info in builtinAssetsInfoList)
                {
                    var localFilePath = $"{assetsDecompressPath}/{info.fileName}";
                    if (localFilePath.EndsWith(".meta"))
                    {
                        continue;
                    }

                    if(!File.Exists(localFilePath) || MD5.GetMd5FromFile(localFilePath) != info.md5)
                    {
                        needDecompressFileList.Add(info.fileName);
                        totalSizeM += info.size / 1024f;
                    }
                }
            }

            return needDecompressFileList.Count > 0;
        }

        /// <summary>
        /// 解压文件到持久化目录
        /// </summary>
        private IEnumerator UnpackToPersistentDataPath(BundleModule bundleModule, Action callback)
        {
            isStartDecompress = true;

            if(!needDecompressFileList.IsNullOrEmpty())
            {
                foreach(var fileName in needDecompressFileList)
                {
                    string filePath = "";
#if UNITY_EDITOR_OSX || UNITY_IOS
                    filePath = $"file://{streamingAssetsBundlePath}/{fileName}";
#else
                    filePath = $"{streamingAssetsBundlePath}/{fileName}";
#endif
                    var unityWebRequest = UnityWebRequest.Get(filePath);
                    unityWebRequest.timeout = 30;
                    yield return unityWebRequest.SendWebRequest();

                    if(unityWebRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Unpack Error:{unityWebRequest.error}");
                    }
                    else
                    {
                        var bytes = unityWebRequest.downloadHandler.data;
                        FileUtility.WriteFile($"{assetsDecompressPath}/{fileName}", bytes);
                        Debug.Log($"Unpack Finsh:{assetsDecompressPath}/{fileName}");

                        alreadyDecompressSizeM += (bytes.Length / 1024f) / 1024f;
                    }

                    unityWebRequest.Dispose();
                }
            }

            callback?.Invoke();
            isStartDecompress = false;
        }
    }
}

