#include "UnityCG.cginc"
#include "DebuggingShared.cginc"

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

    Box a = box_buffer[input.instanceID];
    unity_ObjectToWorld = a.Matrix;

    o.color = color_buffer[input.instanceID];
    int modifications = modifications_buffer[input.instanceID];

    float4 worldPos = mul(unity_ObjectToWorld, float4(input.vertex.xyz, 1.0));

    o.position = mul(UNITY_MATRIX_VP, worldPos);

    if (has_normal_fade(modifications))
    {
        float3 worldViewDir = _WorldSpaceCameraPos.xyz - worldPos;
        float d = dot(worldViewDir, UnityObjectToWorldNormal(input.normal));
        d = saturate(
            smoothstep(0, 0.1, d) // front face
        );

        o.color.a *= max(0.3, d);
    }
    return o;
}