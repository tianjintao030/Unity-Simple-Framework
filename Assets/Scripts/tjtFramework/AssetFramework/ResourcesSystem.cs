using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using tjtFramework.Utiliy;
using tjtFramework.Pool;
using tjtFramework.GameSystem;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace tjtFramework.AssetFramework.GameResources
{
    public class ResourcesSystem : GameSystemBase<ResourcesSystem>
    {
        /// <summary>
        /// 已加载过的资源
        /// key:crc
        /// </summary>
        private Dictionary<uint, BundleItem> loadedBundleItemsByCrc  = new();

        /// <summary>
        /// 按crc缓存的GameObject，key:crc，
        /// 用于按路径复用的GameObject
        /// </summary>
        private Dictionary<uint, List<CacheGameObject>> cachedGameObjectsByCrc = new();
        /// <summary>
        /// 按InstanceId缓存GameObject，key:instanceId，
        /// 用于按InstanceId复用的GameObject
        /// </summary>
        private Dictionary<int, CacheGameObject> cachedGameObjectsByInstanceId = new();

        /// <summary>
        /// CacheGameObject对象池
        /// </summary>
        private ClassObjectPool<CacheGameObject> cacheGameObjectPool = new ClassObjectPool<CacheGameObject>(300);

        /// <summary>
        /// 异步加载任务id列表
        /// </summary>
        private List<long> asyncTaskIdList = new();
        private long asyncTaskId;
        public long NewAsyncTaskId
        {
            get
            {
                if(asyncTaskId >= long.MaxValue)
                {
                    asyncTaskId = 0;
                }
                return asyncTaskId++;
            }
        }

        public override bool needUpdate => false;

        public override void OnInit()
        {
            base.OnInit();
            MarkReady();
        }

        #region 资源加载
        /// <summary>
        /// 同步加载资源（仅加载无需实例化的资源）
        /// </summary>
        public T LoadResource<T>(string path) where T : UnityEngine.Object
        {
            Debug.Log($"同步加载{typeof(T).Name}资源,path:{path}");

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"加载资源时传入的路径为空");
                return null;
            }

            var crc = Crc32.GetCrc32(path);
            var bundleItem = GetOrNewBundleItemFromAssetDic(crc);

            // 若BundleItem已加载过，则直接返回
            if (bundleItem.obj != null)
            {
                return bundleItem.obj as T;
            }

#if UNITY_EDITOR
            if (ResourcesLoadSetting.Instance.isEditorModel)
            {
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
#endif

            var assetBundleLoadSystem = AssetBundleLoadSystem.Current;
            if (assetBundleLoadSystem == null)
            {
                Debug.LogError("未找到AssetBundleLoadSystem实例");
                return null;
            }

            bundleItem = assetBundleLoadSystem.LoadAssetBundle(crc);
            if (bundleItem == null)
            {
                Debug.LogError("bundleItem为空");
                return null;
            }
            if (bundleItem.assetBundle == null)
            {
                Debug.LogError($"bundleItem \"{bundleItem.assetName}\" 的assetBundle为空");
                return null;
            }

            var obj = bundleItem.obj != null ?
                       bundleItem.obj as T :
                       bundleItem.assetBundle.LoadAsset<T>(bundleItem.assetName);

            // 缓存已加载的资源
            bundleItem.obj = obj;
            bundleItem.path = path;
            loadedBundleItemsByCrc[crc] = bundleItem;

            return obj as T;
        }

        /// <summary>
        /// 异步加载资源（仅加载无需实例化的资源）
        /// </summary>
        public async UniTask<T> LoadResourceAsync<T>(string path, Action<UnityEngine.Object> loadFinshCallback = null) where T : UnityEngine.Object
        {
            Debug.Log($"异步加载{typeof(T).Name}资源,path:{path}");

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"加载资源时传入的路径为空");
                return null;
            }

            var crc = Crc32.GetCrc32(path);
            var bundleItem = GetOrNewBundleItemFromAssetDic(crc);

            // 若BundleItem已加载过，则直接返回
            if (bundleItem.obj != null)
            {
                loadFinshCallback?.Invoke(bundleItem.obj);
                return bundleItem.obj as T;
            }

#if UNITY_EDITOR
            if (ResourcesLoadSetting.Instance.isEditorModel)
            {
                loadFinshCallback?.Invoke(bundleItem.obj);
                return AssetDatabase.LoadAssetAtPath<T>(path);
            }
