using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.UI
{
    public abstract class WindowBehaviour
    {
        public GameObject gameObject {  get; set; }

        public Transform transform { get; set; }

        public Canvas Canvas { get; set; }

        public CanvasGroup CanvasGroup { get; set; }

        public string Name {  get; set; }

        public bool Visible { get; set; }

        /// <summary>
        /// 是否是全屏界面
        /// </summary>
        public bool FullScreenWindow { get; set; }

        /// <summary>
        /// 仅在FullScreenWindow为true时使用，是否不伪隐藏遮挡的窗口
        /// （默认是伪隐藏被全屏界面遮挡的界面）
        /// </summary>
        public bool IsNotPseudoHideBelow {  get; set; }

        /// <summary>
        /// 是否需要调用OnUpdate
        /// </summary>
        public bool NeedUpdate {  get; set; }

        public virtual void OnAwake() { }

        public virtual void OnShow() { }

        public virtual void OnUpdate() { }

        public virtual void OnHide() { }

        public virtual void OnDestroy() { }

        public virtual void SetVisible(bool visible) { }

        /// <summary>
        /// 是否是堆栈系统弹出
        /// </summary>
        public bool PopStack {  get; set; }

        public Action<WindowBase> PopStackListener { get; set; }
    }
}
