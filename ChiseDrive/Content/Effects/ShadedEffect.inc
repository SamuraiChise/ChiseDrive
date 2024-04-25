////////////////////////////////////// COMMON STRUCTURES //

struct Light
{
	float4 Color;
	float4 Position;
	float Falloff;
	float Range;
};

struct VertexShaderOutputNormal
{
	// Decl	Name			: Register
	float4	Position		: POSITION;
	float4	Color			: COLOR;
	float2	TexCoords		: TEXCOORD0;
	float3	WorldNormal		: TEXCOORD1;
	float3	WorldPosition	: TEXCOORD2;
	float3	ViewDirection	: TEXCOORD4;
	float3x3 TangentToWorld	: TEXCOORD5;
};

struct PixelShaderInputNormal
{
	float4	Color			: COLOR;
	float2	TexCoords		: TEXCOORD0;
	float3	WorldNormal		: TEXCOORD1;
	float3	WorldPosition	: TEXCOORD2;
	float3	ViewDirection	: TEXCOORD4;
	float3x3 TangentToWorld	: TEXCOORD5;
};

struct BasicShaderData
{
	float4 Position		: POSITION0;
	float4 Color		: COLOR;
	float2 TexCoord		: TEXCOORD0;
};

struct SimpleColor
{
	float4 Color			: COLOR;
};

struct NormalDepthVertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

Light Lights[8];

////////////////////////////////////// SHARED PARAMETERS //

shared float4x4	ViewProjection;
shared float4x4 View;
shared float3	CameraPosition;
shared float4	AmbientLightColor;
shared int		NumLightsPerPass = 0;

/////////////////////////////////////// TEXTURE SAMPLERS //

texture DiffuseTexture;
texture NormalTexture;
texture SpecularTexture;
texture EmittanceTexture;

sampler DiffuseSampler = sampler_state
{
	Texture = <DiffuseTexture>;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};
sampler NormalSampler = sampler_state
{
	Texture = <NormalTexture>;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};
sampler SpecularSampler = sampler_state
{
	Texture = <SpecularTexture>;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};
sampler EmitSampler = sampler_state
{
	Texture = <EmittanceTexture>;
	MagFilter = LINEAR;
	MinFilter = LINEAR;
	MipFilter = LINEAR;
	AddressU = WRAP;
	AddressV = WRAP;
};

//////////////////////////////////////// UNSHARED STATES //

float4x4		World;	// Object specific translation

/////////// EFFECT INSTANCED PARAMETERS (SET AND FORGET) //

float4	MaterialColor;					// Diffuse Color
float	SpecularPower;
float	SpecularIntensity;
bool	DiffuseTextureEnabled = false;
bool	NormalTextureEnabled = false;
bool	SpecularTextureEnabled = false;
bool	EmittanceTextureEnabled = false;
float	TextureUReps = 1.0;
float	TextureVReps = 1.0;
float	AlphaFade = 1.0;

/////////////////////////////////// AMBIENT PIXEL SHADER //
// Takes the input from the BasicVertexShader and processes
// the data to return the ambient color.

float4 AmbientNormalPixelShader(PixelShaderInputNormal input) : COLOR
{
	float4 diffuseTexture = 0;
	
	if (DiffuseTextureEnabled)
	{
		// Take the diffuse from a texture if available
		diffuseTexture = tex2D(DiffuseSampler, input.TexCoords);
	}
	else
	{
		// Otherwise use a default material color
		diffuseTexture = MaterialColor;
	}
	
	// Apply the vertex lighting information to the texture color
    float4 color = diffuseTexture * input.Color;
    color.a = diffuseTexture.a;
    return color;
}

////////////////////////////////// SPECULAR PIXEL SHADER //
// Takes the input from the BasicVertexShader and processes
// the data to return the ambient color.

float4 CalculateSingleLight(
	Light light,
	float3 worldPosition,
	float3 worldNormal,
	float4 diffuseColor,
	float4 specularColor )
{
	float3 lightVector = light.Position - worldPosition;
	float lightDist = length(lightVector);
	float3 directionToLight = normalize(lightVector);
	
	float baseIntensity = pow(saturate(
		(light.Range - lightDist) / light.Range), light.Falloff);
		
	float diffuseIntensity = saturate(dot(directionToLight, worldNormal));
	float4 diffuse = diffuseIntensity * light.Color * diffuseColor;
	
	// Used in Phong per-pixel Calculations
	float3 reflectionVector = normalize(reflect(-directionToLight, worldNormal));
	float3 directionToCamera = normalize(CameraPosition - worldPosition);

	float4 specular = saturate(
		light.Color * specularColor 
		* SpecularIntensity	
		* pow(saturate(dot(reflectionVector, directionToCamera)),
		SpecularPower));
		
	return baseIntensity * (diffuse + specular);
}

float4 MultipleLightPixelShader(PixelShaderInputNormal input) : COLOR
{
	float4 diffuseColor = MaterialColor;
	float4 specularColor = MaterialColor;
	
	if (DiffuseTextureEnabled)
	{
		diffuseColor = tex2D(DiffuseSampler, input.TexCoords);
	}
	
	if (SpecularTextureEnabled)
	{
		specularColor = tex2D(SpecularSampler, input.TexCoords);
	}
	
	float4 color = 0;

	float3 NormalFromMap = 0;
	
	if (NormalTextureEnabled)
	{
		NormalFromMap = tex2D(NormalSampler, input.TexCoords);
		NormalFromMap = mul(NormalFromMap, input.TangentToWorld);
		NormalFromMap = normalize(NormalFromMap);
	}
	else
	{
		NormalFromMap = input.WorldNormal;
	}

	for (int i = 0; i < NumLightsPerPass; i++)
	{
		color += CalculateSingleLight(
			Lights[i],
			input.WorldPosition,
			NormalFromMap,
			diffuseColor,
			specularColor);
	}
	
	color.a = diffuseColor.a;
	
	return color;
}

///////////////////////////////// EMITTANCE PIXEL SHADER //
// Takes the input from the BasicVertexShader and processes
// the data to return the ambient color.
float4 EmitPixelShader(PixelShaderInputNormal input) : COLOR
{
	float4 emit = 0;
	
	if (EmittanceTextureEnabled)
	{
		// Get the base color from the textures and materials
		float4 diffuse = MaterialColor;
		if (DiffuseTextureEnabled)
		{
			diffuse = tex2D(DiffuseSampler, input.TexCoords);
		}
		
		// The emit texture is going to be 1 to 0 for intensity of emittance
		emit = tex2D(EmitSampler, input.TexCoords);
		emit *= diffuse;
	}
	
	// Use the passed alpha value
	//emit.a = input.Color.a;
	
	return emit;
}

////////////////////////////////////// FULL PIXEL SHADER //
// Combines all the shaders in one pass
float4 FullPixelShader(PixelShaderInputNormal input) : COLOR
{
	float4 ambient = AmbientNormalPixelShader(input);
	float4 specular = MultipleLightPixelShader(input);

	float4 color = ambient + specular;
	color.a = ambient.a;

	if (EmittanceTextureEnabled)
	{
		float4 emittance = EmitPixelShader(input);
		color = color + emittance;
	}
	
	return color;
}

//////////////////////////////////// NORMAL DEPTH SHADER //	
// Simple pixel shader for rendering the normal and depth information.
float4 NormalDepthPixelShader(float4 color : COLOR0) : COLOR0
{
    return color;
}
