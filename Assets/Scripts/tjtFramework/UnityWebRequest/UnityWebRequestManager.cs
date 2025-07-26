using System;
using System.Collections;
using System.Collections.Generic;
using tjtFramework.Singleton;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace tjtFramework.UWQ
{
    public class UnityWebRequestManager : MonoSingleton<UnityWebRequestManager>
    {
        /// <summary>
        /// 只用于加载string，byte[]，Texture和AssetBundle
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">资源路径，要加协议</param>
        /// <param name="successCallBack"></param>
        /// <param name="failCallBack"></param>
        public void LoadRes<T>(string path, UnityAction<T> successCallBack, UnityAction failCallBack) where T : class
        {
            StartCoroutine(ReallyLoadRes<T>(path, successCallBack, failCallBack));
        }

        private IEnumerator ReallyLoadRes<T>(string path, UnityAction<T> successCallBack, UnityAction failCallBack) where T : class
        {
            //只用于加载字符串，字节，图片和AssetBundle
            Type type = typeof(T);
            UnityWebRequest req = null;
            if (type == typeof(string) || type == typeof(byte[]))
            {
                req = UnityWebRequest.Get(path);
            }
            else if (type == typeof(Texture))
            {
                req = UnityWebRequestTexture.GetTexture(path);
            }
            else if (type == typeof(AssetBundle))
            {
                req = UnityWebRequestAssetBundle.GetAssetBundle(path);
            }
            else
            {
                failCallBack?.Invoke();
                Debug.LogError($"UnityWebRequest加载{path}资源类型不符合");
                yield break;
            }

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                if (type == typeof(string))
                {
                    successCallBack?.Invoke(req.downloadHandler.text as T);
                }
                else if (type == typeof(byte[]))
                {
                    successCallBack?.Invoke(req.downloadHandler.data as T);
                }
                else if (type == typeof(Texture))
                {
                    successCallBack?.Invoke(DownloadHandlerTexture.GetContent(req) as T);
                }
                else if (type == typeof(AssetBundle))
                {
                    successCallBack?.Invoke(DownloadHandlerAssetBundle.GetContent(req) as T);
                }
            }
            else
            {
                failCallBack?.Invoke();
            }

            req.Dispose();
        }
    }
}


