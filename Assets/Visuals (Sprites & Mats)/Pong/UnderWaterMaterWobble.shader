Shader "Unlit/UnderWaterMaterWobble"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WobbleStrength ("Wobble Strength", Float) = 0.05
        _WobbleSpeed ("Wobble Speed", Float) = 2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _WobbleStrength;
            float _WobbleSpeed;

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

            v2f vert (appdata v)
            {
                v2f o;

                float wobble = sin(_Time.y * _WobbleSpeed + v.vertex.x * 5) * _WobbleStrength;
                v.vertex.y += wobble;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
