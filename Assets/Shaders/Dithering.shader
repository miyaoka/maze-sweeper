/*
http://notargs.com/blog/?p=608
*/

Shader "Custom/Dithering" 
{
	Properties
	{
	_Color("Color", Color) = (1,1,1,1)
	_Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
	[Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
	[HideInInspector] _MainTex("Texture", 2D) = "white" {}
	}
		SubShader{

		//============================
		// レンダリング用のPass
		CGPROGRAM
		#pragma surface surf Standard

		half4 _Color;
		sampler3D	_DitherMaskLOD;

		// 入力構造体
		struct Input {
		float2 uv_MainTex;
		float3 worldPos;
		float4 screenPos;
		};

		void surf(Input IN, inout SurfaceOutputStandard o) {

			// ディザリングで半透明を表現
			half alphaRef = tex3D(_DitherMaskLOD, float3(IN.screenPos.xy / IN.screenPos.w * _ScreenParams.xy * 0.25, _Color.a*0.9375)).a;
			clip(alphaRef - 0.01);

			// 円形に切り抜く
			clip(0.5 - length(IN.uv_MainTex - 0.5));

			// 色
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG

		//============================
		// 影の判定を行うPass
		Pass{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual

			CGPROGRAM
			#pragma target 3.0

			#pragma vertex vert
			#pragma fragment frag

			#define UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
			#define UNITY_STANDARD_USE_DITHER_MASK
			#define UNITY_STANDARD_USE_SHADOW_UVS

			#include "UnityStandardShadow.cginc"

			// 頂点シェーダ出力構造体
			struct VertexOutput
			{
			V2F_SHADOW_CASTER_NOPOS
			float2 tex : TEXCOORD1;
			};

			// 頂点シェーダ
			void vert(VertexInput v, out VertexOutput o, out float4 opos : SV_POSITION)
			{
			TRANSFER_SHADOW_CASTER_NOPOS(o,opos)
			o.tex = v.uv0;
			}

			// フラグメントシェーダ
			half4 frag(VertexOutput i, UNITY_VPOS_TYPE vpos : VPOS) : SV_Target
			{
				// ディザリングで半透明を表現
				half alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy*0.25, _Color.a*0.9375)).a;
				clip(alphaRef - 0.01);

				// 円形に切り抜く
				clip(0.5 - length(i.tex - 0.5));

				SHADOW_CASTER_FRAGMENT(i)
			}

			ENDCG
		}
	}

	FallBack "Differd"
}