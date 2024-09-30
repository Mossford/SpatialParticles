#version 460 core

out vec4 out_color;
uniform sampler2D diffuseTexture;
in vec2 TexCoords;

void main()
{
    //since only used for text just using the red channel
    out_color = vec4(vec3(texture(diffuseTexture, TexCoords).r), 1.0);
}