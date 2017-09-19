// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/OutlineEffect" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Opaque" }
			LOD 200
			ZTest Always
			ZWrite Off
			Cull Off
			
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _OutlineSource;

			struct v2f {
			   float4 position : SV_POSITION;
			   float2 uv : TEXCOORD0;
			};
			
			v2f vert(appdata_img v)
			{
			   	v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				
			   	return o;
			}

			half4 frag (v2f input) : COLOR
			{	
				float2 uv = input.uv;

				half4 originalPixel = tex2D(_MainTex,input.uv);
				half4 outlineSource = tex2D(_OutlineSource, uv);

				half4 outline = 0;
				bool hasOutline = false;
				
				//blend the image and outline
				if(outlineSource.a != 0)
				{
					outline = 1 - outlineSource;
					outline.a = outlineSource.a;
					hasOutline = true;
				}

				if(hasOutline)
					return originalPixel + outline;
				else
					return originalPixel;
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}