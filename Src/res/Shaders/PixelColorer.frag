#version 460 core

layout (std430, binding = 4) restrict buffer pixels
{
    vec4 Colors[];
} pixelColors;

out vec4 out_color;
in vec2 TexCoords;
uniform vec2 particleResolution;
uniform vec2 resolution;
uniform vec2 lightPos;

void main()
{
    vec2 fragPos = vec2(gl_FragCoord.x / resolution.x, gl_FragCoord.y / resolution.y);
    int x = int(fragPos.x * particleResolution.x);
    int y = int(fragPos.y * particleResolution.y);
    //get position index
    int index = int((x * particleResolution.y) - y + particleResolution.y - 1);
    //convert to index in the stored array
    index = int(floor(index / 4));
    //get the component that has the color
    int indexColor = int((x * particleResolution.y) - y + particleResolution.y - 1) % 4;
    //opengl just put the bits into the float and did no conversion
    //so we take the bits and turn them into a int
    int ColorBits = floatBitsToInt(pixelColors.Colors[index][indexColor]);
    //seperate values
    //Remove all values except end 2 bits leaving the value
    vec4 color = vec4((ColorBits & 0xFF) / 255.0, ((ColorBits >> 8) & 0xFF) / 255.0, ((ColorBits >> 16) & 0xFF) / 255.0, ((ColorBits >> 24) & 0xFF) / 255.0);

    //float dist = length(lightPos - vec2(x,y));
    //float attenuation = 1.0 / (1.0 + (0.00005 * dist * dist));

    //color *= vec4(lightPos.x / particleResolution.x * 2 * attenuation, lightPos.y / particleResolution.y * 2 * attenuation, 1 * attenuation, 1.0);

    out_color = color;
}
