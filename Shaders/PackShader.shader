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

    //Returns channel with the highest value
    half max4(half4 i) {
        return max(max(max(i.r, i.g), i.b), i.a);
    }
    
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
            half4 _RedSourceChannel;
            half4 _GreenSourceChannel;
            half4 _BlueSourceChannel;
            half4 _AlphaSourceChannel;
            CBUFFER_END

            float4 vert(float4 positionOS : POSITION) : SV_POSITION
            {
                return TransformObjectToHClip(positionOS.xyz);
            }

            half4 frag(float4 positionHCS : SV_POSITION) : SV_Target
            {
                float2 uv = positionHCS * _RedChannel_TexelSize.xy;
                half r = max4(_RedChannel.Sample(sampler_point_clamp, uv).rgba * _RedSourceChannel);
                half g = max4(_GreenChannel.Sample(sampler_point_clamp, uv).rgba * _GreenSourceChannel);
                half b = max4(_BlueChannel.Sample(sampler_point_clamp, uv).rgba * _BlueSourceChannel);
                half a = max4(_AlphaChannel.Sample(sampler_point_clamp, uv).rgba * _AlphaSourceChannel);
                
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
            half4 _RedSourceChannel;
            half4 _GreenSourceChannel;
            half4 _BlueSourceChannel;
            half4 _AlphaSourceChannel;
            CBUFFER_END

            float4 vert(float4 positionOS : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(positionOS.xyz);
            }

            half4 frag(float4 positionHCS : SV_POSITION) : SV_Target
            {
                float2 uv = positionHCS * _RedChannel_TexelSize.xy;
                half r = max4(_RedChannel.Sample(sampler_point_clamp, uv).rgba * _RedSourceChannel);
                half g = max4(_GreenChannel.Sample(sampler_point_clamp, uv).rgba * _GreenSourceChannel);
                half b = max4(_BlueChannel.Sample(sampler_point_clamp, uv).rgba * _BlueSourceChannel);
                half a = max4(_AlphaChannel.Sample(sampler_point_clamp, uv).rgba * _AlphaSourceChannel);
                
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
