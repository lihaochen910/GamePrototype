#include "murder.fxh"

//-----------------------------------------------------------------------------
// Constants
//-----------------------------------------------------------------------------

static const float TEX_UP = -1.0;

DECLARE_TEXTURE(PositionMap, 1);
DECLARE_TEXTURE(NormalMap, 2);

float attenuation = 1.0;
float intensity = 1.0;
float falloff = 1.0;

float4x4 ModelToClip;
float4x4 WorldToView;
float4 PenColor;
float3 LightPos;


//-----------------------------------------------------------------------------
// Input, output
//-----------------------------------------------------------------------------
struct PointLightVSOutput
{
	float4 position     : SV_Position;
	float4 color        : COLOR0;
	float2 uv           : TEXCOORD0;
    float2 uvScreen     : TEXCOORD1;
    float3 lightViewPos : TEXCOORD2;
};


//-----------------------------------------------------------------------------
// Functions
//-----------------------------------------------------------------------------

PointLightVSOutput VS(
	float4 position	: POSITION0,
	float4 color		: COLOR0,
	float2 uv	    : TEXCOORD0)
{
	PointLightVSOutput output;
	output.position = mul(position, ModelToClip);
	output.color    = color * PenColor;
	output.uv       = uv;
	output.uvScreen = float2( output.position.x * 0.5 + 0.5 , output.position.y * TEX_UP * 0.5 + 0.5 );
	output.lightViewPos = mul(float4( LightPos, 1.0 ), WorldToView).xyz;
	return output;
}

float4 PS(PointLightVSOutput input) : SV_Target0
{
	float4 pos = SAMPLE_TEXTURE(PositionMap, input.uvScreen);
	float4 normalScreen = SAMPLE_TEXTURE(NormalMap, input.uvScreen);

	float3 diff = input.lightViewPos - pos.xyz;
	float l = length( diff );
	float3 normalDiff = float3( diff.x/l, diff.y/l, diff.z/l );

	float r = dot( normalDiff, normalScreen.xyz * 2.0 - 1.0 );
	if( r > 0.0 ) {
		// r = pow( r * 1.2, 4.0 );
		r = pow( r * 1.2, 0.5 );
		// r = pow( min( r, 1.0 ), 0.5 );
		// r = pow( max( r - 0.5, 0.0 ), 2.0 ) + r * 0.5;
	} else {
		discard;
	}

	// r = pow( r, 1.5 );
	// float r = max( dot( normalDiff, normalScreen.xyz * 2.0 - 1.0 ), 0.0 );
	// float r = 1.0;
	l = length( diff );

	float k = 1.0 - clamp( ( l* 2.0 - falloff ) / ( attenuation - falloff ), 0.0, 1.0 ) ;
	float s = ( k * intensity * r ) * input.color.a;
	// s = mix( ( floor( s * 4.0 ) / 4.0 ), s, 0.4 );
	return pow( input.color * s, float4( normalScreen.a, normalScreen.a, normalScreen.a, normalScreen.a ));
		
	//fake dither

	// s *= ( 1.0 + rand( diff.xy ) * 0.035 );
	// FragColor = colorVarying * ( floor( s * 32.0 ) / 32.0 );
}

float Rand(float2 co){
	return frac(sin(dot(co.xy, float2(12.9898,78.233))) * 43758.5453);
}

technique PointLight {
    pass
    {
        PixelShader = compile PS_SHADERMODEL PS();
        VertexShader = compile VS_SHADERMODEL VS();
    }
}
