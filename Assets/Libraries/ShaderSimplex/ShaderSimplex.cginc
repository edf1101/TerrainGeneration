#ifndef NOISE_SIMPLEX_FUNC
#define NOISE_SIMPLEX_FUNC
/* 
------------------------------------------------------------------
 
Ported by:
    Lex-DRL
    I've ported the code from GLSL to CgFx/HLSL for Unity,
    added a couple more optimisations (to speed it up even further)
    and slightly reformatted the code to make it more readable.
 
Original GLSL functions:
    https://github.com/ashima/webgl-noise
    Credits from original glsl file are at the end of this cginc.
 
------------------------------------------------------------------
 
Usage:
 
    float ns = snoise(float2);
 
 
 
Remark about those offsets from the original author:
 
    People have different opinions on whether these offsets should be integers
    for the classic noise functions to match the spacing of the zeroes,
    so we have left that for you to decide for yourself.
    For most applications, the exact offsets don't really matter as long
    as they are not too small or too close to the noise lattice period
    (289 in this implementation).
 
*/
 
// 1 / 289
#define NOISE_SIMPLEX_1_DIV_289 0.00346020761245674740484429065744f
 
float mod289(float x) {
    return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
}
 
float2 mod289(float2 x) {
    return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
}
 
float3 mod289(float3 x) {
    return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
}
 
float4 mod289(float4 x) {
    return x - floor(x * NOISE_SIMPLEX_1_DIV_289) * 289.0;
}
 

float permute(float x) {
    return mod289(
        x*x*34.0 + x
    );
}
 
float3 permute(float3 x) {
    return mod289(
        x*x*34.0 + x
    );
}
 
float4 permute(float4 x) {
    return mod289(
        x*x*34.0 + x
    );
}
 
 
 
float taylorInvSqrt(float r) {
    return 1.79284291400159 - 0.85373472095314 * r;
}
 
float4 taylorInvSqrt(float4 r) {
    return 1.79284291400159 - 0.85373472095314 * r;
}
 
 
 
float4 grad4(float j, float4 ip)
{
    const float4 ones = float4(1.0, 1.0, 1.0, -1.0);
    float4 p, s;
    p.xyz = floor( frac(j * ip.xyz) * 7.0) * ip.z - 1.0;
    p.w = 1.5 - dot( abs(p.xyz), ones.xyz );
 
    // GLSL: lessThan(x, y) = x < y
    // HLSL: 1 - step(y, x) = x < y
    s = float4(
        1 - step(0.0, p)
    );
 
    // Optimization hint Dolkar
    // p.xyz = p.xyz + (s.xyz * 2 - 1) * s.www;
    p.xyz -= sign(p.xyz) * (p.w < 0);
 
    return p;
}
 
 
 
// ----------------------------------- 2D -------------------------------------
 
float snoise(float2 v)
{
    const float4 C = float4(
        0.211324865405187, // (3.0-sqrt(3.0))/6.0
        0.366025403784439, // 0.5*(sqrt(3.0)-1.0)
     -0.577350269189626, // -1.0 + 2.0 * C.x
        0.024390243902439  // 1.0 / 41.0
    );
 
// First corner
    float2 i = floor( v + dot(v, C.yy) );
    float2 x0 = v - i + dot(i, C.xx);
 
// Other corners
 
    float4 x12 = x0.xyxy + C.xxzz;
    int2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
    x12.xy -= i1;
 
// Permutations
    i = mod289(i); // Avoid truncation effects in permutation
    float3 p = permute(
        permute(
                i.y + float3(0.0, i1.y, 1.0 )
        ) + i.x + float3(0.0, i1.x, 1.0 )
    );
 
    float3 m = max(
        0.5 - float3(
            dot(x0, x0),
            dot(x12.xy, x12.xy),
            dot(x12.zw, x12.zw)
        ),
        0.0
    );
    m = m*m ;
    m = m*m ;
 
// Gradients: 41 points uniformly over a line, mapped onto a diamond.
// The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)
 
    float3 x = 2.0 * frac(p * C.www) - 1.0;
    float3 h = abs(x) - 0.5;
    float3 ox = floor(x + 0.5);
    float3 a0 = x - ox;
 
// Normalise gradients implicitly by scaling m
// Approximation of: m *= inversesqrt( a0*a0 + h*h );
    m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );
 
// Compute final noise value at P
    float3 g;
    g.x = a0.x * x0.x + h.x * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}
 
 
//                 Credits from source glsl file:
//
// Description : Array and textureless GLSL 2D/3D/4D simplex
//               noise functions.
//      Author : Ian McEwan, Ashima Arts.
//  Maintainer : ijm
//     Lastmod : 20110822 (ijm)
//     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
//               Distributed under the MIT License. See LICENSE file.
//               https://github.com/ashima/webgl-noise
//
//
//           The text from LICENSE file:
//
//
// Copyright (C) 2011 by Ashima Arts (Simplex noise)
// Copyright (C) 2011 by Stefan Gustavson (Classic noise)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endif