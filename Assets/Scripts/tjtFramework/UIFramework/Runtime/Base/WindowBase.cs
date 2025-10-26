using System.Collections;
using System.Collections.Generic;
using tjtFramework.Utiliy;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace tjtFramework.UI
{
    public class WindowBase : WindowBehaviour
    {
        /// <summary>
        /// Window所在UI层级
        /// </summary>
        public virtual UILayer Layer { get; set; } = UILayer.Normal;

        protected List<Button> buttonList = new();
        protected List<Toggle> toggleList = new();
        protected List<InputField> inputFieldList = new();

        #region 事件管理

        public void AddButtonClickListener(Button button, UnityAction action)
        {
            if(button != null)
            {
                if(!buttonList.Contains(button))
                {
                    buttonList.Add(button);
                }
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
            }
        }

        public void AddToggleClickListener(Toggle toggle, UnityAction<bool,Toggle> action)
        {
            if (toggle != null)
            {
                if (!toggleList.Contains(toggle))
                {
                    toggleList.Add(toggle);
                }
                toggle.onValueChanged.RemoveAllListeners();
                toggle.onValueChanged.AddListener((isOn) =>
                {
                    action?.Invoke(isOn, toggle);
                });
            }
        }

        public void AddInputFieldListener(InputField inputField, UnityAction<string> onChangeAction, UnityAction<string> onEndAction)
        {
            if(inputField != null)
            {
                if(!inputFieldList.Contains(inputField))
                {
                    inputFieldList.Add(inputField);
                }
                inputField.onValueChanged.RemoveAllListeners();
                inputField.onEndEdit.RemoveAllListeners();
                inputField.onValueChanged.AddListener(onChangeAction);
                inputField.onEndEdit.AddListener(onEndAction);
            }
        }

        public void RemoveAllButtonClickListener()
        {
            if(!buttonList.IsNullOrEmpty())
            {
                foreach (var item in buttonList)
                {
                    item.onClick.RemoveAllListeners();
                }
            }
        }

        public void RemoveAllToggleClickListener()
        {
            if (!toggleList.IsNullOrEmpty())
            {
                foreach (var item in toggleList)
                {
                    item.onValueChanged.RemoveAllListeners();
                }
            }
        }

        public void RemoveAllInputFieldListener()
        {
            if (!inputFieldList.IsNullOrEmpty())
            {
                foreach (var item in inputFieldList)
                {
                    item.onValueChanged.RemoveAllListeners();
                    item.onEndEdit.RemoveAllListeners();
                }
            }
        }

        #endregion

        #region 窗口数据
        public class WindowOpenDataBase{}

        private WindowOpenDataBase windowOpenData;

        public void SetData(WindowOpenDataBase windowOpenData)
        {
            this.windowOpenData = windowOpenData;
        }

        /// <summary>
        /// 获取窗口打开数据
        /// </summary>
        protected T GetWindowData<T>() where T : WindowOpenDataBase
        {
            if(windowOpenData == null)
            {
                return null;
            }

            return windowOpenData as T;
        }
        #endregion

        #region 生命周期
        public override void OnAwake()
        {
            base.OnAwake();
        }

        public override void OnShow()
        {
            base.OnShow();
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void OnHide()
        {
            base.OnHide();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            RemoveAllButtonClickListener();
            RemoveAllToggleClickListener();
            RemoveAllInputFieldListener();
            buttonList.Clear();
            toggleList.Clear();
            inputFieldList.Clear();
        }
        #endregion

        public override void SetVisible(bool visible)
        {
            base.SetVisible(visible);

            // 通过CanvasGroup的alpha值控制窗口的显隐，以避免UI网格重绘，来提高性能
            CanvasGroup.alpha = visible ? 1 : 0;
            CanvasGroup.blocksRaycasts = visible;
            Visible = visible;
        }

        /// <summary>
        /// 伪隐藏开关
        /// </summary>
        /// <param name="isHide"></param>
        public virtual void PseudoHidden(bool isHide)
        {
            CanvasGroup.alpha = isHide ? 0 : 1;
            CanvasGroup.blocksRaycasts = !isHide;
        }
    }
}

