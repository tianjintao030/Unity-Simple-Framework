using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tjtFramework.AssetFramework;

public class TestAssetBundleDownload : MonoBehaviour
{
    private HotAssetModule hotAssetModule;

    void Start()
    {
        hotAssetModule = new HotAssetModule(BundleModule.module1, this);
        hotAssetModule.StarHotAssets(GameAppInfoSetting.Instance.GameVersion, StartDownLoad, FinshDownLoad);
    }

    void Update()
    {
        if(hotAssetModule != null)
        {
            hotAssetModule.UpdateDownloader();
        }
    }

    public void StartDownLoad()
    {
        Debug.Log($"开始下载{BundleModule.module1}模块");
    }

    public void FinshDownLoad(BundleModule bundleModule)
    {
        Debug.Log($"下载{bundleModule}模块结束");
    }
}

