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

float3 circle_right_from_world_pos(float3 originWorld, float3 capsuleUp, float radius)
{
    float3 n = _WorldSpaceCameraPos.xyz - originWorld; // Normal
    float d1 = length(n); // Distance to camera
    n = n / d1; // Normalise n
    float3 capsuleRight = normalize(cross(n, capsuleUp));
    
    float r1Sqrd = radius * radius;
    float d = r1Sqrd / d1; // Distance from sphere to place circle
    float r = sqrt(r1Sqrd - d * d); // Radius of circle

    float3 v = float3(1, 0, 0) * r.xxx;
    return originWorld + n * d
        + capsuleRight * v.x
        + capsuleUp * v.y
        - n * v.z;
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
        float3 capsuleUp = normalize(outline.B - outline.A);
        float3 aCircle = circle_right_from_world_pos(outline.A, capsuleUp, outline.Radius);
        float3 bCircle = circle_right_from_world_pos(outline.B, capsuleUp, outline.Radius);

        float4 aProj = mul(UNITY_MATRIX_VP, float4(outline.A, 1.0));
        float4 bProj = mul(UNITY_MATRIX_VP, float4(outline.B, 1.0));
        float2 aCircleProj = mul(UNITY_MATRIX_VP, float4(aCircle, 1.0));
        float2 bCircleProj = mul(UNITY_MATRIX_VP, float4(bCircle, 1.0));

        float2 aToAc = aCircleProj - aProj.xy;
        float2 bToBc = bCircleProj - bProj.xy;
        float aR = length(aToAc);
        float bR = length(bToBc);
        
        float rR = max(bR, aR) - abs(bR - aR);
        float theta = atan(rR / length(bProj.xy - aProj.xy));
        float pi = 3.14159265359;
        float pi90 = pi * 0.5f;
        float theta2 = pi90 - (pi90 - theta);
        
        if(input.vertexID == 0)
        {
            // A
            // aToAc = rotate(aToAc, theta2);
            // aProj += float4(aToAc, 0, 0);
            o.position = float4(aCircleProj, aProj.z, 1.0);
            o.color = color_buffer[input.instanceID];
            return o;
        }
        else
        {
            // B
            // bToBc = rotate(bToBc, theta2);
            // bProj += float4(bToBc, 0, 0);
            o.position = float4(bCircleProj, bProj.z, 1.0);
            o.color = color_buffer[input.instanceID];
            return o;
        }
    }

    o.position = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
    o.color = color_buffer[input.instanceID];
    return o;
}
