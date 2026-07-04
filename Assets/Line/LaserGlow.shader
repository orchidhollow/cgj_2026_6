Shader "Custom/LaserGlow"
{
    Properties
    {
        _Color ("Color", Color) = (0, 1, 1, 1)
        _Intensity ("Intensity", Float) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        
        Pass
        {
            Blend One One
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            
            float4 _Color;
            float _Intensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // 限制颜色范围，防止溢出
                return saturate(_Color * _Intensity);
            }
            ENDCG
        }
    }
}
