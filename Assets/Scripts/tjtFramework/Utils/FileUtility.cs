using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace tjtFramework.Utiliy
{
    public class FileUtility
    {
        /// <summary>
        /// 删除文件夹
        /// </summary>
        public static void DeleteFolder(string folderPath)
        {
            if(Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath);
                foreach (var filePath in files)
                {
                    if(File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                Directory.Delete(folderPath, true);
            }
        }


        public static void WriteFile(string filePath, byte[] content)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            using (var stream = File.Create(filePath))
            {
                stream.Write(content, 0, content.Length);
            }
        }
    }
}

