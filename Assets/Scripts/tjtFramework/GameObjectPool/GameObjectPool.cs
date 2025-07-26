using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tjtFramework.Singleton;

namespace tjtFramework.Pool
{
    public interface IResetable
    {
        void OnReset();
    }
    /// <summary>
    /// 对象池
    /// </summary>
    public class GameObjectPool : MonoSingleton<GameObjectPool>
    {
        private Dictionary<string, List<GameObject>> _cache;

        public override void Init()
        {
            _cache = new Dictionary<string, List<GameObject>>();
        }
        public GameObject CreateObject(string key, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            GameObject go = FindUsableGameObject(key);
            if (go == null)
            {
                go = AddObject(key, prefab);
            }

            UseObject(position, rotation, go);

            if (parent != null)
            {
                go.transform.parent = parent;
            }
            return go;
        }

        public GameObject CreateObject(string key, Transform parent = null)
        {
            GameObject go = FindUsableGameObject(key);
            if (go == null)
            {
                go = new GameObject(key); // 创建一个新的空 GameObject
                AddObject(key, go);
            }
            UseObject(go.transform.position, go.transform.rotation, go);

            if (parent != null)
            {
                go.transform.parent = parent;
            }
            return go;
        }

        public GameObject CreateObject(string key)
        {
            GameObject go = FindUsableGameObject(key);
            if (go == null)
            {
                go = new GameObject(key); // 创建一个新的空 GameObject
                AddObject(key, go);
            }
            UseObject(go.transform.position, go.transform.rotation, go);
            return go;
        }

        public void CollectObject(GameObject go, float delay = 0)
        {
            if (delay == 0)
            {
                if (go != null)
                {
                    go.SetActive(false);
                }
            }
            else
            {
                StartCoroutine(CollectObjectDelay(go, delay));
            }

        }
        public void Clear(string key)
        {
            for (int i = _cache[key].Count - 1; i >= 0; i--)
            {
                Destroy(_cache[key][i]);
            }

            _cache.Remove(key);
        }
        public void ClearAll()
        {
            List<string> keyList = new List<string>(_cache.Keys);
            foreach (var key in keyList)
            {
                Clear(key);
            }
        }
        #region private
        private IEnumerator CollectObjectDelay(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null)
            {
                go.SetActive(false);
            }
        }

        private GameObject FindUsableGameObject(string key)
        {
            if (_cache.ContainsKey(key))
                return _cache[key].Find(g => !g.activeInHierarchy);
            else
                return null;
        }

        private GameObject AddObject(string key, GameObject prefab)
        {
            GameObject go = Instantiate(prefab);
            if (!_cache.ContainsKey(key))
            {
                _cache.Add(key, new List<GameObject>());
            }

            _cache[key].Add(go);
            return go;
        }

        private void UseObject(Vector3 position, Quaternion rotation, GameObject go)
        {
            go.transform.position = position;
            go.transform.rotation = rotation;

            go.SetActive(true);
            foreach (var item in go.GetComponents<IResetable>())
            {
                item.OnReset();
            }
        }
        #endregion
    }

}


