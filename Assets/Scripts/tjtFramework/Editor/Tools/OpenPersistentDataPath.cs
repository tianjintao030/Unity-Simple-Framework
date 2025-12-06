using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public class OpenPersistentDataPath
{
    [MenuItem("Tools/打开PersistentDataPath")]
    public static void OpenFolder()
    {
        string path = Application.persistentDataPath;

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

#if UNITY_EDITOR_WIN
        Process.Start("explorer.exe", path.Replace("/", "\\"));
#elif UNITY_EDITOR_OSX
        Process.Start("open", path);
#elif UNITY_EDITOR_LINUX
        Process.Start("xdg-open", path);
#endif

        UnityEngine.Debug.Log($"已打开持久化路径: {path}");
    }
}
