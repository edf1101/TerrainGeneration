using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeComputeHelper
{
   private ComputeShader myComputeShader;

    private List<biomeDescription> biomesIn;
    private const float noiseFreq=1;
    private Vector2 positionOffset;

    // setter to load the compute shader as this isnt monobehaviour so it cant be linked here
    public void setComputeShader(ComputeShader _Shader) 
    {
        myComputeShader = _Shader;
    }

    public void createBiomes() // will create the biomes
    {
        // set data needed for shader
        myComputeShader.SetInt("biomeNums", biomesIn.Count);
        myComputeShader.SetFloat("noiseFrequency", noiseFreq);
        myComputeShader.SetVector("positionOffset", positionOffset);

        // biomeDescription cant use sizeof in safe context so we need to make custom stride
        int biomeDescriptionStride = sizeof(int) + 2 * sizeof(float) + 2 * sizeof(float) + 4*sizeof(float);
        ComputeBuffer biomesBuffer = new ComputeBuffer(biomesIn.Count, biomeDescriptionStride);


    }


}

[System.Serializable]
public struct biomeDescription
{
    public int id;
   public  Vector2 tempRange;
    public Vector2 humRange;
    public Color colour;
}