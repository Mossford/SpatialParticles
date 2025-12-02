#version 460 core

out vec4 out_color;
uniform sampler2D diffuseTexture;
uniform sampler2D frame;
in vec2 TexCoords;
uniform vec2 resolution;
uniform vec4 uiColor;
uniform int radius;

vec3 boxBlur()
{
    vec3 result = vec3(0.0);
    float count = 0.0;
    
    for (int x = -radius; x <= radius; x++)
    {
        for (int y = -radius; y <= radius; y++)
        {
            vec2 offset = vec2(x, y) * 3.0f / resolution;
            result += texture(frame, gl_FragCoord.xy / resolution + offset).rgb;
            count += 1.0;
        }
    }

    return result / count;
}

void main()
{
    vec3 blurFrame = boxBlur();
    vec3 color = mix(blurFrame, texture(diffuseTexture, TexCoords).rgb * uiColor.rgb, uiColor.a);

    out_color = vec4(color, 1.0);

}