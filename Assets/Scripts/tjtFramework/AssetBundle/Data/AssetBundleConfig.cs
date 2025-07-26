using MemoryPack;
using System.Collections.Generic;

/// <summary>
/// 资源集合的数据
/// </summary>
[MemoryPackable]
public partial class ABData
{
    /// <summary>
    /// 版本号
    /// </summary>
    public int VersionCode { get; set; }

    public Dictionary<string, ABBundle> BundleArray {  get; set; }
    public Dictionary<uint, ABAsset> AssetArray { get; set; }
}

/// <summary>
/// 一个AssetBundle的数据
/// </summary>
[MemoryPackable]
public partial class ABBundle
{
    public string Name { get; set; }

    /// <summary>
    /// bundle的Crc散列码
    /// </summary>
    public uint Crc { get; set; }

    /// <summary>
    /// bundle的大小，单位为字节
    /// </summary>
    public int Size {  get; set; }

    /// <summary>
    /// bundle所包含的资源的路径列表
    /// </summary>
    public List<string> Assets { get; set;}
}

/// <summary>
/// 单个资源的数据
/// </summary>
[MemoryPackable]
public partial class ABAsset
{
    /// <summary>
    /// 路径
    /// </summary>
    public string AssetPath {  get; set; }

    /// <summary>
    /// 所属的AssetBundle
    /// </summary>

    public string BundleName {  get; set; }

    /// <summary>
    /// 所依赖的AssetBundle列表
    /// </summary>
    public List<string> Dependencies { get; set; }
}

