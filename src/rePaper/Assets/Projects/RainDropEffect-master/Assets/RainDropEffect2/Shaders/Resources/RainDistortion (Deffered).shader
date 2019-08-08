Shader "RainDrop/Internal/RainDistortion (Deffered)" {

	Properties {
		_Color ("Main Color", COLOR) = (1,1,1,1)
		_Strength ("Distortion Strength", Range(0.0, 150.0)) = 50.0
		_Relief ("Relief Value", Range(0.0, 2.0)) = 1.5
		_Distortion ("Normalmap", 2D) = "bump" {}
		_ReliefTex ("Relief", 2D) = "black" {}
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
		
		GrabPass {
			Name "BASE"
			Tags { "LightMode" = "Always" }
		}
		
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma exclude_renderers gles
		#pragma surface surf NoLighting
		#pragma vertex vert
		
		half4 LightingNoLighting (SurfaceOutput s, half3 lightDir, half atten) {
           half4 c; 
           c.rgb = s.Albedo;
           c.a = s.Alpha;
           return c;
        }

		sampler2D _GrabTexture : register(s0);
		sampler2D _Distortion : register(s2);
		sampler2D _ReliefTex;
		float _Strength;
		float _Relief;
		float4 _Color;
		float4 _GrabTexture_TexelSize;

		struct Input {
			float2 uv_Distortion;
			float2 uv_ReliefTex;
			float4 screenPos;
			float3 color;
			float3 worldRefl;
			INTERNAL_DATA
		};

        void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			o.color = v.color;
		}
        
		void surf (Input IN, inout SurfaceOutput o) {
			fixed2 norm  = UnpackNormal(tex2D(_Distortion, IN.uv_Distortion)).rg;
			fixed4 tex = tex2D(_ReliefTex, IN.uv_ReliefTex);
			float3 distort = IN.color.rgb + tex.rgb * -_Strength;
			float4 overlay = tex.rgba * _Color.rgba;
		    float2 offset = distort * _GrabTexture_TexelSize.xy;
		    IN.screenPos.xy += offset;
		    float4 refrColor = tex2Dproj(_GrabTexture, IN.screenPos);
		    o.Alpha = refrColor.a;
		    o.Emission = refrColor.rgb + overlay.a * overlay.rgb;
		    o.Emission *= (1-norm.r*_Relief)*1.0;
		}
		ENDCG
	}
	FallBack "Transparent/Diffuse"
}