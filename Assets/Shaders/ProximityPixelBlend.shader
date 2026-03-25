Shader "Custom/ProximityPixelBlend"
{
    Properties
    {
        _ColorFar ("Far Color", Color) = (1,0,0,1)
        _ColorNear ("Near Color", Color) = (0,1,0,1)
        _PixelSize ("Pixel Size (world units)", Float) = 0.25
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 _ColorFar, _ColorNear;
            float _PixelSize;
            int _PointCount;
            float4 _Points[16]; // xyz = pos, w = radius

            struct appdata { float4 vertex : POSITION; };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // snap world position to grid for pixelation effect
                float3 snapped = floor(i.worldPos / _PixelSize) * _PixelSize;

                // find strongest influence (closest, normalized)
                float blend = 0;
                for (int j = 0; j < _PointCount; j++)
                {
                    float dist = distance(snapped, _Points[j].xyz);
                    float t = 1 - saturate(dist / _Points[j].w);
                    blend = max(blend, t);
                }

                return lerp(_ColorFar, _ColorNear, blend);
            }
            ENDCG
        }
    }
}
