using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 生成AssetBundle的对话框
/// </summary>
public class AssetBundleEditorWindow:EditorWindow
{
    [MenuItem("AssetBundle/生成AssetBundle")]
    public static void BuildAssetBundle()
    {
        AssetBundleEditor.CopyAssembly();
        AssetBundleEditor.BuildAssetBundle();
    }

    [MenuItem("AssetBundle/生成AssetBundle并放入StreamingAssets文件夹")]
    public static void BuildAssetBundleToStreamingAsset()
    {
        BuildAssetBundle();
        AssetBundleEditor.SyncToStreamingAssets();
    }

    [MenuItem("AssetBundle/构建AssetBundle面板")]
    public static void BuildAssetBundleWindow()
    {
        ShowWindow("构建AssetBundle");
    }

    private string versionInput = "1";

    private static void ShowWindow(string title)
    {
        var window = (AssetBundleEditorWindow)GetWindow(typeof(AssetBundleEditorWindow), true, title);
        window.minSize = new Vector2(250, 100);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("AB资源版本号：",EditorStyles.boldLabel);
        versionInput = GUILayout.TextField(versionInput);

        if(!int.TryParse(versionInput, out _))
        {
            EditorGUILayout.HelpBox("资源版本号需为 int", MessageType.Warning);
        }

        GUILayout.Space(7);

        if(GUILayout.Button("确定"))
        {
            if(int.TryParse(versionInput, out var versionNum))
            {
                AssetBundleEditor.versionCode = versionNum;
                Debug.Log($"构建AB包资源版本号{versionNum}");

                AssetBundleEditor.CopyAssembly();
                AssetBundleEditor.BuildAssetBundle();
                AssetBundleEditor.SyncToStreamingAssets();
            }

            Close();
        }
    }
}

/// <summary>
/// 生成AssetBundle的编辑器工具
/// </summary>
public class AssetBundleEditor : MonoBehaviour
{
    /// <summary>
    /// 版本号
    /// </summary>
    public static int versionCode;

    /// <summary>
    /// 热更资源根目录
    /// </summary>
    public static string AssetBundleRootPath = Application.dataPath + "/Game";

    /// <summary>
    /// 热更资源输出目录
    /// </summary>
    public static string AssetBundleOutputPath = Application.dataPath + "/../AssetBundle";

    /// <summary>
    /// 需要打包的AssetBundle信息，PS:一个AssetBundle包对应一个AssetBundleBuild
    /// </summary>
    public static List<AssetBundleBuild> AssetBundleBuildList = new List<AssetBundleBuild>();

    /// <summary>
    /// 记录资源和AssetBundle包的所属关系
    /// </summary>
    public static Dictionary<string,string> asset2Bundle = new Dictionary<string,string>();

    /// <summary>
    /// 记录资源所依赖的AssetBundle列表
    /// </summary>
    public static Dictionary<string,List<string>> asset2Dependencies = new Dictionary<string,List<string>>();

    public static void BuildAssetBundle()
    {
        try
        {
            if (!Directory.Exists(AssetBundleOutputPath))
            {
                Directory.CreateDirectory(AssetBundleOutputPath);
            }

            Debug.Log("开始构建AssetBundle");

            DirectoryInfo AssetPathInfo = new DirectoryInfo(AssetBundleRootPath);

            AssetBundleBuildList.Clear();

            asset2Bundle.Clear();
            asset2Dependencies.Clear();

            ScanChildDireations(AssetPathInfo);

            BuildPipeline.BuildAssetBundles
            (
                AssetBundleOutputPath,
                AssetBundleBuildList.ToArray(),
                BuildAssetBundleOptions.None,
                EditorUserBuildSettings.activeBuildTarget
             );

            CalculateDependencies();

            var abData = CreateConfigData();
            SaveConfigDataToJson(abData);
            SaveConfigDataToBin(abData);

            DeleteNotUnity3dFiles(AssetBundleOutputPath);
        }
        finally
        {
            Debug.Log("结束构建AssetBundle");
        }
    }

