#version 460 core

out vec4 out_color;
uniform sampler2D diffuseTexture;
in vec2 TexCoords;

void main()
{
    //since only used for text just using the red channel
    float color = texture(diffuseTexture, TexCoords).r;
    if(color < 0.1)
        discard;
    out_color = vec4(vec3(color), 1.0);
}