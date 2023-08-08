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

    private int[] biomeIndexes;

    private int [] biomesFound; // cant use bool as not blittable so either 1 or 0

   // constructor including all variables that are needed
    public BiomeComputeHelper(ComputeShader _Shader, Vector2 _mapSize, Vector2 _offset,List<biomeDescription> _biomesIn)
    {
        myComputeShader = _Shader;
        mapSize = _mapSize;
        positionOffset = _offset;
        biomesIn = _biomesIn;
        biomeIndexes = new int[(int)mapSize.x* (int)mapSize.y];
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
        myComputeShader.SetVector("mapSize", mapSize);

        // biomeDescription cant use sizeof in safe context so we need to make custom stride
        int biomeDescriptionStride = sizeof(int) + 2 * sizeof(float) + 2 * sizeof(float) + 4*sizeof(float);
        ComputeBuffer biomesBuffer = new ComputeBuffer(biomesIn.Count, biomeDescriptionStride);

        ComputeBuffer indexBuffer = new ComputeBuffer((int)(mapSize.x*mapSize.y), sizeof(int));// buffer stores the biome index value for each tile

        // buffer stores whether a specific biome is found useful so we
        // dont have to waste time doing blur and seperate if doesnt exist in tile
        biomesFound = new int[biomesIn.Count];
        ComputeBuffer biomeFoundBuffer = new ComputeBuffer(biomesIn.Count, sizeof(int));

        // sets up biome buffer
        biomesBuffer.SetData(biomesIn.ToArray());
        myComputeShader.SetBuffer(0, "biomesBuffer", biomesBuffer);

        // sets up index buffer
        indexBuffer.SetData(biomeIndexes);
        myComputeShader.SetBuffer(0, "indexesBuffer", indexBuffer);

        // sets up biomes found buffer
        biomeFoundBuffer.SetData(biomesFound);
        myComputeShader.SetBuffer(0, "biomesFoundBuffer", biomeFoundBuffer);


        myComputeShader.Dispatch(0, Mathf.CeilToInt(mapSize.x  / 16), Mathf.CeilToInt(mapSize.y  / 16), 1);


        indexBuffer.GetData(biomeIndexes); // get the buffer data back into array
        biomeFoundBuffer.GetData(biomesFound);


        indexBuffer.Release(); // release buffers so they dont hassle gpu
        biomesBuffer.Release();
        biomeFoundBuffer.Release();

    }

    public RenderTexture GetColourMap() // getter for colourMap
    {
        return colourMap;
    }

    public int[] getIDMap()// getter for biomeIndexes
    {
        return biomeIndexes;
    }

    public int[] getBiomesFound()
    {
        return biomesFound;
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