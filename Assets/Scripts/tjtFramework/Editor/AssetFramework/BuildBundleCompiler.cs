using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using tjtFramework.AssetFramework;
using Sirenix.Utilities;
using tjtFramework.Utiliy;
using System;
using System.IO;
using UnityEditor;
using Newtonsoft.Json;

public enum BuildType
{
    AssetBundle,
    HotPatch,
}

public class BuildBundleCompiler
{
    private static string gameVersion;
    private static string resourceVersion;
    private static BuildType buildType;
    private static BuildModuleData moduleData;
    private static BundleModule bundleModuleEnum;

    /// <summary>
    /// 所有将要构建Assetbundle的文件路径
    /// </summary>
    private static List<string> allAssetBundlePathList = new();

    /// <summary>
    /// 所有将要构建Assetbundle的文件夹的Bundle字典
    /// key：bundleName，value：路径列表
    /// </summary>
    private static Dictionary<string, List<string>> allFolderBundleDic = new();

    /// <summary>
    /// 所有将要构建Assetbundle的预制体的Bundle字典
    /// key：bundleName，value：路径列表
    /// </summary>
    private static Dictionary<string, List<string>> allPrefabBundleDic = new();

    private static string outPutPath => $"{Application.dataPath}/../AssetBundle/" +
        $"{EditorUserBuildSettings.activeBuildTarget.ToString()}/{bundleModuleEnum.ToString()}/";

    private static string hotAssetPath => $"{Application.dataPath}/../HotAsset/{BuildBundleSetting.Instance.buildTarget}/" +
                                $"{gameVersion}/{resourceVersion}/{bundleModuleEnum}/";

    private static string bundleConfigPath => $"{Application.dataPath}/Editor/AssetBundleConfigs";

    private static string aesSecretKey = "tjtai90";

    /// <summary>
    /// 打包AssetBundle
    /// </summary>
    public static void BuildAssetBundle(BuildModuleData moduleData, 
                                        BuildType buildType,
                                        string gameVersion,
                                        string resourceVersion)
    {
        InitBuildData(moduleData, buildType, gameVersion, resourceVersion);

        IncludeAllFolder();
        IncludeRootSubFolder();
        IncludeAllPrefabs();

        BuildAllAssetBundles();
    }

    /// <summary>
    /// 初始化打包数据
    /// </summary>
    private static void InitBuildData(BuildModuleData _moduleData,
                                        BuildType _buildType,
                                        string _gameVersion,
                                        string _resourceVersion)
    {
        allAssetBundlePathList.Clear();
        allFolderBundleDic.Clear();
        allPrefabBundleDic.Clear();

        moduleData = _moduleData;
        buildType = _buildType;
        gameVersion = _gameVersion;
        resourceVersion = _resourceVersion;
        bundleModuleEnum = (BundleModule)Enum.Parse(typeof(BundleModule), moduleData.moduleName);

        FileUtility.DeleteFolder(outPutPath);
        Directory.CreateDirectory(outPutPath);
    }

