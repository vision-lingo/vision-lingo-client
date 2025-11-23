Shader "Custom/GlowRadial"
{
    Properties
    {
        _ColorA ("색상 A", Color) = (1,0,0.1725,1)
        _ColorB ("색상 B", Color) = (0.98,0.18,0.45,0.88)
        _ColorC ("색상 C", Color) = (0.984,0.372,0.749,0.67)
        _ColorD ("색상 D", Color) = (0.423,0.152,0.455,0.26)
        _MainTex ("텍스처", 2D) = "white" {}
        _InnerRadius ("내부 반경", Float) = 0.2
        _OuterRadius ("외부 반경", Float) = 1.0
        _InnerCutoff ("내부 컷오프(0~1)", Float) = 0.45
        _OuterCutoff ("외부 컷오프(0~1)", Float) = 0.95
        _SphereCenter ("구 중심", Vector) = (0,0,0,0)
        _SphereRadius ("구 반지름", Float) = 0.5
        _FalloffExp ("페이드 지수", Float) = 1.6
        _EdgeSoftness ("엣지 소프트니스", Float) = 1.8
        _ColorIntensity ("색상 강도", Float) = 1.1
        _EdgeAlphaMul ("엣지 알파 곱", Float) = 1.0
        _NearRedBoost ("내측 레드 강조", Float) = 0.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            // 알파 블렌딩으로 배경과 자연스럽게 섞이도록 변경
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _ColorA;
            float4 _ColorB;
            float4 _ColorC;
            float4 _ColorD;
            float _InnerRadius;
            float _OuterRadius;
            float _InnerCutoff;
            float _OuterCutoff;
            float _FalloffExp;
            float _EdgeSoftness;
            float _ColorIntensity;
            float _EdgeAlphaMul;
            float _NearRedBoost;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                // 월드 좌표 전달
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 blendStops(float d)
            {
                // d는 [0,1] 범위입니다. 0은 중심, 1은 가장자리입니다.
                // 4개의 색상 스탑을 균등하게 분배하여 부드럽게 보간합니다.
                float p1 = 0.33;
                float p2 = 0.66;
                float4 col;
                if (d <= p1)
                {
                    float t = saturate(d / p1);
                    col = lerp(_ColorA, _ColorB, t);
                }
                else if (d <= p2)
                {
                    float t = saturate((d - p1) / (p2 - p1));
                    col = lerp(_ColorB, _ColorC, t);
                }
                else
                {
                    float t = saturate((d - p2) / (1.0 - p2));
                    col = lerp(_ColorC, _ColorD, t);
                }
                return col;
            }

            float3 _SphereCenter;
            float _SphereRadius;

            // 색상은 sRGB로 들어오므로 셰이더에서 선형 보간을 위해 변환 필요할 수 있음

            fixed4 frag (v2f i) : SV_Target
            {
                // uv center at 0.5,0.5
                float2 uv = i.uv - 0.5;
                // 코너 거리가 약 1이 되도록 정규화
                float dist = length(uv) / 0.7071;
                dist = saturate(dist);

                // 컬러 스톱 합성 (RGB)
                float4 col = blendStops(dist);

                // 월드 좌표 기준으로 구 내부 픽을 완전 제거하여 구 내부 발광을 방지
                float dToCenter = distance(i.worldPos, _SphereCenter);
                if (dToCenter <= _SphereRadius)
                {
                    discard;
                }

                // 링 형태의 알파: inner 컷오프에서 최대, outer 컷오프로 서서히 0으로 감쇠
                float normalized = saturate((dist - _InnerCutoff) / max(0.0001, (_OuterCutoff - _InnerCutoff))); // 0..1
                // 기본적인 부드러운 감소
                float baseAlpha = pow(1.0 - normalized, max(0.01, _FalloffExp));
                // 가우시안 계열의 소프트 엣지 추가로 배경과 자연스럽게 섞이도록 함
                float gauss = exp(-normalized * normalized * _EdgeSoftness * 4.0);
                float ringAlpha = baseAlpha * gauss;

                // 내측(구 쪽)을 더 붉게 강조: base 컬러와 ColorA를 내측 비율로 블렌드
                float nearFactor = 0.0;
                if (_NearRedBoost > 0.0001)
                {
                    nearFactor = pow(saturate(1.0 - normalized), _NearRedBoost);
                }
                col.rgb = lerp(col.rgb, _ColorA.rgb * _ColorIntensity, nearFactor);

                // 컬러와 알파를 링 알파로 줄여서 외곽이 자연스럽게 사라지도록 함
                col.rgb *= ringAlpha;
                // Edge alpha multiplier should be used as a simple scalar (defaults to 1.0)
                float edgeMul = clamp(_EdgeAlphaMul, 0.0, 1.0);
                col.a *= ringAlpha * edgeMul;

                if (col.a <= 0.001) discard;

                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
