using System.Collections.Generic;
using UnityEngine;
using System.Linq; //so we can use summing arrays 

public class BiomeDataCreator 
{
    private List<biomeDescription> theBiomes;  // all biome types
    private Vector2 biomeIndex;  // what postion the biome is ie x=0,y=0 is spawn

    private const int blurRad=9; // radius of blur gets changed here
    private  Vector2 TileSize = new Vector2(100, 100); // size of tile this should be const but doesnt work with Vector2


    // constructor to pass semi important variables
    public BiomeDataCreator( List<biomeDescription> _theBiomes, Vector2 _biomeIndex) 
    {
        biomeIndex = _biomeIndex; 
        theBiomes = _theBiomes; 
    }


    //shader references
    private ComputeShader biomeCreateShader;
    private ComputeShader biomeSeperateShader;
    private ComputeShader biomeBlurShader;


    // for debug purposes really can find the blur texture for each biome
    private Dictionary<int, RenderTexture> texByID;
    private Dictionary<int, float[]> blursByID;

    private int[] biomeInTile;

    // must be run before creating biome so Compute shaders are set
    public void setShaders(ComputeShader _biomeCreate,ComputeShader _biomeSeperate,ComputeShader _biomeBlur)
    {
        biomeCreateShader = _biomeCreate;
        biomeSeperateShader = _biomeSeperate;
        biomeBlurShader = _biomeBlur;
    }


    // this gets run and creates blur textures for all biomes in tile
    public void createBiome()
    {
        // genrating map size according to blur + tile Size
        Vector2 mapSize = TileSize + Vector2.one * blurRad * 2; 

        //create the base biome map
        BiomeComputeHelper BCHelper = new BiomeComputeHelper(biomeCreateShader, mapSize, biomeIndex * TileSize - Vector2.one * blurRad, theBiomes);
        BCHelper.createBiomes();


        int[] biomeIndexes = BCHelper.getIDMap(); // the indexes for each m^2 in the tile


        // which biomes are in the terrain ie if biome id 0 is in terrain and
        // 1 isnt the array is: [1,0] <- 1 represents it is in terrain like bool
        biomeInTile = BCHelper.getBiomesFound(); 

        //for each biome id it creates a blur texture / array
      
         texByID = new Dictionary<int, RenderTexture>();
        blursByID = new Dictionary<int, float[]>();
        // create the biome seperator instance
        biomeSeperatorHelper BSHelper = new biomeSeperatorHelper(biomeSeperateShader, biomeIndexes, mapSize);

        //iterate through each possible biome
        for (int id = 0; id < biomeInTile.Length; id++) {

            if (biomeInTile[id] == 1)// if biome is in tile run,  else ignore
            {
                float[] seperated = BSHelper.seperateBiome(id); // seperate it

                // blur the seperated biome texture
                BlurBiomeComputeHelper BBHelper = new BlurBiomeComputeHelper(biomeBlurShader, seperated, blurRad, mapSize);
                float[] blurForID = BBHelper.Blur();

                //add the blurred data to dictionaries
                blursByID.Add(id, blurForID);
                texByID.Add(id, BBHelper.getTex());

            }
        }


    }

    public void ApplyHeights()
    {
        Mesh planeFlat = PlaneCreator.createPlane(); // create or reuse plane
        Vector3[] verts = planeFlat.vertices;  // get the flat plane vertices


        for(int vertInd=0; vertInd < verts.Length; vertInd++)
        {
            Vector3 vertex = verts[vertInd]; //current vertex 
            Vector3 vertexWorld = vertex + new Vector3(TileSize.x*biomeIndex.x,TileSize.y*biomeIndex.y); //current vertex world position

            int arrayIndex = ((int)(vertex.y * TileSize.y)) + (int)(vertex.x);
            //need to find all the biome blur values for the current vertex
            // then store the percentage of each biome in the vector in a dictionary
            // containing only keys where biome blur[vertex]>0

            List<float> biomeValues = new List<float>();
            List<biomeDescription> biomeData = new List<biomeDescription>();
            for (int findBiome = 0; findBiome < biomeInTile.Length; findBiome++) // go through all biomes in blursByID
            {
                // if there is a blur texture for specific biome this means its in scene at least
                
                if (blursByID.ContainsKey(findBiome) ) 
                {
                    float biomePer = blursByID[findBiome][arrayIndex]; // store this and do nested if so we only need to fetch from array once (Speed)
                    if (biomePer != 0)// make sure the value isnt 0 - So we dont bother calculating if nothing there
                    {
                        biomeValues.Add(biomePer);
                        biomeData.Add(theBiomes[findBiome]);
                    }
                }
            }

            float[] biomeValuesArray = biomeValues.ToArray();
            biomeDescription[] biomeDataArray = biomeData.ToArray();
            // we now have 2 Lists holding the unnormalised percentage each tile is a biome and what type
            // need to normalise the percentages (so it adds up to 1)

            float sum = biomeValuesArray.Sum();
            for (int i = 0; i < biomeValuesArray.Length; i++)
                biomeValuesArray[i] = biomeValuesArray[i] / sum;
            //all now normalised

            // store find which terrain types are required



            // if count( terrain type) == length of biomes in vertex then its shared between all
            // we this terrainTypeWeight = 1


            // else get the sum of each terrain type weights across each biome blur


            // apply each type of terrain gen according to weight to get a height val

            //add height val to vertex
        }


        //apply vertices back to mesh


        //return mesh at the end
    }




   public Dictionary<int, RenderTexture> getRenderTex()
    {
        return texByID;
    }

}
