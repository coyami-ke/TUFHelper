Shader "UI/ProximityBorder"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // Border Settings
        _BorderWidth ("Border Width", Range(0.001, 0.2)) = 0.05
        _BorderSoftness ("Border Softness", Range(0.001, 0.1)) = 0.01
        _BorderColor1 ("Closest Gradient Color", Color) = (0, 1, 1, 1)   // Cyan
        _BorderColor2 ("Furthest Gradient Color", Color) = (0.7, 0, 1, 1) // Purple

        // Proximity Settings
        _MouseUV ("Mouse Position (UV)", Vector) = (0.5, 0.5, 0, 0)
        _HoverRadius ("Hover Proximity Radius", Range(0.1, 2.0)) = 0.6
        _MinOpacity ("Min Border Opacity", Range(0.0, 1.0)) = 0.1
        _MaxOpacity ("Max Border Opacity", Range(0.0, 1.0)) = 1.0

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

            float _BorderWidth;
            float _BorderSoftness;
            float4 _BorderColor1;
            float4 _BorderColor2;

            float2 _MouseUV;
            float _HoverRadius;
            float _MinOpacity;
            float _MaxOpacity;

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

                float distanceToEdge = min(
                    min(IN.uv.x, 1.0 - IN.uv.x),
                    min(IN.uv.y, 1.0 - IN.uv.y)
                );

                float borderMask = 1.0 - smoothstep(_BorderWidth - _BorderSoftness, _BorderWidth, distanceToEdge);

                float distToMouse = distance(IN.uv, _MouseUV);
                float proximity = smoothstep(0.0, 1.0, 1.0 - saturate(distToMouse / _HoverRadius));

                fixed4 borderColor = lerp(_BorderColor2, _BorderColor1, proximity);
                float borderAlpha = lerp(_MinOpacity, _MaxOpacity, proximity) * borderMask;

                fixed3 finalColor = lerp(mainTex.rgb, borderColor.rgb, borderAlpha * borderColor.a);
                float finalAlpha = max(mainTex.a, borderAlpha);

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