using System;
using System.Collections;
using System.Collections.Generic;
using tjtFramework.Singleton;
using tjtFramework.Utiliy;
using UnityEngine;

namespace tjtFramework.UI
{
    public class UISystem : NoMonoSingleton<UISystem>
    {
        public UISystem() { }

        private Camera uiCamera;
        private Transform uiRoot;

        private Dictionary<string, WindowBase> allWindowDic = new();
        private List<WindowBase> allWindowList = new();
        private List<WindowBase> visibleWindowList = new();

        private Dictionary<UILayer, Transform> layers = new();

        public void Init()
        {
            uiCamera = GameObject.Find("UICamera").GetComponent<Camera>();
            uiRoot = GameObject.Find("UIRoot").transform;
            InitLayers();
        }

        private void InitLayers()
        {
            if(uiRoot == null)
            {
                Debug.LogError("未找到uiRoot,无法初始化各层级");
                return;
            }

            layers.Clear();

            var layerIndex = 0;
            foreach(UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                var layerGo = new GameObject(layer.ToString());
                layerGo.transform.SetParent(uiRoot);
                layerGo.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                layerGo.transform.SetAsLastSibling();

                var canvas = layerGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = uiCamera;
                canvas.overrideSorting = true;
                canvas.sortingOrder = layerIndex * 100;

                layers.Add(layer, layerGo.transform);

                layerIndex++;
            }
        }

        #region 窗口管理
        /// <summary>
        /// 弹出UI Window
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>界面类</returns>
        public T PopUpWindow<T>(WindowBase.WindowOpenDataBase openData = null) where T : WindowBase, new()
        {
            System.Type type = typeof(T);
            var windowName = type.Name;
            var window = GetWindow(windowName);

            if(window != null)
            {
                return ShowWindow(windowName) as T;
            }

            T t = new T();
            return InitWindow(windowName, t) as T;
        }

        private WindowBase PopUpWindow(WindowBase windowBase)
        {
            System.Type type = windowBase.GetType();
            var windowName = type.Name;
            var window = GetWindow(windowName);

            if (window != null)
            {
                return ShowWindow(windowName);
            }

            return InitWindow(windowName, windowBase);
        }

        /// <summary>
        /// 预加载窗口
        /// </summary>
        public void PreLoadWindow<T>() where T : WindowBase, new()
        {
            System.Type type = typeof (T);
            var windowName = type.Name;
            T windowBase = new T();

            var newWindow = LoadWindow(windowName);
            if(newWindow != null)
            {
                windowBase.gameObject = newWindow;
                windowBase.transform = newWindow.transform;
                windowBase.Name = newWindow.name;
                windowBase.Canvas = newWindow.GetComponent<Canvas>();
                windowBase.Canvas.worldCamera = uiCamera;
                var canvasGroup = newWindow.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = newWindow.AddComponent<CanvasGroup>();
                }
                windowBase.CanvasGroup = canvasGroup;

                if (layers.TryGetValue(windowBase.Layer, out var layerRoot))
                {
                    newWindow.transform.SetParent(layerRoot);
                    windowBase.transform.SetAsLastSibling();
                    windowBase.Canvas.overrideSorting = true;
                    windowBase.Canvas.sortingOrder = (int)windowBase.Layer * 100;
                }

                // 设为不可见
                windowBase.OnAwake();
                windowBase.SetVisible(false);

                var windowRect = newWindow.GetComponent<RectTransform>();
                windowRect.anchorMax = Vector2.one;
                windowRect.offsetMax = Vector2.zero;
                windowRect.offsetMin = Vector2.zero;

                allWindowDic.Add(windowName, windowBase);
                allWindowList.Add(windowBase);

                Debug.Log($"预加载窗口：{windowName}");
            }
        }

        /// <summary>
        /// 获取已打开的Window
        /// </summary>
        public T GetVisibleWindow<T>() where T : WindowBase
        {
            System.Type type = typeof(T);

            if (!visibleWindowList.IsNullOrEmpty())
            {
                foreach (var item in visibleWindowList)
                {
                    if (item.Name == type.Name)
                    {
                        return (T)item;
                    }
                }
            }

            Debug.LogError($"未找到可见的界面:{type.Name}");
            return null;
        }

        /// <summary>
        /// 关闭UI Window
        /// </summary>
        public void HideWindow<T>() where T : WindowBase
        {
            HideWindow(typeof(T).Name);
        }

        /// <summary>
        /// 销毁UI Window
        /// </summary>
        public void DestoryWindow<T>() where T : WindowBase
        {
            DestoryWindow(typeof(T).Name);
        }

