using System;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

[CustomEditor(typeof(SmartTextureImporter), true)]
public class SmartTextureImporterEditor : ScriptedImporterEditor
{
    internal static class Styles
    {
        public static readonly GUIContent[] labelChannels =
        {
            EditorGUIUtility.TrTextContent("Red Channel"),
            EditorGUIUtility.TrTextContent("Green Channel"),
            EditorGUIUtility.TrTextContent("Blue Channel"),
            EditorGUIUtility.TrTextContent("Blue Channel"),
        };
        
        public static readonly GUIContent readWrite = EditorGUIUtility.TrTextContent("Read/Write Enabled", "Enable to be able to access the raw pixel data from code.");
        public static readonly GUIContent generateMipMaps = EditorGUIUtility.TrTextContent("Generate Mip Maps");
        public static readonly GUIContent sRGBTexture = EditorGUIUtility.TrTextContent("sRGB (Color Texture)", "Texture content is stored in gamma space. Non-HDR color textures should enable this flag (except if used for IMGUI).");

        public static readonly GUIContent textureFilterMode = EditorGUIUtility.TrTextContent("Filter Mode");
        public static readonly GUIContent textureWrapMode = EditorGUIUtility.TrTextContent("Wrap Mode");
        public static readonly GUIContent textureAnisotropicLevel = EditorGUIUtility.TrTextContent("Aniso Level");

        public static readonly GUIContent crunchCompression = EditorGUIUtility.TrTextContent("Use Crunch Compression");

        public static readonly string[] textureSizeOptions =
        {
            "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192",
        };

        public static readonly string[] textureCompressionOptions = Enum.GetNames(typeof(TextureImporterCompression));
        public static readonly string[] resizeAlgorithmOptions = Enum.GetNames(typeof(TextureResizeAlgorithm));
    }

    SerializedProperty[] m_InputTextures = new SerializedProperty[4];
    SerializedProperty[] m_InputTextureSettings = new SerializedProperty[4];
    
    SerializedProperty m_IsReadableProperty;
    SerializedProperty m_sRGBTextureProperty;
    
    SerializedProperty m_EnableMipMapProperty;
    
    SerializedProperty m_FilterModeProperty;
    SerializedProperty m_WrapModeProperty;
    SerializedProperty m_AnisotropiceLevelPropery;

    SerializedProperty m_TexturePlatformSettingsProperty;

    bool m_ShowAdvanced = false;

    const string k_AdvancedTextureSettingName = "SmartTextureImporterShowAdvanced";
        
    public override void OnEnable()
    {
        base.OnEnable();
        CacheSerializedProperties();
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        m_ShowAdvanced = EditorPrefs.GetBool(k_AdvancedTextureSettingName, m_ShowAdvanced);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Input Masks", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        DrawInputTexture(0);
        DrawInputTexture(1);
        DrawInputTexture(2);
        DrawInputTexture(3);
        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Output Texture", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(m_EnableMipMapProperty, Styles.generateMipMaps);
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(m_FilterModeProperty, Styles.textureFilterMode);
        EditorGUILayout.PropertyField(m_WrapModeProperty, Styles.textureWrapMode);

        EditorGUILayout.IntSlider(m_AnisotropiceLevelPropery, 0, 16, Styles.textureAnisotropicLevel);
        EditorGUILayout.Space();

        // TODO: Figure out how to apply TextureImporterSettings on ScriptedImporter
        //EditorGUILayout.PropertyField(m_IsReadableProperty, Styles.readWrite);
        //EditorGUILayout.PropertyField(m_sRGBTextureProperty, Styles.sRGBTexture);
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // TODO: Figure out how to apply PlatformTextureImporterSettings on ScriptedImporter
        //DrawTextureImporterSettings();
        //EditorGUILayout.Space();
        //EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
        ApplyRevertGUI();
    }

    void DrawInputTexture(int index)
    {
        if (index < 0 || index >= 4)
            return;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(m_InputTextures[index], Styles.labelChannels[index]);
        EditorGUILayout.PropertyField(m_InputTextureSettings[index]);
        EditorGUILayout.EndHorizontal();
    }

    void DrawTextureImporterSettings()
    {
        SerializedProperty maxTextureSize = m_TexturePlatformSettingsProperty.FindPropertyRelative("m_MaxTextureSize");
        SerializedProperty resizeAlgorithm =
            m_TexturePlatformSettingsProperty.FindPropertyRelative("m_ResizeAlgorithm");
        SerializedProperty textureCompression =
            m_TexturePlatformSettingsProperty.FindPropertyRelative("m_TextureCompression");
        SerializedProperty textureCompressionCrunched =
            m_TexturePlatformSettingsProperty.FindPropertyRelative("m_CrunchedCompression");

        EditorGUILayout.LabelField("Texture Platform Settings", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        int sizeOption = EditorGUILayout.Popup("Texture Size", (int)Mathf.Log(maxTextureSize.intValue, 2) - 5, Styles.textureSizeOptions);
        if (EditorGUI.EndChangeCheck())
            maxTextureSize.intValue = 32 << sizeOption;
        
        EditorGUI.BeginChangeCheck();
        int resizeOption = EditorGUILayout.Popup("Resize Algorithm", resizeAlgorithm.intValue, Styles.resizeAlgorithmOptions);
        if (EditorGUI.EndChangeCheck())
            resizeAlgorithm.intValue = resizeOption; 

        EditorGUI.BeginChangeCheck();
        int compressionOption = EditorGUILayout.Popup("Compression", textureCompression.intValue, Styles.textureCompressionOptions);
        if (EditorGUI.EndChangeCheck())
            textureCompression.intValue = compressionOption;

        EditorGUI.BeginChangeCheck();
        bool crunchOption = EditorGUILayout.Toggle(Styles.crunchCompression, textureCompressionCrunched.boolValue);
        if (EditorGUI.EndChangeCheck())
            textureCompressionCrunched.boolValue = crunchOption;
    }
    
    void CacheSerializedProperties()
    {
        SerializedProperty texturesProperty = serializedObject.FindProperty("m_InputTextures");
        SerializedProperty settingsProperty = serializedObject.FindProperty("m_InputTextureSettings");
        for (int i = 0; i < 4; ++i)
        {
            m_InputTextures[i] = texturesProperty.GetArrayElementAtIndex(i);
            m_InputTextureSettings[i] = settingsProperty.GetArrayElementAtIndex(i);
        }
        
        m_IsReadableProperty = serializedObject.FindProperty("m_IsReadable");
        m_sRGBTextureProperty = serializedObject.FindProperty("m_sRGBTexture");
        m_EnableMipMapProperty = serializedObject.FindProperty("m_EnableMipMap");
        
        m_FilterModeProperty = serializedObject.FindProperty("m_FilterMode");
        m_WrapModeProperty = serializedObject.FindProperty("m_WrapMode");
        m_AnisotropiceLevelPropery = serializedObject.FindProperty("m_AnisotricLevel");

        m_TexturePlatformSettingsProperty = serializedObject.FindProperty("m_TexturePlatformSettings");
    }
}
