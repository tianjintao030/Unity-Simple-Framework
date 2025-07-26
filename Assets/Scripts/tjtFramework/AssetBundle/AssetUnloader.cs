using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tjtFramework.Singleton;

/// <summary>
/// 资源卸载器
/// </summary>
public class AssetUnloader : NoMonoSingleton<AssetUnloader>
{
   public AssetUnloader()
    {

    }

    /// <summary>
    /// 卸载当前未被使用的资源
    /// </summary>
    public void UnloadUnusedAssets()
    {
        var unloadList = GetUnloadAssetRefList();
        if (unloadList == null || unloadList.Count == 0)
        {
            Debug.Log("暂无可卸载资源");
            return;
        }

        var unloadCount = 0;
        foreach (var assetRef in unloadList)
        {
            if (IsSafeToUnload(assetRef))
            {
                unloadCount++;
                UnloadAssetRef(assetRef);
            }
        }

        if(unloadCount > 0)
        {
            Resources.UnloadUnusedAssets();
            Debug.Log($"此次资源卸载了{unloadCount}个");
        }
    }

    /// <summary>
    /// 是否可以安全卸载某个资源
    /// </summary>
    private bool IsSafeToUnload(AssetRef assetRef)
    {
        // 如果资源为空或仍有引用，不安全
        if (assetRef == null || assetRef.assetObject == null)
        {
            return false;
        }

        if (assetRef.children == null)
        {
            return false;
        }

        assetRef.children.RemoveWhere(child => child == null);

        // 有对象引用则不能卸载
        if (assetRef.children.Count > 0)
        {
            return false;
        }

        // Bundle为空不应卸载（理论上不应该）
        if (assetRef.bundle == null || assetRef.bundle.assetBundle == null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 卸载单个AssetRef
    /// </summary>
    private void UnloadAssetRef(AssetRef assetRef)
    {
        assetRef.ResetUsage();

        //资源对象置空
        assetRef.assetObject = null;

        //解除与所属Bundle的关系
        assetRef.bundle.children.Remove(assetRef);
        //如果此时发现所属的AssetBundle已经没有被使用的资源了，则可以卸载该AssetBundle
        if(assetRef.bundle.children.Count <= 0 && assetRef.bundle.assetBundle != null)
        {
            assetRef.bundle.assetBundle.Unload(true);
        }

        //解除与依赖Bundle们的关系
        foreach(var dependenciesBundle in assetRef.dependencies)
        {
            dependenciesBundle.children.Remove(assetRef);

            if (dependenciesBundle.children.Count <= 0 && dependenciesBundle.assetBundle != null)
            {
                dependenciesBundle.assetBundle.Unload(true);
            }
        }

        Debug.Log($"已卸载资源：{assetRef.asset?.AssetPath}");
    }

    /// <summary>
    /// 获取可卸载AssetRef列表
    /// </summary>
    private List<AssetRef> GetUnloadAssetRefList()
    {
        var assetRefList = new List<AssetRef>();
        var assetRefDic = AssetLoader.Instance.GetAssetDic();

        foreach(var assetRef in assetRefDic.Values)
        {
            //防止资源在被异步加载时误删
            if(assetRef.assetObject == null || assetRef.children == null)
            {
                continue;
            }

            //该资源没有被任何一个游戏对象依赖了，可以卸载
            assetRef.children.RemoveWhere(child => child == null);
            if(assetRef.children.Count <= 0)
            {
                assetRefList.Add(assetRef);
            }
        }

        return assetRefList;
    }
}
