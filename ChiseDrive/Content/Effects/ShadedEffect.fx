#include "Common.inc"

Light Lights[8];
float4x4 Bones[59];

// SkinnedVertexShader
VertexShaderOutput SkinnedVertexShader(SkinnedVertexShaderInput input)
{
	VertexShaderOutput output;
	
	// Global Translation
	float4x4 worldviewprojection = mul(mul(World, View), Projection);

	// Blend between the weighted bone matrices.
    float4x4 skinTransform = 0;
    skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
    skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
    skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
    skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;
	
	// Skin the vertex position.
    //float4 position = mul(input.Position, skinTransform);
    //output.Position = mul(position, worldviewprojection);
    float4 position = input.Position;
    position = mul(position, skinTransform);
    //position = mul(position, World);
    position = mul(position, View);
    position = mul(position, Projection);
    //position = mul(position, worldviewprojection);
    output.Position = position;
	
	output.WorldNormal = mul(input.Normal, World);
	float4 worldposition = mul(input.Position, World);
	output.WorldPosition = worldposition / worldposition.w;
	
	output.TexCoords = input.TexCoord;
	
	output.Color = MaterialColor;
	
	return output;
	/*
    VertexShaderOutput output;
    
    // Blend between the weighted bone matrices.
    float4x4 skinTransform = 0;
    
    skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
    skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
    skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
    skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;
    
    // Skin the vertex position.
    float4 position = mul(input.Position, skinTransform);
    
    output.Position = mul(mul(position, View), Projection);

    // Skin the vertex normal, then compute lighting.
    float3 normal = normalize(mul(input.Normal, skinTransform));
    
//    float3 light1 = max(dot(normal, Light1Direction), 0) * Light1Color;
//    float3 light2 = max(dot(normal, Light2Direction), 0) * Light2Color;

//    output.Lighting = light1 + light2 + AmbientColor;

    output.TexCoords = input.TexCoord;
    
    output.WorldNormal = mul(normal, World);

	float4 worldposition = mul(position, World);
	output.WorldPosition = worldposition / worldposition.w;
	
	output.Color = MaterialColor;
    
    return output;*/
}

float4 MultipleLightPixelShader(PixelShaderInput input) : COLOR
{
	float4 diffuseColor = MaterialColor;
	float4 specularColor = MaterialColor;
	
	if (DiffuseTexEnabled)
	{
		diffuseColor *= tex2D(DiffuseSampler, input.TexCoords);
	}
	
	if (SpecularTexEnabled)
	{
		specularColor *= tex2D(SpecularSampler, input.TexCoords);
	}
	
	float4 color = float4(0,0,0,0);
	
	for (int i = 0; i < 8; i++)
	{
		if (i < NumLightsPerPass)
		{
			color += CalculateSingleLight(
				Lights[i],
				input.WorldPosition,
				input.WorldNormal,
				diffuseColor,
				specularColor);
		}
	}
	
	//color.a = 1.0;
	//color.a = input.Color.a;
	
	return color;
}

technique MultipleLights
{
	// Call this pass once per render
	pass Ambient
	{
        // Set sampler states
        MagFilter[0] = LINEAR;
        MinFilter[0] = LINEAR;
        MipFilter[0] = LINEAR;
        AddressU[0] = WRAP;
        AddressV[0] = WRAP;
        MagFilter[1] = LINEAR;
        MinFilter[1] = LINEAR;
        MipFilter[1] = LINEAR;
        AddressU[1] = WRAP;
        AddressV[1] = WRAP;
        MagFilter[2] = LINEAR;
        MinFilter[2] = LINEAR;
        MipFilter[2] = LINEAR;
        AddressU[2] = WRAP;
        AddressV[2] = WRAP;
        
        // Set texture states by reference (can be null)
        Texture[0] = <DiffuseTexture>;
        Texture[1] = <SpecularTexture>;
        
        VertexShader = compile vs_3_0 BasicVertexShader();
        PixelShader = compile ps_3_0 AmbientPixelShader();
	}
	// Call this pass every Lights.Count
	pass PointLight
	{
		VertexShader = compile vs_3_0 BasicVertexShader();
		PixelShader = compile ps_3_0 MultipleLightPixelShader();
	}
	pass Emit
	{
        Texture[1] = <EmitTexture>;
		VertexShader = compile vs_3_0 BasicVertexShader();
		PixelShader = compile ps_3_0 EmitPixelShader();	
	}
	pass SkinnedAmbient
	{
        // Set sampler states
        MagFilter[0] = LINEAR;
        MinFilter[0] = LINEAR;
        MipFilter[0] = LINEAR;
        AddressU[0] = WRAP;
        AddressV[0] = WRAP;
        MagFilter[1] = LINEAR;
        MinFilter[1] = LINEAR;
        MipFilter[1] = LINEAR;
        AddressU[1] = WRAP;
        AddressV[1] = WRAP;
        MagFilter[2] = LINEAR;
        MinFilter[2] = LINEAR;
        MipFilter[2] = LINEAR;
        AddressU[2] = WRAP;
        AddressV[2] = WRAP;
        
        // Set texture states by reference (can be null)
        Texture[0] = <DiffuseTexture>;
        Texture[1] = <SpecularTexture>;
        
        VertexShader = compile vs_3_0 SkinnedVertexShader();
        PixelShader = compile ps_3_0 AmbientPixelShader();
	}
	// Call this pass every Lights.Count
	pass SkinnedPointLight
	{
		VertexShader = compile vs_3_0 SkinnedVertexShader();
		PixelShader = compile ps_3_0 MultipleLightPixelShader();
	}
	pass SkinnedEmit
	{
        Texture[1] = <EmitTexture>;
		VertexShader = compile vs_3_0 SkinnedVertexShader();
		PixelShader = compile ps_3_0 EmitPixelShader();	
	}
}