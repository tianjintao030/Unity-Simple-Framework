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
                // 对 Unity 不支持的类型（比如类引用），只显示名字
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
            // 使用 Unity 的默认高度计算，保证多行字段正常排版
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}

