using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.GameAI.BehaviourTree
{
    /// <summary>
    /// 重复执行器
    /// </summary>
    public class RepeaterNode : BehaviourTreeDecoratorNode
    {
        public enum RepeatMode
        {
            FixedCount,     // 固定次数
            Infinite,       // 无限循环
            UntilFailure,   // 直到失败
            UntilSuccess    // 直到成功
        }

        public RepeatMode repeatMode;
        [Tooltip("FixedCount模式时使用")]
        public int repeatCount;
        private int currentCount = 0;

        protected override void OnStart()
        {
            currentCount = 0;
        }

        protected override void OnStop()
        {
            currentCount = 0;
        }

        protected override State OnUpdate()
        {
            if(child == null)
            {
                return State.Failure;
            }

            switch(repeatMode)
            {
                case RepeatMode.FixedCount:
                    if(currentCount < repeatCount)
                    {
                        var result1 = child.Update();
                        if(result1 == State.Failure ||  result1 == State.Success)
                        {
                            currentCount++;
                            // 重置子节点以便重新运行
                            child.Reset();
                        }
                        return currentCount >= repeatCount ? State.Success : State.Running;
                    }
                    return State.Success;
                case RepeatMode.Infinite:
                    var result2 = child.Update();
                    if (result2 == State.Success || result2 == State.Failure)
                    {
                        child.Reset();
                    }
                    return State.Running;
                case RepeatMode.UntilFailure:
                    var result3 = child.Update();
                    if(result3 == State.Failure)
                    {
                        return State.Success;
                    }
                    if(result3 == State.Success)
                    {
                        child.Reset();
                    }
                    return State.Running;
                case RepeatMode.UntilSuccess:
                    var result4 = child.Update();
                    if (result4 == State.Success)
                    {
                        return State.Success;
                    }
                    if (result4 == State.Failure)
                    {
                        child.Reset();
                    }
                    return State.Running;
            }

            return State.Failure;
        }
    }
}

