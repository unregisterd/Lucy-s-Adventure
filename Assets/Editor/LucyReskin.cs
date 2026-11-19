using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Lucy 换贴图工具：菜单在 Tools/Lucy 下。
// 原理：材质的贴图引用靠 .meta 里的 guid，所以只要保持 png 文件名与 .meta 不变、只替换像素，
// 模型里内嵌材质、外部材质、以及各处副本都会自动用上新像素，完全不用碰导入设置。
public static class LucyReskin
{
    const string PlayerRoot = "Assets/Resources/Player";
    const string ModelPath = "Assets/Resources/Player/Model/Lucy.fbx";
    const string PrimaryFolder = "Assets/Resources/Player/Textures";
    const string FbmFolder = "Assets/Resources/Player/Model/Lucy.fbm";

    // 衣服类贴图：只换这些时，脸、眼睛、头发保持不动。
    static readonly string[] ClothesNames =
    {
        "lucy_hi_cn_sweater_b_base_color",
        "lucy_hi_cn_pants_b_base_color",
        "lucy_hi_cn_pants_straps_b_base_color",
        "lucy_hi_cn_hood_b_base_color",
        "lucy_hi_cn_belt_b_base_color",
        "lucy_hi_cn_belt_buckle_b_base_color",
        "lucy_hi_lf_hood_cord_b_base_color",
        "lucy_hi_rt_hood_cord_b_base_color",
        "lucy_hi_lf_hood_buttonhole_b_base_color",
        "lucy_hi_rt_hood_buttonhole_b_base_color",
        "lucy_hi_lf_pocket_b_base_color",
        "lucy_hi_rt_pocket_b_base_color",
        "lucy_hi_lf_shoe_b_base_color",
        "lucy_hi_rt_shoe_b_base_color",
        "lucy_hi_lf_shoelaces_b_base_color",
        "lucy_hi_rt_shoelaces_b_base_color",
    };

    // ---------- 整套套用 ----------

    // 原始 png 的备份放在工程外面（AssetDatabase 管不到，所以直接按文件读）。
    const string BackupFolder = @"C:\Users\Administrator\Documents\deepseek-harness\default-workspace\lucy-texture-backup\Textures";

    [MenuItem("Tools/Lucy/套用整套贴图/恢复原始 png（备份）", false, 8)]
    static void ApplyOriginal()
    {
        if (!Directory.Exists(BackupFolder))
        {
            Debug.LogError($"[LucyReskin] 找不到原始贴图备份：{BackupFolder}");
            return;
        }
        ApplyFolder(BackupFolder, null);
    }

    [MenuItem("Tools/Lucy/套用整套贴图/Textures2（蓝眼·灰衣）", false, 11)]
    static void ApplyTextures2()
    {
        ApplyFolder("Assets/Resources/Player/Textures2", null);
    }

    [MenuItem("Tools/Lucy/套用整套贴图/Textures3（棕衣）", false, 12)]
    static void ApplyTextures3()
    {
        ApplyFolder("Assets/Resources/Player/Textures3", null);
    }

    [MenuItem("Tools/Lucy/套用整套贴图/选择文件夹…", false, 13)]
    static void ApplyAnyFolder()
    {
        string folder = EditorUtility.OpenFolderPanel("选择贴图文件夹", PlayerRoot, "");
        if (string.IsNullOrEmpty(folder)) return;
        ApplyFolder(folder, null);
    }

    // ---------- 只换衣服 ----------

    [MenuItem("Tools/Lucy/只换衣服/套用 Textures3 的棕色衣服（脸不动）", false, 20)]
    static void ApplyClothesFromTextures3()
    {
        ApplyFolder("Assets/Resources/Player/Textures3", ClothesNames);
    }

    [MenuItem("Tools/Lucy/只换衣服/套用 Textures2 的灰色衣服（脸不动）", false, 21)]
    static void ApplyClothesFromTextures2()
    {
        ApplyFolder("Assets/Resources/Player/Textures2", ClothesNames);
    }

    // ---------- 核心 ----------

    static void ApplyFolder(string sourceFolder, string[] onlyNames)
    {
        if (!Directory.Exists(sourceFolder))
        {
            Debug.LogError($"[LucyReskin] 源文件夹不存在：{sourceFolder}");
            return;
        }

        var source = IndexFolder(sourceFolder);
        var names = onlyNames != null ? new List<string>(onlyNames) : new List<string>(source.Keys);

        int applied = 0;
        var noSource = new List<string>();
        var noTarget = new List<string>();

        foreach (var name in names)
        {
            string sourcePath;
            if (!source.TryGetValue(name, out sourcePath))
            {
                noSource.Add(name);
                continue;
            }

            bool wroteAny = false;
            foreach (var targetFolder in new[] { PrimaryFolder, FbmFolder })
            {
                string targetPath = targetFolder + "/" + name + ".png";
                if (!File.Exists(targetPath)) continue;

                byte[] png = BuildPng(sourcePath, targetPath);
                if (png == null) continue;
                File.WriteAllBytes(targetPath, png);
                AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
                wroteAny = true;
                applied++;
            }

            if (!wroteAny) noTarget.Add(name);
        }

        AssetDatabase.SaveAssets();

        string scope = onlyNames != null ? "只换衣服" : "整套";
        Debug.Log($"[LucyReskin] {scope}：{Path.GetFileName(sourceFolder)} → 已替换 {applied} 个 png 文件（名字与 .meta 未动）。");

        if (noSource.Count > 0)
            Debug.LogWarning("[LucyReskin] 源文件夹里没有这些贴图：\n - " + string.Join("\n - ", noSource));
        if (noTarget.Count > 0)
            Debug.LogWarning("[LucyReskin] 目标目录里没有这些 png：\n - " + string.Join("\n - ", noTarget));
    }

