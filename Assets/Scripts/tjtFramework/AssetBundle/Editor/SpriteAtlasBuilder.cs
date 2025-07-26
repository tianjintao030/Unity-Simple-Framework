#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.U2D;
using System.IO;
using System.Collections.Generic;
using UnityEditor.U2D;
using dnlib.DotNet;

/// <summary>
/// 图集构建工具
/// </summary>
public class SpriteAtlasBuilder : EditorWindow
{
    /// <summary>
    /// 要打包图集的文件夹列表
    /// </summary>
    private static List<string> folders = new();
    // 用于滚动视图
    private Vector2 scroll;
    // 图集输出的根目录（所有图集将保存到该目录下）
    private string outputRoot = "Assets/Game/SpriteAtlas";

    [MenuItem("Tools/SpriteAtlas图集构建")]
    public static void ShowWindow()
    {
        GetWindow<SpriteAtlasBuilder>("图集构建工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("图集构建配置", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        // 按钮：添加文件夹
        if (GUILayout.Button("添加文件夹"))
        {
            // 打开系统文件夹选择对话框
            string path = EditorUtility.OpenFolderPanel("选择图片文件夹", "Assets", "");

            // 如果路径合法且在项目 Assets 内部
            if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
            {
                // 转换为 Unity 相对路径
                string relativePath = "Assets" + path.Substring(Application.dataPath.Length);

                // 添加到列表中（避免重复）
                if (!folders.Contains(relativePath))
                {
                    folders.Add(relativePath);
                } 
            }
        }

        // 绘制文件夹列表（可滚动）
        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(150));
        for (int i = 0; i < folders.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();

            // 显示路径文本框
            GUI.enabled = false;
            folders[i] = EditorGUILayout.TextField(folders[i]);
            GUI.enabled = true;

            // 删除按钮
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                folders.RemoveAt(i);
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        // 设置输出根目录的输入框
        outputRoot = EditorGUILayout.TextField("输出根目录", outputRoot);

        // 构建图集按钮
        if (GUILayout.Button("构建图集"))
        {
            BuildAllAtlases(); 
        }
    }

    /// <summary>
    /// 遍历文件夹列表批量构建图集
    /// </summary>
    private void BuildAllAtlases()
    {
        if (!Directory.Exists(outputRoot))
        {
            Directory.CreateDirectory(outputRoot);
        }

        foreach (string folder in folders)
        {
            BuildOneAtlas(folder); 
        }
        Debug.Log("所有图集构建完成！");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 为一个文件夹创建SpriteAtlas
    /// </summary>
    /// <param name="sourceFolder">图片所在的文件夹路径（相对路径）</param>
    private void BuildOneAtlas(string sourceFolder)
    {
        // 去掉前缀 "Assets/"，保留相对路径
        string relativePath = sourceFolder.StartsWith("Assets/") ? sourceFolder.Substring("Assets/".Length) : sourceFolder;

        // 把路径分隔符替换成下划线，作为文件名
        string atlasName = relativePath.Replace("/", "_");

        // 生成最终图集路径
        string atlasPath = Path.Combine(outputRoot, atlasName + ".spriteatlas").Replace("\\", "/");

        // 在该文件夹中查找所有 Texture2D 资源的 GUID
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { sourceFolder });

        List<Object> sprites = new List<Object>();

        foreach (var guid in guids)
        {
            // 获取对应的资源路径
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            // 强制设为 Sprite 模式
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                if(importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.SaveAndReimport();
                }
            }

            // 加载图片资源
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (tex != null)
            {
                sprites.Add(tex);
            } 
        }

        if (sprites.Count == 0)
        {
            Debug.Log($"跳过空文件夹：{sourceFolder}");
            return;
        }

        // 创建图集
        SpriteAtlas atlas = new SpriteAtlas();

        // 设置 Packing 参数（是否旋转、是否紧贴、边距）
        atlas.SetPackingSettings(new SpriteAtlasPackingSettings
        {
            enableRotation = false,
            enableTightPacking = false,
            padding = 2
        });

        // 设置纹理导入参数（滤镜、Mipmap、sRGB）
        atlas.SetTextureSettings(new SpriteAtlasTextureSettings
        {
            readable = false,
            generateMipMaps = false,
            sRGB = true,
            filterMode = FilterMode.Bilinear
        });

        atlas.Add(sprites.ToArray());

        // 将图集保存为 .spriteatlas 资源文件
        AssetDatabase.CreateAsset(atlas, atlasPath);

        Debug.Log($"生成图集成功：{atlasPath}（共 {sprites.Count} 张图片）");
    }
}
