Shader "Custom/Outline" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline ("Outline Width", Range(0.0, 2.0)) = 0.1
    }

    SubShader {
        Tags { "RenderType"="Opaque" }

        // ======================
        // OUTLINE PASS
        // ======================
        Pass {
            Name "OUTLINE"
            Tags { "LightMode"="Always" }
            Cull Off              // 앞/뒤 모두 그리기
            ZWrite On             // 깊이 기록
            ZTest Always          // 항상 렌더링
            ColorMask RGB

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Outline;
            float4 _OutlineColor;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                float3 n = normalize(v.normal);
                float4 pos = v.vertex;
                pos.xyz += n * _Outline;
                o.pos = UnityObjectToClipPos(pos);
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                return _OutlineColor;
            }
            ENDCG
        }

        // ======================
        // MAIN PASS
        // ======================
        Pass {
            Name "BASE"
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target {
                return tex2D(_MainTex, i.uv) * _Color;
            }
            ENDCG
        }
    }
}
