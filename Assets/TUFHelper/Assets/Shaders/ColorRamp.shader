Shader "UI/ColorRamp"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // Required for Canvas Masking & Mask Component
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

            fixed4 _Color;
            float4 _ClipRect;

            // Ramp Data Uniforms
            int _PointCount;
            float _RampPositions[8];
            float4 _RampColors[8];
            float _RampInterpolations[8];

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.uv = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            float3 EvaluateCatmullRom(float3 p0, float3 p1, float3 p2, float3 p3, float t)
            {
                float t2 = t * t;
                float t3 = t2 * t;

                return 0.5 * (
                    (2.0 * p1) +
                    (-p0 + p2) * t +
                    (2.0 * p0 - 5.0 * p1 + 4.0 * p2 - p3) * t2 +
                    (-p0 + 3.0 * p1 - 3.0 * p2 + p3) * t3
                );
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float u = saturate(IN.uv.x);
                float3 finalColor = float3(1, 1, 1);

                if (_PointCount > 0)
                {
                    if (u <= _RampPositions[0])
                    {
                        finalColor = _RampColors[0].rgb;
                    }
                    else if (u >= _RampPositions[_PointCount - 1])
                    {
                        finalColor = _RampColors[_PointCount - 1].rgb;
                    }
                    else
                    {
                        int idx = 0;
                        for (int p = 0; p < _PointCount - 1; p++)
                        {
                            if (u >= _RampPositions[p] && u <= _RampPositions[p + 1])
                            {
                                idx = p;
                                break;
                            }
                        }

                        float pos1 = _RampPositions[idx];
                        float pos2 = _RampPositions[idx + 1];
                        float t = (u - pos1) / max(0.0001, pos2 - pos1);
                        int interp = (int)_RampInterpolations[idx];

                        if (interp == 0) // Constant
                        {
                            finalColor = _RampColors[idx].rgb;
                        }
                        else if (interp == 1) // Linear
                        {
                            finalColor = lerp(_RampColors[idx].rgb, _RampColors[idx + 1].rgb, t);
                        }
                        else // Catmull-Rom
                        {
                            int idx0 = max(0, idx - 1);
                            int idx3 = min(_PointCount - 1, idx + 2);

                            float3 c0 = _RampColors[idx0].rgb;
                            float3 c1 = _RampColors[idx].rgb;
                            float3 c2 = _RampColors[idx + 1].rgb;
                            float3 c3 = _RampColors[idx3].rgb;

                            finalColor = saturate(EvaluateCatmullRom(c0, c1, c2, c3, t));
                        }
                    }
                }

                fixed4 color = fixed4(finalColor, 1.0) * IN.color;

                // Handle Rect Masking (2D ScrollRects / Masks)
                #if UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                return color;
            }
            ENDCG
        }
    }
}