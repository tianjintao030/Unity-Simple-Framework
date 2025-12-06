using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.AssetFramework
{
    [CreateAssetMenu(menuName = "AssetBudle/BuildBundleSetting", fileName = "BuildBundleSetting")]
    public class BuildBundleSetting : InstanceScriptableObject<BuildBundleSetting>
    {
        protected override string ResourcePath => "BuildBundleSetting";

        [TitleGroup("资源加载热更设置"), LabelText("下载AssetBundle地址")]
        public string downLoadUrl = "";
        [TitleGroup("资源加载热更设置"), LabelText("热更模式"), EnumToggleButtons]
        public BundleHotEnum bundleHotEnum;
        [TitleGroup("资源加载热更设置"), LabelText("最大下载线程数")]
        public int maxDownLoadThreadCount;

        [TitleGroup("AssetBundle打包设置"), LabelText("是否加密AssetBundle")]
        public bool isEncrypt;
        [TitleGroup("AssetBundle打包设置"), LabelText("加密密钥")]
        public string secretKey = "";
        [TitleGroup("AssetBundle打包设置"), LabelText("资源打包平台"), EnumToggleButtons]
        public BuildTarget buildTarget;
        [TitleGroup("AssetBundle打包设置"), LabelText("资源打包压缩格式"), EnumToggleButtons]
        public BuildAssetBundleOptions bundleOptions;

        [Title("AssetBundle内嵌文件路径")]
        private string builtinAssetsPath { get
            {
                return $"{Application.streamingAssetsPath}/AssetBundle";
            } 
        }
        [Title("AssetBundle内嵌文件配置文件路径")]
        private string builtinAssetsInfoPath { get
            {
                return $"{Application.dataPath}/Resources/BuiltinAssetsInfos";
            } 
        }
        [Title("AssetBundle热更文件存储路径")]
        private string hotAssetsPath { get
            {
                return $"{Application.persistentDataPath}/HotAssets";
            } 
        }
        [Title("AssetBundle解压文件路径")]
        private string bundleDecompressPath { get
            {
                return $"{Application.persistentDataPath}/DecompressAssets";
            } 
        }

        /// <summary>
        /// 获取资源内嵌路径
        /// </summary>
        public string GetBuiltinAssetsPath(BundleModule bundleModule)
        {
            return $"{builtinAssetsPath}/{bundleModule}";
        }
        public string GetBuiltinAssetInfoPath()
        {
            return builtinAssetsInfoPath;
        }
        /// <summary>
        /// 获取资源热更文件存储路径
        /// </summary>
        public string GetHotAssetsPath(BundleModule bundleModule)
        {
            return $"{hotAssetsPath}/{bundleModule}";
        }
        /// <summary>
        /// 获取资源解压路径
        /// </summary>
        public string GetDecompressAssetsPath(BundleModule bundleModule)
        {
            return $"{bundleDecompressPath}/{bundleModule}";
        }

        /// <summary>
        /// 获取本地热更资源清单路径
        /// </summary>
        public string GetLocalHotAssetsManifestPath(BundleModule bundleModule)
        {
            return $"{Application.persistentDataPath}/tjtFramework/Local/{bundleModule}HotAssetManifest.json";
        }
        /// <summary>
        /// 获取服务器下载下来的热更资源清单路径
        /// </summary>
        public string GetServerHotAssetsManifestPath(BundleModule bundleModule)
        {
            return $"{Application.persistentDataPath}/tjtFramework/Server/{bundleModule}HotAssetManifest.json";
        }
    }

    public enum BundleHotEnum
    {
        Hot,
        No_Hot
    }

    [SerializeField]
    public enum BuildTarget
    {
        StandaloneOSX = 2,

        StandaloneWindows = 5,

        iOS = 9,

        Android = 13,

        StandaloneWindows64 = 19,

        WebGL = 20,

        VisionOS = 47,

        NoTarget = -2
    }

    [SerializeField]
    public enum BuildAssetBundleOptions
    {
        None = 0,

        UncompressedAssetBundle = 1,

        DisableWriteTypeTree = 8,

        ForceRebuildAssetBundle = 0x20,

        IgnoreTypeTreeChanges = 0x40,

        AppendHashToAssetBundleName = 0x80,

        ChunkBasedCompression = 0x100,

        StrictMode = 0x200,

        DryRunBuild = 0x400,

        DisableLoadAssetByFileName = 0x1000,

        DisableLoadAssetByFileNameWithExtension = 0x2000,

        AssetBundleStripUnityVersion = 0x8000,

        UseContentHash = 0x10000
    }
}

