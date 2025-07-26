using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using MemoryPack;
using tjtFramework.Singleton;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 资源加载器
/// </summary>
public partial class AssetLoader : NoMonoSingleton<AssetLoader>, ILoader
{
    public GameObject Clone(string path)
    {
        var assetRef = LoadAssetRef<GameObject>(path);
        if(assetRef == null || assetRef.assetObject == null)
        {
            return null;
        }

        return AfterAssetRef(assetRef);
    }

    public async UniTask<GameObject> CloneAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        AssetRef assetRef = null;

        // 编辑器模式（非AB包）
        if (InEditor && !isAssetBundleMode)
        {
#if UNITY_EDITOR
            assetRef = new AssetRef(null);
            assetRef.assetObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            assetRef.isPrefab = true;
            Debug.Log($"编辑器模式加载Prefab {path}");
#endif
        }
        else
        {
            assetRef = GetAssetRef(path);
            if (assetRef == null)
                return null;

            // 依赖
            foreach (var dep in assetRef.dependencies)
            {
                await HandleBundleRefAsync(dep, assetRef);
            }

            // 主包
            await HandleBundleRefAsync(assetRef.bundle, assetRef);

            // 异步加载Prefab
            var request = assetRef.bundle.assetBundle.LoadAssetAsync<GameObject>("Assets/Game/" + assetRef.asset.AssetPath);
            await request.ToUniTask();

            assetRef.assetObject = request.asset;
            assetRef.isPrefab = true;

            Debug.Log($"AB包模式异步加载Prefab {assetRef.asset.AssetPath}");
        }

        if (assetRef == null || assetRef.assetObject == null)
            return null;

        var go = UnityEngine.Object.Instantiate(assetRef.assetObject) as GameObject;

        if (assetRef.children == null)
            assetRef.children = new HashSet<GameObject>();

        assetRef.children.RemoveWhere(child => child == null);
        assetRef.children.Add(go);

        return go;
    }

    public T LoadAsset<T>(string assetPathRelatively, GameObject gameObject) where T : UnityEngine.Object
    {
        if(string.IsNullOrEmpty(assetPathRelatively))
        {
            return null;
        }

        if(typeof(T) == typeof(GameObject) || assetPathRelatively.EndsWith(".prefab"))
        {
            Debug.LogError($"LoadAsset不可加载GameObject对象{assetPathRelatively}，请使用Clone接口");
            return null;
        }

        if(gameObject == null)
        {
            Debug.LogError($"加载资源{assetPathRelatively}时没有传递一个可挂载的GameObject");
            return null;
        }

        var assetPath = Path.Combine("Assets/Game", assetPathRelatively);

        var assetRef = LoadAssetRef<T>(assetPath);

        if(assetRef == null || assetRef.assetObject == null)
        {
            return null;
        }

        if(assetRef.children == null)
        {
            assetRef.children = new HashSet<GameObject>();
        }

        //清除依赖该AssetRef的GameObject中为空的
        assetRef.children.RemoveWhere(child => child == null);

        //防止重复添加
        if(!assetRef.children.Contains(gameObject))
        {
            assetRef.children.Add(gameObject);
        }

        return assetRef.assetObject as T;
    }

    public async UniTask<T> LoadAssetAsync<T>(string assetPathRelatively, GameObject gameObject) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(assetPathRelatively))
        {
            return null;
        }

        if (typeof(T) == typeof(GameObject) || assetPathRelatively.EndsWith(".prefab"))
        {
            Debug.LogError($"LoadAssetAsync不可加载GameObject对象 {assetPathRelatively}，请使用CloneAsync接口");
            return null;
        }

        if (gameObject == null)
        {
            Debug.LogError($"加载资源 {assetPathRelatively} 时没有传递一个可挂载的GameObject");
            return null;
        }

        var assetPath = Path.Combine("Assets/Game", assetPathRelatively);

        AssetRef assetRef = null;

        // 编辑器模式（非AB包）
        if (InEditor && !isAssetBundleMode)
        {
#if UNITY_EDITOR
            assetRef = new AssetRef(null);
            assetRef.assetObject = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Debug.Log($"编辑器模式加载资源 {assetPath}");
#endif
        }
        else
        {
            // 运行时AB包加载
            assetRef = GetAssetRef(assetPath);
            if (assetRef == null)
                return null;

            // 依赖
            foreach (var dep in assetRef.dependencies)
            {
                await HandleBundleRefAsync(dep, assetRef);
            }

            // 主包
            await HandleBundleRefAsync(assetRef.bundle, assetRef);

            // 异步加载Asset
            var request = assetRef.bundle.assetBundle.LoadAssetAsync<T>("Assets/Game/" + assetRef.asset.AssetPath);
            await request.ToUniTask();

            assetRef.assetObject = request.asset;
            assetRef.isPrefab = false;

            Debug.Log($"AB包模式异步加载资源 {assetRef.asset.AssetPath}");
        }

        if (assetRef == null || assetRef.assetObject == null)
        {
            return null;
        }

        if (assetRef.children == null)
        {
            assetRef.children = new HashSet<GameObject>();
        }

        assetRef.children.RemoveWhere(child => child == null);

        if (!assetRef.children.Contains(gameObject))
        {
            assetRef.children.Add(gameObject);
        }

        return assetRef.assetObject as T;
    }

    public async UniTask LoadConfigData()
    {
        if(!InEditor || isAssetBundleMode)
        {
            BaseData = await LoadConfigData(BaseOrUpdate.Base);
            UpdateData = await LoadConfigData(BaseOrUpdate.Update);
        }
    }

    public void SetBundleMode(bool isBundleMode)
    {
        isAssetBundleMode = isBundleMode;
    }

    /// <summary>
    /// 编辑器模式下是否采用AssetBundle模式
    /// </summary>
    private static bool isAssetBundleMode = false;
    public static bool IsAssetBundleMode => isAssetBundleMode;

