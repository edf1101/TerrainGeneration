using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        RT1 = testReturn(new Vector2(0, 0),0);
        RT2 = testReturn(new Vector2(0, 0),1);

        //  PlaneCreator.createPlane();
       


    }


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
