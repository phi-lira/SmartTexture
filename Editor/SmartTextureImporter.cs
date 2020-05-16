using System.IO;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[ScriptedImporter(k_SmartTextureVersion, k_SmartTextureExtesion)]
public class SmartTextureImporter : ScriptedImporter
{
    public const string k_SmartTextureExtesion = "smartex";
    public const int k_SmartTextureVersion = 1;
    
    [SerializeField] Texture2D m_RedChannel = null;
    [SerializeField] Texture2D m_GreenChannel = null;
    [SerializeField] Texture2D m_BlueChannel = null;
    [SerializeField] Texture2D m_AlphaChannel = null;

    [SerializeField] Vector4 m_InvertColor = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

    [MenuItem("Assets/Create/Smart Texture", priority = 310)]
    static void CreateTexture2DArrayMenuItem()
    {
        // https://forum.unity.com/threads/how-to-implement-create-new-asset.759662/
        string directoryPath = "Assets";
        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            directoryPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(directoryPath) && File.Exists(directoryPath))
            {
                directoryPath = Path.GetDirectoryName(directoryPath);
                break;
            }
        }

        directoryPath = directoryPath.Replace("\\", "/");
        if (directoryPath.Length > 0 && directoryPath[directoryPath.Length - 1] != '/')
            directoryPath += "/";
        if (string.IsNullOrEmpty(directoryPath))
            directoryPath = "Assets/";

        var fileName = string.Format("SmartTexture.{0}", k_SmartTextureExtesion);
        directoryPath = AssetDatabase.GenerateUniqueAssetPath(directoryPath + fileName);
        ProjectWindowUtil.CreateAssetWithContent(directoryPath,
            "SmartTexture.");
    }
    
    public override void OnImportAsset(AssetImportContext ctx)
    {
        PackSettings packSettings;
        packSettings.invertColor = Vector4.zero;
        packSettings.generateOnGPU = true;

        int width = 512;
        int height = 512;
        Texture2D[] textures = {m_RedChannel, m_GreenChannel, m_BlueChannel, m_AlphaChannel};
        bool canGenerateTexture = GetOuputTextureSize(textures, out width, out height);

        Texture2D mask = new Texture2D(width, height, TextureFormat.ARGB32, false);
        if (canGenerateTexture)
        {
            mask.PackChannels(textures[0], textures[1], textures[2], textures[3], packSettings);
            
            // Mark all input textures as dependency to the texture array.
            // This causes the texture array to get re-generated when any input texture changes or when the build target changed.
            foreach (Texture2D t in textures)
            {
                if (t != null)
                {
                    var path = AssetDatabase.GetAssetPath(t);
                    ctx.DependsOnSourceAsset(path);
                }
            }
        }
        
        ctx.AddObjectToAsset("mask", mask);
        ctx.SetMainObject(mask);
    }
    
    bool GetOuputTextureSize(Texture2D[] textures, out int width, out int height)
    {
        Texture2D masterTexture = null;
        foreach (Texture2D t in textures)
        {
            if (t != null && t.isReadable)
            {
                masterTexture = t;
                break;
            }
        }

        if (masterTexture == null)
        {
            var defaultTexture = Texture2D.blackTexture;
            width = defaultTexture.width;
            height = defaultTexture.height;
            return false;
        }

        width = masterTexture.width;
        height = masterTexture.height;
        return true;
    }
}
