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

    HLSLINCLUDE
    Texture2D _RedChannel;
    Texture2D _GreenChannel;
    Texture2D _BlueChannel;
    Texture2D _AlphaChannel;
    sampler sampler_point_clamp;
    
    ENDHLSL

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
                half r = _RedChannel.Sample(sampler_point_clamp, uv).r;
                half g = _GreenChannel.Sample(sampler_point_clamp, uv).r;
                half b = _BlueChannel.Sample(sampler_point_clamp, uv).r;
                half a = _AlphaChannel.Sample(sampler_point_clamp, uv).r;
                
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

    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            CBUFFER_START(UnityPerMaterial)
            float4 _RedChannel_TexelSize;
            half4 _InvertColor;
            CBUFFER_END

            float4 vert(float4 positionOS : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(positionOS.xyz);
            }

            half4 frag(float4 positionHCS : SV_POSITION) : SV_Target
            {
                float2 uv = positionHCS * _RedChannel_TexelSize.xy;
                half r = _RedChannel.Sample(sampler_point_clamp, uv).r;
                half g = _GreenChannel.Sample(sampler_point_clamp, uv).r;
                half b = _BlueChannel.Sample(sampler_point_clamp, uv).r;
                half a = _AlphaChannel.Sample(sampler_point_clamp, uv).r;
                
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
