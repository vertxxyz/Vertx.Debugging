// #include "UnityCG.cginc"
#include "VertxDebuggingShared.cginc"

StructuredBuffer<float4x4> mesh_buffer;

struct vertInput
{
    float4 vertex : POSITION;
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
    unity_ObjectToWorld = mesh_buffer[input.instanceID];
    o.color = color_buffer[input.instanceID];
    o.position = world_to_clip_pos(mul(unity_ObjectToWorld, float4(input.vertex.xyz, 1.0)));
    return o;
}