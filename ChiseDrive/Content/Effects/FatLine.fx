float4x4 World;
float4x4 View;
float4x4 Projection;

struct ShaderData
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

ShaderData VertexShaderFunction(ShaderData input)
{
    ShaderData output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Color = input.Color;

    return output;
}

float4 PixelShaderFunction(ShaderData input) : COLOR0
{
	return input.Color;
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_1_1 PixelShaderFunction();
    }
}
