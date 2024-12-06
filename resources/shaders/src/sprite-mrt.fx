#include "murder.fxh"

//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

static const float TEX_UP = -1.0;
static const float4 Brightness = float4( 0.1, 0.1, 0.2, 0.0 );

// Type of variation.
static const int VariationDull = 0;
static const int VariationBright = 1;
static const int VariationBrightNPC = 2;
static const int VariationHero = 3;

DECLARE_TEXTURE(NormalTexture, 1);
DECLARE_TEXTURE(SamplerIrradiance, 2);


float4x4 ClipToView;
int VariationType;

float2 JohnPos;
float2 SamPos;

float GlowR = 1;
float GlowG;
float GlowB;
float GlowA;

float Time;


//-----------------------------------------------------------------------------
// Input, output
//-----------------------------------------------------------------------------

struct ColorPassVSOutput
{
	float4 position     : SV_Position;
	float4 color        : COLOR0;
	float2 uv           : TEXCOORD0;
    float2 uvScreen     : TEXCOORD1;
};

struct MRTPassVSOutput
{
	float4 position     : SV_Position;
	float4 color        : COLOR0;
	float2 uv           : TEXCOORD0;
    float4 viewPos      : TEXCOORD1;
    float2 distances	   : TEXCOORD3;
};

struct MRTPassPSOutput
{
	float4 albedo : COLOR;
	float4 normal : TEXCOORD0;
	float4 position : TEXCOORD1;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------


ColorPassVSOutput SimpleOutputVertexShader(
	float4 position  : POSITION0,
	float4 color		: COLOR0,
	float2 texCoord0	: TEXCOORD0)
{
	ColorPassVSOutput output;
	output.position = mul(position, MatrixTransform);
	output.color    = color;
	output.uv       = texCoord0;
	output.uvScreen = float2( output.position.x * 0.5 + 0.5 , output.position.y * TEX_UP * 0.5 + 0.5 );
	return output;
}

float4 SimpleOutputPixelShader(ColorPassVSOutput input) : SV_Target0
{
	float4 color = SAMPLE_TEXTURE(Texture, input.uv);
    if ( color.a < 0.5 ) {
        discard;
    }

	return ( SAMPLE_TEXTURE( SamplerIrradiance, input.uvScreen ) + Brightness ) * color * input.color;
}


MRTPassVSOutput MRTVertexShader(
	float4 position  : POSITION0,
	float4 color		: COLOR0,
	float2 texCoord0	: TEXCOORD0)
{
	MRTPassVSOutput output;
	output.position = mul(position, MatrixTransform);
	output.color    = color;
	output.uv       = texCoord0;
	output.viewPos = mul(position, ClipToView);
    output.distances.x = length( ( position.xy - JohnPos.xy ) * float2( 1.0, 0.75 ) );
    output.distances.y = length( ( position.xy - SamPos.xy ) * float2( 1.0, 0.75 ) );
	return output;
}


MRTPassPSOutput MRTPixelShader(MRTPassVSOutput input) : SV_Target0
{
	MRTPassPSOutput output;
	
	float4 color = SAMPLE_TEXTURE(Texture, input.uv);
	if ( color.a < 1.0 ) {
		discard;
	}

	float bright = ( color.g + color.b + color.r ) * 0.02; // + ( 1.0 - c1.a ) * 0.2;
	output.position = input.viewPos;

	if ( VariationType == VariationDull )
	{
		output.normal = float4( 0.5, 0.5 + bright + 0.1, 1.0, 0.7 );
	}
	else
	{
		output.normal = float4( 0.5, 0.5 + bright + 0.1, 1.0, 1.0 );
	}

	output.albedo = color * input.color;

	if ( VariationType == VariationBright )
	{
		output.albedo.a = min(
			smoothstep( 0.3, 0.6, input.distances.x ),
			smoothstep( 0.3, 0.6, input.distances.y )
		) * 0.1 + 0.9;
	}
	else if ( VariationType == VariationBrightNPC )
	{
		output.albedo.a = 0.9;
	}
	else if ( VariationType == VariationHero )
	{
		float4 glowColor = float4( GlowR, GlowG, GlowB, 0 ) * GlowA;
		float l = length( color.rgb );
		glowColor = glowColor * pow( l, 0.5 ) * ( pow( sin( Time * 5.0 )*0.4 + 0.5, 0.5 ) + 0.1 );
		output.albedo = output.albedo + glowColor;
		output.albedo.a = 0.9 - GlowA * 0.5;
	}
	else
	{
		output.albedo.a = 1.0;
	}
	
	return output;
}


//-----------------------------------------------------------------------------
// Techniques
//-----------------------------------------------------------------------------

technique SpriteMRT {
    pass ColorPass
    {
        PixelShader = compile PS_SHADERMODEL SimpleOutputPixelShader();
        VertexShader = compile VS_SHADERMODEL SimpleOutputVertexShader();
    }
    pass MRTPass
    {
        PixelShader = compile PS_SHADERMODEL MRTPixelShader();
        VertexShader = compile VS_SHADERMODEL MRTVertexShader();
    }
}
