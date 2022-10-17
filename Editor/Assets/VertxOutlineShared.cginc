#include "UnityCG.cginc"
#include "VertxDebuggingShared.cginc"

struct Outine
{
    float3 A, B;
    // C can either be (radius, 0, 0)
    // Custom - A vector that points 'up' in a hemisphere, that is of radius length.
    // Custom1 - A: box position, B: box offset, C: box normal
    float3 C;
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

struct Plane
{
    float3 Normal;
    float Distance;
};

bool plane_plane_intersection(
    Plane p1, Plane p2,
    // output args
    out float3 r_point, out float3 r_normal)
{
    // logically the 3rd plane, but we only use the normal component.
    const float3 p3_normal = cross(p1.Normal, p2.Normal);
    const float det = dot(p3_normal, p3_normal);

    // If the determinant is 0, that means parallel planes, no intersection.
    // note: you may want to check against an epsilon value here.
    if (det != 0.0)
    {
        // calculate the final (point, normal)
        r_point = (cross(p3_normal, p2.Normal) * p1.Distance +
            cross(p1.Normal, p3_normal) * p2.Distance) / det;
        r_normal = p3_normal;
        return true;
    }
    return false;
}

int circle_line_intersection(float radius, float2 p, float2 n, out float2 intersection1, out float2 intersection2)
{
    float dx = n.x;
    float dy = n.y;

    float A = dx * dx + dy * dy;
    float B = 2 * (dx * p.x + dy * p.x);
    float C = p.x * p.x + p.y * p.y - radius * radius;

    float det = B * B - 4 * A * C;
    if (A <= 0.0000001 || det < 0)
    {
        // No real solutions.
        intersection1 = float2(0, 0);
        intersection2 = float2(0, 0);
        return 0;
    }
    if (det == 0)
    {
        // One solution.
        float t = -B / (2 * A);
        intersection1 = float2(p.x + t * dx, p.y + t * dy);
        intersection2 = float2(0, 0);
        return 1;
    }

    // Two solutions.
    float t = (-B + sqrt(det)) / (2 * A);
    intersection1 = float2(p.x + t * dx, p.y + t * dy);
    t = (-B - sqrt(det)) / (2 * A);
    intersection2 = float2(p.x + t * dx, p.y + t * dy);
    return 2;
}

bool closest_plane_circle_intersection(
    float3 circleCenter,
    float3 circleNormal,
    float3 circlePerpendicular,
    float circleRadius,
    float3 planePoint,
    float3 planeNormal,
    inout float3 intersection
)
{
    Plane circlePlane;
    circlePlane.Distance = -dot(circleNormal, circleCenter);
    circlePlane.Normal = circleNormal;
    Plane plane;
    plane.Distance = -dot(planeNormal, planePoint);
    plane.Normal = planeNormal;
    float3 pointI, normalI;
    if (!plane_plane_intersection(circlePlane, plane, pointI, normalI))
        return false;
    // zero out positions
    // planePoint -= circleCenter;
    pointI -= circleCenter;
    // project onto circle plane
    float3 circlePerpendicular2 = cross(circleNormal, circlePerpendicular);
    float2 pointILocal = float2(dot(circlePerpendicular, pointI), dot(circlePerpendicular2, pointI));
    float2 normalILocal = float2(dot(circlePerpendicular, normalI), dot(circlePerpendicular2, normalI));

    float2 i1, i2;
    int intersections = circle_line_intersection(circleRadius, pointILocal, normalILocal, i1, i2);
    if (intersections == 0)
        return false;
    if (intersections == 1)
    {
        intersection = circleCenter + circlePerpendicular * i1.x + circlePerpendicular2 * i1.y;
        return true;
    }
    // Reproject back into 3D.
    float3 i13 = circleCenter + circlePerpendicular * i1.x + circlePerpendicular2 * i1.y;
    float3 i23 = circleCenter + circlePerpendicular * i2.x + circlePerpendicular2 * i2.y;
    // Find the closest intersection to our input.
    i1 -= intersection;
    i2 -= intersection;
    float d1 = dot(i1, i1);
    float d2 = dot(i2, i2);
    intersection = d1 < d2 ? i13 : i23;
    return true;
}

v2f vert(vertInput input)
{
    v2f o;
    int index = input.instanceID * 128 + input.vertexID / 2;
    if (index >= _InstanceCount)
    {
        o.position = 0;
        o.color = 0;
        return o;
    }
    
    Outine outline = outline_buffer[index];
    int modifications = modifications_buffer[index];
    o.color = color_buffer[index];
    if (has_normal_fade(modifications))
    {
        float3 originWorld = input.vertexID % 2 == 0 ? outline.A : outline.B;
        float3 cameraDirection = camera_direction_variable(originWorld);
        float d = dot(cameraDirection, outline.C);
        d = saturate(
            smoothstep(0, 0.1, d) // front face
        );

        o.color.a *= max(0.3, d);
        o.position = mul(UNITY_MATRIX_VP, offset_world_towards_camera(float4(originWorld, 1), cameraDirection));
        return o;
    }

    if (has_custom2(modifications))
    {
        // Custom1 - A, B: line positions
        // C: Box line normal. (see BoxShared's NormalFade)
        float3 originWorld = input.vertexID % 2 == 0 ? outline.A : outline.B;
        float3 cameraDirection = camera_direction_variable(originWorld);

        float4 rot = axis_angle(outline.A - outline.B, 3.14159265359 * 0.25);
        float3 normalA = rotate(rot, outline.C);
        float3 normalB = rotate(quaternion_inverse(rot), outline.C);
        
        o.color.a *= max(0.3, step(0, max(dot(cameraDirection, normalA), dot(cameraDirection, normalB))));
        o.position = mul(UNITY_MATRIX_VP, offset_world_towards_camera(float4(originWorld, 1), cameraDirection));
        return o;
    }
    
    float radius = length(outline.C);

    float3 originWorld = input.vertexID % 2 == 0 ? outline.A : outline.B;
    float3 direction = normalize(outline.B - outline.A);

    if (is_orthographic())
    {
        float3 cameraDirection = camera_direction();
        float3 right = normalize(cross(-cameraDirection, direction));

        float3 worldPos = originWorld
            + right * radius;
        
        if (has_custom(modifications))
        {
            if (dot(outline.C, right) < 0)
            {
                o.color.a = 0;
                o.position = 0;
                return o;
            }
        }
        o.position = mul(UNITY_MATRIX_VP, offset_world_towards_camera(float4(worldPos, 1), cameraDirection));
        return o;
    }
    else
    {
        float3 normal = normalize(_WorldSpaceCameraPos.xyz - originWorld);
        float3 right = normalize(cross(normal, direction));

        float offset;
        float3 oNormal;
        get_circle_info_basic(originWorld, radius, offset, oNormal);

        // Find the intersection between this new plane (the one created by get_circle_info)
        // and the circle that is at originWorld, facing in direction, and of radius;
        // it's at the closest intersection that we should position the line vertex.
        // This is not at all performant, but it's only 2 vertices, right? RIGHT?
        // Look, this took me forever, give me a break.
        float3 intersection = originWorld + right * radius;

        if(!closest_plane_circle_intersection(
                originWorld,
                direction,
                right,
                radius,
                originWorld + oNormal * offset,
                oNormal,
                intersection
            ))
        {
            o.color.a = 0;
            o.position = 0;
            return o;
        }

        if (has_custom(modifications))
        {
            // Clip against the hemisphere that makes up a capsule.
            if (dot(outline.C, intersection - originWorld) < 0)
            {
                o.color.a = 0;
                o.position = 0;
                return o;
            }
        }
        o.position = mul(UNITY_MATRIX_VP, offset_world_towards_camera(float4(intersection, 1.0)));
        return o;
    }
}
