////////////////////////////////////// COMMON STRUCTURES //

struct Light
{
	float4 Color;
	float4 Position;
	float Falloff;
	float Range;
};

struct SkinnedVertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float4 BoneIndices : BLENDINDICES0;
    float4 BoneWeights : BLENDWEIGHT0;
};

struct VertexShaderOutput
{
	// Decl	Name			: Register
	float4	Position		: POSITION;
	float4	Color			: COLOR;
	float2	TexCoords		: TEXCOORD0;
	float3	WorldNormal		: TEXCOORD1;
	float3	WorldPosition	: TEXCOORD2;
};

struct PixelShaderInput
{
	float4	Color			: COLOR;
	float2	TexCoords		: TEXCOORD0;
	float3	WorldNormal		: TEXCOORD1;
	float3	WorldPosition	: TEXCOORD2;
};

////////////////////////////////////// SHARED PARAMETERS //

shared float4x4	View;
shared float4x4 Projection;
shared float3	CameraPosition;
shared float4	AmbientLightColor;
shared int		NumLightsPerPass = 0;

/////////////////////////////////////// TEXTURE SAMPLERS //

sampler DiffuseSampler;
sampler SpecularSampler;
sampler EmitSampler;

texture DiffuseTexture;
texture SpecularTexture;
texture EmitTexture;

//////////////////////////////////////// UNSHARED STATES //

float4x4		World;	// Object specific translation

/////////// EFFECT INSTANCED PARAMETERS (SET AND FORGET) //

float4	MaterialColor;					// Diffuse Color
float	SpecularPower;
float	SpecularIntensity;
bool	DiffuseTexEnabled = false;
bool	SpecularTexEnabled = false;
bool	EmitTexEnabled = false;
float	TextureUReps = 1.0;
float	TextureVReps = 1.0;

//////////////////////////////////// BASIC VERTEX SHADER //
// This translates the position and normal by the world.

VertexShaderOutput BasicVertexShader(
	float3	position	: POSITION,
	float3	normal		: NORMAL,
	float4	color		: COLOR,
	float2	texCoord	: TEXCOORD0 )
{
	VertexShaderOutput output;
	
	float4x4 worldviewprojection = mul(mul(World, View), Projection);
	
	output.Position = mul(float4(position, 1.0), worldviewprojection);
	
	output.WorldNormal = mul(normal, World);
	float4 worldposition = mul(float4(position, 1.0), World);
	output.WorldPosition = worldposition / worldposition.w;
	
	output.TexCoords.x = texCoord.x * TextureUReps;
	output.TexCoords.y = texCoord.y * TextureVReps;
	
	output.Color = color;
	
	return output;
}

/////////////////////////////////// AMBIENT PIXEL SHADER //
// Takes the input from the BasicVertexShader and processes
// the data to return the ambient color.

float4 AmbientPixelShader(PixelShaderInput input) : COLOR
{
	float4 diffuse = MaterialColor;
	
	if (DiffuseTexEnabled)
	{
		diffuse *= tex2D(DiffuseSampler, input.TexCoords);
	}
	
	float4 color = (AmbientLightColor * diffuse);
	
	//color.a = diffuse.a;
	//color.a = input.Color.a;
	
	return color;
}

float4 EmitPixelShader(PixelShaderInput input) : COLOR
{
	float4 emit = float4(0,0,0,0);
	
	if (EmitTexEnabled)
	{
		float4 diffuse = MaterialColor;
		if (DiffuseTexEnabled)
		{
			diffuse *= tex2D(DiffuseSampler, input.TexCoords);
		}
		
		emit = tex2D(EmitSampler, input.TexCoords);
		emit *= diffuse;
	}
	
	emit.a = input.Color.a;
	
	return emit;
}

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
	