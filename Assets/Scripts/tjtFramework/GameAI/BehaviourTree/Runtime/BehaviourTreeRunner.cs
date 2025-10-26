using UnityEngine;
using tjtFramework.PublicMono;

namespace tjtFramework.GameAI.BehaviourTree
{
    public class BehaviourTreeRunner : MonoBehaviour
    {
        [SerializeField]
        private BehaviourTree tree;

        void Start()
        {
            tree =  tree.Clone();
            MonoManager.Instance.AddUpdateListener(TreeUpdate);
        }

        private void TreeUpdate()
        {
            tree.Update();
        }
    }
}

