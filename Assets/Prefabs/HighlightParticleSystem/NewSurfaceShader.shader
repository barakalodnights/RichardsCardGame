Shader "Custom/NewSurfaceShader"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.BlendMode)]_Add_Blend("Add_Blend", Float) = 1
		[HDR]_Tint_01("Tint_01", Color) = (1,1,1,0)
		[HDR]_Tint_02("Tint_02", Color) = (1,1,1,0)
		_MainTex("MainTex", 2D) = "white" {}
	    _SpeedDirMainTex("Speed Dir MainTex", Vector) = (0,0,0,0)
		_NoiseMainTex("Noise MainTex", 2D) = "white" {}
		_NoiseStrengthMainTex("Noise Strength MainTex", Float) = 0
		_SpeedDirNoiseMainTex("Speed Dir Noise MainTex", Vector) = (0,0,0,0)
		_AlphaMask("Alpha Mask", 2D) = "white" {}
		_SpeedDirAlphaMask("Speed Dir Alpha Mask", Vector) = (0,0,0,0)
		_NoiseAlphaMask("Noise Alpha Mask", 2D) = "white" {}
		_NoiseAlphaMaskStrength("Noise Alpha Mask Strength", Float) = 0
		_SpeedDirNoiseAlphaMask("Speed Dir Noise Alpha Mask", Vector) = (0,0,0,0)
		_SubAlphaMask("Sub Alpha Mask", 2D) = "white" {}
		_SpeedDirSubAlphaMask("Speed Dir Sub Alpha Mask", Vector) = (0,0,0,0)
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode", Float) = 0
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
    }
    SubShader
    {
        Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha


		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 4.6
		#pragma surface surf Unlit keepalpha noshadow 
		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float4 uv2_texcoord2;
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
		};

		uniform float _CullMode;
		uniform float _Add_Blend;
		uniform float4 _Tint_01;
		uniform float4 _Tint_02;
		uniform sampler2D _MainTex;
		uniform float2 _SpeedDirMainTex;
		uniform float4 _MainTex_ST;
		uniform float _NoiseStrengthMainTex;
		uniform sampler2D _NoiseMainTex;
		uniform float2 _SpeedDirNoiseMainTex;
		uniform float4 _NoiseMainTex_ST;
		uniform sampler2D _AlphaMask;
		uniform float2 _SpeedDirAlphaMask;
		uniform float4 _AlphaMask_ST;
		uniform float _NoiseAlphaMaskStrength;
		uniform sampler2D _NoiseAlphaMask;
		uniform float2 _SpeedDirNoiseAlphaMask;
		uniform sampler2D _SubAlphaMask;
		uniform float2 _SpeedDirSubAlphaMask;
		uniform float4 _SubAlphaMask_ST;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float ControlMoveMainTexByU60 = i.uv2_texcoord2.w;
			float2 appendResult69 = (float2(0.0 , ControlMoveMainTexByU60));
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 panner6 = ( 1.0 * _Time.y * _SpeedDirMainTex + uv_MainTex);
			float2 uv_NoiseMainTex = i.uv_texcoord * _NoiseMainTex_ST.xy + _NoiseMainTex_ST.zw;
			float2 panner11 = ( 1.0 * _Time.y * _SpeedDirNoiseMainTex + uv_NoiseMainTex);
			float4 lerpResult56 = lerp( _Tint_01 , _Tint_02 , tex2D( _MainTex, ( appendResult69 + ( panner6 + ( _NoiseStrengthMainTex * tex2D( _NoiseMainTex, panner11 ).r ) ) ) ));
			o.Emission = ( lerpResult56 * i.vertexColor ).rgb;
			float ControlMoveAlphaByU59 = i.uv2_texcoord2.z;
			float2 appendResult64 = (float2(0.0 , ControlMoveAlphaByU59));
			float2 uv_AlphaMask = i.uv_texcoord * _AlphaMask_ST.xy + _AlphaMask_ST.zw;
			float2 panner18 = ( 1.0 * _Time.y * _SpeedDirAlphaMask + uv_AlphaMask);
			float2 panner23 = ( 1.0 * _Time.y * _SpeedDirNoiseAlphaMask + uv_AlphaMask);
			float2 uv_SubAlphaMask = i.uv_texcoord * _SubAlphaMask_ST.xy + _SubAlphaMask_ST.zw;
			float2 panner30 = ( 1.0 * _Time.y * _SpeedDirSubAlphaMask + uv_SubAlphaMask);
			o.Alpha = ( tex2D( _AlphaMask, ( float4( appendResult64, 0.0 , 0.0 ) + ( float4( panner18, 0.0 , 0.0 ) + ( _NoiseAlphaMaskStrength * tex2D( _NoiseAlphaMask, panner23 ) ) ) ).rg ) * tex2D( _SubAlphaMask, panner30 ) * i.vertexColor.a ).r;
		}
        
        ENDCG
    }
	CustomEditor "ASEMaterialInspector"
    FallBack "Diffuse"
}