        /// <summary>
        /// 销毁所有UI Window
        /// </summary>
        /// <param name="filterList">过滤列表</param>
        public void DestoryAllWindow(List<string> filterList = null)
        {
            if(!allWindowList.IsNullOrEmpty())
            {
                for(int i = allWindowList.Count - 1; i >= 0; i--)
                {
                    var window = allWindowList[i];
                    if(window == null ||
                        (filterList != null && filterList.Contains(window.Name)))
                    {
                        continue;
                    }

                    DestoryWindow(window);
                    Resources.UnloadAsset(window.gameObject);
                }
            }
        }

        /// <summary>
        /// 创建并初始化UI Window
        /// </summary>
        private WindowBase InitWindow(string windowName, WindowBase windowBase, WindowBase.WindowOpenDataBase openData = null)
        {
            var newWindow = LoadWindow(windowName);

            if(newWindow != null)
            {
                // 参数赋值
                windowBase.gameObject = newWindow;
                windowBase.transform = newWindow.transform;
                windowBase.Name = newWindow.name;
                windowBase.Canvas = newWindow.GetComponent<Canvas>();
                windowBase.Canvas.worldCamera = uiCamera;
                var canvasGroup = newWindow.GetComponent<CanvasGroup>();
                if(canvasGroup == null)
                {
                    canvasGroup = newWindow.AddComponent<CanvasGroup>();
                }
                windowBase.CanvasGroup = canvasGroup;

                // 设置层级
                if(layers.TryGetValue(windowBase.Layer, out var layerRoot))
                {
                    newWindow.transform.SetParent(layerRoot);
                    windowBase.transform.SetAsLastSibling();
                    windowBase.Canvas.overrideSorting = true;
                    windowBase.Canvas.sortingOrder = (int)windowBase.Layer * 100;
                }

                // 位置重置
                var windowRect = newWindow.GetComponent<RectTransform>();
                windowRect.anchorMax = Vector2.one;
                windowRect.offsetMax = Vector2.zero;
                windowRect.offsetMin = Vector2.zero;

                // 生命周期
                if(openData != null)
                {
                    windowBase.SetData(openData);
                }
                windowBase.OnAwake();
                windowBase.SetVisible(true);
                OnShowFullScreenWindow(windowBase);
                windowBase.OnShow();

                // 列表缓存
                allWindowDic.Add(windowName, windowBase);
                allWindowList.Add(windowBase);
                visibleWindowList.Add(windowBase);

                return windowBase;
            }
            else
            {
                Debug.LogError($"加载{windowName}失败,检查其预制路径");
                return null;
            }
        }

        private WindowBase ShowWindow(string windowName, WindowBase.WindowOpenDataBase openData = null)
        {
            WindowBase window = null;

            if(allWindowDic.ContainsKey(windowName))
            {
                window = allWindowDic[windowName];
                if(!window.Visible && window.gameObject != null)
                {
                    visibleWindowList.Add(window);
                    window.transform.SetAsLastSibling();

                    if(openData != null)
                    {
                        window.SetData(openData);
                    }
                    
                    window.SetVisible(true);
                    OnShowFullScreenWindow(window);
                    window.OnShow();
                }
            }
            else
            {
                Debug.LogError($"{windowName}未弹出过，不能直接Show，需使用PopUpWindow");
            }

            return window;
        }

        private void HideWindow(string windowName)
        {
            var window = GetWindow(windowName);
            HideWindow(window);
        }

        private void HideWindow(WindowBase window)
        {
            if(window != null && window.Visible)
            {
                visibleWindowList.Remove(window);
                OnHideFullScreenWindow(window);
                window.SetVisible(false);
                window.OnHide();

                window.SetData(null);

                if(window.PopStack)
                {
                    // 若是堆栈中的界面，在关闭时弹出堆栈中下一个界面
                    PopNextStackWindow(window);
                }
            }
        }

        private WindowBase GetWindow(string windowName)
        {
            if(allWindowDic.ContainsKey(windowName))
            {
                return allWindowDic[windowName];
            }
            return null;
        }

