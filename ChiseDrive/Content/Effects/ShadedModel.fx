#include "ShadedEffect.inc"

struct NormalVertexShaderInput
{
    float4 Position		: POSITION0;
    float3 Normal		: NORMAL0;
    float2 TexCoord		: TEXCOORD0;
    float3 Binormal		: BINORMAL0;
    float3 Tangent		: TANGENT0;
};

VertexShaderOutputNormal NormalMappedVertexShader(NormalVertexShaderInput input)
{
	VertexShaderOutputNormal output;
	
	float4x4 worldviewprojection = mul(World, ViewProjection);
	
	output.Position = mul(input.Position, worldviewprojection);
	
	output.WorldNormal = mul(input.Normal, World);
	float4 worldposition = mul(input.Position, World);
	output.WorldPosition = worldposition / worldposition.w;
	
	output.TexCoords.x = input.TexCoord.x * TextureUReps;
	output.TexCoords.y = input.TexCoord.y * TextureVReps;
		
	output.Color = AmbientLightColor;
			
	float3 eyePosition = mul(-View._m30_m31_m32, transpose(View));
	output.ViewDirection = input.Position - eyePosition;
	
	// calculate tangent space to world space matrix using the world space tangent,
    // binormal, and normal as basis vectors.  the pixel shader will normalize these
    // in case the world matrix has scaling.
    output.TangentToWorld[0] = mul(input.Tangent, World);
    output.TangentToWorld[1] = mul(input.Binormal, World);
    output.TangentToWorld[2] = mul(input.Normal, World);
	
    //output.TangentToWorld[0] = 0;
    //output.TangentToWorld[1] = 0;
    //output.TangentToWorld[2] = mul(input.Normal, World);
	
	return output;
}

technique Lit
{
	pass Ambient
	{
        VertexShader = compile vs_3_0 NormalMappedVertexShader();
        PixelShader = compile ps_3_0 AmbientNormalPixelShader();
	}
	// Call this pass every Lights.Count
	pass PointLight
	{
		VertexShader = compile vs_3_0 NormalMappedVertexShader();
		PixelShader = compile ps_3_0 MultipleLightPixelShader();
	}
	pass Emit
	{
		VertexShader = compile vs_3_0 NormalMappedVertexShader();
		PixelShader = compile ps_3_0 EmitPixelShader();	
	}
	pass Full
	{
		VertexShader = compile vs_3_0 NormalMappedVertexShader();
		PixelShader = compile ps_3_0 FullPixelShader();	
	}
}