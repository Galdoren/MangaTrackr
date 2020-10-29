sampler2D implicitInput : register(s0);

float4 PS(float2 uv : TEXCOORD) : COLOR
{
    float4 color = tex2D(implicitInput, uv);
   
    float4 result;  
  
    result.r=1-color.r;
    result.g=1-color.g;
    result.b=1-color.b;
    result.a=color.a;
 
   return result;
}