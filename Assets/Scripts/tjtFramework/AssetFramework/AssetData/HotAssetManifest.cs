using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.AssetFramework
{
    /// <summary>
    /// 热更资源清单
    /// </summary>
    public class HotAssetManifest
    {
        /// <summary>
        /// 游戏版本
        /// </summary>
        public string gameVersion;
        /// <summary>
        /// 下载地址
        /// </summary>
        public string downLoadUrl;
        /// <summary>
        /// 热更补丁列表
        /// </summary>
        public List<HotAssetPatch> patchList = new();
    }

    /// <summary>
    /// 热更文件信息
    /// </summary>
    public class HotFileInfo
    {
        /// <summary>
        /// AssetBundle名称
        /// </summary>
        public string abName;
        public string md5;
        public float size;
    }

    /// <summary>
    /// 热更资源补丁
    /// </summary>
    public class HotAssetPatch
    {
        /// <summary>
        /// 资源版本(补丁版本)
        /// </summary>
        public string resourceVersion;
        /// <summary>
        /// 热更资源列表
        /// </summary>
        public List<HotFileInfo> hotFileList = new();
    }
}

