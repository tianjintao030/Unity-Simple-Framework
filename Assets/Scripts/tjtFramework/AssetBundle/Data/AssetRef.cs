using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetRef
{
    /// <summary>
    /// 静态配置
    /// </summary>
    public ABAsset asset;

    /// <summary>
    /// 资源所属的BundleRef对象
    /// </summary>
    public BundleRef bundle;
    
    /// <summary>
    /// 依赖所在的BundleRef列表
    /// </summary>
    public BundleRef[] dependencies;

    /// <summary>
    /// 提取出的资源对象
    /// </summary>
    public Object assetObject;

    /// <summary>
    /// 最后一次使用该资源的时间
    /// </summary>
    public float lastUsedTime;

    /// <summary>
    /// 该资源累计使用次数
    /// </summary>
    public int usageCount;

    /// <summary>
    /// 是否是预制体
    /// </summary>
    public bool isPrefab;

    /// <summary>
    /// 依赖该AssetRef的GameObject
    /// </summary>
    public HashSet<GameObject> children;

    public void ResetUsage()
    {
        lastUsedTime = -1;
        usageCount = 0;
    }

    /// <summary>
    /// 更新资源使用时间，并增加使用次数
    /// </summary>
    public void UpdateUsage()
    {
        lastUsedTime = Time.realtimeSinceStartup;
        usageCount++;
    }

    public AssetRef(ABAsset asset)
    {
        this.asset = asset;
        ResetUsage();
    }
}
