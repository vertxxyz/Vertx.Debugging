#include "UnityCG.cginc"
#include "DebuggingShared.cginc"

struct Arc
{
    float4x4 Matrix;
    float Turns;
};

StructuredBuffer<Arc> arc_buffer;

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

    Arc a = arc_buffer[input.instanceID];
    unity_ObjectToWorld = a.Matrix;
    UNITY_MATRIX_MV = mul(unity_MatrixV, unity_ObjectToWorld);

    o.color = color_buffer[input.instanceID];
    o.uvAndTurns = float4(input.uv, a.Turns, 0);
    int modifications = modifications_buffer[input.instanceID];

    if (has_custom(modifications))
    {
        // Orthographic
        if (unity_OrthoParams.w == 1)
        {
            o.position = billboard(input.vertex.xyz);
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

            float3 up = UNITY_MATRIX_IT_MV[1].xyz;
            float3 right = normalize(cross(n, up));
            up = normalize(cross(right, n));
            float3 v = input.vertex.xyz * r.xxx;
            float3 worldPos = originWorld + n * d
                + right * v.x
                + up * v.y
                - n * v.z;

            // Get the origin (it's not affected by rotation or scale)
            o.position = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
        }
    }
    else if (has_face_camera(modifications))
    {
        o.position = billboard(input.vertex.xyz);
    }
    else
    {
        // return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
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
    }
    return o;
}
