using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeDataCreator 
{
    private List<biomeDescription> theBiomes;
    private Vector2 biomeIndex;
    private const int blurRad=5;
    private  Vector2 TileSize = new Vector2(100, 100);

    public BiomeDataCreator( List<biomeDescription> _theBiomes, Vector2 _biomeIndex)
    {
        biomeIndex = _biomeIndex;
        theBiomes = _theBiomes;
    }


    private ComputeShader biomeCreateShader;
    private ComputeShader biomeSeperateShader;
    private ComputeShader biomeBlurShader;

    public void setShaders(ComputeShader _biomeCreate,ComputeShader _biomeSeperate,ComputeShader _biomeBlur)
    {
        biomeCreateShader = _biomeCreate;
        biomeSeperateShader = _biomeSeperate;
        biomeBlurShader = _biomeBlur;
    }

    public void createBiome()
    {

        BiomeComputeHelper BCHelper = new BiomeComputeHelper(biomeCreateShader, TileSize + Vector2.one * blurRad * 2, biomeIndex * TileSize - Vector2.one * blurRad, theBiomes);
        BCHelper.createBiomes();
        int[] biomeIndexes = BCHelper.getIDMap();
        int[] biomeInTile = BCHelper.getBiomesFound();

       /* foreach(int id in biomeInTile)
        {

        }



        /*
        BCHelper = new BiomeComputeHelper(biomeComputeShader, new Vector2(100, 100), new Vector2(50, 0), theBiomes);
        BCHelper.createBiomes();
        retTex = BCHelper.GetColourMap();
        biomeIndx = BCHelper.getIDMap();

        BSHelper = new biomeSeperatorHelper(biomeSeperatorShader, biomeIndx, new Vector2(100, 100));
        seps = BSHelper.seperateBiome(1);
        retTex2 = BSHelper.getDisplayTex();


        BBHelper = new BlurBiomeComputeHelper(BBComputeShader, seps, 5, new Vector2(100, 100));
        BBHelper.Blur();
        retTex3 = BBHelper.getTex();*/
    }
}
