using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlasticGui.LaunchDiffParameters;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace tjtFramework.GameAI.BehaviourTree
{
    /// <summary>
    /// 行为树
    /// </summary>
    [CreateAssetMenu(menuName = "BehaviourTree")]
    public class BehaviourTree : ScriptableObject
    {
        public BehaviourTreeNode rootNode;
        public BehaviourTreeNode.State treeState;
        public List<BehaviourTreeNode> nodes = new();

        public BehaviourTreeNode.State Update()
        {
            return rootNode.Update();
        }

#if UNITY_EDITOR
        public BehaviourTreeNode CreateNode(System.Type type)
        {
            BehaviourTreeNode node = ScriptableObject.CreateInstance(type) as BehaviourTreeNode;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            nodes.Add(node);

            AssetDatabase.AddObjectToAsset(node, this);
            AssetDatabase.SaveAssets();

            return node;
        }

        public void DeleteNode(BehaviourTreeNode node)
        {
            if(nodes.Contains(node))
            {
                nodes.Remove(node);
                AssetDatabase.RemoveObjectFromAsset(node);
                AssetDatabase.SaveAssets();
            }
        }
#endif

        public void AddChild(BehaviourTreeNode parent, BehaviourTreeNode child)
        {
            BehaviourTreeRootNode root = parent as BehaviourTreeRootNode;
            if (root)
            {
                root.child = child;
            }

            BehaviourTreeDecoratorNode decorator = parent as BehaviourTreeDecoratorNode;
            if(decorator)
            {
                decorator.child = child;
            }

            BehaviourTreeCompositeNode composite = parent as BehaviourTreeCompositeNode;
            if(composite)
            {
                composite.children.Add(child);
            }
        }

        public void RemoveChild(BehaviourTreeNode parent, BehaviourTreeNode child) 
        {
            BehaviourTreeRootNode root = parent as BehaviourTreeRootNode;
            if (root)
            {
                root.child = null;
            }

            BehaviourTreeDecoratorNode decorator = parent as BehaviourTreeDecoratorNode;
            if (decorator)
            {
                decorator.child = null;
            }

            BehaviourTreeCompositeNode composite = parent as BehaviourTreeCompositeNode;
            if (composite)
            {
                composite.children.Remove(child);
            }
        }

        public List<BehaviourTreeNode> GetChildren(BehaviourTreeNode parent)
        {
            var children = new List<BehaviourTreeNode>();

            BehaviourTreeRootNode root = parent as BehaviourTreeRootNode;
            if (root && root.child != null)
            {
                children.Add(root.child);
            }

            BehaviourTreeDecoratorNode decorator = parent as BehaviourTreeDecoratorNode;
            if (decorator && decorator.child != null)
            {
                children.Add(decorator.child);
            }

            BehaviourTreeCompositeNode composite = parent as BehaviourTreeCompositeNode;
            if (composite && composite.children.Count > 0)
            {
                children = composite.children;
            }

            return children;
        }

        public BehaviourTree Clone()
        {
            BehaviourTree clonedTree = Instantiate(this);
            clonedTree.rootNode = clonedTree.rootNode.Clone();
            return clonedTree;
        }
    }


}
