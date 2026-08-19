Shader "UI/IrisTransition"
{
    Properties
    {
        [PerRendererData] _MainTex (
            "Sprite Texture",
            2D) = "white" {}

        _Color ("Color", Color) = (0, 0, 0, 1)

        _Radius (
            "Radius",
            Range(0, 1)) = 1

        _Softness (
            "Softness",
            Range(0.001, 0.1)) = 0.01

        [HideInInspector]
        _StencilComp ("Stencil Comparison", Float) = 8

        [HideInInspector]
        _Stencil ("Stencil ID", Float) = 0

        [HideInInspector]
        _StencilOp ("Stencil Operation", Float) = 0

        [HideInInspector]
        _StencilWriteMask ("Stencil Write Mask", Float) = 255

        [HideInInspector]
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        [HideInInspector]
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
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
        ZTest Always

        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _Radius;
            float _Softness;

            v2f vert(appdata input)
            {
                v2f output;

                output.vertex =
                    UnityObjectToClipPos(input.vertex);

                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 centered =
                    input.uv - float2(0.5, 0.5);

                float aspect =
                    _ScreenParams.x / _ScreenParams.y;

                centered.x *= aspect;

                float maximumDistance =
                    length(float2(
                        0.5 * aspect,
                        0.5));

                float normalizedDistance =
                    length(centered) /
                    maximumDistance;

                float alpha = smoothstep(
                    _Radius,
                    _Radius + _Softness,
                    normalizedDistance);

                if (_Radius <= 0.001)
                    alpha = 1.0;

                fixed4 color =
                    _Color * input.color;

                color.a *= alpha;

                return color;
            }

            ENDCG
        }
    }
}
