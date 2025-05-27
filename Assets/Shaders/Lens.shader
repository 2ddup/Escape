Shader "Custom/Lens"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,0.5)  // 기본 색상 및 투명도 설정
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha  // 알파 블렌딩 적용
            ZWrite Off                        // 깊이 버퍼 비활성화 (렌즈 뒤쪽 보이게)
            Cull Off                          // 양면 렌더링

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return fixed4(_Color.rgb, _Color.a);  // 투명도 적용
            }
            ENDCG
        }
    }
}