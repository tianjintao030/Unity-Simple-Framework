using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tjtFramework.FSM;
using tjtFramework.Singleton;
using tjtFramework.PublicMono;

namespace tjtFramework.Procedure
{
    public class ProcedureManager : MonoSingleton<ProcedureManager>
    {
        private Fsm<ProcedureManager> fsm;

        public void Initialize(params ProcedureBase[] procedures)
        {
            fsm = new Fsm<ProcedureManager>(this, procedures);
            MonoManager.Instance?.AddUpdateListener(FsmUpdate);
        }

        public void StartProcedure<T>() where T : ProcedureBase
        {
            fsm?.Start<T>();
        }

        public void ChangeProcedure<T>() where T : ProcedureBase
        {
            fsm?.ChangeState<T>();
        }

        public ProcedureBase CurrentProcedure
        {
            get
            {
                if (fsm == null) return null;
                return (ProcedureBase)fsm.CurrentState;
            }
        }

        private void FsmUpdate()
        {
            fsm?.Update(Time.deltaTime);
        }

        void OnDestroy()
        {
            fsm = null;
        }
    }

    // 流程基类
    public abstract class ProcedureBase : FsmState<ProcedureManager>
    {
        public abstract override void OnEnter(ProcedureManager owner);

        public abstract override void OnUpdate(ProcedureManager owner, float deltaTime);

        public abstract override void OnLeave(ProcedureManager owner, bool isShutdown);

    }
}

