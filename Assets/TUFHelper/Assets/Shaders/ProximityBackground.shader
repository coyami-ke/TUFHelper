Shader "UI/ProximityBackground"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // Proximity Colors
        _GlowColor1 ("Closest Color", Color) = (0, 1, 1, 1)   // Cyan
        _GlowColor2 ("Furthest Color", Color) = (0.7, 0, 1, 1) // Purple

        // Proximity Settings
        _MouseUV ("Mouse Position (UV)", Vector) = (0.5, 0.5, 0, 0)
        _HoverRadius ("Hover Radius", Range(0.01, 2.0)) = 0.6
        _MinIntensity ("Min Glow Opacity", Range(0.0, 1.0)) = 0.0
        _MaxIntensity ("Max Glow Opacity", Range(0.0, 1.0)) = 0.8

        // Required UI Stencil properties
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;

            float4 _GlowColor1;
            float4 _GlowColor2;

            float2 _MouseUV;
            float _HoverRadius;
            float _MinIntensity;
            float _MaxIntensity;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.uv = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 mainTex = tex2D(_MainTex, IN.uv) * IN.color;

                // Derive aspect ratio dynamically from screen derivatives
                float aspect = abs(ddy(IN.uv.y) / max(ddx(IN.uv.x), 0.0001));

                // Scale X delta by aspect ratio to maintain circular distance
                float2 delta = IN.uv - _MouseUV;
                delta.x *= aspect;

                float distToMouse = length(delta);

                float proximity = smoothstep(0.0, 1.0, 1.0 - saturate(distToMouse / _HoverRadius));

                fixed4 glowColor = lerp(_GlowColor2, _GlowColor1, proximity);
                float glowAlpha = lerp(_MinIntensity, _MaxIntensity, proximity);

                fixed3 finalColor = lerp(mainTex.rgb, glowColor.rgb, glowAlpha * glowColor.a);
                float finalAlpha = mainTex.a;

                fixed4 finalOutput = fixed4(finalColor, finalAlpha);

                #ifdef UNITY_UI_CLIP_RECT
                finalOutput.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                return finalOutput;
            }
            ENDCG
        }
    }
}