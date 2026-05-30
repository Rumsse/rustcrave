Shader "Custom/XRayVisibilityLit"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        _MetallicGlossMap ("Metallic Map", 2D) = "white" {}
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness ("Smoothness", Range(0.0, 1.0)) = 0.5

        [Normal] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Float) = 1.0

        _OcclusionMap ("Occlusion Map", 2D) = "white" {}
        _OcclusionStrength ("Occlusion Strength", Range(0.0, 1.0)) = 1.0

        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        _EmissionMap ("Emission Map", 2D) = "white" {}

        _Radius ("Radius", Float) = 2.0
        _FadeOpacity ("Fade Opacity", Float) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD3;
                float3 tangentWS : TEXCOORD4;
                float3 bitangentWS : TEXCOORD5;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MetallicGlossMap);
            TEXTURE2D(_BumpMap);
            TEXTURE2D(_OcclusionMap);
            TEXTURE2D(_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float _Metallic;
                float _Smoothness;
                float _BumpScale;
                float _OcclusionStrength;
                float4 _EmissionColor;
                float _Radius;
                float _FadeOpacity;
            CBUFFER_END

            int _TrackedUnitCount;
            float4 _TrackedUnitPositions[100];
            float3 _MainCameraPosition;

            float CalculateAlpha(float3 positionWS)
            {
                if (_TrackedUnitCount <= 0)
                    return 1.0;

                float minLineDist = 10000.0;
                int count = min(_TrackedUnitCount, 8);

                for (int i = 0; i < count; i++)
                {
                    float3 unitPos = _TrackedUnitPositions[i].xyz;
                    float3 lineDir = normalize(unitPos - _MainCameraPosition);
                    float3 pointVec = positionWS - _MainCameraPosition;
                    float proj = dot(pointVec, lineDir);
                    float lineLength = distance(unitPos, _MainCameraPosition);

                    if (proj < 0 || proj > lineLength)
                        continue;

                    float3 closestPoint = _MainCameraPosition + lineDir * proj;
                    float dist = distance(positionWS, closestPoint);

                    if (dist < minLineDist)
                        minLineDist = dist;
                }

                if (minLineDist >= _Radius)
                    return 1.0;

                float edgeDist = _Radius * 0.8;
                return lerp(_FadeOpacity, 1.0, smoothstep(edgeDist, _Radius, minLineDist));
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = IN.uv;

                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS = normalInput.normalWS;
                OUT.tangentWS = normalInput.tangentWS;
                OUT.bitangentWS = normalInput.bitangentWS;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                half4 baseColorMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                half4 baseColor = baseColorMap * _BaseColor;

                half4 metallicGlossMap = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_BaseMap, uv);
                half metallic = metallicGlossMap.r * _Metallic;
                half smoothness = metallicGlossMap.a * _Smoothness;

                half4 normalMap = SAMPLE_TEXTURE2D(_BumpMap, sampler_BaseMap, uv);
                half3 normalTS = UnpackNormalScale(normalMap, _BumpScale);
                half3x3 tangentToWorld = half3x3(IN.tangentWS, IN.bitangentWS, IN.normalWS);
                half3 normalWS = NormalizeNormalPerPixel(mul(normalTS, tangentToWorld));

                half occlusion = lerp(1.0, SAMPLE_TEXTURE2D(_OcclusionMap, sampler_BaseMap, uv).g, _OcclusionStrength);
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_BaseMap, uv).rgb * _EmissionColor.rgb;

                baseColor.a *= CalculateAlpha(IN.positionWS);

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = baseColor.rgb;
                surfaceData.alpha = baseColor.a;
                surfaceData.metallic = metallic;
                surfaceData.smoothness = smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = emission;
                surfaceData.occlusion = occlusion;
                surfaceData.clearCoatMask = 0.0;
                surfaceData.clearCoatSmoothness = 0.0;
                surfaceData.specular = half3(0.0, 0.0, 0.0);

                half4 finalColor = UniversalFragmentPBR(inputData, surfaceData);
                finalColor.a = baseColor.a;

                return finalColor;
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
        UsePass "Universal Render Pipeline/Lit/DepthNormals"
    }
}