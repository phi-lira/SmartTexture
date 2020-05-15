using UnityEngine;
using UnityEngine.Rendering;

// TODO: Add option to pack using compute shader
// TODO: Convert from sRGB to linear color space if necessary.
// TODO: Texture compression / Format
// TODO: Mipmaps

/// <summary>
/// Containts settings that apply color modifiers to each channel.
/// </summary>
public struct PackSettings
{
    /// <summary>
    /// Outputs the inverted color (1.0 - color)
    /// </summary>
    public Vector4 invertColor;

    /// <summary>
    /// Pack textures on GPU. This doesn't require texture to enable read/write.
    /// </summary>
    public bool generateOnGPU;
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

    public static void PackChannels(this Texture2D mask, Texture2D red, Texture2D green, Texture2D blue,
        Texture2D alpha, in PackSettings packSettings)
    {
        Texture2D[] textures = {red, green, blue, alpha};
        int width = mask.width;
        int height = mask.height;
        int pixelCount = width * height;

        foreach (Texture2D t in textures)
        {
            if (t != null)
            {
                if (!packSettings.generateOnGPU && !t.isReadable)
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

        if (packSettings.generateOnGPU)
        {
            packChannelMaterial.SetTexture("_RedChannel", red != null ? red : Texture2D.blackTexture);
            packChannelMaterial.SetTexture("_GreenChannel", green != null ? green : Texture2D.blackTexture);
            packChannelMaterial.SetTexture("_BlueChannel", blue != null ? blue : Texture2D.blackTexture);
            packChannelMaterial.SetTexture("_AlphaChannel", alpha ?? Texture2D.blackTexture);
            packChannelMaterial.SetVector("_InvertColor", packSettings.invertColor);

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
            var redPixels = textures[0] != null ? textures[0].GetPixels(0) : null;
            var greenPixels = textures[1] != null ? textures[1].GetPixels(0) : null;
            var bluePixels = textures[2] != null ? textures[2].GetPixels(0) : null;
            var alphaPixels = textures[3] != null ? textures[3].GetPixels(0) : null;

            bool invertR = (packSettings.invertColor.x > 0.0f);
            bool invertG = (packSettings.invertColor.y > 0.0f);
            bool invertB = (packSettings.invertColor.z > 0.0f);
            bool invertA = (packSettings.invertColor.w > 0.0f);
            Color[] maskPixels = new Color[pixelCount];
            for (int i = 0; i < pixelCount; ++i)
            {
                float r = (redPixels != null) ? redPixels[i].r : 0.0f;
                float g = (greenPixels != null) ? greenPixels[i].r : 0.0f;
                float b = (bluePixels != null) ? bluePixels[i].r : 0.0f;
                float a = (alphaPixels != null) ? alphaPixels[i].r : 0.0f;

                if (invertR)
                    r = 1.0f - r;

                if (invertG)
                    g = 1.0f - g;

                if (invertB)
                    b = 1.0f - b;

                if (invertA)
                    a = 1.0f - a;

                maskPixels[i] = new Color(r, g, b, a);
            }

            mask.SetPixels(maskPixels, 0);
            mask.Apply();
        }
    }
}
