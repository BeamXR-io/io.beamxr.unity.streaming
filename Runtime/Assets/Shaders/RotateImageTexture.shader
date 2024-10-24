Shader "BeamXR/RotateImageTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Rotation ("Rotation Angle", Float) = 0.0
        _Transparency ("Transparency", Range(0, 1)) = 1.0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Rotation;
            float _Transparency;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Apply tiling and offset
                float2 uv = TRANSFORM_TEX(v.uv, _MainTex);

                // Rotate UVs around the center
                float2 center = float2(0.5, 0.5);
                uv -= center;
                float angle = _Rotation;
                float cosAngle = cos(angle);
                float sinAngle = sin(angle);
                float2x2 rotationMatrix = float2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
                uv = mul(rotationMatrix, uv);
                uv += center;

                o.uv = uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // Apply transparency
                col.a *= _Transparency;
                return col;
            }
            ENDCG
        }
    }
}
