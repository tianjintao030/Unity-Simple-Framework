using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace tjtFramework.UI
{
    public class UIManager : MonoBehaviour
    {
        /// <summary>/// 记录当前场景所有面板 /// </summary>
        public static Dictionary<Type, UIPanelBase> panel_dic = null;

        /// <summary>/// UI画布 /// </summary>
        public static Canvas Canvas { get; private set; } = null;

        /// <summary>/// UI事件系统 /// </summary>
        public static EventSystem EventSystem { get; private set; } = null;

        /// <summary>/// UIManager单例 /// </summary>
        public static UIManager Instance { get; private set; } = null;

        /// <summary>/// UI正交相机 /// </summary>
        public static Camera Camera { get; private set; } = null;

        /// <summary>/// UIRoot层级集合 /// </summary>
        private static Dictionary<UIConst.UIPanelLayer, RectTransform> layers = null;

        /// <summary>/// 获取指定UI层级 /// </summary>
        public static RectTransform GetLayer(UIConst.UIPanelLayer layer)
        {
            return layers[layer];
        }

        /// <summary>
        /// 初始化UI框架
        /// </summary>
        public static void Init()
        {
            panel_dic = new Dictionary<Type, UIPanelBase>();

            //克隆UIManager
            var obj = Resources.Load("UIRoot/UIRoot") as GameObject;
            Instance = GameObject.Instantiate(obj).AddComponent<UIManager>();
            Instance.name = nameof(UIManager);
            DontDestroyOnLoad(Instance);

            Canvas = Instance.GetComponentInChildren<Canvas>();
            EventSystem = Instance.GetComponentInChildren<EventSystem>();
            Camera = Instance.GetComponentInChildren<Camera>();

            //获取UIRoot所有层级
            layers = new Dictionary<UIConst.UIPanelLayer, RectTransform>();
            foreach (UIConst.UIPanelLayer layer in Enum.GetValues(typeof(UIConst.UIPanelLayer)))
            {
                layers.Add(layer, Canvas.transform.Find(layer.ToString()) as RectTransform);
            }

            Debug.Log("UIManager 初始化完成");
        }

        /// <summary>
        /// 打开一个面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public static void Open<T>(object data = null) where T : UIPanelBase
        {
            void openPanel(UIPanelBase panel)
            {
                //显示在所在层级的最前面
                panel.transform.SetAsLastSibling();

                panel.SetData(data);

                panel.OnUIEnable();
            }

            UIPanelBase clonePanel()
            {
                //查找面板路径
                var panel_root_path = "UI";
                var panel_sub_path = $"{typeof(T).Name}/{typeof(T).Name}.prefab";
                var panel_path = $"{panel_root_path}/{panel_sub_path}";

                //加载面板
                var obj = Resources.Load(panel_path);
                if (obj == null)
                {
                    Debug.LogError($"{panel_path} 路径下未找到面板{typeof(T).Name}");
                }

                //查找面板所在层级
                RectTransform layer = UIManager.GetLayer(UIConst.UIPanelLayer.Normal);//默认值

                var objects = typeof(T).GetCustomAttributes(typeof(UILayerAttribute), true);
                if (objects?.Length > 0)
                {
                    var layerAttr = objects[0] as UILayerAttribute;
                    layer = UIManager.GetLayer(layerAttr.layer);
                }

                //克隆面板
                var go = GameObject.Instantiate(obj, layer) as GameObject;
                go.name = typeof(T).Name;

                var new_panel = go.AddComponent<T>();

                return new_panel;
            }

            if (panel_dic.TryGetValue(typeof(T), out UIPanelBase panel))
            {
                //打开面板
                openPanel(panel);
            }
            else
            {
                //克隆面板
                panel = clonePanel();

                //加到字典中
                panel_dic.Add(typeof(T), panel);

                panel.OnUIAwake();

                //延迟一帧执行
                Instance.StartCoroutine(Invoke(panel.OnUIStart));

                openPanel(panel);
            }
        }

        /// <summary>
        /// 关闭一个面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Close<T>() where T : UIPanelBase
        {
            if (!panel_dic.TryGetValue(typeof(T), out UIPanelBase panel))
            {
                Debug.LogError($"场景中未找到{typeof(T).Name}面板");
            }
            else
            {
                Close(panel);
            }
        }

        /// <summary>
        /// 关闭面板的扩展方法
        /// </summary>
        /// <param name="panel"></param>
        public static void Close(UIPanelBase panel)
        {
            panel?.OnUIDisable();
        }


        public static void CloseAll()
        {
            foreach (var panel in panel_dic.Values)
            {
                UIManager.Close(panel);
            }
        }

        /// <summary>
        /// 销毁一个面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Destroy<T>() where T : UIPanelBase
        {
            if (!panel_dic.TryGetValue(typeof(T), out UIPanelBase panel))
            {
                Debug.LogError($"场景中未找到{typeof(T).Name}面板");
            }
            else
            {
                Destroy(panel);
            }
        }

        /// <summary>
        /// 销毁面板的扩展方法
        /// </summary>
        /// <param name="panel"></param>
        public static void Destroy(UIPanelBase panel)
        {
            panel?.OnUIDisable();
            panel?.OnUIDestroy();

            //移除脏数据
            if (panel_dic.ContainsKey(panel.GetType()))
            {
                panel_dic.Remove(panel.GetType());
            }
        }

        /// <summary>
        /// 删除所有面板
        /// </summary>
        public static void DestoryAll()
        {
            List<UIPanelBase> panels = new List<UIPanelBase>(panel_dic.Values);

            foreach (var panel in panels)
            {
                UIManager.Destroy(panel);
            }

            panel_dic.Clear();
            panels.Clear();
        }

        /// <summary>
        /// 获取场景中的一个面板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : UIPanelBase
        {
            if (panel_dic.TryGetValue(typeof(T), out UIPanelBase panel))
            {
                return panel as T;
            }
            else
            {
                Debug.LogError($"场景中未找到{typeof(T).Name}面板");
                return default(T);
            }
        }

        /// <summary>
        /// 延迟一帧执行
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        private static IEnumerator Invoke(Action callback)
        {
            yield return new WaitForEndOfFrame();

            callback?.Invoke();
        }

    }
}

