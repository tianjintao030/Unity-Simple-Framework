using UnityEditor;
using UnityEngine;

namespace tjtFramework.Navigation
{
    [CustomEditor(typeof(AStarGridController))]
    public class AStarGridControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // �Ȼ���Ĭ�ϵ� Inspector
            DrawDefaultInspector();

            // ��ȡ�ű�ʵ��
            AStarGridController gridController = (AStarGridController)target;

            GUILayout.Space(10);

            if (GUILayout.Button("������������"))
            {
                gridController.ReSetGrid();

                // �� Unity ֪�������������޸ģ�֧�ֳ���/���棩
                EditorUtility.SetDirty(gridController);
            }
        }
    }
}

