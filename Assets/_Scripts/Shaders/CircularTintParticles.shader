Shader "Custom/CircularTintParticles"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainTint ("Main Tint", Color) = (1, 1, 1, 1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 1
        _TintColor ("Gray Tint Color", Color) = (0.5, 0.5, 0.5, 1)
        _StartAngle ("Start Angle", Range(0, 360)) = 90
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Blend SrcAlpha OneMinusSrcAlpha
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTint;
            float _FillAmount;
            float4 _TintColor;
            float _StartAngle;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _MainTint * i.color;
                
                float2 centered = i.uv - 0.5;
                float angle = atan2(centered.y, centered.x);
                float startRad = radians(_StartAngle);
                
                angle = (angle - startRad) / (3.14159265 * 2);
                
                if (angle < 0)
                    angle += 1;
                
                if (angle > _FillAmount)
                    col.rgb = lerp(col.rgb, _TintColor.rgb, _TintColor.a);
                
                return col;
            }
            ENDCG
        }
    }
}