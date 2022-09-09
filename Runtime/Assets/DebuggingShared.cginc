#define ALPHA_FADE 1
#define NORMAL_FADE 1 << 1
#define FACE_CAMERA 1 << 2

bool HasAlphaFade(int value) { return (value & ALPHA_FADE) != 0; }
bool HasNormalFade(int value) { return (value & NORMAL_FADE) != 0; }
bool HasFaceCamera(int value) { return (value & FACE_CAMERA) != 0; }

StructuredBuffer<int> modifications_buffer;
StructuredBuffer<float4> color_buffer;