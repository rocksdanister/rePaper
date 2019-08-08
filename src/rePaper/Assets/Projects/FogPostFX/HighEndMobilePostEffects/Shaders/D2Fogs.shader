// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/D2Fogs" {
    Properties{
        [HideInInspector]_Size("Size", float) = 2.0
		[HideInInspector]_Speed("Horizontal Speed", float) = 0.2
		[HideInInspector]_VSpeed("Vertical Speed", float) = 0
        [HideInInspector]_Density("Density", float) = 1
        [HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
        [HideInInspector]_Color("Color", Color) = (1, 1, 1, 1)
        
    }

    Subshader{

        Pass{
            Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always
            //Tags{ "Queue" = "Opaque" }

            CGPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Size;
            float _Speed;
			float _VSpeed;
            float _Density;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vertexOutput {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            vertexOutput vert(appdata v)
            {
                vertexOutput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float hash(float n) 
            { 
                return frac(sin(n)*753.5453123); 
            }

            float noise(in float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                f = f*f*(3.0 - 2.0*f);

                float n = p.x + p.y*157.0 + 113.0*p.z;
                return lerp(
                            lerp(
                                lerp(hash(n + 0.0),   hash(n + 1.0),   f.x),
                                lerp(hash(n + 157.0), hash(n + 158.0), f.x), 
                                f.y),
                            lerp(
                                lerp(hash(n + 113.0), hash(n + 114.0), f.x),
                                lerp(hash(n + 270.0), hash(n + 271.0), f.x), 
                                f.y),
                            f.z);
            }



            float fog(in float2 uv)
            {
				float direction = _Time.y * _Speed;
				float Vdirection = _Time.y * _VSpeed;
                float color = 0.0;
                float total = 0.0;
                float k = 0.0;

                for (float i=0; i<6; i++)
                {
                    k = pow(2.0, i); 
                    color += noise(float3((uv.x * _Size + direction * (i+1.0)*0.2) * k, 
								  (uv.y * _Size + Vdirection * (i + 1.0)*0.2) * k,
                                  0.0)) 
                                  / k; 
                    total += 1.0/k;
                }
                color /= total;
                
                return clamp(color, 0.0, 1.0);

            }

            half4 frag(vertexOutput i) : SV_Target
            {
                float f = fog(i.uv);
                float m = min(f*_Density, 1.);
                return tex2D(_MainTex, i.uv)*(1-m)  + m*_Color;
            }
            ENDCG
        }
    }
}