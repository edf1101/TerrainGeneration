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
    public RenderTexture retTex2;
    public int[] biomeIndx;
    public ComputeShader biomeSeperatorShader;
    public biomeSeperatorHelper BSHelper;


    void Start()
    {
       
        BCHelper = new BiomeComputeHelper(biomeComputeShader,new Vector2(100,100),new Vector2(50,0), theBiomes);
        BCHelper.createBiomes();
        retTex = BCHelper.GetColourMap();
        biomeIndx = BCHelper.getIDMap();

        BSHelper = new biomeSeperatorHelper(biomeSeperatorShader, biomeIndx, new Vector2(100, 100));
        BSHelper.seperateBiome(1);
        retTex2 = BSHelper.getDisplayTex();
    }


    private void generateBiomes()
    {

    }
}
