using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 加载器接口
/// </summary>
public interface ILoader
{
    /// <summary>
    /// 加载Config配置文件
    /// </summary>
    /// <returns></returns>
    public UniTask LoadConfigData();

    /// <summary>
    /// 同步加载资源对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetPath">资源路径(以Assets开头)</param>
    /// <param name="gameObject">挂载的游戏对象</param>
    public T LoadAsset<T>(string assetPath, GameObject gameObject) where T : UnityEngine.Object;

    /// <summary>
    /// 异步加载资源对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetPath">资源路径(以Assets开头)</param>
    /// <param name="gameObject">挂载的游戏对象</param>
    public UniTask<T> LoadAssetAsync<T>(string assetPath, GameObject gameObject) where T : UnityEngine.Object;

    /// <summary>
    /// 同步克隆一个GameObject对象
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public GameObject Clone(string path);

    /// <summary>
    /// 异步克隆一个GameObject对象
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public UniTask<GameObject> CloneAsync(string path);

    /// <summary>
    /// 设置编辑器中是否使用AssetBundle模式加载
    /// </summary>
    /// <param name="isBundleMode"></param>
    public void SetBundleMode(bool isBundleMode);
}
