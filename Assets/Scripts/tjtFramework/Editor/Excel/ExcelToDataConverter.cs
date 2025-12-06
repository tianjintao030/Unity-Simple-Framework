using System.Collections.Generic;
using UnityEngine;
using ExcelDataReader;
using System.Text;
using System.IO;
using UnityEditor;
using System.Linq;

namespace tjtFramework.Excel
{
    public static class ExcelToDataConverter
    {
        private static string exportAssetPath = "Assets/Game/DataTable";
        private static string exportScriptPath = "Assets/Scripts/Game/DataTable";

        public static void ConvertExcel(string path)
        {
            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();
                var table = result.Tables[0];

                // 表头
                int columnCount = table.Columns.Count;
                string[] fieldNames = new string[columnCount];
                string[] fieldComments = new string[columnCount];
                string[] fieldTypes = new string[columnCount];

                // Excel表中第一行为字段名
                // 第二行为字段注释
                // 第三行为字段类型
                // 第四行目前暂空
                for (int i = 0; i < columnCount; i++)
                {
                    fieldNames[i] = table.Rows[0][i].ToString();
                    fieldComments[i] = table.Rows[1][i].ToString();
                    fieldTypes[i] = table.Rows[2][i].ToString();
                }

                string className = Path.GetFileNameWithoutExtension(path) + "DataTable";
                var dataList = new List<Dictionary<string, string>>();

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    var row = table.Rows[i];
                    var dict = new Dictionary<string, string>();

                    for (int j = 0; j < columnCount; j++)
                    {
                        dict[fieldNames[j]] = row[j]?.ToString() ?? "";
                    }

                    dataList.Add(dict);
                }

                GenerateCSharpScript(className, fieldNames, fieldTypes, fieldComments);
                GenerateDataFile(className, fieldNames, fieldTypes, dataList);
            }

            AssetDatabase.Refresh();
            Debug.Log($"导入Excel表{path}完成");
        }

        /// <summary>
        /// 生成对应的C#类脚本
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="names">字段名数组</param>
        /// <param name="types">字段类型数组</param>
        /// <param name="comments">字段注释数组</param>
        private static void GenerateCSharpScript(string className, string[] names, string[] types, string[] comments)
        {
            var sb = new StringBuilder();

            // 引用
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine("namespace Game.DataTable");
            sb.AppendLine("{");
            sb.AppendLine($"\tpublic partial class {className}");
            sb.AppendLine("\t{");

            // Item 子类
            sb.AppendLine("\t\t[System.Serializable]");
            sb.AppendLine("\t\tpublic class Item");
            sb.AppendLine("\t\t{");
            for (int i = 0; i < names.Length; i++)
            {
                sb.AppendLine($"\t\t\t/// <summary> {comments[i]} </summary>");
                sb.AppendLine($"\t\t\tpublic {ConvertType(types[i])} {names[i]};");
            }
            sb.AppendLine("\t\t}");
            sb.AppendLine();

            // 字典
            sb.AppendLine($"\t\tpublic static Dictionary<int, Item> DataDict = new();");
            sb.AppendLine();

            // 加载方法
            sb.AppendLine($"\t\tpublic static void LoadFromTextAsset(TextAsset textAsset)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t\tDataDict.Clear();");
            sb.AppendLine("\t\t\tvar lines = textAsset.text.Split('\\n');");
            sb.AppendLine("\t\t\tforeach (var line in lines)");
            sb.AppendLine("\t\t\t{");
            sb.AppendLine("\t\t\t\tif (string.IsNullOrWhiteSpace(line)) continue;");
            sb.AppendLine("\t\t\t\tvar data = JsonUtility.FromJson<Item>(line.Trim());");
            sb.AppendLine("\t\t\t\tDataDict[data.id] = data;");
            sb.AppendLine("\t\t\t}");
            sb.AppendLine("\t\t}");

            sb.AppendLine("\t}"); // class
            sb.AppendLine("}");   // namespace

            var targetScriptPath = Path.Combine(exportScriptPath, className + ".cs");
            var dir = Path.GetDirectoryName(targetScriptPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (File.Exists(targetScriptPath))
            {
                File.Delete(targetScriptPath);
            }

            File.WriteAllText(targetScriptPath, sb.ToString());
        }


        /// <summary>
        /// 生成数据记录文件
        /// </summary>
        /// <param name="className">类名</param>
        /// <param name="fieldNames">字段名数组</param>
        /// <param name="fieldTypes">字段类型数组</param>
        /// <param name="rows">行数据列表</param>
        private static void GenerateDataFile(string className, string[] fieldNames, string[] fieldTypes,
            List<Dictionary<string, string>> rows)
        {
            var sb = new StringBuilder();
            //跳过前四行
            for (int r = 4; r < rows.Count; r++) 
            {
                var row = rows[r];

                sb.Append("{");
                for (int i = 0; i < fieldNames.Length; i++)
                {
                    var field = fieldNames[i];
                    var val = row[field].Replace("\"", "\\\"");
                    string jsonValue = $"\"{field}\": \"{val}\"";

                    if (IsNumericType(fieldTypes[i]))
                    {
                        jsonValue = $"\"{field}\": {val}";
                    }
                    else if (fieldTypes[i].EndsWith("[]"))
                    {
                        if (fieldTypes[i] == "string[]")
                        {
                            var elements = val.Split(',')
                                .Select(e => $"\"{e.Trim()}\"");
                            var arrayStr = string.Join(",", elements);
                            jsonValue = $"\"{field}\": [{arrayStr}]";
                        }
                        else
                        {
                            jsonValue = $"\"{field}\": [{val}]";
                        }
                    }

                    sb.Append(jsonValue);
                    if (i < fieldNames.Length - 1)
                    {
                        sb.Append(", ");
                    }
                }
                sb.AppendLine("}");
            }

            var dataPath = Path.Combine(exportAssetPath, className + ".txt");

            var dir = Path.GetDirectoryName(dataPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (File.Exists(dataPath))
            {
                File.Delete(dataPath);
            }

            File.WriteAllText(dataPath, sb.ToString());
        }

        private static string ConvertType(string t)
        {
            return t switch
            {
                "int[]" => "List<int>",
                "float[]" => "List<float>",
                "string[]" => "List<string>",
                "bool[]" => "List<bool>",
                _ => t
            };
        }

        private static bool IsNumericType(string t) =>
            t == "int" || t == "float" || t == "double";
    }
}

