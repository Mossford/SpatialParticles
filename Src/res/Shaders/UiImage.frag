#version 460 core

out vec4 out_color;
uniform sampler2D diffuseTexture;
in vec2 TexCoords;

void main()
{
    out_color = texture(diffuseTexture, TexCoords);
}