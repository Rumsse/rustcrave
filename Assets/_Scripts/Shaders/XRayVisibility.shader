Shader "Custom/XRayVisibility"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _Radius ("Radius", Float) = 2.0
        _FadeOpacity ("Fade Opacity", Float) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
                float3 positionWS : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _BaseColor;
            float _Radius;
            float _FadeOpacity;

            int _TrackedUnitCount;
            float4 _TrackedUnitPositions[100];
            float3 _MainCameraPosition;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 col = tex2D(_MainTex, IN.uv) * _BaseColor;
                float minLineDist = 10000.0;
                float outAlpha = 1.0;

                if (_TrackedUnitCount <= 0)
                    return col;

                for (int i = 0; i < _TrackedUnitCount; i++)
                {
                    float3 unitPos = _TrackedUnitPositions[i].xyz;
                    float3 lineDir = normalize(unitPos - _MainCameraPosition);
                    float3 pointVec = IN.positionWS - _MainCameraPosition;
                    float proj = dot(pointVec, lineDir);
                    float lineLength = distance(unitPos, _MainCameraPosition);
                    
                    if (proj < 0 || proj > lineLength)
                        continue;

                    float3 closestPoint = _MainCameraPosition + lineDir * proj;
                    float dist = distance(IN.positionWS, closestPoint);
                    
                    if (dist < minLineDist)
                        minLineDist = dist;
                }

                if (minLineDist < _Radius)
                {
                    float edgeDist = _Radius * 0.8;
                    outAlpha = lerp(_FadeOpacity, 1.0, smoothstep(edgeDist, _Radius, minLineDist));
                }

                col.a *= outAlpha;
                return col;
            }
            ENDHLSL
        }
    }
}