    /// <summary>
    /// 所有文件夹归纳为AssetBundle
    /// </summary>
    private static void IncludeAllFolder()
    {
        if(moduleData.signBudlePathArray == null || moduleData.signBudlePathArray.Length <= 0)
        {
            return;
        }

        foreach (var buildFileInfo in moduleData.signBudlePathArray)
        {
            var path = buildFileInfo.bundlePath.Replace(@"\", "/");
            if(IsPrepeatBundleFile(path))
            {
                continue;
            }

            var bundleName = GenerateBundleName(buildFileInfo.bundleName);

            if(allFolderBundleDic.ContainsKey(bundleName))
            {
                allFolderBundleDic[bundleName].Add(path);
            }
            else
            {
                allFolderBundleDic.Add(bundleName, new List<string> { path });
            }
        }
    }

    /// <summary>
    /// 父文件夹下的所有子文件夹归纳为AssetBundle
    /// </summary>
    private static void IncludeRootSubFolder()
    {
        if(moduleData.rootFolderSubBundlePathArray == null || moduleData.rootFolderSubBundlePathArray.Length <= 0)
        {
            return;
        }

        foreach(var item in moduleData.rootFolderSubBundlePathArray)
        {
            var rootPath = item + "/"; //(因为是父文件夹的路径所以要拼一个"/")
            var subFolders = Directory.GetDirectories(rootPath);
            if(subFolders.Length > 0)
            {
                foreach(var subFolder in subFolders)
                {
                    var path = subFolder.Replace(@"\", "/");

                    // 裁剪路径最后一个 / 之后的字符串为BundleName
                    var bundleNameStartIndex = path.LastIndexOf("/");
                    var bundleName = GenerateBundleName(path.Substring(bundleNameStartIndex, path.Length - bundleNameStartIndex));

                    if(!IsPrepeatBundleFile(path))
                    {
                        allAssetBundlePathList.Add(path);

                        if (allFolderBundleDic.ContainsKey(bundleName))
                        {
                            allFolderBundleDic[bundleName].Add(path);
                        }
                        else
                        {
                            allFolderBundleDic.Add(bundleName, new List<string> { path });
                        }
                    }

                    // 处理子文件下的资源
                    var filePaths = Directory.GetFiles(path, "*");
                    if(filePaths.Length > 0)
                    {
                        foreach(var filePath in filePaths)
                        {
                            if(filePath.EndsWith(".meta"))
                            {
                                continue;
                            }

                            var abFilePath = filePath.Replace(@"\", "/");
                            if (!IsPrepeatBundleFile(abFilePath))
                            {
                                allAssetBundlePathList.Add(abFilePath);

                                if (allFolderBundleDic.ContainsKey(bundleName))
                                {
                                    allFolderBundleDic[bundleName].Add(abFilePath);
                                }
                                else
                                {
                                    allFolderBundleDic.Add(bundleName, new List<string> { abFilePath });
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 所有归纳Prefab为AssetBundle
    /// </summary>
    private static void IncludeAllPrefabs()
    {
        if(moduleData.prefabPathArray == null || moduleData.prefabPathArray.Length <= 0)
        {
            return;
        }

        // 获取所有预制的guid
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", moduleData.prefabPathArray);
        foreach(var guid in prefabGuids)
        {
            var filePath = AssetDatabase.GUIDToAssetPath(guid);
            var bundleName = GenerateBundleName(Path.GetFileNameWithoutExtension(filePath));

            if(!allAssetBundlePathList.Contains(filePath))
            {
                // 处理预制体依赖项
                var depends = AssetDatabase.GetDependencies(filePath);
                List<string> dependsList = new();
                for(int i = 0; i < depends.Length; i++)
                {
                    var dependPath = depends[i];
                    // 依赖不是冗余项，则归纳进打包
                    if(!IsPrepeatBundleFile(dependPath))
                    {
                        allAssetBundlePathList.Add(dependPath);
                        dependsList.Add(dependPath);
                    }
                }

                if(!allPrefabBundleDic.ContainsKey(bundleName))
                {
                    allPrefabBundleDic.Add(bundleName, dependsList);
                }
            }
        }
    }


    private static void BuildAllAssetBundles()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            ModifyAllFileBundleName();
            GenerateAssetBundleConfig();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        AssetDatabase.Refresh();

        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                outPutPath, 
                (UnityEditor.BuildAssetBundleOptions)Enum.Parse(typeof(UnityEditor.BuildAssetBundleOptions), BuildBundleSetting.Instance.bundleOptions.ToString()),
                (UnityEditor.BuildTarget)Enum.Parse(typeof(UnityEditor.BuildTarget), BuildBundleSetting.Instance.buildTarget.ToString()));
        if(manifest == null)
        {
            EditorUtility.DisplayDialog("错误", "构建AssetBundle失败", "可恶!快看看是什么报错");
            Debug.LogError("构建AssetBundle失败");
        }
        else
        {
            DeleteAllManifestFiles();

            Debug.Log("构建AssetBundle成功！");

            EcryptAllBundles();

            if(buildType == BuildType.HotPatch)
            {
                GenerateHotAssets();
            }
        }

        AssetDatabase.StartAssetEditing();
        try
        {
            ModifyAllFileBundleName(clear: true);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }
        EditorUtility.ClearProgressBar();
    }

    /// <summary>
    /// 生成Assetbundle配置
    /// </summary>
    private static void GenerateAssetBundleConfig()
    {
        var config = new AssetBundleConfig();

        // key:path，value:AssetBundleName
        var allBundleFilePathDic = new Dictionary<string, string>();
        // 获取所有AssetBundle的路径和名字
        var allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        foreach(var assetBundleName in allAssetBundleNames)
        {
            var bundleFilePaths = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleName);
            foreach(var filePath in bundleFilePaths)
            {
                if (!filePath.EndsWith(".cs"))
                {
                    allBundleFilePathDic.Add(filePath, assetBundleName);
                }
            }
        }

        // 生成AssetBundle配置
        foreach(var item in allBundleFilePathDic)
        {
            var filePath = item.Key;

            var info = new AssetBundleInfo();
            info.path = filePath;
            info.AssetBundleName = item.Value;
            info.AssetName = Path.GetFileName(filePath);
            info.crc = Crc32.GetCrc32(filePath);
            info.dependencies = new List<string>();

            var depends = AssetDatabase.GetDependencies(filePath);
            foreach(var dependPath in depends)
            {
                if(!string.Equals(dependPath, filePath) && !dependPath.EndsWith(".cs"))
                {
                    if(allBundleFilePathDic.TryGetValue(filePath, out var assetBundleName))
                    {
                        if(!info.dependencies.Contains(assetBundleName))
                        {
                            info.dependencies.Add(assetBundleName);
                        }
                    }
                }
            }

            config.assetBundleInfoList.Add(info);
        }

        // 写入Json
        var json = JsonConvert.SerializeObject(config, Formatting.Indented);
        var assetBundleConfigPath = $"{bundleConfigPath}/{bundleModuleEnum.ToString().ToLower()}assetbundleconfig.json";
        StreamWriter streamWriter = File.CreateText(assetBundleConfigPath);
        streamWriter.Write(json);
        streamWriter.Dispose();
        streamWriter.Close();

        var importer = AssetImporter.GetAtPath(assetBundleConfigPath.Replace(Application.dataPath, "Assets"));
        if(importer != null)
        {
            importer.assetBundleName = $"{bundleModuleEnum.ToString().ToLower()}bundleconfig.ab";
        }
    }

    /// <summary>
    /// 修改或者清空Assetbundle的BundleName
    /// </summary>
    private static void ModifyAllFileBundleName(bool clear  = false)
    {
        // 修改所有文件夹的AssetBundleName
        int i = 0;
        foreach(var item in allFolderBundleDic)
        {
            i++;
            EditorUtility.DisplayProgressBar("Modify AssetBundle BundleName", $"Name:{item.Key}", i * 1.0f / allFolderBundleDic.Count);
            foreach(var path in item.Value)
            {
                var importer = AssetImporter.GetAtPath(path);
                if(importer != null)
                {
                    importer.assetBundleName = clear ? "" : $"{item.Key}.ab";
                }
            }
        }

        // 修改所有预制体的AssetBundleName
        i = 0;
        foreach (var item in allPrefabBundleDic)
        {
            i++;
            EditorUtility.DisplayProgressBar("Modify AssetBundle BundleName", $"Name:{item.Key}", i * 1.0f / allPrefabBundleDic.Count);
            foreach (var path in item.Value)
            {
                var importer = AssetImporter.GetAtPath(path);
                if (importer != null)
                {
                    importer.assetBundleName = clear ? "" : $"{item.Key}.ab";
                }
            }
        }

        if(clear)
        {
            // 清理AssetBundleConfig的BundleName
            var assetBundleConfigPath = $"{bundleConfigPath}/{bundleModuleEnum.ToString().ToLower()}assetbundleconfig.json";
            var importer = AssetImporter.GetAtPath(assetBundleConfigPath.Replace(Application.dataPath, "Assets"));
            if (importer != null)
            {
                importer.assetBundleName = "";
            }

            // 清理没有使用的AssetBundleName
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }
    }

    /// <summary>
    /// Bundle文件是否重复了
    /// </summary>
    private static bool IsPrepeatBundleFile(string path)
    {
        if(!allAssetBundlePathList.IsNullOrEmpty())
        {
            foreach(var item in allAssetBundlePathList)
            {
                if(string.Equals(item, path) ||
                    item.Contains(path) || 
                    path.EndsWith(".cs"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 生成AssetBundle的名字
    /// </summary>
    private static string GenerateBundleName(string abName)
    {
        return bundleModuleEnum.ToString() + abName.Replace("/","_");
    }

    /// <summary>
    /// 删除自动生成的AssetBundle的Manifest清单文件
    /// </summary>
    private static void DeleteAllManifestFiles()
    {
        var outPutFiles = Directory.GetFiles(outPutPath);
        foreach(var file in outPutFiles)
        {
            if(file.EndsWith(".manifest"))
            {
                File.Delete(file);
            }
        }
    }

    /// <summary>
    /// 加密AssetBundle包
    /// </summary>
    private static void EcryptAllBundles()
    {
        if(!BuildBundleSetting.Instance.isEncrypt)
        {
            return;
        }

        var directoryInfo = new DirectoryInfo(outPutPath);
        var fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        for(int i = 0;i < fileInfos.Length; i++)
        {
            EditorUtility.DisplayProgressBar("加密AssetBundle", $"{fileInfos[i].FullName}", (i * 1.0f) / fileInfos.Length);
            AES.AESEncrypt(fileInfos[i].FullName, aesSecretKey);
        }
        EditorUtility.ClearProgressBar();
        Debug.Log("加密AssetBundle完成");
    }

    /// <summary>
    /// 内嵌AssetBundle到包体中
    /// </summary>
    public static void CopyAssetBundlesToStreamingAssets(BuildModuleData moduleData)
    {
        bundleModuleEnum = (BundleModule)Enum.Parse(typeof(BundleModule), moduleData.moduleName);

        var directoryInfo = new DirectoryInfo(outPutPath);
        var fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

        var streamingAssetsPath = $"{Application.streamingAssetsPath}/AssetBundle/{bundleModuleEnum}/";
        FileUtility.DeleteFolder(streamingAssetsPath);
        Directory.CreateDirectory(streamingAssetsPath);

        var buildInBundleInfos = new List<BuildInAssetBundleInfo>();
        for (int i = 0; i < fileInfos.Length; i++)
        {
            EditorUtility.DisplayProgressBar("内嵌AssetBundle", $"{fileInfos[i].FullName}", (i * 1.0f) / fileInfos.Length);
            // 复制到StreamingAssets
            File.Copy(fileInfos[i].FullName, $"{streamingAssetsPath}/{fileInfos[i].Name}", true);

            // 生成内嵌AssetBundle信息
            var buildInInfo = new BuildInAssetBundleInfo();
            buildInInfo.fileName = fileInfos[i].FullName;
            buildInInfo.md5 = MD5.GetMd5FromFile(fileInfos[i].FullName);
            buildInInfo.size = fileInfos[i].Length / 1024;
            buildInBundleInfos.Add(buildInInfo);
        }

        // 生成配置文件
        var json = JsonConvert.SerializeObject(buildInBundleInfos, Formatting.Indented);
        var infoPath = BuildBundleSetting.Instance.GetBuiltinAssetInfoPath();
        if(!Directory.Exists(infoPath))
        {
            Directory.CreateDirectory(infoPath);
        }
        FileUtility.WriteFile($"{infoPath}/{bundleModuleEnum}info.json", System.Text.Encoding.UTF8.GetBytes(json));

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        Debug.Log($"内嵌{bundleModuleEnum}资源成功");
    }

    /// <summary>
    /// 生成热更文件
    /// </summary>
    private static void GenerateHotAssets()
    {
        FileUtility.DeleteFolder(hotAssetPath);
        Directory.CreateDirectory(hotAssetPath);

        var bundlePatch = Directory.GetFiles(outPutPath, "*.ab");
        for(int i = 0; i < bundlePatch.Length; i++)
        {
            var bundlePath = bundlePatch[i];
            var hotPath = hotAssetPath + Path.GetFileName(bundlePath);
            EditorUtility.DisplayProgressBar("生成热更文件", $"{hotPath}", (i * 1.0f) / bundlePatch.Length);
            File.Copy(bundlePath, hotPath, true);
        }

        GenerateHotAssetManifest();

        Debug.Log("生成热更文件成功");
    }

    /// <summary>
    /// 生成热更清单
    /// </summary>
    private static void GenerateHotAssetManifest()
    {
        // 设置补丁清单
        var manifest = new HotAssetManifest();
        manifest.gameVersion = gameVersion;
        manifest.downLoadUrl = $"{BuildBundleSetting.Instance.downLoadUrl}/HotAsset/{BuildBundleSetting.Instance.buildTarget}/" +
                            $"{gameVersion}/{resourceVersion}/{bundleModuleEnum.ToString()}/";

        // 设置补丁
        var hotPatch = new HotAssetPatch();
        hotPatch.resourceVersion = resourceVersion;

        // 设置热更补丁文件信息
        if(!Directory.Exists(hotAssetPath))
        {
            Directory.CreateDirectory(hotAssetPath);
        }
        var directoryInfo = new DirectoryInfo(hotAssetPath);
        var fileInfos = directoryInfo.GetFiles("*.ab");
        foreach(var fileInfo in fileInfos)
        {
            var hotFileInfo = new HotFileInfo();
            hotFileInfo.abName = fileInfo.Name;
            hotFileInfo.md5 = MD5.GetMd5FromFile(fileInfo.FullName);
            hotFileInfo.size = fileInfo.Length / 1024;

            hotPatch.hotFileList.Add(hotFileInfo);
        }

        manifest.patchList.Add(hotPatch);

        // 生成Json文件
        var json = JsonConvert.SerializeObject(manifest, Formatting.Indented);
        FileUtility.WriteFile($"{Application.dataPath}/../HotAsset/{BuildBundleSetting.Instance.buildTarget}/" +
                                $"{gameVersion}/{bundleModuleEnum}HotAssetManifest.json",
                                System.Text.Encoding.UTF8.GetBytes(json));
    }
}
