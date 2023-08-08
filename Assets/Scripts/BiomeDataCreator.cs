using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeDataCreator 
{
    private List<biomeDescription> theBiomes;
    private Vector2 biomeIndex;
    private const int blurRad=5;

    public BiomeDataCreator( List<biomeDescription> _theBiomes, Vector2 _biomeIndex)
    {
        biomeIndex = _biomeIndex;
        theBiomes = _theBiomes;
    }


    private ComputeShader biomeCreateShader;
    private ComputeShader biomeSeperateShader;
    private ComputeShader biomeBlurShader;

    public void setShaders(ComputeShader _biomeCreate,ComputeShader _biomeSeperate,ComputeShader _biomeBlur)
    {
        biomeCreateShader = _biomeCreate;
        biomeSeperateShader = _biomeSeperate;
        biomeBlurShader = _biomeBlur;
    }
}
