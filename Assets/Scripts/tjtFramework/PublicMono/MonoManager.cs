using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using tjtFramework.Singleton;

namespace tjtFramework.PublicMono
{
    /// <summary>
    /// 公共Mono：
    /// 1.统一管理Mono帧更新或定时更新生命周期函数
    /// 2.让不继承MonoBehavior的类可以执行帧更新或定时更新，协同程序
    /// </summary>
    public class MonoManager : MonoSingleton<MonoManager>
    {
        private event UnityAction update_event;
        private event UnityAction fixedupdate_event;
        private event UnityAction lateupdate_event;

        private void Update()
        {
            update_event?.Invoke();
        }

        private void FixedUpdate()
        {
            fixedupdate_event?.Invoke();
        }

        private void LateUpdate()
        {
            lateupdate_event?.Invoke();
        }

        public void AddUpdateListener(UnityAction action)
        {
            update_event += action;
        }

        public void RemoveUpdateListener(UnityAction action)
        {
            update_event -= action;
        }

        public void AddFixedUpdateListener(UnityAction action)
        {
            fixedupdate_event += action;
        }

        public void RemoveFixedUpdateListener(UnityAction action)
        {
            fixedupdate_event -= action;
        }

        public void AddLateUpdateListener(UnityAction action)
        {
            lateupdate_event += action;
        }

        public void RemoveLateUpdateListener(UnityAction action)
        {
            lateupdate_event -= action;
        }
    }

}