#region 平台
    /// <summary>
    /// 是否在编辑器
    /// </summary>
    public static bool InEditor
    {
        get
        {
#if UNITY_EDITOR
        return true;
#else
        return false;
#endif
        }
    }

    /// <summary>
    /// 是否Android
    /// </summary>
    public static bool IsAndroid
    {
        get
        {
#if !UNITY_EDITOR && UNITY_ANDROID
        return true;
#else
        return false;
#endif
        }
    }

    /// <summary>
    /// 是否iOS
    /// </summary>
    public static bool IsIOS
    {
        get
        {
#if !UNITY_EDITOR && UNITY_IOS
        return true;
#else
        return false;
#endif
        }
    }

    /// <summary>
    /// 是否Windows
    /// </summary>
    public static bool IsWindows
    {
        get
        {
#if UNITY_STANDALONE_WIN
        return true;
#else
        return false;
#endif
        }
    }

    /// <summary>
    /// 是否MacOS
    /// </summary>
    public static bool IsMacOS
    {
        get
        {
#if UNITY_STANDALONE_OSX
        return true;
#else
        return false;
#endif
        }
    }

    /// <summary>
    /// 是否Linux
    /// </summary>
    public static bool IsLinux
    {
        get
        {
#if UNITY_STANDALONE_LINUX
        return true;
#else
        return false;
#endif
        }
    }

    /// <summary>
    /// 是否PC平台
    /// </summary>
    public static bool IsStandalone
    {
        get
        {
#if UNITY_STANDALONE
        return true;
#else
        return false;
#endif
        }
    }

    /// <summary>
    /// 是否移动平台(Android或iOS)
    /// </summary>
    public static bool IsMobile
    {
        get
        {
#if UNITY_ANDROID || UNITY_IOS
            return true;
#else
        return false;
#endif
        }
    }
    #endregion

