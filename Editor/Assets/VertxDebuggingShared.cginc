#define ALPHA_FADE 1
#define NORMAL_FADE 1 << 1
#define FACE_CAMERA 1 << 2
#define CUSTOM 1 << 3
#define CUSTOM2 1 << 4
#define Z_GREATER_FADE 0.2

int _InstanceCount;

bool has_alpha_fade(int value) { return (value & ALPHA_FADE) != 0; }
bool has_normal_fade(int value) { return (value & NORMAL_FADE) != 0; }
bool has_face_camera(int value) { return (value & FACE_CAMERA) != 0; }
bool has_custom(int value) { return (value & CUSTOM) != 0; }
bool has_custom2(int value) { return (value & CUSTOM2) != 0; }

bool is_orthographic() { return unity_OrthoParams.w == 1; }

float get_scale()
{
    return length(float3(
        unity_ObjectToWorld[0].x,
        unity_ObjectToWorld[1].x,
        unity_ObjectToWorld[2].x
    ));
}

float3 camera_direction() { return UNITY_MATRIX_IT_MV[2].xyz; }

float3 camera_direction_variable(float3 originWorld)
{
    if(is_orthographic())
        return camera_direction();
    return normalize(_WorldSpaceCameraPos.xyz - originWorld);
}

float3 offset_world_towards_camera(float3 originWorld)
{
    return originWorld + camera_direction_variable(originWorld) * 0.001;
}

float4 offset_world_towards_camera(float4 originWorld)
{
    return float4(originWorld.xyz + camera_direction_variable(originWorld.xyz) * 0.001, originWorld.w);
}

float4 offset_world_towards_camera(float4 originWorld, float3 cameraDirection)
{
    return float4(originWorld.xyz + cameraDirection * 0.001, originWorld.w);
}

float4 world_to_clip_pos(float3 pos)
{
    return mul(UNITY_MATRIX_VP, float4(offset_world_towards_camera(pos), 1.0));
}

float4 billboard(float3 vertex)
{
    // Get the origin (it's not affected by rotation or scale)
    float4 origin = mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0));

    return mul(
        UNITY_MATRIX_P,
        origin
        + float4(
            vertex * // Position the unscaled verts
            get_scale().xxx, // Scale the verts
            0.0
        )
    );
}

void get_circle_info(
    float3 originWorld,
    float radius,
    out float newRadius,
    out float offsetToNewCircle,
    out float3 offsetNormal
)
{
    offsetNormal = _WorldSpaceCameraPos.xyz - originWorld; // Normal
    float d1 = length(offsetNormal); // Distance to camera
    offsetNormal = offsetNormal / d1; // Normalise n
    float r1Sqrd = radius * radius;
    offsetToNewCircle = r1Sqrd / d1; // Distance from sphere to place circle
    newRadius = sqrt(r1Sqrd - offsetToNewCircle * offsetToNewCircle); // Radius of circle
}

void get_circle_info_basic(
    float3 originWorld,
    float radius,
    out float offsetToNewCircle,
    out float3 offsetNormal
)
{
    offsetNormal = _WorldSpaceCameraPos.xyz - originWorld; // Normal
    float d1 = length(offsetNormal); // Distance to camera
    offsetNormal = offsetNormal / d1; // Normalise n
    float r1Sqrd = radius * radius;
    offsetToNewCircle = r1Sqrd / d1; // Distance from sphere to place circle
}

float4 axis_angle(float3 axis, float angle)
{
    float sina, cosa;
    sincos(0.5f * angle, sina, cosa);
    return float4(axis * sina, cosa);
}

float4 quaternion_inverse(float4 q)
{
    return rcp(dot(q, q)) * q * float4(-1.0f, -1.0f, -1.0f, 1.0f);
}

float3 rotate(float4 q, float3 v)
{
    float3 t = 2 * cross(q.xyz, v);
    return v + q.w * t + cross(q.xyz, t);
}

void RotateAboutAxis_Radians_float(float3 In, float3 Axis, float Rotation, out float3 Out)
{
    float s = sin(Rotation);
    float c = cos(Rotation);
    float one_minus_c = 1.0 - c;

    Axis = normalize(Axis);
    float3x3 rot_mat = 
    {   one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
        one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
        one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
    };
    Out = mul(rot_mat,  In);
}


float2 rotate_2d(float2 v, float angle)
{
    float s, c;
    sincos(angle, s, c);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}