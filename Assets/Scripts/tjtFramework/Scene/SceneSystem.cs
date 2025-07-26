using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using tjtFramework.Singleton;

namespace tjtFramework.Scene
{
    public class SceneSystem : MonoSingleton<SceneSystem>
    {
        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callBack"></param>
        public void LoadScene(string name, UnityAction callBack = null)
        {
            SceneManager.LoadScene(name);
            callBack?.Invoke();
        }

        /// <summary>
        /// 可等待异步加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="onProgress"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public async Task LoadSceneAsync(string sceneName, Action<float> onProgress = null, UnityAction onComplete = null)
        {
            AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName);
            ao.allowSceneActivation = true;

            while (!ao.isDone)
            {
                float progress = Mathf.Clamp01(ao.progress / 0.9f); // 因为 progress 最大到 0.9
                onProgress?.Invoke(progress);

                // 场景加载完成前，progress 只会到 0.9
                await Task.Yield();
            }

            // 最后一次调用进度回调，确保是 1.0f
            onProgress?.Invoke(1f);

            onComplete?.Invoke();
        }

        /// <summary>
        /// 可等待异步叠加加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="onProgress"></param>
        /// <param name="onComplete"></param>
        /// <returns></returns>
        public async Task LoadSceneAsyncAdditive(
        string sceneName,
        Action<float> onProgress = null,
        UnityAction onComplete = null)
        {
            AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            ao.allowSceneActivation = true;

            while (!ao.isDone)
            {
                float progress = Mathf.Clamp01(ao.progress / 0.9f);
                onProgress?.Invoke(progress);
                await Task.Yield();
            }

            onProgress?.Invoke(1f);
            onComplete?.Invoke();
        }


    }
}


