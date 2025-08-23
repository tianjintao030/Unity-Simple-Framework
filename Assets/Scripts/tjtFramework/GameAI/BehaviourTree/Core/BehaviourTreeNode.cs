using System.ComponentModel;
using UnityEngine;

namespace tjtFramework.GameAI.BehaviourTree
{
    /// <summary>
    /// 行为树节点基类
    /// </summary>
    public abstract class BehaviourTreeNode : ScriptableObject
    {
        public enum State
        {
            /// <summary>
            /// 节点未运行/初始状态
            /// </summary>
            Invalid,
            /// <summary>
            /// 正在运行
            /// </summary>
            Running,
            /// <summary>
            /// 失败
            /// </summary>
            Failure,
            /// <summary>
            /// 成功
            /// </summary>
            Success
        }

        [ReadOnlyInInspector]
        public string guid;
        [ReadOnlyInInspector]
        public Vector2 position;

        protected State state = State.Invalid;
        public bool IsRunning => state == State.Running;
        public bool IsSuccess => state == State.Success;
        public bool IsFailure => state == State.Failure;
        /// <summary>
        /// 是否运行结束
        /// </summary>
        public bool IsTerminated => IsFailure || IsSuccess;
        [ReadOnlyInInspector]
        public bool hasStarted = false;

        public string description;

        public State Update()
        {
            if(!hasStarted)
            {
                hasStarted = true;
                OnStart();
            }

            state = OnUpdate();

            if(IsTerminated)
            {
                OnStop();
                hasStarted = false;
            }

            return state;
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
        protected abstract State OnUpdate();

        public virtual BehaviourTreeNode Clone()
        {
            BehaviourTreeNode node = Instantiate(this) as BehaviourTreeNode;
            return node;
        }

        public void Reset()
        {
            state = State.Invalid;
            hasStarted = false;
        }
    }
}

