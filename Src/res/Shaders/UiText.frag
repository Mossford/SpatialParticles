#version 460 core

out vec4 out_color;
uniform sampler2D diffuseTexture;
uniform sampler2D frame;
in vec2 TexCoords;
uniform vec2 resolution;
uniform vec4 uiColor;
uniform int radius;

void main()
{
    //since only used for text just using the red channel
    float textColor = texture(diffuseTexture, TexCoords).r;
    if(textColor < 0.1)
        discard;

    vec3 color = vec3(textColor) * uiColor.rgb;
    
    out_color = vec4(color, 1.0);
}