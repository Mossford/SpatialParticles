#version 460 core

layout (std430, binding = 4) restrict buffer pixels
{
    vec4 Colors[];
} pixelColors;

out vec4 out_color;
in vec2 TexCoords;
uniform vec2 resolution;

void main()
{
    vec2 fragPos = vec2(gl_FragCoord.x / (resolution.x), gl_FragCoord.y / (resolution.y));
    int x = int(fragPos.x * 1920.0);
    int y = int(fragPos.y * -1080.0);
    int index = int(x * 1080 + y);
    vec4 Color = pixelColors.Colors[index];
    out_color = vec4(vec3(Color), 1.0);
}