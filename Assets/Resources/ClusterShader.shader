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

            struct sphere
            {
				float3 center;
				float radius;
			};

            struct meshlet
            {
                uint triangleOffset;
                uint triangleCount;
                uint lod;
                sphere sph;
                sphere parentSph;
                float error;
                float parentError;
            };

            StructuredBuffer<meshlet> _DataBuffer;
            StructuredBuffer<float3> _VertexBuffer;
            StructuredBuffer<uint> _IndexBuffer;
            StructuredBuffer<uint> _ResultBuffer;

            v2f vert (uint vertexID : SV_VERTEXID, uint instanceID : SV_INSTANCEID)
            {
                v2f o;
                uint actulId = _ResultBuffer[instanceID];
                uint offset = _DataBuffer[actulId].triangleOffset;
                if(vertexID >= _DataBuffer[actulId].triangleCount * 3) vertexID = _DataBuffer[actulId].triangleCount * 3 - 1;
                int index = _IndexBuffer[offset + vertexID];
                // int dindex = _DataBuffer[index];
                float3 pos = _VertexBuffer[index];
                o.pos = TransformWorldToHClip(pos.xyz);
                float seed = fmod(actulId, 123.456);
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