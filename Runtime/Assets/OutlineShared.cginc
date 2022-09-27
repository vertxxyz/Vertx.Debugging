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

float get_tangent_rotation(float aR, float bR, float d)
{
    float rR = max(aR, bR) - min(aR, bR);
    float theta = acos(rR / d);
    return 3.14159265359 * 0.5f - theta;
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
        float aR, offsetToNewCircleA, bR, offsetToNewCircleB;
        float3 offsetNormalA, offsetNormalB;
        get_circle_info(outline.A, outline.Radius, aR, offsetToNewCircleA, offsetNormalA);
        get_circle_info(outline.B, outline.Radius, bR, offsetToNewCircleB, offsetNormalB);
        float3 circleAPos = outline.A + offsetNormalA * offsetToNewCircleA;
        float3 circleBPos = outline.B + offsetNormalB * offsetToNewCircleB;
        // float totalDistance = distance(circleAPos, circleBPos);

        float3 direction = normalize(outline.B - outline.A);
        float3 capsuleRightA = normalize(cross(offsetNormalA, direction));
        float3 capsuleRightB = normalize(cross(offsetNormalB, direction));
        
        float3 circleTangentA = circleAPos
            + capsuleRightA * aR;
        float3 circleTangentB = circleBPos
            + capsuleRightB * bR;

        float4 circleCenterA_NDC = ComputeScreenPos(mul(UNITY_MATRIX_VP, float4(circleAPos, 1.0)));
        float4 circleCenterB_NDC = ComputeScreenPos(mul(UNITY_MATRIX_VP, float4(circleBPos, 1.0)));
        float4 circleTangentA_NDC = ComputeScreenPos(mul(UNITY_MATRIX_VP, float4(circleTangentA, 1.0)));
        float4 circleTangentB_NDC = ComputeScreenPos(mul(UNITY_MATRIX_VP, float4(circleTangentB, 1.0)));
        float2 circleCenterA_screen = circleCenterA_NDC.xy / circleCenterA_NDC.w;
        float2 circleCenterB_screen = circleCenterB_NDC.xy / circleCenterB_NDC.w;
        float2 circleTangentA_screen = circleTangentA_NDC.xy / circleTangentA_NDC.w;
        float2 circleTangentB_screen = circleTangentB_NDC.xy / circleTangentB_NDC.w;

        float d = distance(circleCenterA_screen, circleCenterB_screen);
        aR = distance(circleCenterA_screen, circleTangentA_screen);
        bR = distance(circleCenterB_screen, circleTangentB_screen);

        float rotation = get_tangent_rotation(aR, bR, d);

        if (input.vertexID == 0)
        {
            // A
            float3 outA;
            RotateAboutAxis_Radians_float(circleTangentA - circleAPos, offsetNormalA, -rotation, outA);
            outA = circleAPos + outA;
            o.position = mul(UNITY_MATRIX_VP, float4(outA, 1.0));
            o.color = color_buffer[input.instanceID];
            return o;
        }
        else
        {
            // B
            float3 outB;
            RotateAboutAxis_Radians_float(circleTangentB - circleBPos, offsetNormalB, -rotation, outB);
            outB = circleBPos + outB;
            o.position = mul(UNITY_MATRIX_VP, float4(outB, 1.0));
            o.color = float4(1, 0, 0, 1);
            return o;
        }
    }

    o.position = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
    o.color = color_buffer[input.instanceID];
    return o;
}
