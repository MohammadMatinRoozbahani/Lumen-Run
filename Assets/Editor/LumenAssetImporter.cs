using UnityEditor;
using UnityEngine;

public sealed class LumenAssetImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Resources/Art/")) return;
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Default;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Bilinear;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.isReadable = true;
        importer.maxTextureSize = 2048;
    }

    static void OnPostprocessAllAssets(string[] imported, string[] deleted,
        string[] moved, string[] movedFrom, bool didDomainReload)
    {
        LumenProjectSetup.Schedule();
    }
}
