using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurBiomeComputeHelper 
{

    private ComputeShader myShader;// compute shader we will dispatch
    private float[] singleVals;// array of all the single biome datas
    private int blurRad; //radius we blur (kernel number) must be odd
    private Vector2 mapSize; //self explanatory


    private RenderTexture intermediateRT;
    private RenderTexture outputRT;



    //constructor to import essential data
    public BlurBiomeComputeHelper(ComputeShader _shader, float[] _singleVals, int _blurRad,Vector2 _mapSize)
    {
        myShader = _shader;
        singleVals = _singleVals;
        blurRad = _blurRad;
        mapSize = _mapSize;
    }

    public float[] Blur()
    {
        //width and height of map
        int width = (int)mapSize.x;
        int height = (int)mapSize.y;

       
        //create the input buffer - this is for horizontal pass
        ComputeBuffer inpBufferHor = new ComputeBuffer(singleVals.Length, sizeof(float));
        inpBufferHor.SetData(singleVals);
        myShader.SetBuffer(0, "indexesBuffer", inpBufferHor);

        // output buffer
        float[] outArray = new float[singleVals.Length];
        ComputeBuffer outBufferHor = new ComputeBuffer(singleVals.Length, sizeof(float));
        outBufferHor.SetData(outArray);
        myShader.SetBuffer(0, "outBuffer", outBufferHor); // set it to the horizontal pass (Kernel 0)


  
        myShader.SetInt("KernelSize", blurRad); // set variables to shader
        myShader.SetVector("mapSize", mapSize);

        myShader.Dispatch(0, width, height, 1); // dispatch horizontal pass (kernel 0)

        outBufferHor.GetData(outArray); // get the output buffer back


        inpBufferHor.SetData(outArray);// set the horizontal output as vertical input
        myShader.SetBuffer(1, "indexesBuffer", inpBufferHor);


        outArray = new float[singleVals.Length]; // recreate output buffer for vertical pass (kernel 1)
        outBufferHor.SetData(outArray);
        myShader.SetBuffer(1, "outBuffer", outBufferHor);

        //create output texture so we can visualise mainly for debug
        outputRT = new RenderTexture(width, height, 0);
        outputRT.enableRandomWrite = true;
        outputRT.Create();
        myShader.SetTexture(1, "OutputTexture", outputRT);// set texture to vertical pass


        myShader.Dispatch(1, width, height, 1);// dispatch vertical pass

        outBufferHor.GetData(outArray); // return the array of blurs
        return outArray;
    }

    public RenderTexture getTex() // getter for the debug texture
    {
        return outputRT;
    }

    
}
