Shader "Custom/CRTEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.5
        _ScanlineCount ("Scanline Count", Float) = 200
        _Curvature ("Screen Curvature", Range(0, 0.1)) = 0.02
        _VignetteStrength ("Vignette Strength", Range(0, 1)) = 0.3
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.01)) = 0.002
        _Brightness ("Brightness", Range(0.5, 1.5)) = 1.0
        _GreenTint ("Green Tint", Range(0, 2)) = 1.2
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScanlineIntensity;
            float _ScanlineCount;
            float _Curvature;
            float _VignetteStrength;
            float _ChromaticAberration;
            float _Brightness;
            float _GreenTint;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float2 CurveUV(float2 uv)
            {
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) / float2(_Curvature, _Curvature);
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;
                return uv;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Apply screen curvature
                float2 curvedUV = CurveUV(i.uv);
                
                // Discard pixels outside curved screen
                if (curvedUV.x < 0.0 || curvedUV.x > 1.0 || curvedUV.y < 0.0 || curvedUV.y > 1.0)
                    discard;

                // Chromatic aberration
                float2 offset = (_ChromaticAberration * (curvedUV - 0.5));
                float r = tex2D(_MainTex, curvedUV - offset).r;
                float g = tex2D(_MainTex, curvedUV).g;
                float b = tex2D(_MainTex, curvedUV + offset).b;
                
                fixed4 col = fixed4(r, g * _GreenTint, b, 1.0);

                // Scanlines
                float scanline = sin(curvedUV.y * _ScanlineCount * 3.14159 * 2.0);
                scanline = scanline * 0.5 + 0.5;
                scanline = lerp(1.0, scanline, _ScanlineIntensity);
                col.rgb *= scanline;

                // Vignette
                float2 vignetteUV = curvedUV * (1.0 - curvedUV);
                float vignette = vignetteUV.x * vignetteUV.y * 15.0;
                vignette = pow(vignette, _VignetteStrength);
                col.rgb *= vignette;

                // Apply brightness
                col.rgb *= _Brightness;

                // Slight flicker (time-based)
                float flicker = 0.95 + 0.05 * sin(_Time.y * 50.0);
                col.rgb *= flicker;

                return col;
            }
            ENDCG
        }
    }
}
