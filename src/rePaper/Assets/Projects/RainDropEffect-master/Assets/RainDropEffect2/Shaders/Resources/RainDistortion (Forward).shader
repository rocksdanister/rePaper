// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "RainDrop/Internal/RainDistortion (Forward)" {

	Properties {
		_Color ("Main Color", COLOR) = (1,1,1,1)
		_Strength ("Distortion Strength", Range(0.0, 550.0)) = 50.0
		//_HeightOffset ("Height Offset", Range(0.0, 1.0)) = 0.0
		_Relief ("Relief Value", Range(0.0, 2.0)) = 1.5
		_Distortion ("Normalmap", 2D) = "black" {}
		_ReliefTex ("Relief", 2D) = "black" {}
		_Blur ("Blur", Range(0.0, 50.0)) = 3.0
		_Darkness ("Darkness", Range(0.0, 100.0)) = 10.0
	}

	SubShader {
		Tags {
			"RenderType"="Transparent"
			"Queue"="Transparent" 
		}
		LOD 100
		
		Cull Back
		ZWrite On
		ZTest Always
		ColorMask RGB
		
		// Grab the screen behind the object into _GrabTexture
		GrabPass {
		}

		// Render the object with the texture generated above, and invert the colors
		Pass {
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "RainDropGlobal.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile BLUR

			sampler2D _Distortion : register(s2);
			sampler2D _GrabTexture;
			half4 _GrabTexture_TexelSize;
			fixed4 _Color;
			sampler2D _ReliefTex;
		 	half _Strength;
		 	//float _HeightOffset;
			half _Relief;
			half _Blur;
			half _Darkness;

			struct a2v {
				fixed4 vertex : POSITION;
				fixed4 color : COLOR;
				//float3 normal : NORMAL;
				fixed2 texcoord : TEXCOORD0;
			};
				
			struct v2f {
				fixed4 vertex : POSITION;
				float2 color : COLOR;
				float2 texcoord : TEXCOORD0;
				half4 uvgrab : TEXCOORD1;
				half distortion : TEXCOORD2;
				half dark : TEXCOORD3;
				//float3 hoff : TEXCOORD2;
			};

			v2f vert (a2v i) {
				v2f o;
				o.vertex = UnityObjectToClipPos(i.vertex);
				o.color = i.color;
				o.texcoord = i.texcoord;
				o.uvgrab = ComputeGrabScreenPos(o.vertex);
				o.distortion = _Strength * _GrabTexture_TexelSize.x;
				o.dark = _Darkness * _Color.a;
				//o.hoff = 1.0/(1.0-_HeightOffset);
				return o;
			}

			fixed4 frag(v2f i) : COLOR{
				fixed2 norm = UnpackNormal(tex2D(_Distortion, i.texcoord)).rg;
				//float hoff = (norm.rg - _HeightOffset) * i.hoff;
				fixed4 relf = tex2D(_ReliefTex, i.texcoord);
				relf.a = saturate(relf.r * relf.g * relf.b);
				i.uvgrab.xy -= i.distortion * norm.rg;
#ifdef BLUR
				fixed4 col = fixed4 (ComputeBlur(_GrabTexture, i.uvgrab, _GrabTexture_TexelSize, norm.rg, _Blur), 1.0);
#else
				fixed4 col = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
#endif
				col.rgb += (relf.rgb * _Color.rgb) * _Color.a;
				col.rgb *= saturate (1 - i.dark * relf.a);
				col.rgb *= (1 - norm.r *_Relief);
				return col;
			}
			ENDCG
		}
	}
}
