uniform extern float4x4 World;
uniform extern float4x4 View;
uniform extern float4x4 Projection;
uniform extern texture SpriteTexture;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float Size : PSIZE;
};

struct VertexShaderOutput
{
	float4 Position : POSITION;
    float4 Color : COLOR;
    float Size : PSIZE;
};

sampler Sampler = sampler_state
{
	Texture = <SpriteTexture>;
};

VertexShaderOutput VertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;
	
	float4x4 WVPMatrix = mul(mul(World, View), Projection);
	
	output.Position = mul(input.Position, WVPMatrix);
	output.Color = input.Color;
	output.Size = input.Size * (1/(output.Position.z+1)) * 200;
	
	return output;
}

struct PixelShaderInput
{
#ifdef XBOX
    float2 TextureCoordinate : SPRITETEXCOORD;
#else
    float2 TextureCoordinate : TEXCOORD0;
#endif
    float4 Color : COLOR0;
};

float4 PixelShader(PixelShaderInput input) : COLOR0
{
    return tex2D(Sampler, input.TextureCoordinate) * input.Color;
}

technique PointSpriteTechnique
{
	pass P0
	{
		vertexShader = compile vs_1_1 VertexShader();
		pixelShader = compile ps_1_1 PixelShader();
	}
}    
