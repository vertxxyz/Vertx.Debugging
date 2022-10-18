// #include "UnityCG.cginc"
#include "VertxDebuggingShared.cginc"

struct Cast
{
    float4x4 Matrix;
    float3 Vector;
};

struct CastGroup
{
    Cast A;
    float4 Color;
};

StructuredBuffer<CastGroup> cast_buffer;

struct vertInput
{
    float2 uv : TEXCOORD0;
    uint vertexID : SV_VertexID;
    uint instanceID : SV_InstanceID;
};

struct geoInput
{
    float4 color : TEXCOORD1;
    // uint vertexID : SV_VertexID;
    uint instanceID : SV_InstanceID;
};

struct fragInput
{
    float4 position : SV_POSITION;
    float4 color : TEXCOORD1;
};

geoInput vert(vertInput input)
{
    geoInput o;
    uint index = input.instanceID * 128 + input.vertexID / 2;
    o.instanceID = index;
    if (index >= _InstanceCount)
    {
        o.color = 0;
        return o;
    }
    o.color = cast_buffer[index].Color;
    return o;
}

[maxvertexcount(4)]
void geo(line geoInput input[2], inout LineStream<fragInput> outputStream)
{
    const float3 vertices[8] =
    {
        float3(-1, -1, -1),
        float3(1, -1, -1),
        float3(-1, 1, -1),
        float3(1, 1, -1),
        float3(-1, -1, 1),
        float3(1, -1, 1),
        float3(-1, 1, 1),
        float3(1, 1, 1)
    };

    int instance = input[0].instanceID;
    if (instance >= _InstanceCount)
        return;
    Cast c = cast_buffer[instance].A;
    float3 center = mul(c.Matrix, float4(0, 0, 0, 1)).xyz;
    float3 v = normalize(c.Vector);

    float highest = -10000;
    float lowest = 10000;
    float3 h, l;
    for (int i = 0; i < 8; i++)
    {
        float3 p = vertices[i];
        p = mul(c.Matrix, float4(p, 1)).xyz;

        float3 direction = camera_direction_variable(p);
        float3 perpendicular = normalize(cross(direction, v));

        float3 pl = p - center;
        float d = dot(pl, perpendicular);
        float vd = dot(v, normalize(pl));
        // If our box is aligned with the cast vector, hint the distance comparisons with values that are aligned to the corners furthest along the cast.
        vd = max(1 - distance(0.5773502691896258, vd), 0) * 0.001;
        if (d - vd < lowest)
        {
            lowest = d - vd;
            l = p;
        }
        if (d + vd > highest)
        {
            highest = d + vd;
            h = p;
        }
    }

    fragInput f1;
    f1.color = input[0].color;
    f1.position = world_to_clip_pos(h);
    outputStream.Append(f1);

    fragInput f1b;
    f1b.color = f1.color;
    f1b.position = world_to_clip_pos(h + c.Vector);
    outputStream.Append(f1b);

    outputStream.RestartStrip();

    fragInput f2;
    f2.color = input[0].color;
    f2.position = world_to_clip_pos(l);
    outputStream.Append(f2);

    fragInput f2b;
    f2b.color = f2.color;
    f2b.position = world_to_clip_pos(l + c.Vector);
    outputStream.Append(f2b);
}
