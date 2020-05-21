﻿using UnityEngine;
using UnityEngine.Rendering;

// TODO: Add option to pack using compute shader
// TODO: Convert from sRGB to linear color space if necessary.
// TODO: Texture compression / Format
// TODO: Mipmaps

[System.Serializable]
/// <summary>
/// Containts settings that apply color modifiers to each channel.
/// </summary>
public struct TexturePackingSettings
{
    /// <summary>
    /// Outputs the inverted color (1.0 - color)
    /// </summary>
    public bool invertColor;
}

public static class TextureExtension
{
    static Material s_PackChannelMaterial = null;

    private static Material packChannelMaterial
    {
        get
        {
            if (s_PackChannelMaterial == null)
            {
                Shader packChannelShader = Shader.Find("Hidden/PackChannel");
                if (packChannelShader == null)
                    return null;
                
                s_PackChannelMaterial = new Material(packChannelShader);
            }

            return s_PackChannelMaterial;
        }
    }

    static bool CompressedTextureFormat(TextureFormat format)
    {
        //There should be a much better way to work this out, this might not even be an exhaustive list :(
        return format == TextureFormat.BC4
            || format == TextureFormat.BC5
            || format == TextureFormat.BC6H
            || format == TextureFormat.BC7
            || format == TextureFormat.DXT1
            || format == TextureFormat.DXT1Crunched
            || format == TextureFormat.DXT5
            || format == TextureFormat.DXT5Crunched;
    }
    public static void PackChannels(this Texture2D mask, Texture2D[] textures, TexturePackingSettings[] settings = null, bool generateOnGPU = true)
    {
        if (textures == null || textures.Length != 4)
        {
            Debug.LogError("Invalid parameter to PackChannels. An array of 4 textures is expected");
        }

        if (settings == null)
        {
            settings = new TexturePackingSettings[4];
            for (int i = 0; i < settings.Length; ++i)
            {
                settings[i].invertColor = false;
            }
        }
        
        int width = mask.width;
        int height = mask.height;
        int pixelCount = width * height;
        bool invalidTextures = false;

        for (int i = 0; i < textures.Length; i++)
        {
            var t = textures[i];
            if (t != null)
            {
                if (!generateOnGPU && !t.isReadable)
                {
                    Debug.LogError($"SmartTexture Aborting: {t.name} texture is not readable so we cannot import on CPU. Toogle Read/Write Enable in texture importer. ", t);
                    invalidTextures = true;
                }

                if (t.width != width || t.height != height)
                {
                    Debug.LogWarning($"SmartTexture: {t.name} does not match expected size. This can cause artfacts as the texture will be sampled onto a different size target, mismatches are not advised", t);
                }

                if (CompressedTextureFormat(t.format))
                {
                    Debug.LogWarning($"SmartTexture: {t.name} is already compressed, channel {i} will be double compressed. Please use an uncompressed format, input texture size isn't relevant to the build", t);
                }
            }
        }
        if (invalidTextures) return;

        if (generateOnGPU)
        {
            float[] invertColor =
            {
                settings[0].invertColor ? 1.0f : 0.0f,
                settings[1].invertColor ? 1.0f : 0.0f,
                settings[2].invertColor ? 1.0f : 0.0f,
                settings[3].invertColor ? 1.0f : 0.0f,
            };
            packChannelMaterial.SetTexture("_RedChannel", textures[0] != null ? textures[0] : Texture2D.blackTexture);
            packChannelMaterial.SetTexture("_GreenChannel", textures[1] != null ? textures[1] : Texture2D.blackTexture);
            packChannelMaterial.SetTexture("_BlueChannel", textures[2] != null ? textures[2] : Texture2D.blackTexture);
            packChannelMaterial.SetTexture("_AlphaChannel", textures[3] != null ? textures[3] : Texture2D.blackTexture);
            packChannelMaterial.SetVector("_InvertColor", new Vector4(invertColor[0], invertColor[1], invertColor[2], invertColor[3]));

            var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.Linear);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            CommandBuffer cmd = new CommandBuffer();
            cmd.Blit(null, rt, packChannelMaterial);
            cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            Graphics.ExecuteCommandBuffer(cmd);
            mask.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            mask.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
        }
        else
        {
            Color[][] pixels = 
            {
                textures[0] != null ? textures[0].GetPixels(0) : null,
                textures[1] != null ? textures[1].GetPixels(0) : null,
                textures[2] != null ? textures[2].GetPixels(0) : null,
                textures[3] != null ? textures[3].GetPixels(0) : null,
            };
            
            Color[] maskPixels = new Color[pixelCount];
            for (int i = 0; i < pixelCount; ++i)
            {
                float r = (textures[0] != null) ? pixels[0][i].r : 0.0f;
                float g = (textures[1] != null) ? pixels[1][i].r : 0.0f;
                float b = (textures[2] != null) ? pixels[2][i].r : 0.0f;
                float a = (textures[3] != null) ? pixels[3][i].r : 0.0f;

                if (settings[0].invertColor)
                    r = 1.0f - r;

                if (settings[1].invertColor)
                    g = 1.0f - g;

                if (settings[2].invertColor)
                    b = 1.0f - b;

                if (settings[3].invertColor)
                    a = 1.0f - a;

                maskPixels[i] = new Color(r, g, b, a);
            }

            mask.SetPixels(maskPixels, 0);
            mask.Apply();
        }
    }
}
