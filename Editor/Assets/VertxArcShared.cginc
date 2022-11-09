#include "UnityCG.cginc"
#include "VertxDebuggingShared.cginc"

struct Arc
{
    float4x4 Matrix;
    float Turns;
};

struct ArcGroup
{
    Arc A;
    float4 Color;
    int Modifications;
};

StructuredBuffer<ArcGroup> arc_buffer;

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
    float4 uvAndTurns : TEXCOORD2;
};

v2f vert(vertInput input)
{
    v2f o;

    int index = input.instanceID * 4 + input.vertexID / 64; // 64 vert circle.
    if (index >= _InstanceCount)
    {
        o.position = 0;
        o.color = 0;
        o.uvAndTurns = 0;
        return o;
    }
    
    ArcGroup ag = arc_buffer[index];
    Arc a = ag.A;
    unity_ObjectToWorld = a.Matrix;
    UNITY_MATRIX_MV = mul(unity_MatrixV, unity_ObjectToWorld);

    o.color = ag.Color;
    int modifications = ag.Modifications;

    if (has_custom(modifications))
    {
        // Aligned with camera (special spherical billboard)
        // Orthographic
        if (is_orthographic())
        {
            if (a.Turns != 1)
            {
                float radius = get_scale();
                float3 n = -camera_direction(); // Normal

                float3 originWorld = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)).xyz;
                float3 worldFacing = normalize(mul(unity_ObjectToWorld, float4(1.0, 0.0, 0.0, 1.0)).xyz - originWorld);
                float3 up = normalize(cross(n, worldFacing));
                float3 right = normalize(cross(up, n));

                float3 v = input.vertex.xyz * radius.xxx;
                float3 localPos = right * v.x + up * v.y;
                float3 worldPos = originWorld + localPos;

                // If this is not a smoothstep, it doesn't seem to properly interpolate.
                a.Turns = smoothstep(-0.01, 0.01, dot(localPos, worldFacing));

                o.position = mul(UNITY_MATRIX_VP, offset_world_towards_camera(float4(worldPos, 1.0)));
            }
            else
            {
                o.position = billboard(input.vertex.xyz);
            }
        }
        // Perspective
        else
        {
            float3 originWorld = mul(unity_ObjectToWorld, float4(0.0, 0.0, 0.0, 1.0)).xyz;
            float r1 = get_scale(); // Original radius
            float3 n = _WorldSpaceCameraPos.xyz - originWorld; // Normal
            float d1 = length(n); // Distance to camera
            n = n / d1; // Normalise n
            float r1Sqrd = r1 * r1;
            float d = r1Sqrd / d1; // Distance from sphere to place circle
            float r = sqrt(r1Sqrd - d * d); // Radius of circle

            float3 up, right;
            if (a.Turns != 1)
            {
                float3 worldFacing = normalize(mul(unity_ObjectToWorld, float4(1.0, 0.0, 0.0, 1.0)).xyz - originWorld);
                up = normalize(cross(n, worldFacing));
                right = normalize(cross(up, n));

                float3 v = input.vertex.xyz * r.xxx;
                float3 localPos = n * d
                    + right * v.x
                    + up * v.y
                    - n * v.z;
                float3 worldPos = originWorld + localPos;

                // If this is not a smoothstep, it doesn't seem to properly interpolate.
                a.Turns = smoothstep(-0.01, 0.01, dot(localPos, worldFacing));

                o.position = mul(UNITY_MATRIX_VP, offset_world_towards_camera(float4(worldPos, 1.0)));
            }
            else
            {
                up = UNITY_MATRIX_IT_MV[1].xyz;
                right = normalize(cross(n, up));
                up = normalize(cross(right, n));

                float3 v = input.vertex.xyz * r.xxx;
                float3 worldPos = originWorld + n * d
                    + right * v.x
                    + up * v.y
                    - n * v.z;

                o.position = mul(UNITY_MATRIX_VP, offset_world_towards_camera(float4(worldPos, 1.0)));
            }
        }
    }
    else if (has_custom2(modifications))
    {
        // Spiral
        // return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
        input.vertex.xyz *= max(0, lerp(a.Turns, 1, (input.uv.y + 1) * 0.5));
        float4 worldPos = mul(unity_ObjectToWorld, float4(input.vertex.xyz, 1.0));

        o.position = mul(UNITY_MATRIX_VP, offset_world_towards_camera(worldPos));
        o.uvAndTurns = float4(input.uv, 1, 0);
        return o;
    }
    else
    {
        // return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
        float angle = input.uv.y * 3.14159265359;
        angle *= a.Turns;
        input.vertex.xy = float2(cos(angle), sin(angle));
        a.Turns = 1;
        
        float4 worldPos = mul(unity_ObjectToWorld, float4(input.vertex.xyz, 1.0));

        o.position = mul(UNITY_MATRIX_VP, offset_world_towards_camera(worldPos));

        if (has_normal_fade(modifications))
        {
            float3 worldViewDir = _WorldSpaceCameraPos.xyz - worldPos;
            float d = dot(worldViewDir, UnityObjectToWorldNormal(input.vertex));
            d = saturate(
                smoothstep(0, 0.01, d) // front face
            );

            o.color.a *= max(0.3, d);
        }
    }
    o.uvAndTurns = float4(abs(input.uv), a.Turns, 0);
    return o;
}
