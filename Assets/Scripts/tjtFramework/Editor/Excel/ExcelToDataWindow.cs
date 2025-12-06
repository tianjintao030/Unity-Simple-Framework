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
            GUILayout.Label("Excel 配置转数据工具", EditorStyles.boldLabel);
            if (GUILayout.Button("选择Excel文件"))
            {
                excelPath = EditorUtility.OpenFilePanel("选择Excel文件", "", "xlsx");
            }

            GUILayout.Label("当前Excel路径:");
            GUILayout.TextField(excelPath);

            GUILayout.Space(10);

            if (GUILayout.Button("开始导入"))
            {
                if (string.IsNullOrEmpty(excelPath)) return;
                ExcelToDataConverter.ConvertExcel(excelPath);
            }
        }
    }
}

