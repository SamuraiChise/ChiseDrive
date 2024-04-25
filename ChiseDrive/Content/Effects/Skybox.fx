float4x4 World;
float4x4 View;
float4x4 Projection;
Texture SkyTexture;

sampler TextureSampler = sampler_state 
{
	texture = <SkyTexture>;
};

struct ShaderData
{
    float4 Position : POSITION0;
    float2 TextureCoords:TEXCOORD0;
};

struct NormalDepthVertexShaderOutput
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

    output.TextureCoords = input.TextureCoords;

    return output;
}

float4 PixelShaderFunction(ShaderData input) : COLOR0
{
	float4 color = tex2D(TextureSampler, input.TextureCoords);
	return color;
}

NormalDepthVertexShaderOutput NormalVertexFunction(ShaderData input)
{
	NormalDepthVertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

	output.Color = float4(0, 0, 0, 0) - output.Position;
    output.Color.a = output.Position.z / output.Position.w;
	
    return output;
}

float4 NormalDepthPixelShader(float4 color : COLOR0) : COLOR0
{
    return color;
}

technique SkyBox
{
    pass Full
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_1_1 PixelShaderFunction();
    }
}

technique NormalDepth
{
    pass Full
    {
        VertexShader = compile vs_3_0 NormalVertexFunction();
        PixelShader = compile ps_3_0 NormalDepthPixelShader();
    }
}
