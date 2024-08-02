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
    //get position index
    int index = int(x * 1080 + y);
    //convert to index in the stored array
    index = int(floor(index / 4));
    //get the component that has the color
    int indexColor = int(x * 1080 + y) % 4;
    //opengl just put the bits into the float and did no conversion
    //so we take the bits and turn them into a int
    int Color = floatBitsToInt(pixelColors.Colors[index][indexColor]);
    //seperate values
    //Remove all values except end 2 bits leaving the value
    float r = (Color & 0xFF) / 255.0;
    float g = ((Color >> 8) & 0xFF) / 255.0;
    float b = ((Color >> 16) & 0xFF) / 255.0;
    float a = ((Color >> 24) & 0xFF) / 255.0;

    out_color = vec4(r, g, b, 1.0);
}