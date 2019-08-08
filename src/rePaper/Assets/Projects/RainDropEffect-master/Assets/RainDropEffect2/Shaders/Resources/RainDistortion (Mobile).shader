// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "RainDrop/Internal/RainDistortion (Mobile)" {

	Properties {
		_Strength ("Distortion Strength", Range(0.0, 1000.0)) = 50.0
		_Distortion ("Normalmap", 2D) = "bump" {}
	}

    SubShader
    {
       Tags {
			"RenderType"="Transparent"
			"Queue"="Transparent" 
		}
		LOD 100
		
		Cull Back
		ZWrite On
		ZTest Always
		ColorMask RGB

        // Grab the screen behind the object into _BackgroundTexture
		GrabPass{
			"_BackgroundTexture"
		}

        // Render the object with the texture generated above, and invert the colors
		Pass {
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "RainDropGlobal.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			sampler2D _Distortion : register(s2);
			sampler2D _BackgroundTexture;
			half4 _BackgroundTexture_TexelSize;
			half _Strength;

			struct a2v {
				fixed4 vertex : POSITION;
				fixed2 texcoord : TEXCOORD0;
			};

			struct v2f {
				fixed4 vertex : POSITION;
				fixed2 texcoord : TEXCOORD0;
				half4 uvgrab : TEXCOORD1;
				fixed2 distort : TEXCOORD2;
			};
			
			v2f vert (a2v v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				o.uvgrab = ComputeGrabScreenPos(o.vertex);
				o.distort = _Strength * _BackgroundTexture_TexelSize.xy;
				return o;
			}

			fixed4 frag (v2f i) : COLOR {
				fixed2 norm = UnpackNormal(tex2D(_Distortion, i.texcoord)).rg;
				i.uvgrab.xy -= i.distort * norm.rg;
				return tex2Dproj(_BackgroundTexture, UNITY_PROJ_COORD(i.uvgrab));
			}
			ENDCG
		}
    }
}
