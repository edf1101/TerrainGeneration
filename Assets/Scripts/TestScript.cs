using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    public ComputeShader biomeComputeShader;
    public ComputeShader biomeSeperatorShader;
    public ComputeShader BBComputeShader;

    public List<biomeDescription> theBiomes;

    private BiomeDataCreator BDC;

    public RenderTexture RT1;
    public RenderTexture RT2;

    void Start()
    {
        Stopwatch SW =new  Stopwatch();
        BDC = new BiomeDataCreator(theBiomes, new Vector2(0,0));
        BDC.setShaders(biomeComputeShader, biomeSeperatorShader, BBComputeShader);
        BDC.createBiome();
        SW.Start();

        BDC.ApplyHeights();
        SW.Stop();

        UnityEngine.Debug.Log(SW.ElapsedMilliseconds);
        SW.Reset();
        SW.Start();

        BDC.ApplyHeights();
        SW.Stop();

        UnityEngine.Debug.Log(SW.ElapsedMilliseconds);
        temp = BDC.temp;
    }
    public float[] temp;

    private void generateBiomes()
    {

    }

    private RenderTexture testReturn(Vector2 index,int biomeID)
    {
        BDC = new BiomeDataCreator(theBiomes, index);
        BDC.setShaders(biomeComputeShader, biomeSeperatorShader, BBComputeShader);
        BDC.createBiome();
        return BDC.getRenderTex()[biomeID];
    }
}
