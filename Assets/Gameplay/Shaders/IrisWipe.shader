Shader "UI/IrisWipe"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Mask Color", Color) = (0,0,0,1)
        _Radius ("Iris Radius", Range(0, 1.5)) = 1.5
        _Center ("Iris Center", Vector) = (0.5, 0.5, 0, 0)
        _Softness ("Softness", Range(0.0001, 0.1)) = 0.005
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
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

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
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            float _Radius;
            float4 _Center;
            float _Softness;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 center = _Center.xy;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                if (aspect <= 0.001) aspect = 16.0 / 9.0;

                float2 uv = IN.texcoord;
                float2 distVec = uv - center;
                distVec.x *= aspect;

                float dist = length(distVec);

                // Max distance from center to corner with aspect ratio
                float maxDist = length(float2(0.5 * aspect, 0.5));
                float currentRadius = _Radius * maxDist;

                float alpha = smoothstep(currentRadius, currentRadius + _Softness, dist);

                return fixed4(IN.color.rgb, alpha * IN.color.a);
            }
            ENDCG
        }
    }
}
