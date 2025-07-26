using System.Collections.Generic;
using UnityEngine;

public class BundleRef
{
    /// <summary>
    /// AssetBundle的静态配置信息
    /// </summary>
    public ABBundle bundle;

    /// <summary>
    /// 记录AssetBundle加载路径
    /// </summary>
    public BaseOrUpdate witch;

    /// <summary>
    /// 加载到内存的AssetBundle对象
    /// </summary>
    public AssetBundle assetBundle;

    /// <summary>
    /// 该BundleRef对应的包被哪些AssetRef依赖
    /// </summary>
    public List<AssetRef> children;

    public BundleRef(ABBundle bundle,BaseOrUpdate witch)
    {
        this.bundle = bundle;
        this.witch = witch;
    }
}

/// <summary>
/// 选择路径
/// </summary>
public enum BaseOrUpdate
{
    /// <summary>
    /// APP安装时生成的原装只读路径
    /// </summary>
    Base = 0,
    /// <summary>
    /// APP提供的可读可写路径
    /// </summary>
    Update = 1
}
