using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeComputeHelper
{
   private ComputeShader myComputeShader;

    private List<biomeDescription> biomesIn;
    private const float noiseFreq=0.01f;
    private Vector2 positionOffset;
    private Vector2 mapSize;

    private RenderTexture colourMap;

   // constructor including all variables that are needed
    public BiomeComputeHelper(ComputeShader _Shader, Vector2 _mapSize, Vector2 _offset,List<biomeDescription> _biomesIn)
    {
        myComputeShader = _Shader;
        mapSize = _mapSize;
        positionOffset = _offset;
        biomesIn = _biomesIn;
    }

    public void createBiomes() // will create the biomes
    {
        // creates the return texture for the colour map
        colourMap = new RenderTexture((int)mapSize.x, (int)mapSize.y, 24);
        colourMap.enableRandomWrite = true;
        colourMap.Create();
        myComputeShader.SetTexture(0, "colourMap", colourMap);

         
        // set data needed for shader
        myComputeShader.SetInt("biomeNums", biomesIn.Count);
        myComputeShader.SetFloat("noiseFrequency", noiseFreq);
        myComputeShader.SetVector("positionOffset", positionOffset);

        // biomeDescription cant use sizeof in safe context so we need to make custom stride
        int biomeDescriptionStride = sizeof(int) + 2 * sizeof(float) + 2 * sizeof(float) + 4*sizeof(float);
        ComputeBuffer biomesBuffer = new ComputeBuffer(biomesIn.Count, biomeDescriptionStride);

        // sets up biome buffer
        biomesBuffer.SetData(biomesIn.ToArray());
        myComputeShader.SetBuffer(0, "biomesBuffer", biomesBuffer);


        myComputeShader.Dispatch(0, Mathf.CeilToInt(mapSize.x  / 8), Mathf.CeilToInt(mapSize.y  / 8), 1);

       

        biomesBuffer.Release();

    }

    public RenderTexture GetColourMap() // getter for colourMap
    {
        return colourMap;
    }


}

// struct for biome description we use this is the c# version to convert between c# and hlsl
[System.Serializable]
public struct biomeDescription
{
    public int id;
   public  Vector2 tempRange;
    public Vector2 humRange;
    public Color colour;
}