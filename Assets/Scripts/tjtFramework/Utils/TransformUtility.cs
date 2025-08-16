using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.Utiliy
{
    public static class TransformUtility
    {
        /// <summary>
        /// 在自身及向上遍历父节点寻找组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="child"></param>
        /// <returns></returns>
        public static T FindComponentInParents<T>(Transform child)
        {
            Transform current = child;
            //向上遍历父节点
            while (current != null)
            {
                T component = current.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }

                current = current.parent;//移动到父节点
            }

            return default(T);
        }

        /// <summary>
        /// 遍历子节点找到第一个对应的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T FindFirstComponentInChildren<T>(Transform parent)
        {
            //深度优先搜索
            foreach (Transform child in parent)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }

                component = FindFirstComponentInChildren<T>(child);
                if (component != null)
                {
                    return component;
                }
            }

            return default(T);
        }

        /// <summary>
        /// 遍历子节点找到第一个对应的组件(包括自身)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T FindFirstComponentInChildrenIncludingSelf<T>(Transform parent)
        {
            // 先检查 parent 自身
            T component = parent.GetComponent<T>();
            if (component != null)
                return component;

            // 再递归检查子物体
            foreach (Transform child in parent)
            {
                component = FindFirstComponentInChildrenIncludingSelf<T>(child);
                if (component != null)
                    return component;
            }

            return default(T);
        }

        /// <summary>
        /// 遍历子节点找到所有对应组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static List<T> FindAllComponentsInChildren<T>(Transform parent)
        {
            List<T> components = new List<T>();

            // 深度优先搜索获取所有组件
            foreach (Transform child in parent)
            {
                T component = child.GetComponent<T>();
                if (component != null)
                {
                    components.Add(component);
                }

                components.AddRange(FindAllComponentsInChildren<T>(child));
            }

            return components;
        }

        /// <summary>
        /// 遍历自身及子节点，找到所有对应组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static List<T> FindAllComponentsInChildrenIncludingSelf<T>(Transform parent)
        {
            List<T> components = new List<T>();

            // 先检查自身
            T component = parent.GetComponent<T>();
            if (component != null)
            {
                components.Add(component);
            }

            // 再递归检查子物体
            foreach (Transform child in parent)
            {
                components.AddRange(FindAllComponentsInChildrenIncludingSelf<T>(child));
            }

            return components;
        }

    }
}
