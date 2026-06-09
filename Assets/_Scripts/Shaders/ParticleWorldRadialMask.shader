Shader "Custom/ParticleWorldRadialMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTint ("Main Tint", Color) = (1, 1, 1, 1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 1
        _StartAngle ("Start Angle", Range(0, 360)) = 90
        _CenterPos ("Center Pos", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Blend SrcAlpha One
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTint;
            float _FillAmount;
            float _StartAngle;
            float4 _CenterPos;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _MainTint * i.color;
                
                float2 offset = i.worldPos.xz - _CenterPos.xz;
                float angle = atan2(offset.y, offset.x);
                float startRad = radians(_StartAngle);
                
                angle = (angle - startRad) / (3.14159265 * 2);
                
                if (angle < 0)
                    angle += 1;
                    
                if (angle > _FillAmount)
                    col.a = 0;
                
                return col;
            }
            ENDCG
        }
    }
}