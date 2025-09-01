using UnityEditor;
using UnityEngine;

namespace tjtFramework.Navigation
{
    [CustomEditor(typeof(AStarGridController))]
    public class AStarGridControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // 先绘制默认的 Inspector
            DrawDefaultInspector();

            // 获取脚本实例
            AStarGridController gridController = (AStarGridController)target;

            GUILayout.Space(10);

            if (GUILayout.Button("重新生成网格"))
            {
                gridController.ReSetGrid();

                // 让 Unity 知道物体数据有修改（支持撤销/保存）
                EditorUtility.SetDirty(gridController);
            }
        }
    }
}

