using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The fill gaps compute shader helps to fill up blank space around pixels
// this is the c# helper for that shader

public class FillGapsComputeHelper 
{
    // reference to shader we use
    static private ComputeShader myShader;

    // setter for shader
    static public void setShader(ComputeShader _shader)
    {
        myShader = _shader;
    }


    
    private RenderTexture finalMap; // debug for final map
    private Color[] finalColors; // what we actually return

    public Color[] FillGaps(Color[] _partials)
    {
      
        int mapSize = (int)Mathf.Sqrt(_partials.Length); // calculate map size

        // create new Render Texture for us to write to at the end, mainly debug
        finalMap = new RenderTexture(mapSize, mapSize, 24);
        finalMap.enableRandomWrite = true;
        finalMap.Create();
        myShader.SetTexture(0, "finalTex", finalMap);

        // send mapSize to the shader
        myShader.SetInt("mapSize", mapSize);


        // create compute buffer for colours going into the shader
        ComputeBuffer coloursInBuffer = new ComputeBuffer(_partials.Length, sizeof(float) * 4);
        coloursInBuffer.SetData(_partials); // set it to the partial special colours
        myShader.SetBuffer(0, "coloursInBuffer", coloursInBuffer);

        // create a compute buffer for the colours coming out once theyve been expanded
        ComputeBuffer coloursOutBuffer = new ComputeBuffer(_partials.Length, sizeof(float) * 4);
        finalColors = new Color[_partials.Length]; // blank array for now
        coloursOutBuffer.SetData(finalColors);
        myShader.SetBuffer(0, "coloursOutBuffer", coloursOutBuffer); // send it to the shader

        myShader.Dispatch(0, mapSize, mapSize, 1); // dispatch shader

        coloursOutBuffer.GetData(finalColors); // get complete data

        coloursInBuffer.Release(); // release buffers so GPU happy
        coloursOutBuffer.Release();

        return finalColors; // return final map
    }

    public RenderTexture returnTexture() // getter for the debug texture
    {
        return finalMap;
    }
}
