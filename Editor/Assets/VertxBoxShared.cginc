#include "UnityCG.cginc"
#include "VertxDebuggingShared.cginc"

struct Box
{
    float4x4 Matrix;
};

StructuredBuffer<Box> box_buffer;

struct vertInput
{
    float2 uv : TEXCOORD0;
    float4 vertex : POSITION;
    float4 normal : NORMAL;
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

    int index = input.instanceID * 11 + input.vertexID / 24; // 12 edges, 2 verts each.
    if (index >= _InstanceCount)
    {
        o.position = 0;
        o.color = 0;
        return o;
    }

    Box a = box_buffer[index];
    unity_ObjectToWorld = a.Matrix;

    o.color = color_buffer[index];
    int modifications = modifications_buffer[index];

    float4 worldPos = mul(unity_ObjectToWorld, float4(input.vertex.xyz, 1.0));
    float3 cameraDirection = camera_direction_variable(worldPos);
    
    o.position = mul(UNITY_MATRIX_VP, offset_world_towards_camera(worldPos, cameraDirection));

    if (has_normal_fade(modifications))
    {
        float3 normalA = UnityObjectToWorldNormal(input.normal * float3(1, 0, 0));
        float3 normalB = UnityObjectToWorldNormal(input.normal * float3(0, 1, 0));
        float3 normalC = UnityObjectToWorldNormal(input.normal * float3(0, 0, 1));
        
        o.color.a *= max(0.3, step(0, max(max(dot(cameraDirection, normalA), dot(cameraDirection, normalB)), dot(cameraDirection, normalC))));
    }
    return o;
}