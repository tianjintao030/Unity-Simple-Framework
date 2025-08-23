using System.Collections;
using System.Collections.Generic;
using tjtFramework.Utiliy;
using UnityEngine;

namespace tjtFramework.GameAI.BehaviourTree
{
    /// <summary>
    /// 顺序器
    /// 依次执行childrenNode，有一个失败即失败，一个成功了就继续执行下一个，直到children都执行成功
    /// </summary>
    public class SequenceNode : BehaviourTreeCompositeNode
    {
        /// <summary>
        /// 当前运行子节点的索引
        /// </summary>
        [ReadOnlyInInspector]
        public int currentChildIndex;

        protected override void OnStart()
        {
            currentChildIndex = 0;

            ResetChildren();
        }

        protected override void OnStop()
        {
            currentChildIndex = 0;

            ResetChildren();
        }

        protected override State OnUpdate()
        {
            if(children.IsNullOrEmpty())
            {
                return State.Failure;
            }

            if(currentChildIndex >= children.Count)
            {
                return State.Success;
            }

            var currentNode = children[currentChildIndex];
            var result = currentNode.Update();

            switch (result)
            {
                case State.Running:
                    return State.Running;

                case State.Failure:
                    return State.Failure;

                case State.Success:
                    currentChildIndex++;
                    return currentChildIndex == children.Count ? State.Success : State.Running;
            }

            return State.Failure;
        }
    }
}

