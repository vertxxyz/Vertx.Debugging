#define ALPHA_FADE 1
#define NORMAL_FADE 1 << 1
#define FACE_CAMERA 1 << 2
#define CUSTOM 1 << 3

bool has_alpha_fade(int value) { return (value & ALPHA_FADE) != 0; }
bool has_normal_fade(int value) { return (value & NORMAL_FADE) != 0; }
bool has_face_camera(int value) { return (value & FACE_CAMERA) != 0; }
bool has_custom(int value) { return (value & CUSTOM) != 0; }

StructuredBuffer<int> modifications_buffer;
StructuredBuffer<float4> color_buffer;

float get_scale()
{
    return length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x,
                                unity_ObjectToWorld[2].x));
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
