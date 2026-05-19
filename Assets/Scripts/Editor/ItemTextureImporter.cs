using UnityEditor;

/// <summary>
/// Configura automáticamente las texturas de la carpeta Items para que
/// tengan Alpha Is Transparency activado y se vean sin fondo en el juego.
/// </summary>
public class ItemTextureImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.Contains("Sprites/Items/") || !assetPath.EndsWith(".png"))
            return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType         = TextureImporterType.Default;
        importer.alphaSource         = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled       = false;
        importer.filterMode          = UnityEngine.FilterMode.Point;
        importer.maxTextureSize      = 256;
    }
}
