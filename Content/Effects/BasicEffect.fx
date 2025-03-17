#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Parameters for control
float TotalTime;
float2 Resolution; // Screen resolution for distortion effects

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

// Function to create a psychedelic plasma effect
float3 createPlasma(float2 uv, float time)
{
    float v1 = sin((uv.x * 10 + time));
    float v2 = sin((uv.y * 10 + time));
    float v3 = sin((uv.x * 10 + uv.y * 10 + time));
    
    float cx = uv.x + sin(time * 0.1) * 0.5;
    float cy = uv.y + cos(time * 0.2) * 0.5;
    float v4 = sin(sqrt(100 * ((cx * cx) + (cy * cy)) + 1) + time);
    
    float v = (v1 + v2 + v3 + v4) * 0.25;
    
    return float3(
        sin(v * 3.14159 + time) * 0.5 + 0.5,
        sin(v * 3.14159 + time * 1.3) * 0.5 + 0.5,
        sin(v * 3.14159 + time * 0.7) * 0.5 + 0.5
    );
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Calculate effects cycle (changes every few seconds)
    float effectCycle = floor(TotalTime / 3.0) % 4.0; // Cycle through 4 effects every 3 seconds
    
    // Original texture coordinates
    float2 uv = input.TextureCoordinates;
    
    // Apply distortion based on the current effect cycle
    if (effectCycle < 1.0) {
        // Effect 1: Wavy distortion
        float2 wavyOffset = float2(
            sin(uv.y * 10 + TotalTime * 5) * 0.02,
            cos(uv.x * 10 + TotalTime * 5) * 0.02
        );
        uv += wavyOffset;
    }
    else if (effectCycle < 2.0) {
        // Effect 2: Spiral distortion
        float angle = TotalTime * 2;
        float radius = length(uv - 0.5);
        float spiral = radius * 15 + angle;
        float2 spiralOffset = float2(
            sin(spiral) * 0.015 * radius,
            cos(spiral) * 0.015 * radius
        );
        uv += spiralOffset;
    }
    else if (effectCycle < 3.0) {
        // Effect 3: Pulse zoom
        float zoom = sin(TotalTime * 4) * 0.05 + 0.95;
        uv = (uv - 0.5) / zoom + 0.5;
    }
    // Effect 4: No distortion (gives a break between crazy effects)
    
    // Sample the texture with the distorted coordinates
    float4 texelColor = tex2D(SpriteTextureSampler, uv);
    float4 blendedColor = texelColor * input.Color;
    
    // Store the original alpha
    float originalAlpha = blendedColor.a;
    
    // Color effects that cycle continuously
    float colorEffectCycle = TotalTime * 0.5;
    
    // Generate color effects
    if (effectCycle < 1.0) {
        // Effect 1: Rainbow colors
        float r = sin(colorEffectCycle) * 0.5 + 0.5;
        float g = sin(colorEffectCycle + 2.0) * 0.5 + 0.5;
        float b = sin(colorEffectCycle + 4.0) * 0.5 + 0.5;
        blendedColor.rgb *= float3(r, g, b);
    }
    else if (effectCycle < 2.0) {
        // Effect 2: Plasma effect
        float3 plasma = createPlasma(uv, TotalTime * 2);
        blendedColor.rgb = lerp(blendedColor.rgb, plasma, 0.7); // Blend with original
    }
    else if (effectCycle < 3.0) {
        // Effect 3: Negative/solarize
        blendedColor.rgb = lerp(blendedColor.rgb, 1.0 - blendedColor.rgb, (sin(TotalTime * 5) * 0.5 + 0.5));
    }
    else {
        // Effect 4: Pulsating brightness
        float pulse = sin(TotalTime * 8) * 0.3 + 0.7;
        blendedColor.rgb *= pulse;
    }
    
    // Add vibrating edges
    float edgeIntensity = 0.3 * (sin(TotalTime * 10) * 0.5 + 0.5);
    float edge = 1.0 - step(0.1, originalAlpha) * step(0.01, 1.0 - originalAlpha);
    blendedColor.rgb = lerp(blendedColor.rgb, float3(1, 1, 1), edge * edgeIntensity);
    
    // Restore alpha
    blendedColor.a = originalAlpha;
    
    // Discard transparent pixels
    if (texelColor.a < 0.01f)
        discard;
    
    return blendedColor;
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};