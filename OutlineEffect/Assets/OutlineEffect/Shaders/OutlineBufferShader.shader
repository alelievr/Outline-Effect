Shader "Hidden/OutlineBufferEffect" {
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
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

		//unlit surface
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

		void getColor(float2 uv, inout half4 color, Input IN)
		{
			half4 c = tex2D(_MainTex, IN.uv_MainTex + uv);
			if (color.a < c.a)
				color = c;
		}

		void surf (Input IN, inout SurfaceOutput o)
		{
			if (_FlipY == 1)
				IN.uv_MainTex.y = 1 - IN.uv_MainTex.y;

			//discard if no outline
			if (_Color.a == 0 || _LineThickness <= 0 || _LineIntensity == 0)
				discard;
			
			//outline thickness
			half4 color = half4(0);
			if (_LineThickness == 1)
			{
				getColor(float2(0, + _MainTex_TexelSize.y), color, IN);
				getColor(float2(0, - _MainTex_TexelSize.y), color, IN);
				getColor(float2(- _MainTex_TexelSize.x, 0), color, IN);
				getColor(float2(+ _MainTex_TexelSize.x, 0), color, IN);
			}
			else if (_LineThickness == 2)
			{
				getColor(float2(0, + _MainTex_TexelSize.y * 2), color, IN);
				getColor(float2(0, - _MainTex_TexelSize.y * 2), color, IN);
				getColor(float2(- _MainTex_TexelSize.x * 2, 0), color, IN);
				getColor(float2(+ _MainTex_TexelSize.x * 2, 0), color, IN);
				getColor(float2(+ _MainTex_TexelSize.x, + _MainTex_TexelSize.y), color, IN);
				getColor(float2(+ _MainTex_TexelSize.x, - _MainTex_TexelSize.y), color, IN);
				getColor(float2(- _MainTex_TexelSize.x, + _MainTex_TexelSize.y), color, IN);
				getColor(float2(- _MainTex_TexelSize.x, - _MainTex_TexelSize.y), color, IN);
			}
			else
			{
				int xo = _LineThickness;
				int yo = 0;
				int	err = 0;
				while (xo >= yo)
				{
					float x = xo * _MainTex_TexelSize;
					float y = yo * _MainTex_TexelSize;
					getColor(float2( x,  y), color, IN);
					getColor(float2( y,  x), color, IN);
					getColor(float2(-y,  x), color, IN);
					getColor(float2(-x, -y), color, IN);
					getColor(float2(-y, -x), color, IN);
					getColor(float2( y, -x), color, IN);
					getColor(float2( x, -y), color, IN);

					yo++;
					err += 1 + 2 * yo;
					if (2 * (err - xo) + 1 > 0)
					{
						xo -= 1;
						err += 1 - 2 * xo;
					}
				}
			}

			//get current pixel
			half4 c = tex2D(_MainTex, IN.uv_MainTex);
			if (color.a != 0)
			{
				if ((_FullSprite == 1)
					|| (_FullSprite == 0 && c.a == 0 && _AlphaCutoff == 0)
					|| (_FullSprite == 0 && c.a <= _AlphaCutoff && color.a >= _AlphaCutoff))
				{
					if (_AutoColor)
					{
						o.Albedo = (1 - color) / 2;
						o.Alpha = color.a;
					}
					else
					{
						//TODO: blend with others glows
						o.Albedo = (1 - _Color) / 2;
						o.Alpha = _Color.a * _LineIntensity;
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
