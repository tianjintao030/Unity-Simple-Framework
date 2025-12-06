using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.AssetFramework
{
    [System.Serializable]
    public class AssetBundleConfig
    {
        public List<AssetBundleInfo> assetBundleInfoList = new();
    }

    [System.Serializable]
    /// <summary>
    /// AssetBundle信息
    /// </summary>
    public class AssetBundleInfo
    {
        public string path;
        public uint crc;
        public string AssetBundleName;
        public string AssetName;
        public List<string> dependencies;
    }

    [System.Serializable]
    /// <summary>
    /// 内嵌的AssetBundle信息
    /// </summary>
    public class BuildInAssetBundleInfo
    {
        public string fileName;
        /// <summary>
        ///  校验本地已解压文件是否与包体中一致，若不一致则重新解压
        ///  (前提为模块为非热更模块)
        /// </summary>
        public string md5;
        public float size; 
    }
}

