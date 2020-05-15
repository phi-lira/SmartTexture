using System.IO;
using UnityEditor;
using UnityEngine;

public class TextureChannelPackerWindow : EditorWindow
{
    [MenuItem("Window/TextureChannelPacker")]
    public static void ShowWindow()
    {
        var window = GetWindow<TextureChannelPackerWindow>();
        window.Show();
    }

    static class Styles
    {
        public static readonly GUIContent generateButton = EditorGUIUtility.TrTextContent("Generate Texture", "Generate packed texture.");
        public static readonly GUIContent generateOnGPU = EditorGUIUtility.TrTextContent("Generate On GPU", "Generate on GPU. Doesn't require textures to be enable read/write");
    }

    public Texture2D m_RedChannel;
    public Texture2D m_GreenChannel;
    public Texture2D m_BlueChannel;
    public Texture2D m_AlphaChannel;
    public bool m_GenerateOnGPU = true;
    public Vector4 m_InvertColor = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
    public Vector4 m_sRGBToLinear = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
    PackSettings m_PackSettings;
    
    Texture2D DrawTexturePreviewBox(string label, Texture2D previewObject, ref float invertColor)
    {
        EditorGUILayout.LabelField(label);
        EditorGUILayout.BeginHorizontal();
        bool invert = EditorGUILayout.Toggle("Invert Color", invertColor > 0.0f);
        invertColor = (invert) ? 1.0f : 0.0f;
        var texture = DrawTexturePreview("", previewObject);
        EditorGUILayout.EndHorizontal();
        
        if (!m_GenerateOnGPU && texture != null && !texture.isReadable)
        {
            EditorGUILayout.HelpBox("Texture is not readable", MessageType.Error);
        }

        return texture;
    }
    
    Texture2D DrawTexturePreview(string label, Texture2D previewObject)
    {
        return (Texture2D)EditorGUILayout.ObjectField(label, previewObject, typeof(Texture2D), false);
    }
    
    void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Input Masks", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;

        m_GenerateOnGPU = EditorGUILayout.Toggle(Styles.generateOnGPU, m_GenerateOnGPU);
        
        m_PackSettings.generateOnGPU = m_GenerateOnGPU;
        m_RedChannel = DrawTexturePreviewBox("Red Channel", m_RedChannel, ref m_PackSettings.invertColor.x);
        m_GreenChannel = DrawTexturePreviewBox( "Green Channel", m_GreenChannel, ref m_PackSettings.invertColor.y);
        m_BlueChannel = DrawTexturePreviewBox( "Blue Channel", m_BlueChannel, ref m_PackSettings.invertColor.z);
        m_AlphaChannel = DrawTexturePreviewBox("Alpha Channel", m_AlphaChannel, ref m_PackSettings.invertColor.w);

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        int width, height;
        Texture2D[] textures = {m_RedChannel, m_BlueChannel, m_GreenChannel, m_AlphaChannel};
        bool canGenerateTexture = GetOuputTextureSize(textures, out width, out height);
        
        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
        if (!canGenerateTexture)
        {
            DrawTexturePreview("RGB", Texture2D.blackTexture);
            return;
        }

        Texture2D mask = new Texture2D(width, height, TextureFormat.ARGB32, false);
        mask.PackChannels(textures[0], textures[1], textures[2], textures[3], m_PackSettings);
        DrawTexturePreview("RGB", mask);
        EditorGUILayout.Space();

        var buttonRect = GUILayoutUtility.GetRect(Styles.generateButton, GUIStyle.none);
        buttonRect.height *= 2;
        if (GUI.Button(buttonRect, Styles.generateButton))
        {
            var path = EditorUtility.SaveFilePanel(
                "Save texture",
                "",
                "mask.png",
                "png");
            
            if (path.Length > 0)
            {
                var png = mask.EncodeToPNG();
                File.WriteAllBytes(path, png);
            }
        }
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
