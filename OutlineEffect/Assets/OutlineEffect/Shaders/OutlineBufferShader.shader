/*
//  Copyright (c) 2015 José Guerreiro. All rights reserved.
//
//  MIT license, see http://www.opensource.org/licenses/mit-license.php
//  
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//  
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
//  
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
*/

Shader "Hidden/OutlineBufferEffect" {
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		Blend One OneMinusSrcAlpha

		CGPROGRAM
		#pragma surface surf NoLighting vertex:vert nofog keepalpha
		#pragma multi_compile _ PIXELSNAP_ON

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo; 
			c.a = s.Alpha;
			return c;
		}


		uniform float4 _MainTex_TexelSize;
		
		sampler2D	_MainTex;
		fixed4		_Color;
		float		_AlphaCutoff;
		int			_PixelSnap = 0;
		int			_FullSprite = 0;
		int			_FlipY = 0;
		int			_AutoColor = 0;
		float		_LineThickness = 1;
		float		_LineIntensity = 1;
		float		_AllowOutlineOverlap = 0;

		struct Input
		{
			float2 uv_MainTex;
			fixed4 color;
			half2 texcoord : TEXCOORD0;
		};
		
		void vert (inout appdata_full v, out Input o)
		{
			if (_PixelSnap)
				v.vertex = UnityPixelSnap (v.vertex);
			
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.color = v.color;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			if (_FlipY == 1)
				IN.uv_MainTex.y = 1 - IN.uv_MainTex.y;

			//discarf if no outline
			if (_Color.a == 0 || _LineThickness <= 0 || _LineIntensity == 0)
				discard;
			
			//get current pixel
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

			//1 width outline:
			half4 up = tex2D(_MainTex, IN.uv_MainTex + float2(0, _MainTex_TexelSize.y));
			half4 down = tex2D(_MainTex, IN.uv_MainTex - float2(0, _MainTex_TexelSize.y));
			half4 right = tex2D(_MainTex, IN.uv_MainTex - float2(_MainTex_TexelSize.x, 0));
			half4 left = tex2D(_MainTex, IN.uv_MainTex + float2(_MainTex_TexelSize.x, 0));

			half4 color;
			if (up.a != 0)
				color = up;
			else if (down.a != 0)
				color = down;
			else if (left.a != 0)
				color = left;
			else if (right.a != 0)
				color = right;
			
			if (color.a != 0)
			{
				if (_FullSprite == 1 || (_FullSprite == 0 && c.a == 0))
				{
					if (_AutoColor)
					{
						o.Albedo = (1 - color) / 2;
						o.Alpha = color.a;
					}
					else
					{
						o.Albedo = (1 - _Color) / 2;
						o.Alpha = _Color.a * _LineIntensity; //TODO: blend with others glows
					}
				}
				else if (!_AllowOutlineOverlap)
				{
					o.Albedo = float4(1, 1, 1, 1);
					o.Alpha = 1;
				}
			}
		}
		ENDCG
	}

	Fallback "Transparent/VertexLit"
}
