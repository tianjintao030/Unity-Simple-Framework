using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using tjtFramework.GameSystem;
using tjtFramework.Pool;
using tjtFramework.Utiliy;
using UnityEngine;

namespace tjtFramework.AssetFramework
{
    public class AssetBundleLoadSystem : GameSystemBase<AssetBundleLoadSystem>
    {
        public override bool needUpdate => false;

        // key:crc
        private Dictionary<uint, BundleItem> allAssetBundleItems = new();

        // key:asset bundle name
        private Dictionary<string, AssetBundleCache> loadedAssetBundleCaches = new();

        private ClassObjectPool<AssetBundleCache> assetBundleCachePool = new ClassObjectPool<AssetBundleCache>(200);

        public override void OnInit()
        {
            base.OnInit();
            MarkReady();
        }

        /// <summary>
        /// 通过资源路径crc加载带有该资源的AssetBundle
        /// </summary>
        public BundleItem LoadAssetBundle(uint crc)
        {
            if(allAssetBundleItems.TryGetValue(crc, out var bundleItem))
            {
                // 资源的AssetBundle没加载到内存，需要加载AssetBundle
                if(bundleItem.assetBundle == null)
                {
                    bundleItem.assetBundle = LoadAssetBundleReally(bundleItem.bundleName, bundleItem.bundleModule);

                    // 加载其依赖的AssetBundle
                    if (!bundleItem.bundleDependencies.IsNullOrEmpty())
                    {
                        foreach(var dependencyBundleName in bundleItem.bundleDependencies)
                        {
                            if(dependencyBundleName != bundleItem.bundleName)
                            {
                                LoadAssetBundleReally(dependencyBundleName, bundleItem.bundleModule);
                            }
                        }
                    }
                }

                return bundleItem;
            }
            else
            {
                Debug.LogError($"AssetBundle配置文件中未记录crc值为{crc}的资源，无法加载资源");
                return null;
            }
        }

        /// <summary>
        /// 加载AssetBundle配置
        /// </summary>
        public void LoadAssetBundleConfig(BundleModule bundleModule)
        {
            try
            {
                if (GeneratorBundleConfigPath(bundleModule, out var bundleConfigPath, out var bundleConfigName))
                {
                    // 加载配置文件
                    var bundleConfigAsstBundle = AssetBundle.LoadFromFile(bundleConfigPath);
                    var bundleConfigJson = bundleConfigAsstBundle.LoadAsset<TextAsset>(bundleConfigName).text;
                    var bundleConfig = JsonConvert.DeserializeObject<AssetBundleConfig>(bundleConfigJson);

                    if (!bundleConfig.assetBundleInfoList.IsNullOrEmpty())
                    {
                        foreach (var info in bundleConfig.assetBundleInfoList)
                        {
                            if (!allAssetBundleItems.ContainsKey(info.crc))
                            {
                                var item = new BundleItem();
                                item.path = info.path;
                                item.crc = info.crc;
                                item.bundleName = info.AssetBundleName;
                                item.assetName = info.AssetName;
                                item.bundleDependencies = info.dependencies;
                                item.bundleModule = bundleModule;
                                allAssetBundleItems.Add(item.crc, item);
                            }
                            else
                            {
                                Debug.Log($"AssetBundle--{info.AssetName}已存在，可能重复打包");
                            }
                        }

                        // 释放配置文件AB包
                        bundleConfigAsstBundle.Unload(true);
                    }
                }
                else
                {
                    Debug.LogError($"未找到AssetBundle配置文件的AB包:{bundleModule}");
                }
            }
            catch(Exception e)
            {
                Debug.LogError($"加载AssetBundle配置文件失败:{e}");
            }
        }

        /// <summary>
        /// 释放AssetBundle及其占用的内存资源
        /// </summary>
        public void ReleaseAsset(BundleItem bundleItem, bool unload)
        {
            // 释放本身AssetBundle
            ReleaseAssetBundleByBundleItem(bundleItem, unload);

            // 释放其依赖
            if (!bundleItem.bundleDependencies.IsNullOrEmpty())
            {
                foreach(var dependencyName in bundleItem.bundleDependencies)
                {
                    ReleaseAssetBundleByName(dependencyName, unload);
                }
            }
        }

