using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

[CustomEditor(typeof(SmartTextureImporter), true)]
public class SmartTextureImporterEditor : ScriptedImporterEditor
{
    SerializedProperty m_RedChannelProperty;
    SerializedProperty m_GreenChannelProperty;
    SerializedProperty m_BlueChannelProperty;
    SerializedProperty m_AlphaChannelProperty;
    SerializedProperty m_InvertColor;

    internal static class Styles
    {
        public static readonly GUIContent redChannel = EditorGUIUtility.TrTextContent("Red Channel");
        public static readonly GUIContent greenChannel = EditorGUIUtility.TrTextContent("Green Channel");
        public static readonly GUIContent blueChannel = EditorGUIUtility.TrTextContent("Blue Channel");
        public static readonly GUIContent alphaChannel = EditorGUIUtility.TrTextContent("Alpha Channel");
    }
    public override void OnEnable()
    {
        base.OnEnable();

        m_RedChannelProperty = serializedObject.FindProperty("m_RedChannel");
        m_GreenChannelProperty = serializedObject.FindProperty("m_GreenChannel");
        m_BlueChannelProperty = serializedObject.FindProperty("m_BlueChannel");
        m_AlphaChannelProperty = serializedObject.FindProperty("m_AlphaChannel");
        m_InvertColor = serializedObject.FindProperty("m_AlphaChannel");
    }

    void DrawTexturePreviewBox(string label, SerializedProperty textureProperty, ref float invertColor)
    {
        Texture2D previewTexture = textureProperty.objectReferenceValue as Texture2D;
        if (previewTexture == null)
            return;
        
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField(label);
        EditorGUILayout.BeginHorizontal();
        bool invert = EditorGUILayout.Toggle("Invert Color", invertColor > 0.0f);
        invertColor = (invert) ? 1.0f : 0.0f;
        previewTexture = DrawTexturePreview("", previewTexture);
        EditorGUILayout.EndHorizontal();
        if (EditorGUI.EndChangeCheck())
        {
            textureProperty.objectReferenceValue = previewTexture;
        }
    }
    
    Texture2D DrawTexturePreview(string label, Texture2D previewObject)
    {
        return (Texture2D)EditorGUILayout.ObjectField(label, previewObject, typeof(Texture2D), false);
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Input Masks", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUI.indentLevel++;

        //DrawTexturePreviewBox("Red Channel", m_RedChannelProperty, ref a);
        //DrawTexturePreviewBox("Blue Channel", m_BlueChannelProperty, ref a);
        //DrawTexturePreviewBox("Green Channel", m_GreenChannelProperty, ref a);
        //DrawTexturePreviewBox("Alpha Channel", m_AlphaChannelProperty, ref a);
        EditorGUILayout.PropertyField(m_RedChannelProperty,Styles.redChannel);
        EditorGUILayout.PropertyField(m_GreenChannelProperty, Styles.greenChannel);
        EditorGUILayout.PropertyField(m_BlueChannelProperty, Styles.blueChannel);
        EditorGUILayout.PropertyField(m_AlphaChannelProperty, Styles.alphaChannel);
//        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        //EditorGUILayout.PropertyField(m_GreenChannelProperty, Styles.greenChannel);
        //EditorGUILayout.PropertyField(m_BlueChannelProperty, Styles.blueChannel);
        //EditorGUILayout.PropertyField(m_AlphaChannelProperty, Styles.alphaChannel);
//        m_GenerateOnGPU = EditorGUILayout.Toggle(Styles.generateOnGPU, m_GenerateOnGPU);
//        
//        m_PackSettings.generateOnGPU = m_GenerateOnGPU;
//        
//        m_RedChannel = DrawTexturePreview("Red Channel", m_RedChannel);
//        m_GreenChannel = DrawTexturePreview( "Green Channel", m_GreenChannel, ref m_PackSettings.invertColor.y);
//        m_BlueChannel = DrawTexturePreview( "Blue Channel", m_BlueChannel, ref m_PackSettings.invertColor.z);
//        m_AlphaChannel = DrawTexturePreview("Alpha Channel", m_AlphaChannel, ref m_PackSettings.invertColor.w);

//        EditorGUI.indentLevel--;
//        EditorGUILayout.Space();
//        EditorGUILayout.Space();
//
//        int width, height;
//        Texture2D[] textures =
//        {
//            (Texture2D)m_RedChannelProperty.objectReferenceValue,
//            (Texture2D)m_GreenChannelProperty.objectReferenceValue,
//            (Texture2D)m_BlueChannelProperty.objectReferenceValue,
//            (Texture2D)m_AlphaChannelProperty.objectReferenceValue,
//        };
//        bool canGenerateTexture = GetOuputTextureSize(textures, out width, out height);
//        
//        EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
//        if (!canGenerateTexture)
//        {
//            DrawTexturePreview("RGB", Texture2D.blackTexture);
//            return;
//        }

        //Texture2D mask = new Texture2D(width, height, TextureFormat.ARGB32, false);
        //mask.PackChannels(textures[0], textures[1], textures[2], textures[3], m_PackSettings);
        //DrawTexturePreview("RGB", mask);
        //EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();
        ApplyRevertGUI();
    }
}
