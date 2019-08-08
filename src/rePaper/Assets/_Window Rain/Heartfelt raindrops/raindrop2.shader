// based on: https://www.shadertoy.com/view/ltffzl
Shader "Unlit/raindrop2"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_RainIntensity("Rain Amount", Range(0,1)) = 1.0

		_RainDrop1("Rain Drop1", Range(0,1)) = 0
		_RainDrop2("Rain Drop2", Range(0,1)) = 0
		_RainStatic("Rain Static", Range(0,1)) = 0

		_ZoomOut("Zoom Out", Range(0,1)) = 0.15
		_Speed("Drop Speed", Range(0,1)) = 0.25
		_RainNormal("Rain Normal", Range(0,1)) = 0.6
		_OvColor("Overlay Color", Color) = (1,1,1,1)
		_Blur("Blur Amount", Range(0,1)) = 1.0
		_BlurAdjust("Blur Adjust", Range(0,7)) = 7.0 //global blur adjust.
		//[KeywordEnum(No,Yes,Yes_Lightning,Cheap_Blur,Snow_Blur,Rev_Rain)] _isRain("Rain Type", Float) = 0
		//heat distort
		//_NoiseTex("Noise Texture", 2D) = "white" {}
		//_Heat("Heat Wave", Range(0.0,1.0)) = 1.0
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Transparent"}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			//#pragma multi_compile_fog

			#include "UnityCG.cginc"

			#define S(a, b, t) smoothstep(a, b, t)
			//#define USE_POST_PROCESSING
			//I'm most likely making this more complicated than it needs to be ._."
			#pragma multi_compile _ISRAIN_NO _ISRAIN_YES _SNOW_BLUR //_HEAT 
			#pragma multi_compile _GOOD_BLUR _CHEAP_BLUR _HIGH_BLUR
			#pragma multi_compile _ _REV_RAIN
			#pragma multi_compile _ _DXVA_COLOR
			#pragma multi_compile _ _ISRAIN_YES_LIGHTNING

			#if _ISRAIN_YES
				#define YES_RAIN
			#endif

			#if _ISRAIN_YES_LIGHTNING
				#define YES_RAIN
				#define USE_POST_PROCESSING
			#endif

			#if _CHEAP_BLUR
				#define CHEAP_BLUR
			#endif

			#if _GOOD_BLUR
				#define GOOD_BLUR
			#endif

			#if _HIGH_BLUR
				#define HIGH_BLUR
			#endif
			/*
			#if _HEAT
				#define HEAT_DIST
			#endif
			*/
			#if _SNOW_BLUR
				#define SNOW_BLUR
			#endif

			sampler2D _MainTex;//, _NoiseTex;// _GrabTexture,;
			float _RainNormal;
			float _RainIntensity;
			float _ZoomOut;
			float4 _OvColor;
			float _Speed;
			float _Blur;
			//float _Heat;
			//float _Contrast;
			float _RainDrop1;
			float _RainDrop2;
			float _RainStatic;
			//float _TimeDivide;
			float _BlurAdjust;
			//float _BlurSample;
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				//float4 grabUv : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			//sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//o.grabUv = UNITY_PROJ_COORD(ComputeGrabScreenPos(o.vertex));
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}


			float3 N13(float p) {
				//  from DAVE HOSKINS
				float3 p3 = frac(float3(p,p,p) * float3(.1031, .11369, .13787));
				p3 += dot(p3, p3.yzx + 19.19);
				return frac(float3((p3.x + p3.y)*p3.z, (p3.x + p3.z)*p3.y, (p3.y + p3.z)*p3.x));
			}

			float4 N14(float t) {
				return frac(sin(t*float4(123., 1024., 1456., 264.))*float4(6547., 345., 8799., 1564.));
			}
			float N(float t) {
				return frac(sin(t*12345.564)*7658.76);
			}

			float Saw(float b, float t) {
				return S(0., b, t)*S(1., b, t);
			}


			float2 DropLayer2(float2 uv, float t) {
				float2 UV = uv;

				uv.y += t * 0.75;
				float2 a = float2(6., 1.);
				float2 grid = a * 2.;
				float2 id = floor(uv*grid);

				float colShift = N(id.x);
				uv.y += colShift;

				id = floor(uv*grid);
				float3 n = N13(id.x*35.2 + id.y*2376.1);
				float2 st = frac(uv*grid) - float2(.5, 0);

				float x = n.x - .5;

				float y = UV.y*20.;
				float wiggle = sin(y + sin(y));
				x += wiggle * (.5 - abs(x))*(n.z - .5);
				x *= .7;
				float ti = frac(t + n.z);
				y = (Saw(.85, ti) - .5)*.9 + .5;
				float2 p = float2(x, y);

				float d = length((st - p)*a.yx);

				float mainDrop = S(.4, .0, d);

				float r = sqrt(S(1., y, st.y));
				float cd = abs(st.x - x);
				float trail = S(.23*r, .15*r*r, cd);
				float trailFront = S(-.02, .02, st.y - y);
				trail *= trailFront * r*r;

				y = UV.y;
				float trail2 = S(.2*r, .0, cd);
				float droplets = max(0., (sin(y*(1. - y)*120.) - st.y))*trail2*trailFront*n.z;
				y = frac(y*10.) + (st.y - .5);
				float dd = length(st - float2(x, y));
				droplets = S(.3, 0., dd);
				float m = mainDrop + droplets * r*trailFront;

				return float2(m, trail);
			}

			float StaticDrops(float2 uv, float t) {
				uv *= 40.;

				float2 id = floor(uv);
				uv = frac(uv) - .5;
				float3 n = N13(id.x*107.45 + id.y*3543.654);
				float2 p = (n.xy - .5)*.7;
				float d = length(uv - p);

				float fade = Saw(.025, frac(t + n.z));
				float c = S(.3, 0., d)*frac(n.z*10.)*fade;
				return c;
			}

			float2 Drops(float2 uv, float t, float l0, float l1, float l2) {
				float s = StaticDrops(uv, t)*l0;
				float2 m1 = DropLayer2(uv, t)*l1;
				float2 m2 = DropLayer2(uv*1.85, t)*l2;

				float c = s + m1.x + m2.x;
				c = S(.3, 1., c);

				return float2(c, max(m1.y*l0, m2.y*l1));
			}

			//random no..
			float N21(float2 p) {
				p = frac(p*float2(123.34, 345.45));
				p += dot(p, p + 34.345);
				return frac(p.x*p.y);
			}

			/*
			//...dxva color corrction
			float4 AdjustContrast(float4 color) {
				return saturate(lerp(float4(0.5, 0.5, 0.5, 0.5), color, _Contrast));
			}

			float4 AdjustContrastCurve(float4 color) {
				return pow(abs(color * 2 - 1), 1 / max(_Contrast, 0.0001)) * sign(color - 0.5) + 0.5;
			}
			inline float3 applyHue(float3 aColor, float aHue)
			{
				float angle = radians(aHue);
				float3 k = float3(0.57735, 0.57735, 0.57735);
				float cosAngle = cos(angle);
				//Rodrigues' rotation formula
				return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
			}

			inline float4 applyHSBEffect(float4 startColor, fixed4 hsbc)
			{
				float _Hue = 360 * hsbc.r;
				float _Brightness = hsbc.g * 2 - 1;
				float _Contrast = hsbc.b * 2;
				float _Saturation = hsbc.a * 2;

				float4 outputColor = startColor;
				outputColor.rgb = applyHue(outputColor.rgb, _Hue);
				outputColor.rgb = (outputColor.rgb - 0.5f) * (_Contrast)+0.5f;
				outputColor.rgb = outputColor.rgb + _Brightness;
				float3 intensity = dot(outputColor.rgb, float3(0.299, 0.587, 0.114));
				outputColor.rgb = lerp(intensity, outputColor.rgb, _Saturation);

				return outputColor;
			}
			*/

			fixed4 frag(v2f i) : SV_Target
			{
				float4 col = 0;
				col = tex2D(_MainTex, i.uv)* _OvColor;
				//blur variables
				float blur,numSamples, a, m;

			#ifdef YES_RAIN 
				float2 uv = ((i.uv * _ScreenParams.xy) - .5*_ScreenParams.xy) / _ScreenParams.y;
				float2 UV = i.uv.xy;
				float3 M = float3(0.0, 0.0, 0.0);
				float T = _Time.y;

				float t = T * .2;

				float rainAmount = _RainIntensity;

				float maxBlur = lerp(2., 5., rainAmount);
				float minBlur = 1.5;

				float story = 0.;
				float heart = 0.;

				float zoom = -cos(T*.2);
				uv *= _ZoomOut * 12;
				t *= _Speed*2;

				float3 allDrops = 0;

				float staticDrops = S(-.5, 1., rainAmount + _RainStatic )*2.;
				float layer1 = S(.25, .75, rainAmount + _RainDrop1 );
				float layer2 = S(.0, .5, rainAmount + 0.05 + _RainDrop2 );

			#if _REV_RAIN
				float2 c = Drops(uv, -1 * t, staticDrops, layer1, layer2);
				float2 e = float2(.001, 0.)*_RainNormal;
				float cx = Drops(uv + e, -1 * t, staticDrops, layer1, layer2).x;
				float cy = Drops(uv + e.yx, -1 * t, staticDrops, layer1, layer2).x;
				float2 n = float2(cx - c.x, cy - c.x);		// expensive normals
			#else
				//t = t / 2;
				float2 c = Drops(uv, t, staticDrops, layer1, layer2);
				float2 e = float2(.001, 0.)*_RainNormal;
				float cx = Drops(uv + e, t, staticDrops, layer1, layer2).x;
				float cy = Drops(uv + e.yx, t, staticDrops, layer1, layer2).x;
				float2 n = float2(cx - c.x, cy - c.x);		// expensive normals
				//#endif
			#endif

				//mipmap blur, slightly reduced usage paired with regular blur loop.
				float focus = lerp(maxBlur*_Blur - c.y, minBlur*_Blur, S(.1, .2, c.x));
				float4 texCoord = float4(UV.x + n.x, UV.y + n.y, 0, _Blur * 25 * _BlurAdjust / 7);
				//float4 texCoord = float4(UV.x + n.x, UV.y + n.y, 0,focus);
				float4 lod = tex2Dlod(_MainTex, texCoord);
				//col = lod.rgba;

			#ifdef CHEAP_BLUR
				//..blur using loop for Rain
				blur = _Blur * _BlurAdjust;
				blur *= .01;
				numSamples = 4;
				a = N21(i.uv)*6.2831;
				//[unroll]
				for (m = 0; m < numSamples; m++) {
					float2 offs = float2(sin(a), cos(a))*blur;
					float d = frac(sin((m + 1)*546.)*5424.);
					d = sqrt(d);
					offs *= d;
					col += tex2Dlod(_MainTex, texCoord + float4(offs.x, offs.y, 0, 0));
					a++;
				}
				col /= numSamples;
				col /= 1.5;

			#endif

			#ifdef GOOD_BLUR
										
				blur = _Blur * _BlurAdjust;
				blur *= .01;
				numSamples = 16;
				a = N21(i.uv)*6.2831;
				for (m = 0; m < numSamples; m++) {
					float2 offs = float2(sin(a), cos(a))*blur;
					float d = frac(sin((m + 1)*546.)*5424.);
					d = sqrt(d);
					offs *= d;
					col += tex2Dlod(_MainTex,  texCoord + float4(offs.x,offs.y,0,0));
					a++;
				}
				col /= numSamples;
			#endif


			#ifdef HIGH_BLUR

				blur = _Blur * _BlurAdjust;
				blur *= .01;
				numSamples = 64;
				a = N21(i.uv)*6.2831;
				for (m = 0; m < numSamples; m++) {
					float2 offs = float2(sin(a), cos(a))*blur;
					float d = frac(sin((m + 1)*546.)*5424.);
					d = sqrt(d);
					offs *= d;
					col += tex2Dlod(_MainTex, texCoord + float4(offs.x, offs.y, 0, 0));
					a++;
				}
				col /= numSamples;

			#endif

			#ifdef USE_POST_PROCESSING
				t = (T + 3.)*.5;										// make time sync with first lightnoing
				//float colFade = sin(t*.2)*.5 + .5 + story;
				//col *= lerp(float3(1.,1.,1.), float3(.8, .9, 1.3), colFade);	// subtle color shift
				float fade = 1;// S(0., 10., T);							// fade in at the start
				float lightning = sin(t*sin(t*10.));				
				//float lightning = sin(t*2*sin(t*10));				// lighting flicker
				lightning *= pow(max(0., sin(t + sin(t))), 50.);		// lightning flash, orig
				col *= 1. + 5 * lightning * fade*lerp(1., .1, 0.5);	// composite lightning
				// col *= 1. + lightning *lerp(1., .1, story*story)*2.;
				//col *= 1. - dot(UV -= .5, UV);							// vignette
				//col *= fade;										// composite start and end fade
			#endif

				col *= 1. - dot(UV -= .5, UV);							// vignette
				col = col * _OvColor;

			#endif //YES_RAIN END

			#ifdef SNOW_BLUR // blur without rain

				blur = 0.1 * _BlurAdjust;
				blur *= .01;
				numSamples = 10;
				a = N21(i.uv)*6.2831;
				for (m = 0; m < numSamples; m++) {
					float2 offs = float2(sin(a), cos(a))*blur;
					float d = frac(sin((m + 1)*546.)*5424.);
					d = sqrt(d);
					offs *= d;
					col += tex2D(_MainTex, i.uv + offs);//+ c.xy + float2(cx, cy));
					a++;
				}
				col /= numSamples;
				col *= _OvColor;

			#endif

			/*
			#ifdef HEAT_DIST
				float2 p_m = i.uv.xy;// / iResolution.xy;
				float2 p_d = p_m;

				p_d.y -= _Time.y * 0.1;

				float4 dst_map_val = tex2D(_NoiseTex, p_d);

				float2 dst_offset = dst_map_val.xy;
				dst_offset -= float2(.5, .5);
				dst_offset *= 2.;
				dst_offset *= 0.01*_Heat;


				//reduce effect towards Y top
				dst_offset *= (0 + p_m.y);

				float2 dist_tex_coord = p_m.xy + dst_offset;

				col = tex2D(_MainTex, dist_tex_coord)*  _OvColor;
			#endif
			*/

			#ifdef NO_RAIN

				col = tex2D(_MainTex, i.uv)* _OvColor;
			#endif

			#if _DXVA_COLOR  //dxva brightness correction(approx)
				//col = applyHSBEffect(col, (0, 0,5, 5));
				//1*2-1
				float4 intensity = dot(col, float4(0.299, 0.587, 0.114, 1.1)); //float4(0.299, 0.587, 0.114,1)			
				col = lerp(intensity, col, 1.1);
				return col;
			#else
				return col;
			#endif
			}
			ENDCG
		}
	}
}
