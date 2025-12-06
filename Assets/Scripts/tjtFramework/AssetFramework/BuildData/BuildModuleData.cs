using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.AssetFramework
{
    /// <summary>
    /// 构建模块数据
    /// </summary>
    [System.Serializable]
    public class BuildModuleData
    {
        /// <summary>
        /// AssetBundle模块id
        /// </summary>
        public long bundleId;

        /// <summary>
        /// 模块名称
        /// </summary>
        public string moduleName;

        /// <summary>
        /// 是否需要构建
        /// </summary>
        public bool isBuild;

        /// <summary>
        /// 上次点击的时间
        /// </summary>
        [HideInInspector]
        public float lastClickTime;

        public string[] prefabPathArray;
        public string[] rootFolderSubBundlePathArray;
        public BundleFileInfo[] signBudlePathArray;
    }

    /// <summary>
    /// 包文件夹数据
    /// </summary>
    [System.Serializable]
    public class BundleFileInfo
    {
        public string bundleName;
        [FolderPath]
        public string bundlePath;
    }
}

