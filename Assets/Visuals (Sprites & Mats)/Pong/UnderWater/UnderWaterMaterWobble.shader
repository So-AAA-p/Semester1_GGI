Shader "Unlit/UnderWaterMaterWobble"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (0, 0.208, 0.4, 1) // Default to your blue #003566
        _WobbleStrength ("Wobble Strength", Float) = 0.05
        _WobbleSpeed ("Wobble Speed", Float) = 2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off // Standard for transparent 2D objects

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color; // Declare the color variable
            float _WobbleStrength;
            float _WobbleSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                // Add vertex color support so SpriteRenderer color works too!
                float4 color : COLOR; 
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                
                // Keep your cool wobble logic
                float wobble = sin(_Time.y * _WobbleSpeed + v.vertex.x * 5) * _WobbleStrength;
                v.vertex.y += wobble;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color; // Pass vertex color to fragment
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Multiply texture * shader color * SpriteRenderer color
                fixed4 texColor = tex2D(_MainTex, i.uv);
                return texColor * _Color * i.color;
            }
            ENDCG
        }
    }
}