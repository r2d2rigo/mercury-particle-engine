Texture2D<float4> Texture : register(t0);
sampler TextureSampler : register(s0);

cbuffer Parameters : register(b0)
{
	float4x4 WVP : packoffset(c0);
	bool FastFade : packoffset(c4.x);
};

struct VS_IN
{
	float2 pos : POSITION;
	float4 color : COLOR0;
	float2 tex : TEXCOORD;
	float age : COLOR1;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	float4 color : COLOR0;
	float2 tex : TEXCOORD;
};


PS_IN SpriteVertexShader(VS_IN input)
{
	PS_IN output = (PS_IN)0;

	output.pos = mul(float4(input.pos, 0, 1), WVP);
	output.tex = input.tex;
	output.color = input.color;

	if (FastFade)
	{
		output.color.a = 1.0f - input.age;
	}

	return output;
}

float4 SpritePixelShader(PS_IN input) : SV_Target
{
	return Texture.Sample(TextureSampler, input.tex) * input.color;
}