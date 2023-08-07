using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    public ComputeShader biomeComputeShader;
    private BiomeComputeHelper BCHelper;
    public List<biomeDescription> theBiomes;
    public RenderTexture retTex;

    void Start()
    {
        BCHelper = new BiomeComputeHelper(biomeComputeShader,new Vector2(100,100),new Vector2(0,0), theBiomes);
        BCHelper.createBiomes();
        retTex = BCHelper.GetColourMap();

    }


    private void generateBiomes()
    {

    }
}
