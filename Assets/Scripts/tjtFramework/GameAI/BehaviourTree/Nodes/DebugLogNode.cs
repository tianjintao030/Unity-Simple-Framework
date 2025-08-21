using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tjtFramework.GameAI.BehaviourTree
{
    public class DebugLogNode : BehaviourTreeActionNode
    {
        [SerializeField]
        private string message;

        protected override void OnStart()
        {
            Debug.Log(message);
        }

        protected override void OnStop()
        {
            
        }

        protected override State OnUpdate()
        {
            return state;
        }
    }
}