#region 内部方法
    /// <summary>
    /// 加载AssetBundle的二进制配置文件
    /// </summary>
    private async UniTask<ABData> LoadConfigData(BaseOrUpdate baseOrUpdate, bool isTemp = false)
    {
        var url = GetConfigBinPath(baseOrUpdate, isTemp);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            try
            {
                await request.SendWebRequest().ToUniTask();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    using (var compressedStream = new MemoryStream(request.downloadHandler.data))
                    {
                        return MemoryPackSerializer.Deserialize<ABData>(compressedStream.ToArray());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"加载AssetBundle热更配置二进制文件报错：{e.Message}");
            }
        }

        return null;
    }

    /// <summary>
    /// 加载AssetRef对象
    /// </summary>
    private AssetRef LoadAssetRef<T>(string assetPath) where T : UnityEngine.Object
    {
        if(!InEditor || isAssetBundleMode)
        {
            return LoadAssetRefRuntime<T>(assetPath);
        }
        else
        {
            return LoadAssetRefEditor<T>(assetPath);
        }
    }

    /// <summary>
    /// AB包加载模式下加载资源AssetRef
    /// </summary>
    private AssetRef LoadAssetRefRuntime<T>(string assetPath) where T : UnityEngine.Object
    {
        var assetRef = GetAssetRef(assetPath);
        if(assetRef == null)
        {
            return null;
        }

        //assetObject不为空，说明对应的资源对象已加载过，存在于内存中
        if(assetRef.assetObject != null)
        {
            assetRef.UpdateUsage();

            return assetRef;
        }

        //1.处理assetRef依赖的BundleRef列表
        foreach(var dependenceBundleRef in assetRef.dependencies)
        {
            HandleBundleRef(dependenceBundleRef, assetRef);
        }

        //2.处理assetRef所属的BundleRef
        HandleBundleRef(assetRef.bundle, assetRef);

        //3.从AssetBundle中提取asset
        assetRef.assetObject = assetRef.bundle.assetBundle.LoadAsset<T>("Assets/Game/" + assetRef.asset.AssetPath);

        if(typeof(T) == typeof(GameObject) && assetRef.asset.AssetPath.EndsWith(".prefab"))
        {
            assetRef.isPrefab = true;
        }
        else
        {
            assetRef.isPrefab = false;
        }

        Debug.Log($"AB包模式从AssetBundle包{assetRef.bundle.bundle.Name}加载资源{assetRef.asset.AssetPath}");
        assetRef.UpdateUsage();
        return assetRef;
    }

    /// <summary>
    /// 在编辑器模式下使用非AB包模式加载AssetRef
    /// </summary>
    private AssetRef LoadAssetRefEditor<T>(string assetPath) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if(string.IsNullOrEmpty(assetPath))
        {
            return null;
        }

        AssetRef assetRef = new AssetRef(null);

        assetRef.assetObject = AssetDatabase.LoadAssetAtPath<T>(assetPath);

        Debug.Log($"编辑器模式加载资源{assetPath}");
        return assetRef;
#else
        return null;
