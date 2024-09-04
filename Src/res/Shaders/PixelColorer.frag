#version 460 core

layout (std430, binding = 4) restrict buffer pixels
{
    vec4 Colors[];
} pixelColors;

layout (std430, binding = 5) restrict buffer lights
{
    //x is the index (convert float to int)
    //y is the color (convert float to color)
    //z is the intensity 
    //w is the range
    vec4 Lights[];
} Lights;

out vec4 out_color;
in vec2 TexCoords;
uniform vec2 particleResolution;
uniform vec2 resolution;

vec4 UnpackFloat(float data)
{
    int ColorBits = floatBitsToInt(data);
    return vec4((ColorBits & 0xFF) / 255.0, ((ColorBits >> 8) & 0xFF) / 255.0, ((ColorBits >> 16) & 0xFF) / 255.0, ((ColorBits >> 24) & 0xFF) / 255.0);
}

void main()
{
    vec2 fragPos = vec2(gl_FragCoord.x / resolution.x, gl_FragCoord.y / resolution.y);
    int x = int(fragPos.x * particleResolution.x);
    int y = int(fragPos.y * particleResolution.y);
    //get position index
    int index = int((x * particleResolution.y) - y + particleResolution.y - 1);
    //convert to index in the stored array
    int indexQuart = int(floor(index / 4));
    //get the component that has the color
    int indexColor = int((x * particleResolution.y) - y + particleResolution.y - 1) % 4;
    vec4 color = UnpackFloat(pixelColors.Colors[indexQuart][indexColor]);

    //horrible but dont know how to do it any other way
    vec4 totalAccumLight = vec4(1);
    int totalAccumLightCount = 0;
    for(int xL = -5; xL < 5; xL++)
    {
        for(int yL = -5; yL < 5; yL++)
        {
            if(xL == 0 && yL == 0)
                continue;

            vec2 lightPos = vec2(xL + x, yL + y);
            int indexLight = int((lightPos.x * particleResolution.y) - lightPos.y + particleResolution.y - 1);

            if(indexLight < 0 || indexLight > particleResolution.x * particleResolution.y)
                continue;

            totalAccumLightCount++;

            vec4 lightColor = UnpackFloat(Lights.Lights[indexLight].y);
            float intensity = Lights.Lights[indexLight].z;
            float range = Lights.Lights[indexLight].w;

            float dist = length(lightPos - vec2(x,y));
            float attenuation = 1.0 / (1.0 + (0.00005 * dist * dist));

            totalAccumLight += lightColor * attenuation;

        }
    }

    if (totalAccumLightCount > 0) 
    {
        totalAccumLight /= float(totalAccumLightCount);
    }

    out_color = color * totalAccumLight;
}
