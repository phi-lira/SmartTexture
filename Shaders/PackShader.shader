Shader "Hidden/PackChannel"
{
    Properties
    {
        _RedChannel("RedChannel", 2D) = "white" {}
        _GreenChannel("GreenChannel", 2D) = "white" {}
        _BlueChannel("BlueChannel", 2D) = "white" {}
        _AlphaChannel("AlphaChannel", 2D) = "white" {}
        _InvertColor("_InvertColor", Vector) = (0.0, 0.0, 0.0, 0.0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline"}

        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_RedChannel);
            TEXTURE2D(_GreenChannel);
            TEXTURE2D(_BlueChannel);
            TEXTURE2D(_AlphaChannel);
            SAMPLER(sampler_point_clamp);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _RedChannel_TexelSize;
            half4 _InvertColor;
            CBUFFER_END

            float4 vert(float4 positionOS : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(positionOS.xyz);
            }

            half4 frag(float4 positionHCS : SV_POSITION) : SV_Target
            {
                float2 uv = positionHCS * _RedChannel_TexelSize.xy;
                half r = SAMPLE_TEXTURE2D(_RedChannel, sampler_point_clamp, uv).r;
                half g = SAMPLE_TEXTURE2D(_GreenChannel, sampler_point_clamp, uv).r;
                half b = SAMPLE_TEXTURE2D(_BlueChannel, sampler_point_clamp, uv).r;
                half a = SAMPLE_TEXTURE2D(_AlphaChannel, sampler_point_clamp, uv).r;
                
                if (_InvertColor.x > 0.0)
                    r = 1.0 - r;

                if (_InvertColor.g > 0.0)
                    g = 1.0 - g;

                if (_InvertColor.b > 0.0)
                    b = 1.0 - b;

                if (_InvertColor.a > 0.0)
                    a = 1.0 - a;
                return half4(r, g, b, a);
            }
            ENDHLSL
        }
    }
}