        private GameObject LoadWindow(string windowName)
        {
            var windowObject = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>($"Window/{windowName}"));
            windowObject.transform.SetParent(uiRoot);
            windowObject.transform.localPosition = Vector3.zero;
            windowObject.transform.localRotation = Quaternion.identity;
            windowObject.transform.localScale = Vector3.one;
            // Instantiate出来的GameObject会带有(Clone)后缀,所以要重新赋名字
            windowObject.name = windowName;
            return windowObject;
        }

        private void DestoryWindow(string windowName)
        {
            var window = GetWindow(windowName);
            DestoryWindow(window);
        }

        private void DestoryWindow(WindowBase window)
        {
            if(window != null)
            {
                if(allWindowDic.ContainsKey(window.Name))
                {
                    allWindowDic.Remove(window.Name);
                    allWindowList.Remove(window);
                    visibleWindowList.Remove(window);
                }
                OnHideFullScreenWindow(window);
                window.SetVisible(false);
                window.OnHide();
                window.OnDestroy();

                window.SetData(null);

                if (window.PopStack)
                {
                    // 若是堆栈中的界面，在销毁时弹出堆栈中下一个界面
                    PopNextStackWindow(window);
                }

                GameObject.Destroy(window.gameObject);
            }
        }
        #endregion

        #region 帧更新
        public void OnUpdate()
        {
            if(visibleWindowList.Count > 0)
            {
                foreach(var window in visibleWindowList)
                {
                    if(window != null && window.NeedUpdate)
                    {
                        window.OnUpdate();
                    }
                }
            }
        }
        #endregion

        #region 堆栈系统
        /// <summary>
        /// 管理弹出循环弹出的窗口队列
        /// </summary>
        private Queue<WindowBase> windowQueue = new();
        private bool startPopupStatus = false;

        /// <summary>
        /// 进栈一个界面
        /// </summary>
        public void PushWindowToStack<T>(Action<WindowBase> onShow) where T : WindowBase, new()
        {
            T window = new T();
            window.PopStackListener = onShow;
            windowQueue.Enqueue(window);
        }

        /// <summary>
        /// 压入并弹出堆栈弹窗
        /// </summary>
        public void PushAndPopStackWindow<T>(Action<WindowBase> onShow) where T : WindowBase, new()
        {
            PushWindowToStack<T>(onShow);
            PopFirstStackWindow();
        }

        /// <summary>
        /// 弹出堆栈中第一个界面
        /// </summary>
        public void PopFirstStackWindow()
        {
            if(startPopupStatus)
            {
                return;
            }

            startPopupStatus = true;
            PopStackWindow();
        }

        /// <summary>
        /// 弹出堆栈中下一个界面
        /// </summary>
        private void PopNextStackWindow(WindowBase window)
        {
            if(window != null && 
                startPopupStatus && 
                window.PopStack)
            {
                window.PopStack = false;
                PopStackWindow();
            }
        }

        private bool PopStackWindow()
        {
            if (windowQueue.Count > 0)
            {
                var window = windowQueue.Dequeue();
                var popWindow = PopUpWindow(window);
                if(popWindow == null)
                {
                    PopStackWindow();
                    return false;
                }

                popWindow.PopStackListener = window.PopStackListener;
                popWindow.PopStack = true;
                popWindow.PopStackListener?.Invoke(popWindow);

                return true;
            }
            else
            {
                startPopupStatus = false;
                return false;
            }
        }

        #endregion

        #region 智能显隐（全屏界面存在时）
        //1.隐藏：当一个界面为全屏界面时，采用伪隐藏的方式隐藏被全屏界面遮挡的界面，避免这些看不到的界面参与渲染
        //2.显示：当全屏界面关闭时，找到上一个伪隐藏的界面将其设为可见，如果上一个伪隐藏的界面不是全屏窗口，则会再往前找一层界面，直到找到全屏界面
        //伪隐藏：仅在可见性上隐藏，界面逻辑照常

        /// <summary>
        /// 处理全屏界面打开
        /// </summary>
        private void OnShowFullScreenWindow(WindowBase window)
        {
            if(window == null || 
                !window.FullScreenWindow ||
                window.IsNotPseudoHideBelow)
            {
                return;
            }

            if(visibleWindowList.Count > 0)
            {
                foreach(var visibleWindow in visibleWindowList)
                {
                    if((int)visibleWindow.Layer < (int)window.Layer || 
                        visibleWindow.Canvas.sortingOrder < window.Canvas.sortingOrder)
                    {
                        visibleWindow.PseudoHidden(true);
                    }
                }
            }
        }

        /// <summary>
        /// 处理全屏界面关闭
        /// </summary>
        private void OnHideFullScreenWindow(WindowBase window)
        {
            if (window == null ||
                !window.FullScreenWindow ||
                window.IsNotPseudoHideBelow)
            {
                return;
            }

            if(visibleWindowList.Count > 0)
            {
                for(int i = visibleWindowList.Count - 1; i >= 0; i--)
                {
                    if (((int)visibleWindowList[i].Layer < (int)window.Layer ||
                        visibleWindowList[i].Canvas.sortingOrder < window.Canvas.sortingOrder) &&
                        visibleWindowList[i].Name != window.Name)
                    {
                        visibleWindowList[i].PseudoHidden(false);

                        //找到遮挡下的下一个全屏界面，若没有则循环到最终
                        if (visibleWindowList[i].FullScreenWindow)
                        {
                            break;
                        }
                    }
                }
            }
        }
        #endregion
    }
}