    /// <summary>
    /// 将热更代码的dll拷贝出来
    /// </summary>
    public static void CopyAssembly()
    {
        var sourcaPath = Path.Combine(Application.dataPath, "../Library/ScriptAssemblies/Game.dll");
        var destinationDirectory = Path.Combine(Application.dataPath, "Game/Src");
        var destinationPath = Path.Combine(destinationDirectory, "Game.dll.bytes");

        if(!Directory.Exists(destinationDirectory))
        {
            Directory.CreateDirectory(destinationDirectory);
        }

        if(File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
            Debug.Log($"删除原来的{destinationPath}");
        }

        if(File.Exists(sourcaPath))
        {
            File.Copy(sourcaPath, destinationPath, true);
            Debug.Log($"文件复制完成{destinationPath}");
        }
        else
        {
            Debug.LogError($"源文件{sourcaPath}不存在");
        }

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 将AssetBundle同步到StreamingAssets文件夹
    /// </summary>
    public static void SyncToStreamingAssets()
    {
        var streamingAssetsPath = Application.streamingAssetsPath;
        if(Directory.Exists(streamingAssetsPath))
        {
            Directory.Delete(streamingAssetsPath, true);
        }

        CopyDirectory(AssetBundleOutputPath, streamingAssetsPath);

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 拷贝文件夹
    /// </summary>
    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        if(!Directory.Exists(sourceDir))
        {
            Debug.LogError($"需要拷贝的源文件夹不存在，{sourceDir}");
            return;
        }

        if(!Directory.Exists(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }

        foreach(var file in  Directory.GetFiles(sourceDir))
        {
            var fileName = Path.GetFileName(file);
            var destFile = Path.Combine(destinationDir, fileName);
            File.Copy(file, destFile, true);
        }

        foreach(var dir in Directory.GetDirectories(sourceDir)) 
        {
            var dirName = Path.GetFileName(dir);
            var destDir = Path.Combine(destinationDir, dirName);
            CopyDirectory(dir, destDir);
        }
    }

    /// <summary>
    /// 保存ABData数据为二进制格式
    /// </summary>
    private static void SaveConfigDataToBin(ABData assetData)
    {
        string binPath = AssetBundleOutputPath + "/config.bin";
        if(File.Exists(binPath))
        {
            File.Delete(binPath);
        }
        File.Create(binPath).Dispose();

        byte[] binData = MemoryPack.MemoryPackSerializer.Serialize(assetData);
        using(FileStream stream = new FileStream(binPath,FileMode.Create))
        {
            stream.Write(binData, 0, binData.Length);
        }
        Debug.Log("保存ABData数据为二进制格式");
    }

    /// <summary>
    /// 保存ABData数据为json格式
    /// </summary>
    private static void SaveConfigDataToJson(ABData assetData)
    {
        string jsonPath = AssetBundleOutputPath + "/config.json";
        if(File.Exists(jsonPath))
        {
            File.Delete(jsonPath);
        }
        File.Create(jsonPath).Dispose();

        var jsonData = LitJson.JsonMapper.ToJson(assetData);
        File.WriteAllText(jsonPath, ConvertJsonString(jsonData));
        Debug.Log("保存ABData数据为json格式");
    }

    /// <summary>
    /// 将json字串格式化
    /// </summary>
    private static string ConvertJsonString(string str)
    {
        JsonSerializer serializer = new JsonSerializer();
        TextReader tr = new StringReader(str);
        JsonTextReader jtr = new JsonTextReader(tr);
        object obj = serializer.Deserialize(jtr);
        if (obj != null)
        {
            StringWriter textWriter = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 4,
                IndentChar = ' '
            };
            serializer.Serialize(jsonWriter, obj);
            return textWriter.ToString();
        }
        else
        {
            return str;
        }
    }

    /// <summary>
    /// 创建ABData
    /// </summary>
    private static ABData CreateConfigData()
    {
        int prefixLength = $"Assets/Game/".Length;

        var assetData = new ABData()
        {
            VersionCode = versionCode,
            BundleArray = new Dictionary<string, ABBundle>(),
            AssetArray = new Dictionary<uint, ABAsset>()
        };

        //填充BundleArray
        foreach(var build in AssetBundleBuildList)
        {
            var bundle = new ABBundle()
            {
                Assets = new List<string>()
            };
            bundle.Name = build.assetBundleName;

            foreach(var asset in build.assetNames)
            {
                if(!string.IsNullOrEmpty(asset))
                {
                    bundle.Assets.Add(asset.Substring(prefixLength));
                }
            }

            string abFilePath = Path.Combine(AssetBundleOutputPath, bundle.Name);
            using(FileStream stream = File.OpenRead(abFilePath))
            {
                bundle.Crc = Convert.ToUInt32(CRC.GetCRC32Hash(stream),16);
                bundle.Size = (int)stream.Length;
            }

            assetData.BundleArray.Add(bundle.Name, bundle);
        }

        //填充AssetArray
        foreach (var item in asset2Bundle)
        {
            var assetInfo = new ABAsset()
            {
                Dependencies = new List<string>()
            };

            uint asset_path_crc = Convert.ToUInt32(CRC.GetCRC32Hash(item.Key), 16);
            assetInfo.AssetPath = item.Key.Substring(prefixLength);

            assetInfo.BundleName = item.Value;

            bool result = asset2Dependencies.TryGetValue(item.Key, out List<string> dependencies);
            if (result)
            {
                for (int i = 0; i < dependencies.Count; i++)
                {
                    string bundleName = dependencies[i];
                    assetInfo.Dependencies.Add(bundleName);
                }
            }

            assetData.AssetArray.Add(asset_path_crc, assetInfo);
        }

        Debug.Log("创建ABData配置数据完成");
        return assetData;
    }

    /// <summary>
    /// 删除非.unity3d文件
    /// </summary>
    private static void DeleteNotUnity3dFiles(string directory)
    {
        FileInfo[] files = new DirectoryInfo(directory).GetFiles();
        foreach (FileInfo file in files)
        {
            if(file.Name == "config.json" || file.Name == "config.bin")
            { 
                continue; 
            }

            if(!file.FullName.EndsWith(".unity3d") || file.FullName.EndsWith(".manifest"))
            {
                file.Delete();
            }
        }
        Debug.Log("删除无用文件完成");
    }

    /// <summary>
    /// 扫描文件夹
    /// 1.把该路径下一级子文件夹打成一个AssetBundle
    /// 2.遍历该路径下所有子文件夹
    /// </summary>
    private static void ScanChildDireations(DirectoryInfo directoryInfo)
    {
        //把当前路径下子文件打成一个AssetBundle
        ScanCurrentDirectory(directoryInfo);

        //遍历子文件夹
        DirectoryInfo[] dirs  = directoryInfo.GetDirectories();
        if(dirs.Length > 0)
        {
            foreach (DirectoryInfo dir in dirs)
            {
                ScanChildDireations(dir);
            }
        }
     }

    /// <summary>
    /// 遍历当前文件夹下的文件，把它们打成一个AssetBundle
    /// </summary>
    private static void ScanCurrentDirectory(DirectoryInfo directoryInfo)
    {
        Debug.Log($"开始扫描{directoryInfo.FullName}");

        List<string> assetNameList = new List<string>();

        FileInfo[] fileInfos = directoryInfo.GetFiles();

        foreach(FileInfo fileInfo in fileInfos)
        {
            //跳过meta文件和DS_Store文件(mac系统)
            if(fileInfo.FullName.EndsWith(".meta") || fileInfo.FullName.EndsWith(".DS_Store"))
            {
                continue;
            }

            ///将D:\\xx\\Assets裁剪为以Assets开头的
            string assetName = fileInfo.FullName.Substring(Application.dataPath.Length - "Assets".Length).Replace("\\", "/");
            assetNameList.Add(assetName);
        }

        if( assetNameList.Count > 0 )
        {
            //斜杠替换为下划线
            string assetBundleName = directoryInfo.FullName.Substring(Application.dataPath.Length + 1).Replace('\\', '_');
            assetBundleName = directoryInfo.FullName.Substring(Application.dataPath.Length + 1).Replace('/', '_').ToLower();

            assetBundleName = $"{assetBundleName}.unity3d";

            AssetBundleBuild build = new AssetBundleBuild()
            {
                assetBundleName = assetBundleName,
                assetNames = assetNameList.ToArray()
            };

            foreach(var assetName in assetNameList)
            {
                //记录单个资源属于哪个AsstBundle
                asset2Bundle.Add(assetName, assetBundleName);
            }

            AssetBundleBuildList.Add(build);
        }

        Debug.Log($"结束扫描{directoryInfo.FullName}");
    }

    /// <summary>
    /// 计算资源依赖的AssetBundle列表
    /// </summary>
    private static void CalculateDependencies()
    {
        Debug.Log("开始计算依赖");
        foreach (var assetName in asset2Bundle.Keys)
        {
            string[] dependencies = AssetDatabase.GetDependencies(assetName);
            List<string> assetList = new List<string>();
            if(dependencies.Length > 0 )
            {
                foreach(var dependency in dependencies)
                {
                    //排除资源自身
                    if(dependency == assetName)
                    {
                        continue;
                    }
                    assetList.Add(dependency);
                }
            }

            string assetBundleName = asset2Bundle[assetName];
            if (assetList.Count > 0)
            {
                List<string> bundleList = new List<string>();
                foreach(var asset in assetList)
                {
                    var result = asset2Bundle.TryGetValue(asset, out string bundle);
                    if(result)
                    {
                        if(bundle != assetBundleName && !bundleList.Contains(bundle))
                        {
                            bundleList.Add(bundle);
                        }
                    }
                }

                asset2Dependencies.Add(assetName,bundleList);
            }
        }
        Debug.Log("计算依赖完成");
    }
}
