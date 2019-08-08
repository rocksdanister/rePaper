// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Glow" {
	Properties{
		_MainTex("Texture", 2D) = "white" {}
	_Color("Color", Color) = (1,1,1,1)
		_Glow("Intensity", Range(0, 3)) = 1
	}
		SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100
		Cull Off
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

		sampler2D _MainTex;
	half4 _MainTex_ST;
	fixed4 _Color;
	half _Glow;

	struct vertIn {
		float4 pos : POSITION;
		half2 tex : TEXCOORD0;
	};

	struct v2f {
		float4 pos : SV_POSITION;
		half2 tex : TEXCOORD0;
	};

	v2f vert(vertIn v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.pos);
		o.tex = v.tex * _MainTex_ST.xy + _MainTex_ST.zw;
		return o;
	}

	fixed4 frag(v2f f) : SV_Target{
		fixed4 col = tex2D(_MainTex, f.tex);
	col *= _Color;
	col *= _Glow;
	return col;
	}
		ENDCG
	}
	}
}