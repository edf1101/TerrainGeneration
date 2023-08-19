using UnityEngine;


/* This is the helper class for the RGBBlur shader
 * it similar to the biome blur shader but it blurs in RGB not greyscale
 */

public class rgbBlurComputeHelper 
{
    private static ComputeShader blurComputeShader; // reference to shader we need

    private int kernelSize; // kernel size gets set later but we'll declare it here


    private RenderTexture outputRT; // output Render Texture used for debugging mainly

    // a setter for our shader since its a private variable
    public static void setShader(ComputeShader _shader)
    {
        blurComputeShader = _shader;
    }

    // a getter for the debug Render Texture
    public RenderTexture retTex()
    {
        return outputRT;
    }


    // The function to blur a color array with a specified radius
    public Color[] rbgBlur(Color[] _inpArray, int _radius)
    {
        kernelSize = _radius;

        // set important data mapSize and kernel(blur) size to the shader
        int mapSize = (int)Mathf.Sqrt(_inpArray.Length);
        blurComputeShader.SetInt("mapSize", mapSize);
        blurComputeShader.SetInt("KernelSize", kernelSize);

        // create a blank texture for output and send it to the shader
        outputRT = new RenderTexture(mapSize, mapSize, 24);
        outputRT.enableRandomWrite = true;
        outputRT.Create();
        blurComputeShader.SetTexture(0, "OutputTexture", outputRT);


        // create the input and output color buffers and send it to the shader
        ComputeBuffer colInBufferHor = new ComputeBuffer(mapSize * mapSize, sizeof(float) * 4);
        colInBufferHor.SetData(_inpArray);
        // send it on kernel 0 so its for the horizontal pass
        blurComputeShader.SetBuffer(0, "coloursInBufferHor", colInBufferHor);

        ComputeBuffer colOutBufferHor = new ComputeBuffer(mapSize * mapSize, sizeof(float) * 4);
        Color[] outArray = new Color[mapSize * mapSize];
        colOutBufferHor.SetData(outArray);
        blurComputeShader.SetBuffer(0, "coloursOutBufferHor", colOutBufferHor);

     
        // run the horizontal pass with the data
        blurComputeShader.Dispatch(0, mapSize, mapSize, 1);


        colOutBufferHor.GetData(outArray); // get the finalised data out into the output Array


        // set the input buffer with the data from the horizontal pass output
        colInBufferHor.SetData(outArray);
        // set it on kernel 1 so it does a vertical pass
        blurComputeShader.SetBuffer(1, "coloursInBufferHor", colInBufferHor);

        outArray = new Color[mapSize * mapSize]; // clear output buffer now so its ready
        colOutBufferHor.SetData(outArray);
        blurComputeShader.SetBuffer(1, "coloursOutBufferHor", colOutBufferHor);

        // wipe the output debug render texture so we can reuse it
        outputRT = new RenderTexture(mapSize, mapSize, 24);
        outputRT.enableRandomWrite = true;
        outputRT.Create();
        blurComputeShader.SetTexture(1, "OutputTexture", outputRT);

        // dispatch kernel 1 so we do a vertical blur pass
        blurComputeShader.Dispatch(1, mapSize, mapSize, 1);

        // get the final data out now both passes done
        colOutBufferHor.GetData(outArray);

        // release buffers so GPU happy
        colInBufferHor.Release();
        colOutBufferHor.Release();

        // return blurred colour map
        return outArray;
    }

}