        /// <summary>
        /// 生成AssetBundle配置文件路径
        /// </summary>
        private bool GeneratorBundleConfigPath(BundleModule bundleModule, out string bundleConfigPath, out string bundleConfigName)
        {
            bundleConfigName = $"{bundleModule.ToString().ToLower()}bundleconfig";
            bundleConfigPath = $"{BuildBundleSetting.Instance.GetHotAssetsPath(bundleModule)}/{bundleConfigName}.ab";
            if (!File.Exists(bundleConfigPath))
            {
                bundleConfigPath = $"{BuildBundleSetting.Instance.GetDecompressAssetsPath(bundleModule)}/{bundleConfigName}.ab";
                if (!File.Exists(bundleConfigPath))
                {
                    Debug.LogError($"无法找到{bundleModule}的Assetbundle配置文件,用户网络可能有问题");
                    bundleConfigPath = string.Empty;
                    bundleConfigName = string.Empty;
                    return false;
                }
            }
            return true;
        }


        private AssetBundle LoadAssetBundleReally(string bundleName, BundleModule bundleModule)
        {
            AssetBundleCache assetBundleCache = null;
            if (loadedAssetBundleCaches.TryGetValue(bundleName, out assetBundleCache))
            {
                // 若未加载则加载出来
                if(assetBundleCache == null || (assetBundleCache != null && assetBundleCache.assetBundle == null))
                {
                    assetBundleCache = assetBundleCachePool.Spawn();
                    var hotFilePath = $"{BuildBundleSetting.Instance.GetHotAssetsPath(bundleModule)}/{bundleName}";
                    var hotAssetSystem = HotAssetsSystem.Current;
                    var hotAssetModule = hotAssetSystem.GetHotBundleModule(bundleModule);
                    var isHotPath = true;

                    // 判断路径是否为热更路径
                    if (hotAssetModule == null)
                    {
                        isHotPath = File.Exists(hotFilePath);
                    }
                    else
                    {
                        if(hotAssetSystem.hotAssetCount <= 0)
                        {
                            isHotPath = File.Exists(hotFilePath);
                        }
                        else
                        {
                            isHotPath = hotAssetModule.IsHotAssetExist(bundleName);
                        }
                    }

                    // 若非热更路径，则去解压到本地的路径中去找
                    var bundlePath = isHotPath ? hotFilePath : 
                        $"{BuildBundleSetting.Instance.GetDecompressAssetsPath(bundleModule)}/{bundleName}";

                    // 判断是否需解密
                    if (BuildBundleSetting.Instance.isEncrypt)
                    {
                        byte[] bytes = AES.AESFileByteDecrypt(bundlePath, BuildBundleSetting.Instance.secretKey);
                        assetBundleCache.assetBundle = AssetBundle.LoadFromMemory(bytes);
                    }
                    else
                    {
                        assetBundleCache.assetBundle = AssetBundle.LoadFromFile(bundlePath);
                    }

                    if(assetBundleCache.assetBundle == null)
                    {
                        Debug.LogError($"AssetBundle Load 失败:{bundlePath}");
                        return null;
                    }

                    loadedAssetBundleCaches.Add(bundleName, assetBundleCache);
                }

                // 增加引用计数
                assetBundleCache.referenceCount++;

                return assetBundleCache.assetBundle;
            }
            else
            {
                return null;
            }
        }


        private void ReleaseAssetBundleByBundleItem(BundleItem bundleItem, bool unload)
        {
            if (bundleItem == null)
            {
                Debug.LogError("要释放的BundleItem为空");
                return;
            }

            if (bundleItem.obj != null)
            {
                bundleItem.obj = null;
            }

            ReleaseAssetBundleByName(bundleItem.bundleName, unload);
        }

        private void ReleaseAssetBundleByName(string bundleName, bool unload)
        {
            if (!string.IsNullOrEmpty(bundleName) &&
                loadedAssetBundleCaches.TryGetValue(bundleName, out var assetBundleCache))
            {
                if (assetBundleCache.assetBundle != null)
                {
                    assetBundleCache.referenceCount--;
                    if (assetBundleCache.referenceCount <= 0)
                    {
                        assetBundleCache.assetBundle.Unload(unload);

                        assetBundleCache.Release();
                        loadedAssetBundleCaches.Remove(bundleName);
                        assetBundleCachePool.Release(assetBundleCache);
                    }
                }
            }
        }
    }

    public class BundleItem
    {
        public string path;
        public uint crc;
        public string bundleName;
        public string assetName;
        public BundleModule bundleModule;
        public List<string> bundleDependencies;
        public AssetBundle assetBundle;
        public UnityEngine.Object obj;
    }

    public class AssetBundleCache
    {
        public AssetBundle assetBundle;
        /// <summary>
        /// 引用计数
        /// </summary>
        public int referenceCount;

        public void Release()
        {
            assetBundle = null;
            referenceCount = 0;
        }
    }
}

