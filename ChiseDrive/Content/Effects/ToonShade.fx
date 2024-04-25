#include "ShadedEffect.inc"

float4x4 Bones[59];

float ToonThresholds[4] = { 0.95, 0.9, 0.27, 0.20 };
float ToonBrightnessLevels[5] = { 1.3, 1.05, 1.0, 0.65, 0.35 };
float SceneLightLevel = 0.2;

float Delta1 = 20;//1 / (ToonThresholds[0] - ToonThresholds[1]);
float Delta2 = 14;//1 / (ToonThresholds[2] - ToonThresholds[3]);

float LightDelta1 = 0.3;//ToonBrightnessLevels[0] - ToonBrightnessLevels[2];
float LightDelta2 = 0.65;//ToonBrightnessLevels[2] - ToonBrightnessLevels[4];

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



VertexShaderOutputNormal SkinnedToonVertexShader(SkinnedNormalVertexShaderInput input)
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

float4 ConstantPixelShader(SimpleColor input) : COLOR0
{
	return input.Color;
}
float4 FlatPixelShader(PixelShaderInputNormal input) : COLOR0
{
	float4 color = 0;
	
	if (DiffuseTextureEnabled)
	{
		color = tex2D(DiffuseSampler, input.TexCoords);
	}
	else
	{
		color = MaterialColor;
	}
	
	return color;
}

float4 BasicToonPixelShader(BasicShaderData input) : COLOR0
{
	float4 color = 0;
	if (DiffuseTextureEnabled)
	{
		color = tex2D(DiffuseSampler, input.TexCoord);
	}
	color *= input.Color;
	return color;
}

BasicShaderData BasicSkinnedToonVertexShader(SkinnedNormalVertexShaderInput input)
{
	BasicShaderData output;
	
	float4x4 skinTransform = 0;
    skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
    skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
    skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
    skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;

	// Skin the vertex position.
    float4 position = input.Position;
    position = mul(position, skinTransform);
    position = mul(position, World);
    position = mul(position, ViewProjection);
    output.Position = position;
	
	output.TexCoord = input.TexCoord;
	
    return output;
}

// Pixel shader applies a cartoon shading algorithm.
float4 ToonPixelShader(PixelShaderInputNormal input) : COLOR0
{
	float4 diffuseColor = AmbientLightColor;
	float4 specularColor = 0;
	float3 normalFromMap = 0;
	float4 color = 0;

	if (SpecularTextureEnabled)
	{
		specularColor = tex2D(SpecularSampler, input.TexCoords);
	}
	else
	{
		specularColor = 0.5;
	}
	if (NormalTextureEnabled)
	{
		normalFromMap = tex2D(NormalSampler, input.TexCoords);
		normalFromMap = mul(normalFromMap, input.TangentToWorld);
		normalFromMap = normalize(normalFromMap);
	}
	else
	{
		//normalFromMap = mul(input.WorldNormal, input.TangentToWorld);
		//normalFromMap = normalize(normalFromMap);
		//normalFromMap = mul(input.TangentToWorld, input.WorldNormal);//input.WorldNormal;
		//normalFromMap = normalize(normalFromMap);
		normalFromMap = float3(0, 0, 1);
		normalFromMap = mul(normalFromMap, input.TangentToWorld);
		normalFromMap = normalize(normalFromMap);

	}
	
	for (int i = 0; i < NumLightsPerPass; i++)
	{
		color += CalculateSingleLight(
			Lights[i],
			input.WorldPosition,
			normalFromMap,
			diffuseColor,
			specularColor);
	}
		
	float scenelight = color.r + color.g + color.b;
	float light = 0;

    if (scenelight > ToonThresholds[0])
        light = ToonBrightnessLevels[0];
    else if (scenelight > ToonThresholds[1])
    {
		float delta = (scenelight - ToonThresholds[1]) * Delta1;
		light = LightDelta1 * delta + ToonBrightnessLevels[2];
		//float delta = (scenelight - ToonThresholds[1]) / (ToonThresholds[0] - ToonThresholds[1]);
		//light = (ToonBrightnessLevels[0] - ToonBrightnessLevels[1]) * delta + ToonBrightnessLevels[2];
    }
    else if (scenelight > ToonThresholds[2])
        light = ToonBrightnessLevels[2];
    else if (scenelight > ToonThresholds[3])
    {
    	float delta = (scenelight - ToonThresholds[3]) * Delta2;
		light = LightDelta2 * delta + ToonBrightnessLevels[4];
		//float delta = (scenelight - ToonThresholds[3]) / (ToonThresholds[2] - ToonThresholds[3]);
		//light = (ToonBrightnessLevels[2] - ToonBrightnessLevels[3]) * delta + ToonBrightnessLevels[4];
    }
    else
        light = ToonBrightnessLevels[4];
            
	if (DiffuseTextureEnabled)
	{
		color = tex2D(DiffuseSampler, input.TexCoords);
	}
	else
	{
		color = MaterialColor;
	}

	//scenelight *= AmbientLightColor;
	color.rgb *= light + AmbientLightColor;
	
    color.a *= AlphaFade;
    
    return color;
}

NormalDepthVertexShaderOutput NormalSkinnedToonVertexShader(SkinnedNormalVertexShaderInput input)
{
	NormalDepthVertexShaderOutput output;

	float4x4 skinTransform = 0;

    skinTransform += Bones[input.BoneIndices.x] * input.BoneWeights.x;
    skinTransform += Bones[input.BoneIndices.y] * input.BoneWeights.y;
    skinTransform += Bones[input.BoneIndices.z] * input.BoneWeights.z;
    skinTransform += Bones[input.BoneIndices.w] * input.BoneWeights.w;

	// Skin the vertex position.
    float4 position = input.Position;
    position = mul(position, skinTransform);
    position = mul(position, World);
    position = mul(position, ViewProjection);
    output.Position = position;
	
	// Output color holds the normal information 0 - 1
	float3 worldNormal = mul(mul(input.Normal, skinTransform), World);
	output.Color.rbg = (worldNormal + 1) / 2;

    // The output alpha holds the depth, scaled to fit into a 0 - 1.
    output.Color.a = output.Position.z / output.Position.w;
    
    return output;
}

technique Lit
{
	pass Ambient
	{
        VertexShader = compile vs_3_0 SkinnedToonVertexShader();
        PixelShader = compile ps_3_0 AmbientNormalPixelShader();
	}
	// Call this pass every Lights.Count
	pass PointLight
	{
		VertexShader = compile vs_3_0 SkinnedToonVertexShader();
		PixelShader = compile ps_3_0 MultipleLightPixelShader();
	}
	pass Emit
	{
		VertexShader = compile vs_3_0 SkinnedToonVertexShader();
		PixelShader = compile ps_3_0 EmitPixelShader();	
	}
	pass Full
	{
		VertexShader = compile vs_3_0 SkinnedToonVertexShader();
		PixelShader = compile ps_3_0 ToonPixelShader();	
	}
}

technique NormalDepth
{
    pass Full
    {
        VertexShader = compile vs_1_1 NormalSkinnedToonVertexShader();
        PixelShader = compile ps_1_1 NormalDepthPixelShader();
    }
}