#endif
    }

    /// <summary>
    /// 根据资源路径获取AssetRef
    /// </summary>
    private AssetRef GetAssetRef(string assetPath)
    {
        if(string.IsNullOrEmpty(assetPath))
        {
            return null;
        }

        uint assetPathCrc = Convert.ToUInt32(CRC.GetCRC32Hash(assetPath),16);

        AssetDic.TryGetValue(assetPathCrc, out AssetRef existAssetRef);
        if(existAssetRef != null)
        {
            return existAssetRef;
        }

        ConfigData.AssetArray.TryGetValue(assetPathCrc, out ABAsset assetInfo);
        if(assetInfo == null)
        {
            return null;
        }

        var assetRef = new AssetRef(assetInfo);
        assetRef.bundle = GetBundleRef(assetInfo.BundleName);

        int dependenciesCount = assetInfo.Dependencies.Count;
        assetRef.dependencies = new BundleRef[dependenciesCount];
        for(int i = 0; i < dependenciesCount; i++)
        {
            assetRef.dependencies[i] = GetBundleRef(assetInfo.Dependencies[i]);
        }

        AssetDic.Add(assetPathCrc, assetRef);

        return assetRef;
    }

    /// <summary>
    /// 根据AssetBundle名字获取BundleRef
    /// </summary>
    private BundleRef GetBundleRef(string bundleName)
    {
        BundleDic.TryGetValue(bundleName, out BundleRef existBundleRef);
        if(existBundleRef != null)
        {
            return existBundleRef;
        }

        ConfigData.BundleArray.TryGetValue(bundleName,out ABBundle bundle);
        if(bundle == null)
        {
            return null;
        }

        //判断是base路径还是热更路径
        var isInUpdatePath = File.Exists(GetAssetBundlePath(BaseOrUpdate.Update, bundleName));
        BaseOrUpdate witch = isInUpdatePath ? BaseOrUpdate.Update : BaseOrUpdate.Base;

        var bundleRef = new BundleRef(bundle, witch);

        BundleDic.Add(bundleName, bundleRef);

        return bundleRef;
    }

    /// <summary>
    /// 处理BundleRef数据
    /// </summary>
    private void HandleBundleRef(BundleRef bundleRef, AssetRef assetRef)
    {
        if(bundleRef.assetBundle == null)
        {
            var bundlePath = GetAssetBundlePath(bundleRef.witch, bundleRef.bundle.Name);
            bundleRef.assetBundle = AssetBundle.LoadFromFile(bundlePath);
        }

        if(bundleRef.children == null)
        {
            bundleRef.children = new List<AssetRef>();
        }    

        bundleRef.children.Add(assetRef);
    }

    /// <summary>
    /// 异步处理BundleRef数据
    /// </summary>
    private async UniTask HandleBundleRefAsync(BundleRef bundleRef, AssetRef assetRef)
    {
        if (bundleRef.assetBundle == null)
        {
            string bundlePath = GetAssetBundlePath(bundleRef.witch, bundleRef.bundle.Name);

            // 异步加载AssetBundle
            var request = AssetBundle.LoadFromFileAsync(bundlePath);
            await request.ToUniTask();

            bundleRef.assetBundle = request.assetBundle;
        }

        if (bundleRef.children == null)
        {
            bundleRef.children = new List<AssetRef>();
        }

        if (!bundleRef.children.Contains(assetRef))
        {
            bundleRef.children.Add(assetRef);
        }
    }

    /// <summary>
    /// 根据一个AssetRef实例化一个GameObject
    /// </summary>
    private GameObject AfterAssetRef(AssetRef assetRef)
    {
        GameObject go = UnityEngine.Object.Instantiate(assetRef.assetObject) as GameObject;

        if(assetRef.children == null)
        {
            assetRef.children = new HashSet<GameObject>();
        }

        assetRef.children.RemoveWhere(child => child == null);
        assetRef.children.Add(go);

        return go;
    }
 #endregion

    /// <summary>
    /// 获取二进制配置文件路径
    /// </summary>
    public static string GetConfigBinPath(BaseOrUpdate baseOrUpdate, bool isTemp = false)
    {
        var temp = isTemp ? "_temp" : "";
        var fileName = $"config{temp}.bin";
        string filePath = null;
        
        if(baseOrUpdate == BaseOrUpdate.Update)
        {
            // 更新资源放到可读可写目录
            filePath = Path.Combine(Application.persistentDataPath, fileName);
        }
        else
        {
            // Base资源放到只读目录
#if UNITY_ANDROID
            filePath = Path.Combine(Application.streamingAssetsPath, fileName);
#elif UNITY_IOS
            filePath = "file://" + Path.Combine(Application.streamingAssetsPath, fileName);
#elif UNITY_STANDALONE_WIN
            filePath = Path.Combine(Application.streamingAssetsPath, fileName);
#elif UNITY_STANDALONE_OSX
            filePath = Path.Combine(Application.streamingAssetsPath, fileName);
#else
            // 其他平台默认到StreamingAssets
            filePath = Path.Combine(Application.streamingAssetsPath, fileName);
#endif
        }

        return filePath;
    }

    /// <summary>
    /// 根据模块名和AssetBundle名获取其实际资源路径
    /// </summary>
    public static string GetAssetBundlePath(BaseOrUpdate baseOrUpdate, string bundleName)
    {
        if(baseOrUpdate == BaseOrUpdate.Update)
        {
            return Path.Combine(Application.persistentDataPath, bundleName);
        }
        else
        {
            return Path.Combine(Application.streamingAssetsPath, bundleName);
        }
    }

    

    /// <summary>
    /// app安装时随包的只读路径下的资源配置数据
    /// </summary>
    private ABData BaseData;
    /// <summary>
    /// app的可读可写的路径下的资源配置数据
    /// </summary>
    private ABData UpdateData;

    public ABData ConfigData
    {
        get
        {
            if(UpdateData != null)
            {
                return UpdateData;
            }

            return BaseData;
        }
    }

    /// <summary>
    /// 资源集合
    /// </summary>
    private Dictionary<uint, AssetRef> AssetDic;
    /// <summary>
    /// Bundle集合
    /// </summary>
    private Dictionary<string, BundleRef> BundleDic;

    public Dictionary<uint, AssetRef> GetAssetDic()
    {
        return AssetDic;
    }

    public AssetLoader()
    {
        AssetDic = new Dictionary<uint, AssetRef>();
        BundleDic = new Dictionary<string, BundleRef>();
    }
}
