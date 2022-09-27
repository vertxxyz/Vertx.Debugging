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

float2 rotate(float2 v, float a)
{
    float s, c;
    sincos(a, s, c);
    float tx = v.x;
    float ty = v.y;
    v.x = c * tx - s * ty;
    v.y = s * tx + c * ty;
    return v;
}

float2 rotate(float2 v, float s, float c)
{
    float tx = v.x;
    float ty = v.y;
    v.x = c * tx - s * ty;
    v.y = s * tx + c * ty;
    return v;
}

bool closest_plane_circle_intersection(
    float3 circleCenter, 
    float3 circleNormal, 
    float circleRadius, 
    float3 planePoint,
    float3 planeNormal,
    out float3 intersection
)
{
    
}

v2f vert(vertInput input)
{
    v2f o;
    Outine outline = outline_buffer[input.instanceID];

    float3 originWorld = input.vertexID == 0 ? outline.A : outline.B;

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
        float3 normal = normalize(_WorldSpaceCameraPos.xyz - originWorld); // Normal
        
        float3 direction = normalize(outline.B - outline.A);
        float3 right = normalize(cross(normal, direction)) * outline.Radius;

        float newRadius, offset;
        float3 oNormal;
        get_circle_info(originWorld, outline.Radius, newRadius, offset, oNormal);

        // TODO find the intersection between this new plane (the one created by get_circle_info)
        // and the circle that is at originWorld, facing in direction, and of outline.Radius;
        // it's at the closest intersection that we should position the line vertex.
        // First, we must simply the equation to move the plane's origin to 0, 0, 0

        float3 intersection;
        if (!closest_plane_circle_intersection(originWorld, normal, outline.Radius, originWorld + oNormal * offset, oNormal, intersection))
            intersection = originWorld + right;

        if (input.vertexID == 0)
        {
            // A
            o.position = mul(UNITY_MATRIX_VP, float4(outline.A + right, 1.0));
            o.color = color_buffer[input.instanceID];
            return o;
        }
        else
        {
            // B
            o.position = mul(UNITY_MATRIX_VP, float4(outline.B + right, 1.0));
            o.color = float4(1, 0, 0, 1);
            return o;
        }
    }

    o.position = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
    o.color = color_buffer[input.instanceID];
    return o;
}
