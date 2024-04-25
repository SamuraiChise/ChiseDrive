float4x4 World;
float4x4 View;
float4x4 Projection;
texture TextureMap;

sampler TextureSampler = sampler_state 
{
	Texture = <TextureMap>;
};

struct ShaderData
{
    float4 Position			: POSITION0;
    float2 TextureCoords	: TEXCOORD0;
};

ShaderData VertexShaderFunction(ShaderData input)
{
    ShaderData output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);

    output.Position = mul(viewPosition, Projection);
	output.TextureCoords = input.TextureCoords;

    return output;
}

float4 PixelShaderFunction(ShaderData input) : COLOR0
{
	return tex2D(TextureSampler, input.TextureCoords);
}

technique UnlitTexture
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_1_1 PixelShaderFunction();
    }
}