using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace tjtFramework.Excel
{
    public class ExcelToDataWindow : EditorWindow
    {
        private string excelPath = "";

        [MenuItem("Tools/Excel To Data")]
        public static void ShowWindow()
        {
            GetWindow<ExcelToDataWindow>("Excel To Data Converter");
        }

        private void OnGUI()
        {
            GUILayout.Label("Excel ����ת���ݹ���", EditorStyles.boldLabel);
            if (GUILayout.Button("ѡ��Excel�ļ�"))
            {
                excelPath = EditorUtility.OpenFilePanel("ѡ��Excel�ļ�", "", "xlsx");
            }

            GUILayout.Label("��ǰExcel·��:");
            GUILayout.TextField(excelPath);

            GUILayout.Space(10);

            if (GUILayout.Button("��ʼ����"))
            {
                if (string.IsNullOrEmpty(excelPath)) return;
                ExcelToDataConverter.ConvertExcel(excelPath);
            }
        }
    }
}

