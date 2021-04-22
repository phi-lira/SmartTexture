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
            EditorGUIUtility.TrTextContent("Red Channel", "This texture source channel will be packed into the Output texture red channel"),
            EditorGUIUtility.TrTextContent("Green Channel", "This texture source channel will be packed into the Output texture green channel"),
            EditorGUIUtility.TrTextContent("Blue Channel", "This texture source channel will be packed into the Output texture blue channel"),
            EditorGUIUtility.TrTextContent("Alpha Channel", "This texture source channel will be packed into the Output texture alpha channel"),
        };

        public static readonly GUIContent sourceTexture = EditorGUIUtility.TrTextContent("Source Texture", "The texture from which the channel will be extracted");
        public static readonly GUIContent sourceChannel = EditorGUIUtility.TrTextContent("Source Channel", "The channel of the texture that should be extracted");
        
        public static readonly GUIContent invertColor = EditorGUIUtility.TrTextContent("Invert Color", "If enabled outputs the inverted color (1.0 - color)");
        
        public static readonly GUIContent readWrite = EditorGUIUtility.TrTextContent("Read/Write Enabled", "Enable to be able to access the raw pixel data from code.");
        public static readonly GUIContent generateMipMaps = EditorGUIUtility.TrTextContent("Generate Mip Maps");
        public static readonly GUIContent streamingMipMaps = EditorGUIUtility.TrTextContent("Streaming Mip Maps");
        public static readonly GUIContent streamingMipMapsPrio = EditorGUIUtility.TrTextContent("Streaming Mip Maps Priority");
        public static readonly GUIContent sRGBTexture = EditorGUIUtility.TrTextContent("sRGB (Color Texture)", "Texture content is stored in gamma space. Non-HDR color textures should enable this flag (except if used for IMGUI).");

        public static readonly GUIContent textureFilterMode = EditorGUIUtility.TrTextContent("Filter Mode");
        public static readonly GUIContent textureWrapMode = EditorGUIUtility.TrTextContent("Wrap Mode");
        public static readonly GUIContent textureAnisotropicLevel = EditorGUIUtility.TrTextContent("Anisotropic Level");

        public static readonly GUIContent crunchCompression = EditorGUIUtility.TrTextContent("Crunch");
        public static readonly GUIContent useExplicitTextureFormat = EditorGUIUtility.TrTextContent("Use Explicit Texture Format");

        public static readonly string[] textureSizeOptions =
        {
            "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192",
        };

        public static readonly string[] textureCompressionOptions = Enum.GetNames(typeof(TextureImporterCompression));
        public static readonly string[] textureChannels = Enum.GetNames(typeof(TextureChannels));
        public static readonly string[] textureFormat = Enum.GetNames(typeof(TextureFormat));
        public static readonly string[] resizeAlgorithmOptions = Enum.GetNames(typeof(TextureResizeAlgorithm));
    }

    SerializedProperty[] m_InputTextures = new SerializedProperty[4];
    SerializedProperty[] m_InputTextureSettings = new SerializedProperty[4];
    
    SerializedProperty m_IsReadableProperty;
    SerializedProperty m_sRGBTextureProperty;
    
    SerializedProperty m_EnableMipMapProperty;
    SerializedProperty m_StreamingMipMaps;
    SerializedProperty m_StreamingMipMapPriority;

    SerializedProperty m_FilterModeProperty;
    SerializedProperty m_WrapModeProperty;
    SerializedProperty m_AnisotropiceLevelPropery;

    SerializedProperty m_TexturePlatformSettingsProperty;

    SerializedProperty m_TextureFormat;
    SerializedProperty m_UseExplicitTextureFormat;

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
        EditorGUILayout.LabelField("Input Textures", EditorStyles.boldLabel);
        using (new EditorGUI.IndentLevelScope())
        {
            DrawInputTexture(0);
            DrawInputTexture(1);
            DrawInputTexture(2);
            DrawInputTexture(3);
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        EditorGUILayout.LabelField("Output Texture", EditorStyles.boldLabel);
        using (new EditorGUI.IndentLevelScope())
        {
            EditorGUILayout.PropertyField(m_EnableMipMapProperty, Styles.generateMipMaps);
            EditorGUILayout.PropertyField(m_StreamingMipMaps, Styles.streamingMipMaps);
            EditorGUILayout.PropertyField(m_StreamingMipMapPriority, Styles.streamingMipMapsPrio);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_FilterModeProperty, Styles.textureFilterMode);
            EditorGUILayout.PropertyField(m_WrapModeProperty, Styles.textureWrapMode);

            EditorGUILayout.IntSlider(m_AnisotropiceLevelPropery, 0, 16, Styles.textureAnisotropicLevel);
            EditorGUILayout.Space();

            // TODO: Figure out how to apply TextureImporterSettings on ScriptedImporter
            EditorGUILayout.PropertyField(m_IsReadableProperty, Styles.readWrite);
            EditorGUILayout.PropertyField(m_sRGBTextureProperty, Styles.sRGBTexture);
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // TODO: Figure out how to apply PlatformTextureImporterSettings on ScriptedImporter
        DrawTextureImporterSettings();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
        ApplyRevertGUI();
    }

    void DrawInputTexture(int index)
    {
        if (index < 0 || index >= 4)
            return;

        EditorGUILayout.LabelField(Styles.labelChannels[index], EditorStyles.boldLabel);
        using (new EditorGUI.IndentLevelScope())
        {
            EditorGUILayout.PropertyField(m_InputTextures[index], Styles.sourceTexture);

            SerializedProperty sourceChannel = m_InputTextureSettings[index].FindPropertyRelative("sourceChannel");
            sourceChannel.intValue = EditorGUILayout.Popup(Styles.sourceChannel, sourceChannel.intValue, Styles.textureChannels);

        SerializedProperty invertColor = m_InputTextureSettings[index].FindPropertyRelative("invertColor");
        invertColor.boolValue = EditorGUILayout.Toggle(Styles.invertColor, invertColor.boolValue);

        EditorGUILayout.Space();
                
        }
        
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
        using (new EditorGUI.IndentLevelScope())
        {
            EditorGUI.BeginChangeCheck();
            int sizeOption = EditorGUILayout.Popup("Texture Size", (int)Mathf.Log(maxTextureSize.intValue, 2) - 5, Styles.textureSizeOptions);
            if (EditorGUI.EndChangeCheck())
                maxTextureSize.intValue = 32 << sizeOption;

            EditorGUI.BeginChangeCheck();
            int resizeOption = EditorGUILayout.Popup("Resize Algorithm", resizeAlgorithm.intValue, Styles.resizeAlgorithmOptions);
            if (EditorGUI.EndChangeCheck())
                resizeAlgorithm.intValue = resizeOption;

            EditorGUILayout.LabelField("Compression", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.BeginChangeCheck();
                bool explicitFormat = EditorGUILayout.Toggle(Styles.useExplicitTextureFormat, m_UseExplicitTextureFormat.boolValue);
                if (EditorGUI.EndChangeCheck())
                    m_UseExplicitTextureFormat.boolValue = explicitFormat;

                using (new EditorGUI.DisabledScope(explicitFormat))
                {
                    GUILayout.BeginHorizontal();
                    EditorGUI.BeginChangeCheck();
                    int compressionOption = EditorGUILayout.Popup("Texture Type", textureCompression.intValue, Styles.textureCompressionOptions);
                    if (EditorGUI.EndChangeCheck())
                        textureCompression.intValue = compressionOption;

                    EditorGUI.BeginChangeCheck();
                    var oldWidth = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 100f;
                    bool crunchOption = EditorGUILayout.Toggle(Styles.crunchCompression, textureCompressionCrunched.boolValue);
                    EditorGUIUtility.labelWidth = oldWidth;
                    if (EditorGUI.EndChangeCheck())
                        textureCompressionCrunched.boolValue = crunchOption;
                    GUILayout.EndHorizontal();
                }

                using (new EditorGUI.DisabledScope(!explicitFormat))
                {
                    EditorGUI.BeginChangeCheck();
                    int format = EditorGUILayout.EnumPopup("Texture Format", (TextureFormat)m_TextureFormat.intValue).GetHashCode();//("Compression", m_TextureFormat.enumValueIndex, Styles.textureFormat);
                    if (EditorGUI.EndChangeCheck())
                        m_TextureFormat.intValue = format;
                }
            }
        }
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
        m_StreamingMipMaps = serializedObject.FindProperty("m_StreamingMipMaps");
        m_StreamingMipMapPriority = serializedObject.FindProperty("m_StreamingMipMapPriority");

        m_FilterModeProperty = serializedObject.FindProperty("m_FilterMode");
        m_WrapModeProperty = serializedObject.FindProperty("m_WrapMode");
        m_AnisotropiceLevelPropery = serializedObject.FindProperty("m_AnisotricLevel");

        m_TexturePlatformSettingsProperty = serializedObject.FindProperty("m_TexturePlatformSettings");
        m_TextureFormat = serializedObject.FindProperty("m_TextureFormat");
        m_UseExplicitTextureFormat = serializedObject.FindProperty("m_UseExplicitTextureFormat");
    }
}
