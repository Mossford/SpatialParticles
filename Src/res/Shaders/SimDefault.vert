#version 460 core
layout (location = 0) in vec3 aPosition;

out vec3 color;

uniform mat4 proj;
uniform mat4 model;

void main()
{
    gl_Position = model * proj * vec4(aPosition, 1.0);
}