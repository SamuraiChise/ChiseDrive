#include "ShadedEffect.inc"

float4x4 Bones[59];

struct SkinnedNormalVertexShaderInput
{
    float4 Position		: POSITION0;
    float3 Normal		: NORMAL0;
    float2 TexCoord		: TEXCOORD0;
    float4 BoneIndices	: BLENDINDICES0;
    float4 BoneWeights	: BLENDWEIGHT0;
    float3 Binormal		: BINORMAL0;
    float3 Tangent		: TANGENT0;
};

VertexShaderOutputNormal SkinnedVertexShader(SkinnedNormalVertexShaderInput input)
{
	VertexShaderOutputNormal output;

	float4x4 skinTransform = 0;

    skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
    skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
    skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
    skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;

	//float4x4 worldviewprojection = mul(mul(World, View), Projection);
	//float4 position = input.Position;//mul(input.Position, skinTransform);
	//output.Position = mul(position, worldviewprojection);


	// Skin the vertex position.
    float4 position = input.Position;
    position = mul(position, skinTransform);
    position = mul(position, World);
    position = mul(position, ViewProjection);
    output.Position = position;
	
	output.WorldNormal = mul(input.Normal, World);
	float4 worldposition = mul(input.Position, World);
	output.WorldPosition = worldposition / worldposition.w;
	
	output.TexCoords = input.TexCoord;
    output.Color = AmbientLightColor;
	
	float3 eyePosition = mul(-View._m30_m31_m32, transpose(View));
	output.ViewDirection = position - eyePosition;	
	 
    // calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors.  the pixel shader will normalize these
    // in case the world matrix has scaling.
    output.TangentToWorld[0] = normalize(mul(mul(input.Tangent, skinTransform), World));
    output.TangentToWorld[1] = normalize(mul(mul(input.Binormal, skinTransform), World));
    output.TangentToWorld[2] = normalize(mul(mul(input.Normal, skinTransform), World));
        	
	return output;
}

technique Lit
{
	pass Ambient
	{
        VertexShader = compile vs_3_0 SkinnedVertexShader();
        PixelShader = compile ps_3_0 AmbientNormalPixelShader();
	}
	// Call this pass every Lights.Count
	pass PointLight
	{
		VertexShader = compile vs_3_0 SkinnedVertexShader();
		PixelShader = compile ps_3_0 MultipleLightPixelShader();
	}
	pass Emit
	{
		VertexShader = compile vs_3_0 SkinnedVertexShader();
		PixelShader = compile ps_3_0 EmitPixelShader();	
	}
	pass Full
	{
		VertexShader = compile vs_3_0 SkinnedVertexShader();
		PixelShader = compile ps_3_0 FullPixelShader();	
	}
}