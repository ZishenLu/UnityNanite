// ProceduralShader.shader
Shader "Custom/ProceduralShader"
{
    SubShader
    {
        Cull Back
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                uint vertexID : SV_VertexID;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            StructuredBuffer<uint> _DataBuffer;
            StructuredBuffer<float3> _VertexBuffer;

            v2f vert (uint vertexID : SV_VERTEXID, uint instanceID : SV_INSTANCEID)
            {
                v2f o;
                int index = vertexID + instanceID * 103 * 3;
                int dindex = _DataBuffer[index];
                float3 pos = _VertexBuffer[dindex];
                o.pos = TransformWorldToHClip(pos.xyz);
                float seed = fmod(instanceID, 123.456);
                float3 randomColor = frac(sin(float3(seed, seed+1, seed+2)) * 43758.5453);
                o.color = float4(randomColor, 1); // Ä¬ÈÏ°×É«
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    }
}