#endif

            var assetBundleLoadSystem = AssetBundleLoadSystem.Current;
            if (assetBundleLoadSystem == null)
            {
                Debug.LogError("未找到AssetBundleLoadSystem实例");
                return null;
            }

            bundleItem = assetBundleLoadSystem.LoadAssetBundle(crc);
            if (bundleItem == null)
            {
                Debug.LogError("bundleItem为空");
                return null;
            }
            if (bundleItem.assetBundle == null)
            {
                Debug.LogError($"bundleItem \"{bundleItem.assetName}\" 的assetBundle为空");
                return null;
            }

            UnityEngine.Object obj = null;
            if(bundleItem.obj != null)
            {
                obj = bundleItem.obj as T;
            }
            else
            {
                var assetBundleObj = await bundleItem.assetBundle.LoadAssetAsync<T>(bundleItem.assetName);
                obj = assetBundleObj as T;
            }

            bundleItem.obj = obj;
            bundleItem.path = path;
            loadedBundleItemsByCrc[crc] = bundleItem;

            loadFinshCallback?.Invoke(obj);
            return obj as T;
        }
        #endregion

        #region GameObject克隆
        /// <summary>
        /// 同步克隆游戏对象(路径)
        /// </summary>
        public GameObject InstantiateByPath(string path, Transform parent)
        {
            path = path.EndsWith(".prefab") ? path : $"{path}.prefab";

            // 缓存GameObject对象池中若有则直接返回
            var cacheGameObject = GetGameObjectFromCacheGameObjectPool(Crc32.GetCrc32(path));
            if(cacheGameObject != null)
            {
                cacheGameObject.SetActive(true);
                cacheGameObject.transform.SetParent(parent);
                return cacheGameObject;
            }

            // 没有缓存则加载出资源
            var loadObj = LoadResource<GameObject>(path);
            if (loadObj != null)
            {
                return InstantiateGameObject(path, parent, loadObj);
            }
            else
            {
                Debug.LogError($"加载 {path} 游戏对象失败");
                return null;
            }
        }

        /// <summary>
        /// 异步克隆游戏对象(路径)
        /// </summary>
        public async UniTask InstantiateByPathAsync(string path, Transform parent, Action<GameObject, object, object> callback,
                                                                object callbackParam1 = null, object callbackParam2 = null)
        {
            path = path.EndsWith(".prefab") ? path : $"{path}.prefab";

            var cacheGameObject = GetGameObjectFromCacheGameObjectPool(Crc32.GetCrc32(path));
            if(cacheGameObject != null)
            {
                cacheGameObject.SetActive(true);
                cacheGameObject.transform.SetParent(parent);
                callback?.Invoke(cacheGameObject, callbackParam1, callbackParam2);
                return;
            }

            var taskId = NewAsyncTaskId;
            lock (asyncTaskIdList)
            {
                asyncTaskIdList.Add(taskId);
            }
            
            var loadObj = await LoadResourceAsync<GameObject>(path);
            if(loadObj != null)
            {
                lock (asyncTaskIdList)
                {
                    if (asyncTaskIdList.Contains(taskId))
                    {
                        asyncTaskIdList.Remove(taskId);

                        var go = InstantiateGameObject(path, parent, loadObj);
                        callback?.Invoke(go, callbackParam1, callbackParam2);
                    }
                }
            }
            else
            {
                if(asyncTaskIdList.Contains(taskId))
                {
                    asyncTaskIdList.Remove(taskId);
                }
                Debug.LogError($"加载 {path} 游戏对象失败");
            }
        }
        #endregion

        #region 资源释放
        /// <summary>
        /// 释放游戏对象
        /// </summary>
        public void ReleaseGameObject(GameObject gameObject, bool destory = false)
        {
            var instanceId = gameObject.GetInstanceID();
            if(!cachedGameObjectsByInstanceId.TryGetValue(instanceId, out var cacheGameObject))
            {
                Debug.LogError($"池中无法找到InstanceId为{instanceId}的{gameObject.name},无法释放");
                return;
            }

            var crc = cacheGameObject.crc;

            if (destory)
            {
                GameObject.Destroy(gameObject);

                cacheGameObject.Release();
                cacheGameObjectPool.Release(cacheGameObject);

                cachedGameObjectsByInstanceId.Remove(instanceId);

                if (!cachedGameObjectsByCrc.TryGetValue(crc, out var cacheGameObjectsWithCrc) ||
                    cacheGameObjectsWithCrc.IsNullOrEmpty())
                {
                    // 若对象在对象池中不存在或者全部释放完了，卸载该对象的AssetBundle资源占用
                    if (loadedBundleItemsByCrc.TryGetValue(crc, out var bundleItem))
                    {
                        AssetBundleLoadSystem.Current.ReleaseAsset(bundleItem, true);
                        loadedBundleItemsByCrc.Remove(crc);
                    }
                }
            }
            else
            {
                gameObject.SetActive(false);

                if (cachedGameObjectsByCrc.TryGetValue(cacheGameObject.crc, out var cacheGameObjectsWithCrc))
                {
                    cacheGameObjectsWithCrc.Add(cacheGameObject);
                }
                else
                {
                    cachedGameObjectsByCrc[crc] = new List<CacheGameObject> { cacheGameObject };
                }
            }
        }

        /// <summary>
        /// 清理加载的资源，释放内存
        /// </summary>
        /// <param name="absoluteCleaning">是否深度清理</param>
        public void ClearResourcesAssets(bool absoluteCleaning)
        {
            if(absoluteCleaning)
            {
                if(cachedGameObjectsByInstanceId != null && cachedGameObjectsByInstanceId.Count > 0)
                {
                    foreach(var item in cachedGameObjectsByInstanceId)
                    {
                        var cacheGameObject = item.Value;
                        if(cacheGameObject.gameObject != null)
                        {
                            // 销毁GameObject对象，回收缓存类对象
                            GameObject.Destroy(cacheGameObject.gameObject);
                            cacheGameObject.Release();
                            cacheGameObjectPool.Release(cacheGameObject);
                        }
                    }
                }

                cachedGameObjectsByInstanceId.Clear();
                cachedGameObjectsByCrc.Clear();
                ClearAllAsyncLoadTask();
            }
            else
            {
                if(cachedGameObjectsByCrc != null && cachedGameObjectsByCrc.Count > 0)
                {
                    foreach (var item in cachedGameObjectsByCrc)
                    {
                        var cacheGameObjectList = item.Value;
                        if (!cacheGameObjectList.IsNullOrEmpty())
                        {
                            foreach(var cacheGameObject in cacheGameObjectList)
                            {
                                GameObject.Destroy(cacheGameObject.gameObject);
                                cacheGameObject.Release();
                                cacheGameObjectPool.Release(cacheGameObject);
                            }
                        }
                    }
                }
                cachedGameObjectsByCrc.Clear();
            }

            if(loadedBundleItemsByCrc != null && loadedBundleItemsByCrc.Count > 0)
            {
                var assetBundleSystem = AssetBundleLoadSystem.Current;
                if(assetBundleSystem != null)
                {
                    foreach(var bundleItem in loadedBundleItemsByCrc.Values)
                    {
                        assetBundleSystem.ReleaseAsset(bundleItem, absoluteCleaning);
                    }
                }
            }

            loadedBundleItemsByCrc.Clear();
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
        #endregion

        /// <summary>
        /// 预加载GameObject
        /// </summary>
        public void PreLoadGameObject(string path, int count = 1)
        {
            if(count <= 0)
            {
                return;
            }

            for(int i = 0; i < count; i++)
            {
                var go = InstantiateByPath(path, null);
                ReleaseGameObject(go);
            }
        }

        /// <summary>
        /// 预加载资源
        /// </summary>
        public void PreLoadResource<T>(string path) where T : UnityEngine.Object
        {
            LoadResource<T>(path);
        }

        public void ClearAllAsyncLoadTask()
        {

        }

        private BundleItem GetOrNewBundleItemFromAssetDic(uint crc)
        {
            return loadedBundleItemsByCrc .TryGetValue(crc, out var bundleItem) ?
                   bundleItem :
                   new BundleItem { crc = crc , obj = null};
        }

        /// <summary>
        /// 实例化一个游戏对象
        /// </summary>
        private GameObject InstantiateGameObject(string path, Transform parent, GameObject gameObject)
        {
            gameObject = GameObject.Instantiate(gameObject, parent);
            var cacheObject = cacheGameObjectPool.Spawn();
            cacheObject.gameObject = gameObject;
            cacheObject.path = path;
            cacheObject.crc = Crc32.GetCrc32(path);
            cacheObject.instanceId = gameObject.GetInstanceID();

            cachedGameObjectsByInstanceId.Add(cacheObject.instanceId, cacheObject);
            return gameObject;
        }

        /// <summary>
        /// 从池中获取一个缓存的游戏对象
        /// </summary>
        private GameObject GetGameObjectFromCacheGameObjectPool(uint crc)
        {
            if(cachedGameObjectsByCrc.TryGetValue(crc, out var objectList))
            {
                if (!objectList.IsNullOrEmpty())
                {
                    var cacheObject = objectList[0];
                    objectList.RemoveAt(0);
                    return cacheObject.gameObject;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 缓存的GameObject
    /// </summary>
    public class CacheGameObject
    {
        public uint crc;
        public string path;
        public int instanceId;
        public GameObject gameObject;

        public void Release()
        {
            crc = 0;
            path = null;
            instanceId = 0;
            gameObject = null;
        }
    }
}

