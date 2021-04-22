Shader "Hidden/PackChannel"
{
    Properties
    {
        _RedChannel("RedChannel", 2D) = "white" {}
        _GreenChannel("GreenChannel", 2D) = "white" {}
        _BlueChannel("BlueChannel", 2D) = "white" {}
        _AlphaChannel("AlphaChannel", 2D) = "white" {}
        _InvertColor("_InvertColor", Vector) = (0.0, 0.0, 0.0, 0.0)
        _RedSource("_RedSource", Int) = 0
        _GreenSource("_GreenSource", Int) = 0
        _BlueSource("_BlueSource", Int) = 0
        _AlphaSource("_AlphaSource", Int) = 0
    }

    HLSLINCLUDE
    Texture2D _RedChannel;
    Texture2D _GreenChannel;
    Texture2D _BlueChannel;
    Texture2D _AlphaChannel;
    int _RedSource;
    int _GreenSource;
    int _BlueSource;
    int _AlphaSource;
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
                
                half r =
                    _RedSource == 0 ? _RedChannel.Sample(sampler_point_clamp, uv).r :
                    _RedSource == 1 ? _RedChannel.Sample(sampler_point_clamp, uv).g :
                    _RedSource == 2 ? _RedChannel.Sample(sampler_point_clamp, uv).b :
                                      _RedChannel.Sample(sampler_point_clamp, uv).a;
                
                half g =
                    _GreenSource == 0 ? _GreenChannel.Sample(sampler_point_clamp, uv).r :
                    _GreenSource == 1 ? _GreenChannel.Sample(sampler_point_clamp, uv).g :
                    _GreenSource == 2 ? _GreenChannel.Sample(sampler_point_clamp, uv).b :
                                        _GreenChannel.Sample(sampler_point_clamp, uv).a;
                
                half b =
                    _BlueSource == 0 ? _BlueChannel.Sample(sampler_point_clamp, uv).r :
                    _BlueSource == 1 ? _BlueChannel.Sample(sampler_point_clamp, uv).g :
                    _BlueSource == 2 ? _BlueChannel.Sample(sampler_point_clamp, uv).b :
                                       _BlueChannel.Sample(sampler_point_clamp, uv).a;
                                        
                half a =
                    _AlphaSource == 0 ? _AlphaChannel.Sample(sampler_point_clamp, uv).r :
                    _AlphaSource == 1 ? _AlphaChannel.Sample(sampler_point_clamp, uv).g :
                    _AlphaSource == 2 ? _AlphaChannel.Sample(sampler_point_clamp, uv).b :
                                        _AlphaChannel.Sample(sampler_point_clamp, uv).a;
                
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