    static byte[] BuildPng(string sourcePath, string targetPath)
    {
        var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!source.LoadImage(File.ReadAllBytes(sourcePath)))
        {
            Debug.LogError($"[LucyReskin] 读不出图片：{sourcePath}");
            Object.DestroyImmediate(source);
            return null;
        }

        // 源图是 jpg 时没有 alpha；目标原图有 alpha 的（例如腰带扣）沿用原 alpha，避免发黑或丢边。
        var original = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        original.LoadImage(File.ReadAllBytes(targetPath));
        if (original.width == source.width && original.height == source.height)
        {
            var sourcePixels = source.GetPixels32();
            var originalPixels = original.GetPixels32();
            for (int i = 0; i < sourcePixels.Length && i < originalPixels.Length; i++)
                sourcePixels[i].a = originalPixels[i].a;
            source.SetPixels32(sourcePixels);
            source.Apply();
        }
        Object.DestroyImmediate(original);

        byte[] bytes = source.EncodeToPNG();
        Object.DestroyImmediate(source);
        return bytes;
    }

    static Dictionary<string, string> IndexFolder(string folder)
    {
        var map = new Dictionary<string, string>();
        foreach (var file in Directory.GetFiles(folder))
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") continue;
            string name = Path.GetFileNameWithoutExtension(file);
            if (!map.ContainsKey(name)) map[name] = file.Replace('\\', '/');
        }
        return map;
    }

    // ---------- 诊断 ----------

    [MenuItem("Tools/Lucy/诊断/贴图文件夹概览", false, 40)]
    static void Overview()
    {
        if (!Directory.Exists(PlayerRoot))
        {
            Debug.LogError($"[LucyReskin] 找不到 {PlayerRoot}");
            return;
        }

        string primary = PrimaryFolder;
        int primaryCount = 0;
        var primarySizes = new Dictionary<string, Vector2Int>();
        foreach (var file in Directory.GetFiles(primary))
        {
            if (Path.GetExtension(file).ToLowerInvariant() != ".png") continue;
            primaryCount++;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(file.Replace('\\', '/'));
            if (texture != null) primarySizes[Path.GetFileNameWithoutExtension(file)] = new Vector2Int(texture.width, texture.height);
        }

        Debug.Log($"[LucyReskin] 当前生效目录 {primary}：{primaryCount} 张 png。");

        foreach (var dir in Directory.GetDirectories(PlayerRoot))
        {
            string name = Path.GetFileName(dir);
            if (name == "Textures" || name == "Materials" || name == "Weapons" || name == "Model") continue;

            int count = 0;
            var different = new List<string>();
            foreach (var file in Directory.GetFiles(dir))
            {
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") continue;
                count++;
                string key = Path.GetFileNameWithoutExtension(file);
                Vector2Int size;
                if (!primarySizes.TryGetValue(key, out size)) continue;
                var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(file.Replace('\\', '/'));
                if (texture != null && (texture.width != size.x || texture.height != size.y))
                    different.Add($"{key}（{size.x}x{size.y} → {texture.width}x{texture.height}）");
            }

            string extra = different.Count > 0
                ? "尺寸不同的 " + different.Count + " 张：" + string.Join("、", different)
                : "尺寸与当前目录一致";
            Debug.Log($"[LucyReskin] 备选目录 {name}：{count} 张，{extra}");
        }
    }

    [MenuItem("Tools/Lucy/诊断/Lucy 的材质状态", false, 41)]
    static void MaterialState()
    {
        var importer = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError($"[LucyReskin] 找不到 ModelImporter：{ModelPath}");
            return;
        }

        int mats = Directory.Exists(FbmFolder) ? Directory.GetFiles(FbmFolder, "*.mat").Length : 0;
        string location = importer.materialLocation == ModelImporterMaterialLocation.External
            ? "外部材质（改 .mat 会生效）"
            : "内嵌在模型里（改 .mat 不会生效，只能换贴图像素）";

        Debug.Log($"[LucyReskin] Lucy 材质位置：{location}；{FbmFolder} 里的 .mat 数量：{mats}。");

        if (mats == 0 && importer.materialLocation == ModelImporterMaterialLocation.External)
            Debug.LogWarning("[LucyReskin] 导入设置说是外部材质，但没找到 .mat —— Unity 的 FBXImporter 一致性检查可能把改动挡回去了。");
    }
}
