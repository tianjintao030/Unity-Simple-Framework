using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using tjtFramework.AssetFramework.GameResources;
using UnityEngine;

namespace tjtFramework.Utiliy
{
    public static class ResourcesUtility
    {
        public static void PreLoadGameObject(string path, int count = 1)
        {
            if (ResourcesSystem.Current == null)
            {
                return;
            }
            ResourcesSystem.Current.PreLoadGameObject(path, count);
        }

        public static void PreLoadResource<T>(string path) where T : UnityEngine.Object
        {
            if (ResourcesSystem.Current == null)
            {
                return;
            }
            ResourcesSystem.Current.PreLoadResource<T>(path);
        }


        public static GameObject Instantiate(string path, Transform parent = null)
        {
            if (ResourcesSystem.Current == null)
            {
                return null;
            }
            return ResourcesSystem.Current.InstantiateByPath(path, parent);
        }


        public static async UniTask InstantiateAsync(string path, Transform parent = null, Action<GameObject, object, object> callback = null,
                                                                object callbackParam1 = null, object callbackParam2 = null)
        {
            if (ResourcesSystem.Current == null)
            {
                return;
            }
            await ResourcesSystem.Current.InstantiateByPathAsync(path, parent, callback, callbackParam1, callbackParam2);
        }

        public static void ReleaseGameObject(GameObject go, bool destroy = false)
        {
            if (ResourcesSystem.Current == null)
            {
                return;
            }
            ResourcesSystem.Current.ReleaseGameObject(go, destroy);
        }

        public static Sprite LoadSprite(string path)
        {
            if (ResourcesSystem.Current == null)
            {
                return null;
            }
            return ResourcesSystem.Current.LoadResource<Sprite>(path);
        }

        //public static Sprite LoadSpriteFromAtlas(string atlasPath, string spriteName)
        //{
        //    if (ResourcesSystem.Current == null)
        //    {
        //        return null;
        //    }
            
        //}

        //public static Texture LoadTexture(string path)
        //{

        //}

        //public static AudioClip LoadAudio(string path)
        //{

        //}

        public static async UniTask LoadSpriteAsync(string path, Action<UnityEngine.Object> loadFinshCallback = null)
        {

        }

        public static async UniTask LoadTextureAsync(string path, Action<UnityEngine.Object> loadFinshCallback = null)
        {

        }


        public static async UniTask LoadAudioAsync(string path, Action<UnityEngine.Object> loadFinshCallback = null)
        {

        }


        public static void ClearAllAsyncLoadTask()
        {

        }

        /// <summary>
        /// 清空资源
        /// </summary>
        /// <param name="absoluteClean">是否深度清理</param>
        public static void ClearResourceAssets(bool absoluteClean = false)
        {

        }
    }
}

