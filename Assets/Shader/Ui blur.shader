Shader "Custom/UIBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 10)) = 2.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; float4 color : COLOR; };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _BlurSize;

            v2f vert(appdata v) { v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color; return o; }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = fixed4(0,0,0,0);
                float2 ts = _MainTex_TexelSize.xy * _BlurSize;
                // Gaussian 3x3
                col += tex2D(_MainTex, i.uv + float2(-ts.x, -ts.y)) * 0.0625;
                col += tex2D(_MainTex, i.uv + float2(0,    -ts.y)) * 0.125;
                col += tex2D(_MainTex, i.uv + float2( ts.x, -ts.y)) * 0.0625;
                col += tex2D(_MainTex, i.uv + float2(-ts.x,  0   )) * 0.125;
                col += tex2D(_MainTex, i.uv + float2(0,      0   )) * 0.25;
                col += tex2D(_MainTex, i.uv + float2( ts.x,  0   )) * 0.125;
                col += tex2D(_MainTex, i.uv + float2(-ts.x,  ts.y)) * 0.0625;
                col += tex2D(_MainTex, i.uv + float2(0,      ts.y)) * 0.125;
                col += tex2D(_MainTex, i.uv + float2( ts.x,  ts.y)) * 0.0625;
                col.a *= i.color.a;
                return col;
            }
            ENDCG
        }
    }
}