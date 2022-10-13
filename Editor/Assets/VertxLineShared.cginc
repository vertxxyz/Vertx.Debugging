// #include "UnityCG.cginc"
#include "VertxDebuggingShared.cginc"

struct Line
{
    float3 A, B;
};

StructuredBuffer<Line> line_buffer;

struct vertInput
{
    float2 uv : TEXCOORD0;
    uint vertexID : SV_VertexID;
    uint instanceID : SV_InstanceID;
};

struct v2f
{
    float4 position : SV_POSITION;
    float4 color : TEXCOORD1;
};

v2f vert(vertInput input)
{
    v2f o;
    Line l = line_buffer[input.instanceID];
    o.position = world_to_clip_pos(input.vertexID == 0 ? l.A : l.B);
    o.color = color_buffer[input.instanceID];
    return o;
}