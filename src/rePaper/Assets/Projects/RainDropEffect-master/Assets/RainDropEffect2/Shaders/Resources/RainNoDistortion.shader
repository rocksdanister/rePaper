// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "RainDrop/Internal/RainNoDistortion"
{
	Properties
	{
		_Color("Main Color", COLOR) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Distortion("Normalmap", 2D) = "black" {}
		_Relief("Relief Value", Range(0.0, 2.0)) = 1.5
		_Darkness("Darkness", Range(0.0, 100.0)) = 10.0
	}

	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
			CGPROGRAM
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			float4 _Color;
			sampler2D _MainTex;
			sampler2D _Distortion;
			float _Darkness;
			float _Relief;

			struct a2v {
				float4 vertex : POSITION;
				float4 color : COLOR;
				float3 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			v2f vert(a2v i) {
				v2f o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.color.rgb = _Color.rgb;
				o.color.rgb *= saturate(1 - _Darkness);
				o.color.a = _Color.a;
				o.texcoord = i.texcoord;
				return o;
			}

			half4 frag(v2f i) : COLOR {
				half2 norm = UnpackNormal(tex2D(_Distortion, i.texcoord)).rg;
				fixed4 tex = tex2D(_MainTex, i.texcoord);
				i.color.rgb *= tex.rgb;
				i.color.rgb *= (1 - norm.r * _Relief);
				i.color.a *= tex.r * tex.g * tex.b;
				return i.color;
			}
			ENDCG
		}
	}
}
