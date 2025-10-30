Shader "Custom/SeeThroughCircle"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _MainTex("Texture", 2D) = "white" {}
        _CutoutPosition("Cutout Position", Vector) = (0.5, 0.5, 0, 0)
        _CutoutRadius("Cutout Radius", Float) = 0.15
        _CutoutSoftness("Cutout Softness", Float) = 0.05
        _CutoutVisible("Cutout Visible", Float) = 0.0
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
            Pass
            {
                HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                struct Attributes
                {
                    float4 positionOS : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct Varyings
                {
                    float4 positionHCS : SV_POSITION;
                    float2 uv : TEXCOORD0;
                    float4 screenPos : TEXCOORD1;
                };

                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                float4 _BaseColor;
                float4 _CutoutPosition;
                float _CutoutRadius;
                float _CutoutSoftness;
                float _CutoutVisible;

                Varyings vert(Attributes IN)
                {
                    Varyings OUT;
                    OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                    OUT.uv = IN.uv;
                    OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                    return OUT;
                }

                half4 frag(Varyings IN) : SV_Target
                {
                    float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                    float dist = distance(screenUV, _CutoutPosition.xy);
                    float cutout = smoothstep(_CutoutRadius, _CutoutRadius - _CutoutSoftness, dist);

                    half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _BaseColor;

                    // Blend transparency only when cutout is active
                    if (_CutoutVisible > 0.5)
                        col.a *= cutout;

                    return col;
                }
                ENDHLSL
            }
        }
}
