using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq; //so we can use summing arrays 
using TriangleNet.Geometry;// TriangleNet used for triangulation
using TriangleNet.Topology;

[Serializable]
public class BiomeDataCreator 
{
    private List<biomeDescription> theBiomes;  // all biome types
    private Vector2 biomeIndex;  // what postion the biome is ie x=0,y=0 is spawn

    private const int blurRad=15; // radius of blur gets changed here
    private  Vector2 TileSize = new Vector2(100, 100); // size of tile this should be const but doesnt work with Vector2
    private const int maxDelaunayWarp = 2; // the delaunay 2d plane gets warped a bit and we need to account for this

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

    public RenderTexture testRT;
    public float[] tempArray;

    private RenderTexture myBiomeTex;

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
        Vector2 mapSize = TileSize + Vector2.one * blurRad * 2 + Vector2.one*maxDelaunayWarp*2; 

        //create the base biome map
        BiomeComputeHelper BCHelper = new BiomeComputeHelper(biomeCreateShader, mapSize, biomeIndex * TileSize - Vector2.one * blurRad - Vector2.one *maxDelaunayWarp, theBiomes);
        BCHelper.createBiomes();

        //this texture holds map of what biomes are in this tile
        myBiomeTex = BCHelper.GetColourMap(); // useful for debugging

        int[] biomeIndexes = BCHelper.getIDMap(); // the indexes for each m^2 in the tile
        testRT= BCHelper.GetColourMap();
       
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
                tempArray = blurForID;
                //add the blurred data to dictionaries
                blursByID.Add(id, blurForID);
                texByID.Add(id, BBHelper.getTex());
                
            }
        }


    }

    public RenderTexture getBiomeMap()
    {
        return myBiomeTex;
    }
    

    public UnityEngine.Mesh ApplyHeights()
    {
        UnityEngine.Mesh planeFlat = PlaneCreator.createPlaneUnity(); // create or reuse plane
       
        Vector3[] verts = planeFlat.vertices;  // get the flat plane vertices

        for(int vertInd=0; vertInd < verts.Length; vertInd++)
        {

            Vector3 vertex = verts[vertInd]; //current vertex

            // so it all starts at (0,0,0) needed for referencing from textures/ arrays
            Vector3 correctedVert = vertex + new Vector3(1,0,1)*(blurRad+maxDelaunayWarp);

            Vector3 vertexWorld = vertex + new Vector3(TileSize.x*biomeIndex.x,TileSize.y*biomeIndex.y); //current vertex world position
           
            // the index that we look at in arrays
            int arrayIndex =  (int) ((int)correctedVert.z * (TileSize.y+2*blurRad+2*maxDelaunayWarp)) + (int)(correctedVert.x);


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


            // find which terrain types are required and how much

            //this will hold the weight for each terrain type by type name
            Dictionary<string, float> terrainTypeWeight = new Dictionary<string, float>();

            for(int biomeLook = 0; biomeLook < biomeData.Count; biomeLook++) // we will go through each biome
            {
                // the strength of the biome we're looking at
                float biomeStrength = biomeValues[biomeLook];

                // go through each possible terrain type in said biome
                string[] possTerraintypes = biomeDataArray[biomeLook].terrainGenTypes;
                for (int terrainTypeIndex=0; terrainTypeIndex<possTerraintypes.Length; terrainTypeIndex++)
                {

                    if (terrainTypeWeight.ContainsKey(possTerraintypes[terrainTypeIndex]) == false)
                    {
                        //if the terrainWeight dict doesnt already contain this then add it
                        terrainTypeWeight.Add(possTerraintypes[terrainTypeIndex], biomeStrength);
                    }
                    else
                    {
                        // if its already there then just add to the terrain type weighting
                        terrainTypeWeight[possTerraintypes[terrainTypeIndex]] += biomeStrength;
                    }
                }
            }

            // need to go through each terrain type and sum the noises

            float heightValue = 0; // initial height is 0

            foreach(KeyValuePair<string,float> dataPair in terrainTypeWeight)
            {
                // part below is commented out because I havent made that script yet but principle works
                heightValue += terrainNoise.GetNoise(vertexWorld, dataPair.Key) * dataPair.Value;
            }
            
            //add height val to vertex
            verts[vertInd] += Vector3.up * heightValue;
        }


        //apply vertices back to mesh
        planeFlat.vertices = verts;
        planeFlat.RecalculateNormals();

        //return mesh at the end

        return planeFlat;
    }


    public Dictionary<int, RenderTexture> getRenderTex()
    {
        return texByID;
    }

}
