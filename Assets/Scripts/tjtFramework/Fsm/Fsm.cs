using System.Collections.Generic;
using System;

namespace tjtFramework.FSM
{
    /// <summary>
    /// 状态基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FsmState<T> where T : class
    {
        public virtual void OnEnter(T owner) { }
        public virtual void OnUpdate(T owner, float deltaTime) { }
        public virtual void OnLeave(T owner, bool isShutdown) { }
        public virtual void OnDestroy(T owner) { }
    }

    /// 有限状态机
    public class Fsm<T> where T : class
    {
        private T owner;
        private FsmState<T> currentState;
        private Dictionary<Type, FsmState<T>> mStates;
        private float currentStateTime;

        public Fsm(T owner, FsmState<T>[] states)
        {
            this.owner = owner;
            mStates = new Dictionary<Type, FsmState<T>>();

            foreach (var state in states)
            {
                mStates.Add(state.GetType(), state);
            }
        }

        public void Start<TState>() where TState : FsmState<T>
        {
            if (currentState != null)
            {
                currentState.OnLeave(owner, false);
            }

            if (mStates.TryGetValue(typeof(TState), out var newState))
            {
                currentState = newState;
                currentStateTime = 0f;
                currentState.OnEnter(owner);
            }
        }

        public void ChangeState<TState>() where TState : FsmState<T>
        {
            Start<TState>();
        }

        public void Update(float deltaTime)
        {
            if (currentState != null)
            {
                currentStateTime += deltaTime;
                currentState.OnUpdate(owner, deltaTime);
            }
        }

        public TState GetState<TState>() where TState : FsmState<T>
        {
            if (mStates.TryGetValue(typeof(TState), out var state))
            {
                return (TState)state;
            }
            return null;
        }

        public FsmState<T> CurrentState => currentState;
        public float CurrentStateTime => currentStateTime;
    }
}


