using UnityEngine;
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
    
    /// <summary>
    /// Outputs the inverted color (1.0 - color)
    /// </summary>
    public int sourceChannel;
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

        foreach (Texture2D t in textures)
        {
            if (t != null)
            {
                if (!generateOnGPU && !t.isReadable)
                {
                    Debug.LogError(t + " texture is not readable. Toogle Read/Write Enable in texture importer. ");
                    return;
                }

                if (t.width != width || t.height != height)
                {
                    Debug.LogError(t + " input textures must have the same size.");
                    return;
                }
            }
        }

        if (generateOnGPU)
        {
            float[] invertColor =
            {
                settings[0].invertColor ? 1.0f : 0.0f,
                settings[1].invertColor ? 1.0f : 0.0f,
                settings[2].invertColor ? 1.0f : 0.0f,
                settings[3].invertColor ? 1.0f : 0.0f,
            };

            int[] channelSource =
            {
                settings[0].sourceChannel,
                settings[1].sourceChannel,
                settings[2].sourceChannel,
                settings[3].sourceChannel
            };
            
            packChannelMaterial.SetTexture("_RedChannel", textures[0] != null ? textures[0] : Texture2D.blackTexture);
            packChannelMaterial.SetTexture("_GreenChannel", textures[1] != null ? textures[1] : Texture2D.blackTexture);
            packChannelMaterial.SetTexture("_BlueChannel", textures[2] != null ? textures[2] : Texture2D.blackTexture);
            packChannelMaterial.SetTexture("_AlphaChannel", textures[3] != null ? textures[3] : Texture2D.blackTexture);
            packChannelMaterial.SetVector("_InvertColor", new Vector4(invertColor[0], invertColor[1], invertColor[2], invertColor[3]));
            packChannelMaterial.SetInt("_RedSource", channelSource[0]);
            packChannelMaterial.SetInt("_GreenSource", channelSource[1]);
            packChannelMaterial.SetInt("_BlueSource", channelSource[2]);
            packChannelMaterial.SetInt("_AlphaSource", channelSource[3]);

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
