Shader "BeamXR/UnlitTransparent"
{
    Properties
    {
        _Color ("Tint Color", Color) = (1,1,1,1)
        _MainTex ("Main Tex", 2D) = "white" {}
        _BackgroundTex ("Background Tex", 2D) = "white" {}
        _Transparency ("Transparency", Range(0, 1)) = 0.95
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            sampler2D _BackgroundTex;
            float4 _MainTex_ST;
            float4 _BackgroundTex_ST;
            float4 _Color;
            float _Transparency;
            
            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }
            
            half4 frag (v2f i) : SV_Target
            {
                half4 foregroundColor = tex2D(_MainTex, i.texcoord) * _Color;
                half4 backgroundColor = tex2D(_BackgroundTex, i.texcoord);
                foregroundColor.a *= _Transparency;
                half4 blendedColor = lerp(backgroundColor, foregroundColor, foregroundColor.a);
                return blendedColor;
            }
            ENDCG
        }
    }
}
