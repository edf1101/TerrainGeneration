using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurBiomeComputeHelper 
{

    private ComputeShader myShader;// compute shader we will dispatch
    private float[] singleVals;// array of all the single biome datas
    private int blurRad; //radius we blur (kernel number) must be odd
    private Vector2 mapSize; //self explanatory


    //constructor to import essential data
    public BlurBiomeComputeHelper(ComputeShader _shader, float[] _singleVals, int _blurRad,Vector2 _mapSize)
    {
        myShader = _shader;
        singleVals = _singleVals;
        blurRad = _blurRad;
        mapSize = _mapSize;
    }

    public void Blur()
    {

    }

    
}
