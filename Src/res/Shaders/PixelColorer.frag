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
} particleLights;

out vec4 out_color;
in vec2 TexCoords;
uniform vec2 particleResolution;
uniform vec2 resolution;
uniform bool enableParticleLighting;
uniform bool enableGpuComp;
uniform sampler2D lightingTex;

vec4 UnpackFloat(float data)
{
    int ColorBits = floatBitsToInt(data);
    return vec4((ColorBits & 0xFF) / 255.0, ((ColorBits >> 8) & 0xFF) / 255.0, ((ColorBits >> 16) & 0xFF) / 255.0, ((ColorBits >> 24) & 0xFF) / 255.0);
}

vec4 CalculateLighting(vec2 particlePos, int index)
{
    //check if there is a particle at this position
    if(floatBitsToInt(particleLights.Lights[index].x) == -1)
        return vec4(1.0);

    int range = floatBitsToInt(particleLights.Lights[index].w);

    vec4 totalAccumLight = vec4(0);
    int totalAccumLightCount = 0;
    float cornerPreCalc = particleResolution.x * particleResolution.y;
    for(int xL = -range; xL < range; xL++)
    {
        for(int yL = -range; yL < range; yL++)
        {
            vec2 lightPos = vec2(particlePos.x + xL, particlePos.y + yL);

            int indexLight = int((lightPos.x * particleResolution.y) - lightPos.y + particleResolution.y - 1);

            if(indexLight < 0 || indexLight >= cornerPreCalc)
                continue;

            //handle bleed over. X pos is handled by bounds checking
            if(lightPos.y > particleResolution.y || lightPos.y < 0)
                continue;

            totalAccumLightCount++;

            vec4 lightColor = UnpackFloat(particleLights.Lights[indexLight].y);
            float intensity = round(particleLights.Lights[indexLight].z);

            totalAccumLight += lightColor * intensity;

        }
    }

    if (totalAccumLightCount > 0) 
    {
        totalAccumLight /= float(totalAccumLightCount);
    }

    return totalAccumLight;
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

    vec4 particleLighting = vec4(1.0);
    if(enableParticleLighting)
    {
        if(enableGpuComp)
        particleLighting = texture(lightingTex, fragPos);
        if(!enableGpuComp)
        particleLighting = CalculateLighting(vec2(x,y), index);
    }

    out_color = color * particleLighting;
}
