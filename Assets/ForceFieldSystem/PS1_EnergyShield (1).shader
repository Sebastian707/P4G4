Shader "Custom/PS1_EnergyShield"
{
    Properties
    {
        _PlayerPosition     ("Player Position",     Vector)     = (0,0,0,0)
        _RevealRadius       ("Reveal Radius",        Range(0.1, 20))  = 3.5
        _EdgeSoftness       ("Edge Softness",        Range(0.01, 5))  = 1.2
        _ImpactPosition     ("Impact Position",      Vector)     = (0,-9999,0,0)
        _ImpactRadius       ("Impact Radius",        Range(0, 8))     = 0.0
        [HDR] _ShieldColor  ("Shield Color",         Color)      = (0.1, 0.8, 1.0, 1.0)
        _EmissionIntensity  ("Emission Intensity",   Range(0, 10))    = 2.0
        _FresnelPower       ("Fresnel Power",        Range(0.1, 8))   = 4.0
        _DitherStrength     ("Dither Strength",      Range(0, 2))     = 1.0
        _ScanlineDensity    ("Scanline Density",     Range(5, 200))   = 40.0
        _ScanlineStrength   ("Scanline Strength",    Range(0, 0.5))   = 0.3
        _FlickerSpeed       ("Flicker Speed",        Range(0, 30))    = 8.0
        _FlickerAmount      ("Flicker Amount",       Range(0, 0.3))   = 0.07
        _NoiseScale         ("Noise Scale",          Range(0.1, 10))  = 2.5
        _NoiseStrength      ("Noise Edge Strength",  Range(0, 0.5))   = 0.15
        _HexTiling          ("Hex Tiling",           Range(1, 20))    = 6.0
        _HexContrast        ("Hex Contrast",         Range(0, 1))     = 0.15
        _DistortionSpeed    ("Distortion Speed",     Range(0, 5))     = 1.2
        _DistortionStrength ("Distortion Strength",  Range(0, 0.2))   = 0.04
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        // ── Pass 1: Back faces ──────────────────────────────────────
        Pass
        {
            Name "ShieldBack"
            Tags { "LightMode" = "UniversalForward" }
            Cull   Front
            ZWrite Off
            Blend  SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _PlayerPosition;
                float  _RevealRadius;
                float  _EdgeSoftness;
                float4 _ImpactPosition;
                float  _ImpactRadius;
                float4 _ShieldColor;
                float  _EmissionIntensity;
                float  _FresnelPower;
                float  _DitherStrength;
                float  _ScanlineDensity;
                float  _ScanlineStrength;
                float  _FlickerSpeed;
                float  _FlickerAmount;
                float  _NoiseScale;
                float  _NoiseStrength;
                float  _HexTiling;
                float  _HexContrast;
                float  _DistortionSpeed;
                float  _DistortionStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                float4 screenPos  : TEXCOORD3;
            };

            float Bayer4x4(float2 screenXY)
            {
                int2 p = int2(fmod(screenXY, 4.0));
                const float m[16] = {
                     0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
                    12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
                     3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
                    15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
                };
                return m[p.y * 4 + p.x];
            }

            float Hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(Hash(i + float2(0,0)), Hash(i + float2(1,0)), u.x),
                    lerp(Hash(i + float2(0,1)), Hash(i + float2(1,1)), u.x),
                    u.y);
            }

            float GridPattern(float2 uv, float tiling)
            {
                float2 g = frac(uv * tiling);
                float2 b = smoothstep(0.0, 0.08, g) * smoothstep(1.0, 0.92, g);
                return b.x * b.y;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   vni = GetVertexNormalInputs(IN.normalOS);
                OUT.positionCS = vpi.positionCS;
                OUT.positionWS = vpi.positionWS;
                OUT.normalWS   = vni.normalWS;
                OUT.uv         = IN.uv;
                OUT.screenPos  = ComputeScreenPos(vpi.positionCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float distP   = length(IN.positionWS - _PlayerPosition.xyz);
                float normDist = distP / _RevealRadius;
                clip(1.0 - normDist);

                float edgeFrac = _EdgeSoftness * 0.3;
                float softEdge = 1.0 - saturate((normDist - (1.0 - edgeFrac)) / max(edgeFrac, 0.001));
                softEdge = smoothstep(0.0, 1.0, softEdge);

                float distI = length(IN.positionWS - _ImpactPosition.xyz);
                float impactMask = 0.0;
                if (_ImpactRadius > 0.001)
                {
                    impactMask = 1.0 - saturate(distI / _ImpactRadius);
                    impactMask = smoothstep(0.0, 1.0, impactMask);
                }

                float revealAlpha = max(softEdge, impactMask);

                float2 noiseUV  = IN.positionWS.xz * _NoiseScale + _Time.y * 0.4;
                float edgeNoise = ValueNoise(noiseUV) * 2.0 - 1.0;
                float edgeWeight = 1.0 - smoothstep(0.3, 0.9, revealAlpha);
                revealAlpha += edgeNoise * _NoiseStrength * edgeWeight;

                float flicker = 1.0
                    + sin(_Time.y * _FlickerSpeed) * _FlickerAmount
                    + sin(_Time.y * _FlickerSpeed * 2.7 + 1.3) * _FlickerAmount * 0.4;
                revealAlpha *= flicker;

                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float lineY     = floor(screenUV.y * _ScanlineDensity * _ScreenParams.y / 100.0);
                float scanMask  = 1.0 - (fmod(lineY, 2.0) * _ScanlineStrength);
                revealAlpha *= scanMask;
                revealAlpha = saturate(revealAlpha);

                float2 pixelXY = screenUV * _ScreenParams.xy;
                float bayerT   = Bayer4x4(pixelXY);
                float threshold = lerp(bayerT, 0.5, 1.0 - _DitherStrength);
                clip(revealAlpha - threshold);

                float2 distUV = IN.uv;
                distUV.x += sin(_Time.y * _DistortionSpeed + IN.positionWS.y * 3.14) * _DistortionStrength;
                distUV.y += cos(_Time.y * _DistortionSpeed * 0.7 + IN.positionWS.x * 3.14) * _DistortionStrength;

                float grid    = GridPattern(distUV, _HexTiling) * _HexContrast;
                float3 viewDir = normalize(GetCameraPositionWS() - IN.positionWS);
                float3 normWS  = normalize(IN.normalWS);
                float NdotV    = saturate(dot(normWS, viewDir));
                float fresnel  = pow(1.0 - NdotV, _FresnelPower);

                float3 baseCol  = _ShieldColor.rgb + grid;
                float3 emission = baseCol * fresnel * _EmissionIntensity;
                float3 finalCol = (baseCol * 0.2 + emission) * 0.6;

                return half4(finalCol, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShieldFront"
            Tags { "LightMode" = "UniversalForward" }
            Cull   Back
            ZWrite Off
            Blend  SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _PlayerPosition;
                float  _RevealRadius;
                float  _EdgeSoftness;
                float4 _ImpactPosition;
                float  _ImpactRadius;
                float4 _ShieldColor;
                float  _EmissionIntensity;
                float  _FresnelPower;
                float  _DitherStrength;
                float  _ScanlineDensity;
                float  _ScanlineStrength;
                float  _FlickerSpeed;
                float  _FlickerAmount;
                float  _NoiseScale;
                float  _NoiseStrength;
                float  _HexTiling;
                float  _HexContrast;
                float  _DistortionSpeed;
                float  _DistortionStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                float4 screenPos  : TEXCOORD3;
            };

            float Bayer4x4(float2 screenXY)
            {
                int2 p = int2(fmod(screenXY, 4.0));
                const float m[16] = {
                     0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
                    12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
                     3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
                    15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
                };
                return m[p.y * 4 + p.x];
            }

            float Hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(Hash(i + float2(0,0)), Hash(i + float2(1,0)), u.x),
                    lerp(Hash(i + float2(0,1)), Hash(i + float2(1,1)), u.x),
                    u.y);
            }

            float GridPattern(float2 uv, float tiling)
            {
                float2 g = frac(uv * tiling);
                float2 b = smoothstep(0.0, 0.08, g) * smoothstep(1.0, 0.92, g);
                return b.x * b.y;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   vni = GetVertexNormalInputs(IN.normalOS);
                OUT.positionCS = vpi.positionCS;
                OUT.positionWS = vpi.positionWS;
                OUT.normalWS   = vni.normalWS;
                OUT.uv         = IN.uv;
                OUT.screenPos  = ComputeScreenPos(vpi.positionCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float distP    = length(IN.positionWS - _PlayerPosition.xyz);
                float normDist = distP / _RevealRadius;
                clip(1.0 - normDist);

                float edgeFrac = _EdgeSoftness * 0.3;
                float softEdge = 1.0 - saturate((normDist - (1.0 - edgeFrac)) / max(edgeFrac, 0.001));
                softEdge = smoothstep(0.0, 1.0, softEdge);

                float distI = length(IN.positionWS - _ImpactPosition.xyz);
                float impactMask = 0.0;
                if (_ImpactRadius > 0.001)
                {
                    impactMask = 1.0 - saturate(distI / _ImpactRadius);
                    impactMask = smoothstep(0.0, 1.0, impactMask);
                }

                float revealAlpha = max(softEdge, impactMask);

                float2 noiseUV   = IN.positionWS.xz * _NoiseScale + _Time.y * 0.4;
                float edgeNoise  = ValueNoise(noiseUV) * 2.0 - 1.0;
                float edgeWeight = 1.0 - smoothstep(0.3, 0.9, revealAlpha);
                revealAlpha += edgeNoise * _NoiseStrength * edgeWeight;

                float flicker = 1.0
                    + sin(_Time.y * _FlickerSpeed) * _FlickerAmount
                    + sin(_Time.y * _FlickerSpeed * 2.7 + 1.3) * _FlickerAmount * 0.4;
                revealAlpha *= flicker;

                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float lineY     = floor(screenUV.y * _ScanlineDensity * _ScreenParams.y / 100.0);
                float scanMask  = 1.0 - (fmod(lineY, 2.0) * _ScanlineStrength);
                revealAlpha *= scanMask;
                revealAlpha = saturate(revealAlpha);

                float2 pixelXY = screenUV * _ScreenParams.xy;
                float bayerT   = Bayer4x4(pixelXY);
                float thresh   = lerp(bayerT, 0.5, 1.0 - _DitherStrength);
                clip(revealAlpha - thresh);

                float2 distUV = IN.uv;
                distUV.x += sin(_Time.y * _DistortionSpeed + IN.positionWS.y * 3.14) * _DistortionStrength;
                distUV.y += cos(_Time.y * _DistortionSpeed * 0.7 + IN.positionWS.x * 3.14) * _DistortionStrength;

                float grid     = GridPattern(distUV, _HexTiling) * _HexContrast;
                float3 viewDir = normalize(GetCameraPositionWS() - IN.positionWS);
                float3 normWS  = normalize(IN.normalWS);
                float NdotV    = saturate(dot(normWS, viewDir));
                float fresnel  = pow(1.0 - NdotV, _FresnelPower);

                float3 baseCol  = _ShieldColor.rgb + grid;
                float3 emission = baseCol * fresnel * _EmissionIntensity;
                float3 finalCol = baseCol * 0.2 + emission;

                return half4(finalCol, 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest  LEqual
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex   shadowVert
            #pragma fragment shadowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float3 _LightDirection;
            float4 _ShadowBias; 

            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct ShadowVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            ShadowVaryings shadowVert(ShadowAttributes IN)
            {
                ShadowVaryings OUT;

                float3 posWS  = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normWS = TransformObjectToWorldNormal(IN.normalOS);
                float invNdotL = 1.0 - saturate(dot(_LightDirection, normWS));
                posWS += normWS * (invNdotL * _ShadowBias.y);
                posWS += _LightDirection * _ShadowBias.x;

                float4 posCS = TransformWorldToHClip(posWS);

                #if UNITY_REVERSED_Z
                    posCS.z = min(posCS.z, posCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    posCS.z = max(posCS.z, posCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                OUT.positionCS = posCS;
                return OUT;
            }

            half4 shadowFrag(ShadowVaryings IN) : SV_Target { return 0; }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}