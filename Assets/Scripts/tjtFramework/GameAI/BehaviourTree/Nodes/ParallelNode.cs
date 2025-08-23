using System.Collections;
using System.Collections.Generic;
using System.Linq;
using tjtFramework.Utiliy;
using UnityEngine;

namespace tjtFramework.GameAI.BehaviourTree
{
    /// <summary>
    /// 并行器
    /// </summary>
    public class ParallelNode : BehaviourTreeCompositeNode
    {
        [SerializeField, ReadOnlyInInspector]
        private List<State> childStates = new();

        public ParallelPolicy policy;

        [Tooltip("仅 NofMSuccess 使用")]
        public int successThreshold = 0;

        protected override void OnStart()
        {
            ResetChildren();
            childStates.Clear();
            if(!children.IsNullOrEmpty())
            {
                foreach(var child in children)
                {
                    childStates.Add(State.Invalid);
                }
            }
        }

        protected override void OnStop()
        {
            ResetChildren();
            if (!children.IsNullOrEmpty())
            {
                for(int i=0;i<childStates.Count;i++)
                {
                    childStates[i] = State.Invalid;
                }
            }
        }

        protected override State OnUpdate()
        {
            if(children.IsNullOrEmpty())
            {
                return State.Success;
            }

            var successCount = 0;

            for(int i=0;i<children.Count;i++)
            {
                if (childStates[i] == State.Success || childStates[i] == State.Failure)
                {
                    continue;
                }

                var result = children[i].Update();
                childStates[i] = result;

                switch(policy)
                {
                    case ParallelPolicy.FailOnAnyFailure_AllSuccess:
                        if(result == State.Failure)
                        {
                            return State.Failure;
                        }
                        break;
                    case ParallelPolicy.SuccessOnAny:
                        if(result == State.Success)
                        {
                            return State.Success;
                        }
                        break;
                    case ParallelPolicy.NofMSuccess:
                        if (result == State.Success)
                        {
                            successThreshold++;
                        }
                        break;
                }
            }

            switch(policy)
            {
                case ParallelPolicy.FailOnAnyFailure_AllSuccess:
                    return childStates.All(s => s == State.Success) ? State.Success : State.Running;

                case ParallelPolicy.SuccessOnAny:
                    return childStates.All(s => s == State.Failure) ? State.Failure : State.Running;

                case ParallelPolicy.NofMSuccess:
                    return successCount >= successThreshold ? State.Success : State.Running;
            }

            return State.Running;
        }
    }

    /// <summary>
    /// 并行器执行策略
    /// </summary>
    public enum ParallelPolicy
    {
        /// <summary>
        /// FailFast + AllSuccess
        /// 任意子节点失败 → 并行器失败，
        /// 所有子节点成功 → 并行器成功
        /// 其他情况 → Running
        /// </summary>
        FailOnAnyFailure_AllSuccess,

        /// <summary>
        /// SuccessOnAny
        /// 任意子节点成功 → 并行器成功，
        /// 所有子节点失败 → 并行器失败
        /// 其他情况 → Running
        /// </summary>
        SuccessOnAny,

        /// <summary>
        /// N-of-M Success
        /// 指定至少 N 个子节点成功 → 并行器成功，
        /// 任意子节点失败达到阈值 → 并行器失败
        /// </summary>
        NofMSuccess
    }

}

