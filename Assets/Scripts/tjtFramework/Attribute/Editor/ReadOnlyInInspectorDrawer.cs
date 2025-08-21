using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace tjtFramework
{
    [CustomPropertyDrawer(typeof(ReadOnlyInInspectorAttribute))]
    public class ReadOnlyInInspectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;

            if (property.propertyType == SerializedPropertyType.Generic)
            {
                // �� Unity ��֧�ֵ����ͣ����������ã���ֻ��ʾ����
                EditorGUI.LabelField(position, label.text, "(Not Supported)");
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }

            GUI.enabled = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // ʹ�� Unity ��Ĭ�ϸ߶ȼ��㣬��֤�����ֶ������Ű�
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}

