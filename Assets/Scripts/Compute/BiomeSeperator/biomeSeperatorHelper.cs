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
        int[] returnMap = new int[(int)(mapSize.x * mapSize.y)];

        displayTex = new RenderTexture((int)mapSize.x, (int)mapSize.y, 24);
        displayTex.enableRandomWrite = true;
        displayTex.Create();
        myShader.SetTexture(0, "displayTex", displayTex);

        myShader.SetInt("biomeID", biomeID);
        myShader.SetVector("mapSize", mapSize);

        ComputeBuffer inpBuffer = new ComputeBuffer(biomeMap.Length, sizeof(int));
        ComputeBuffer outBuffer = new ComputeBuffer(biomeMap.Length, sizeof(float));


        float[] biomeOut = new float[(int)(mapSize.x * mapSize.y)];
        inpBuffer.SetData(biomeMap);
        outBuffer.SetData(biomeOut);

        myShader.SetBuffer(0, "biomeInBuffer", inpBuffer);
        myShader.SetBuffer(0, "singleOutBuffer", outBuffer);

        myShader.Dispatch(0, Mathf.CeilToInt(mapSize.x / 16), Mathf.CeilToInt(mapSize.y / 16), 1);

        outBuffer.GetData(biomeOut);

        outBuffer.Release();
        inpBuffer.Release();

        return biomeOut;
    }
    public RenderTexture getDisplayTex()
    {
        return displayTex;
    }

}
