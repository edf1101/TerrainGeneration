using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class biomeSeperatorHelper
{
    private ComputeShader myShader;// shader we use to seperate biomes
    private int[] biomeMap;// map of all biomes
    private Vector2 mapSize;

    private RenderTexture displayTex;


    public biomeSeperatorHelper(ComputeShader _shader, int[] _biomeMap,Vector2 _mapSize) // constructor for required variables
    {
        myShader = _shader;
        biomeMap = _biomeMap;
        mapSize = _mapSize;
    }


    public float[] seperateBiome(int biomeID)
    { 

        displayTex = new RenderTexture((int)mapSize.x, (int)mapSize.y, 24); // create a texture we use for debugging if it works
        displayTex.enableRandomWrite = true;
        displayTex.Create();
        myShader.SetTexture(0, "displayTex", displayTex);// set the texture to the shader

        myShader.SetInt("biomeID", biomeID);// set the shader variables
        myShader.SetVector("mapSize", mapSize);

        ComputeBuffer inpBuffer = new ComputeBuffer(biomeMap.Length, sizeof(int));// create buffers, for map in
        ComputeBuffer outBuffer = new ComputeBuffer(biomeMap.Length, sizeof(float));// and the single biome out


        float[] biomeOut = new float[(int)(mapSize.x * mapSize.y)];// set initial data for the buffers
        inpBuffer.SetData(biomeMap);
        outBuffer.SetData(biomeOut);

        myShader.SetBuffer(0, "biomeInBuffer", inpBuffer);
        myShader.SetBuffer(0, "singleOutBuffer", outBuffer);

        myShader.Dispatch(0, Mathf.CeilToInt(mapSize.x / 16), Mathf.CeilToInt(mapSize.y / 16), 1); // run the compute shader

        outBuffer.GetData(biomeOut);

        outBuffer.Release(); // release so the GPU is happy
        inpBuffer.Release();

        return biomeOut;
    }
    public RenderTexture getDisplayTex() // return texture to the main script (debugging mainly)
    {
        return displayTex;
    }

}
