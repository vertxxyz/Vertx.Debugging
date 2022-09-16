#include "UnityCG.cginc"
#include "DebuggingShared.cginc"

struct Outine
{
    float3 A, B;
    float Radius;
};

StructuredBuffer<Outine> outline_buffer;

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
    Outine outline = outline_buffer[input.instanceID];

    float3 originWorld = input.vertexID == 0
                             ? outline.A
                             : outline.B;

    float3 worldPos;
    if (is_orthographic())
    {
        float3 capsuleDirection = outline.B - outline.A;
        float3 up = normalize(capsuleDirection);
        float3 right = normalize(cross(-camera_direction(), up));


        worldPos = originWorld
            + right * outline.Radius;
    }
    else
    {
        float r1 = outline.Radius; // Original radius
        float3 n = _WorldSpaceCameraPos.xyz - originWorld; // Normal
        float d1 = length(n); // Distance to camera
        n = n / d1; // Normalise n
        float r1Sqrd = r1 * r1;
        float d = r1Sqrd / d1; // Distance from sphere to place circle
        float r = sqrt(r1Sqrd - d * d); // Radius of circle

        /*float3 up = UNITY_MATRIX_IT_MV[1].xyz;
        float3 right = normalize(cross(n, up));
        up = normalize(cross(right, n));*/

        float3 capsuleDirection = outline.B - outline.A;
        float3 up = normalize(capsuleDirection);
        float3 right = normalize(cross(n, up));
        up = normalize(cross(right, n));


        worldPos = originWorld
            + right * outline.Radius;
    }

    o.position = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
    o.color = color_buffer[input.instanceID];
    return o;
}
