#version 460 core
layout (location = 0) in vec2 aPosition;

uniform mat4 proj;
uniform mat4 model;

void main()
{
    gl_Position = proj * model * vec4(aPosition, 0.0, 1.0);
}