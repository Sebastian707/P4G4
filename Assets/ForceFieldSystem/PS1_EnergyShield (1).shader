// =====================================================================
//  PS1_EnergyShield.shader
//  Universal Render Pipeline — Transparent / Two-Sided
//
//  Drop into Assets/Shaders/ and assign to a material.
//  Drive at runtime via ShieldController.cs
// =====================================================================

Shader "Custom/PS1_EnergyShield"
{
    Properties
    {
        // --- Reveal ---
        _PlayerPosition     ("Player Position",     Vector)     = (0,0,0,0)
        _RevealRadius       ("Reveal Radius",        Range(0.1, 20))  = 3.5
        _EdgeSoftness       ("Edge Softness",        Range(0.01, 5))  = 1.2

        // --- Impact flash ---
        _ImpactPosition     ("Impact Position",      Vector)     = (0,-9999,0,0)
        _ImpactRadius       ("Impact Radius",        Range(0, 8))     = 0.0

        // --- Appearance ---
        [HDR] _ShieldColor  ("Shield Color",         Color)      = (0.1, 0.8, 1.0, 1.0)
        _EmissionIntensity  ("Emission Intensity",   Range(0, 10))    = 2.0
        _FresnelPower       ("Fresnel Power",        Range(0.1, 8))   = 4.0

        // --- PS1 dither ---
        _DitherStrength     ("Dither Strength",      Range(0, 2))     = 1.0

        // --- Scanlines ---
        _ScanlineDensity    ("Scanline Density",     Range(5, 200))   = 40.0
        _ScanlineStrength   ("Scanline Strength",    Range(0, 0.5))   = 0.3

        // --- Flicker ---
        _FlickerSpeed       ("Flicker Speed",        Range(0, 30))    = 8.0
        _FlickerAmount      ("Flicker Amount",       Range(0, 0.3))   = 0.07

        // --- Noise edge ---
        _NoiseScale         ("Noise Scale",          Range(0.1, 10))  = 2.5
        _NoiseStrength      ("Noise Edge Strength",  Range(0, 0.5))   = 0.15

        // --- Hex/Grid pattern ---
        _HexTiling          ("Hex Tiling",           Range(1, 20))    = 6.0
        _HexContrast        ("Hex Contrast",         Range(0, 1))     = 0.15

        // --- UV distortion ---
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

        // Two passes: back-faces first (inside of dome), then front-faces
        // so the dome looks correct from both inside the arena and outside.

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
                float4 positionCS  : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float4 screenPos   : TEXCOORD3;
            };

            // ── Bayer 4×4 ordered dither matrix ──────────────────────
            // Returns a threshold in [0,1]. Compare against alpha:
            // if alpha < threshold → discard (transparent), else → opaque.
            float Bayer4x4(float2 screenXY)
            {
                int2 p = int2(fmod(screenXY, 4.0));
                // Row-major Bayer 4×4 / 16
                const float m[16] = {
                     0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
                    12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
                     3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
                    15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
                };
                return m[p.y * 4 + p.x];
            }

            // ── Value noise (cheap hash-based) ────────────────────────
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
                float2 u = f * f * (3.0 - 2.0 * f); // smoothstep
                return lerp(
                    lerp(Hash(i + float2(0,0)), Hash(i + float2(1,0)), u.x),
                    lerp(Hash(i + float2(0,1)), Hash(i + float2(1,1)), u.x),
                    u.y
                );
            }

            // ── Quantised grid pattern (mimics hex / PS1 polygon fill) 
            float GridPattern(float2 uv, float tiling)
            {
                float2 g = frac(uv * tiling);
                // soft step to make rounded cell borders
                float2 b = smoothstep(0.0, 0.08, g) * smoothstep(1.0, 0.92, g);
                return b.x * b.y;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vpi = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   vni = GetVertexNormalInputs(IN.normalOS);
                OUT.positionCS  = vpi.positionCS;
                OUT.positionWS  = vpi.positionWS;
                OUT.normalWS    = vni.normalWS;
                OUT.uv          = IN.uv;
                OUT.screenPos   = ComputeScreenPos(vpi.positionCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // ── 1. Proximity reveal mask ──────────────────────────
                float3 toPlayer = IN.positionWS - _PlayerPosition.xyz;
                float  distP    = length(toPlayer);

                // HARD GATE: anything outside RevealRadius is 100% invisible.
                // This runs before noise/flicker so they can never bleed pixels
                // into the invisible zone.
                float  normDist = distP / _RevealRadius;
                clip(1.0 - normDist); // clips if normDist >= 1 (outside radius)

                // Soft dither edge: full alpha in the inner zone, fades toward
                // the edge over the last EdgeSoftness fraction of the radius.
                float  edgeFrac = _EdgeSoftness * 0.3;
                float  softEdge = 1.0 - saturate((normDist - (1.0 - edgeFrac)) / max(edgeFrac, 0.001));
                softEdge        = smoothstep(0.0, 1.0, softEdge);

                // ── 2. Impact reveal mask ─────────────────────────────
                float  distI      = length(IN.positionWS - _ImpactPosition.xyz);
                float  impactMask = 0.0;
                if (_ImpactRadius > 0.001)
                {
                    impactMask = 1.0 - saturate(distI / _ImpactRadius);
                    impactMask = smoothstep(0.0, 1.0, impactMask);
                }

                // ── 3. Combine reveals ────────────────────────────────
                float revealAlpha = max(softEdge, impactMask);

                // ── 4. World-space noise perturbs edge ────────────────
                // Noise only affects pixels already in the visible zone so it
                // cannot push invisible pixels into visibility.
                float2 noiseUV   = IN.positionWS.xz * _NoiseScale + _Time.y * 0.4;
                float  edgeNoise = ValueNoise(noiseUV) * 2.0 - 1.0;
                // Weight noise by how close we are to the edge (zero at centre)
                float  edgeWeight = 1.0 - smoothstep(0.3, 0.9, revealAlpha);
                revealAlpha      += edgeNoise * _NoiseStrength * edgeWeight;

                // ── 5. Flicker (multiplied, not added, so it only dims) ──
                float flicker = 1.0
                    + sin(_Time.y * _FlickerSpeed) * _FlickerAmount
                    + sin(_Time.y * _FlickerSpeed * 2.7 + 1.3) * _FlickerAmount * 0.4;
                revealAlpha  *= flicker;

                // ── 6. Scanlines ──────────────────────────────────────
                float2 screenUV  = IN.screenPos.xy / IN.screenPos.w;
                float  lineY     = floor(screenUV.y * _ScanlineDensity * _ScreenParams.y / 100.0);
                float  scanMask  = 1.0 - (fmod(lineY, 2.0) * _ScanlineStrength);
                revealAlpha     *= scanMask;

                revealAlpha = saturate(revealAlpha);

                // ── 7. PS1 Bayer dither (only on the soft transition band) ──
                float2 pixelXY  = screenUV * _ScreenParams.xy;
                float  bayerT   = Bayer4x4(pixelXY);
                float  threshold = lerp(bayerT, 0.5, 1.0 - _DitherStrength);
                clip(revealAlpha - threshold);

                // ── 8. Color: grid + Fresnel rim ──────────────────────
                // Distort UVs slightly for energy wobble
                float2 distUV = IN.uv;
                distUV.x += sin(_Time.y * _DistortionSpeed + IN.positionWS.y * 3.14) * _DistortionStrength;
                distUV.y += cos(_Time.y * _DistortionSpeed * 0.7 + IN.positionWS.x * 3.14) * _DistortionStrength;

                float  grid     = GridPattern(distUV, _HexTiling) * _HexContrast;

                // Fresnel: stronger at grazing angles
                float3 viewDir  = normalize(GetCameraPositionWS() - IN.positionWS);
                float3 normWS   = normalize(IN.normalWS);
                float  NdotV    = saturate(dot(normWS, viewDir));
                float  fresnel  = pow(1.0 - NdotV, _FresnelPower);

                float3 baseCol  = _ShieldColor.rgb + grid;
                float3 emission = baseCol * fresnel * _EmissionIntensity;
                float3 finalCol = baseCol * 0.2 + emission; // dim base, bright rim

                // Back-face: slightly dimmer
                finalCol *= 0.6;

                return half4(finalCol, 1.0); // alpha handled by dither clip above
            }
            ENDHLSL
        }

        // ── Pass 2: Front faces ─────────────────────────────────────
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
                float4 positionCS  : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float2 uv          : TEXCOORD2;
                float4 screenPos   : TEXCOORD3;
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
                    u.y
                );
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
                OUT.positionCS  = vpi.positionCS;
                OUT.positionWS  = vpi.positionWS;
                OUT.normalWS    = vni.normalWS;
                OUT.uv          = IN.uv;
                OUT.screenPos   = ComputeScreenPos(vpi.positionCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // ── 1. Proximity reveal ───────────────────────────────
                float3 toPlayer = IN.positionWS - _PlayerPosition.xyz;
                float  distP    = length(toPlayer);

                // HARD GATE: 100% invisible beyond RevealRadius, no exceptions.
                float  normDist = distP / _RevealRadius;
                clip(1.0 - normDist);

                float  edgeFrac = _EdgeSoftness * 0.3;
                float  softEdge = 1.0 - saturate((normDist - (1.0 - edgeFrac)) / max(edgeFrac, 0.001));
                softEdge        = smoothstep(0.0, 1.0, softEdge);

                // ── 2. Impact reveal ──────────────────────────────────
                float distI      = length(IN.positionWS - _ImpactPosition.xyz);
                float impactMask = 0.0;
                if (_ImpactRadius > 0.001)
                {
                    impactMask = 1.0 - saturate(distI / _ImpactRadius);
                    impactMask = smoothstep(0.0, 1.0, impactMask);
                }

                float revealAlpha = max(softEdge, impactMask);

                // ── 3. Noise edge ─────────────────────────────────────
                float2 noiseUV   = IN.positionWS.xz * _NoiseScale + _Time.y * 0.4;
                float  edgeNoise = ValueNoise(noiseUV) * 2.0 - 1.0;
                float  edgeWeight = 1.0 - smoothstep(0.3, 0.9, revealAlpha);
                revealAlpha      += edgeNoise * _NoiseStrength * edgeWeight;

                // ── 4. Flicker (multiply so it only dims, never adds) ─
                float flicker = 1.0
                    + sin(_Time.y * _FlickerSpeed) * _FlickerAmount
                    + sin(_Time.y * _FlickerSpeed * 2.7 + 1.3) * _FlickerAmount * 0.4;
                revealAlpha  *= flicker;

                // ── 5. Scanlines ──────────────────────────────────────
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float  lineY    = floor(screenUV.y * _ScanlineDensity * _ScreenParams.y / 100.0);
                float  scanMask = 1.0 - (fmod(lineY, 2.0) * _ScanlineStrength);
                revealAlpha    *= scanMask;

                revealAlpha = saturate(revealAlpha);

                // ── 6. Bayer dither ───────────────────────────────────
                float2 pixelXY = screenUV * _ScreenParams.xy;
                float  bayerT  = Bayer4x4(pixelXY);
                float  thresh  = lerp(bayerT, 0.5, 1.0 - _DitherStrength);
                clip(revealAlpha - thresh);

                // ── 7. Color ──────────────────────────────────────────
                float2 distUV = IN.uv;
                distUV.x += sin(_Time.y * _DistortionSpeed + IN.positionWS.y * 3.14) * _DistortionStrength;
                distUV.y += cos(_Time.y * _DistortionSpeed * 0.7 + IN.positionWS.x * 3.14) * _DistortionStrength;

                float  grid    = GridPattern(distUV, _HexTiling) * _HexContrast;

                float3 viewDir = normalize(GetCameraPositionWS() - IN.positionWS);
                float3 normWS  = normalize(IN.normalWS);
                float  NdotV   = saturate(dot(normWS, viewDir));
                float  fresnel = pow(1.0 - NdotV, _FresnelPower);

                float3 baseCol  = _ShieldColor.rgb + grid;
                float3 emission = baseCol * fresnel * _EmissionIntensity;
                float3 finalCol = baseCol * 0.2 + emission;

                return half4(finalCol, 1.0);
            }
            ENDHLSL
        }

        // ── Shadow caster: shield always casts shadows ──────────────
        // (optional — remove this pass if you don't want shadow casting)
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct ShadowAttributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct ShadowVaryings   { float4 positionCS : SV_POSITION; };

            ShadowVaryings shadowVert(ShadowAttributes IN)
            {
                ShadowVaryings OUT;
                OUT.positionCS = TransformObjectToHClip(ApplyShadowBias(IN.positionOS.xyz, IN.normalOS, _LightDirection));
                return OUT;
            }
            half4 shadowFrag(ShadowVaryings IN) : SV_Target { return 0; }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
