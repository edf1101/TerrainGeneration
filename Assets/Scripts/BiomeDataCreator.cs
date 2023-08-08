using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeDataCreator 
{
    private List<biomeDescription> theBiomes;
    private Vector2 biomeIndex;
    private const int blurRad=9;
    private  Vector2 TileSize = new Vector2(100, 100);

    public BiomeDataCreator( List<biomeDescription> _theBiomes, Vector2 _biomeIndex)
    {
        biomeIndex = _biomeIndex;
        theBiomes = _theBiomes;
    }


    private ComputeShader biomeCreateShader;
    private ComputeShader biomeSeperateShader;
    private ComputeShader biomeBlurShader;

    private Dictionary<int, RenderTexture> texByID;

    public void setShaders(ComputeShader _biomeCreate,ComputeShader _biomeSeperate,ComputeShader _biomeBlur)
    {
        biomeCreateShader = _biomeCreate;
        biomeSeperateShader = _biomeSeperate;
        biomeBlurShader = _biomeBlur;
    }

    public void createBiome()
    {
        Vector2 mapSize = TileSize + Vector2.one * blurRad * 2;
        BiomeComputeHelper BCHelper = new BiomeComputeHelper(biomeCreateShader, mapSize, biomeIndex * TileSize - Vector2.one * blurRad, theBiomes);
        BCHelper.createBiomes();
        int[] biomeIndexes = BCHelper.getIDMap();
        int[] biomeInTile = BCHelper.getBiomesFound();

        Dictionary<int, float[]> blursByID = new Dictionary<int, float[]>();
         texByID = new Dictionary<int, RenderTexture>();

        biomeSeperatorHelper BSHelper = new biomeSeperatorHelper(biomeSeperateShader, biomeIndexes, mapSize);
        for (int id = 0; id < biomeInTile.Length; id++) { 
            if (biomeInTile[id] == 1)
            {
                float[] seperated = BSHelper.seperateBiome(id);
                BlurBiomeComputeHelper BBHelper = new BlurBiomeComputeHelper(biomeBlurShader, seperated, blurRad, mapSize);
                float[] blurForID = BBHelper.Blur();
                  blursByID.Add(id, blurForID);
                  texByID.Add(id, BBHelper.getTex());

            }
        }


    }

   public Dictionary<int, RenderTexture> getRenderTex()
    {
        return texByID;
    }

}
