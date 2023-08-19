using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq; //so we can use summing arrays 


[Serializable]
public class BiomeDataCreator 
{
    private static List<biomeDescription> theBiomes;  // all biome types
    private Vector2 biomeIndex;  // what postion the biome is ie x=0,y=0 is spawn

    private const int blurRad=19; // radius of blur gets changed here
    private static  Vector2 TileSize = new Vector2(100, 100); // size of tile this should be const but doesnt work with Vector2

    // the delaunay 2d plane gets warped a bit and we need to account for this
    // we also want to account for the space we need to make additional points for colouring
    private const int moreClearance = 15; 

    // constructor to pass semi important variables
    public BiomeDataCreator(  Vector2 _biomeIndex) 
    {
        biomeIndex = _biomeIndex; 

    }

    // getter for tileSize
    public static Vector2 getTileSize()
    {
        return TileSize;
    }

    // getter for the clearance we include
    public static int getMaxDelaunayWarp()
    {
        return moreClearance;
    }

    //shader references
    private static ComputeShader biomeCreateShader;
    private static ComputeShader biomeSeperateShader;
    private static ComputeShader biomeBlurShader;


    // for debug purposes really can find the blur texture for each biome
    private Dictionary<int, RenderTexture> texByID;
    private Dictionary<int, float[]> blursByID;

    // is which biome in this tile?
    private int[] biomeInTile;

   
    // texture of what biomes are where debug purposes
    private RenderTexture myBiomeTex;

    // what biomes are where array
    private int[] biomeIndexes;

    // must be run before creating biome so Compute shaders are set
    public static void setShaders(ComputeShader _biomeCreate,ComputeShader _biomeSeperate,ComputeShader _biomeBlur)
    {
        biomeCreateShader = _biomeCreate;
        biomeSeperateShader = _biomeSeperate;
        biomeBlurShader = _biomeBlur;
    }

    // a getter for returning index map for biomes
    public int[] getBiomeIndexes()
    {
        return biomeIndexes;
    }

    // must be run before creating biomes so biome list is set
    public static void setBiomes(List<biomeDescription> _biomes)
    {
        theBiomes = _biomes;
    }
    // this gets run and creates blur textures for all biomes in tile
    public void createBiome()
    {
        // genrating map size according to blur + tile Size
        Vector2 mapSize = TileSize + Vector2.one * blurRad * 2 + Vector2.one* moreClearance * 2; 

        //create the base biome map
        BiomeComputeHelper BCHelper = new BiomeComputeHelper(biomeCreateShader, mapSize, biomeIndex * TileSize - Vector2.one * blurRad - Vector2.one * moreClearance, theBiomes);
        BCHelper.createBiomes();

        //this texture holds map of what biomes are in this tile
        myBiomeTex = BCHelper.GetColourMap(); // useful for debugging

        biomeIndexes = BCHelper.getIDMap(); // the indexes for each m^2 in the tile
       
       
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

    // a getter for returning Texture map for biomes
    public RenderTexture getBiomeMap()
    {
        return myBiomeTex;
    }
    

    // applies height to the terrain
    public UnityEngine.Mesh ApplyHeights()
    {
        UnityEngine.Mesh planeFlat = PlaneCreator.createPlaneUnity(); // create or reuse plane
       
        Vector3[] verts = planeFlat.vertices;  // get the flat plane vertices

        for(int vertInd=0; vertInd < verts.Length; vertInd++)
        {

            Vector3 vertex = verts[vertInd]; //current vertex

            
            
            //add height val to vertex
            verts[vertInd] += Vector3.up * heightAtPoint(vertex);
        }


        //apply vertices back to mesh
        planeFlat.vertices = verts;
        planeFlat.RecalculateNormals();

        //return mesh at the end

        return planeFlat;
    }


    // This goes through and calcualtes additional point around edges of the terrain
    // required for accuarate colouring in of terrain

    public List<addtionalPoint> calculateAdditionalPoints()
    {
        // create a list of the addtional Points
        List<addtionalPoint> addtionalPoints = new List<addtionalPoint>();


        // left edge

        // Start at the edge of the clearance zone ie -5 and go up to 0 in x direction
        for(int x = -moreClearance + 1; x < 0; x += 3)
        {
            // start at - more clearance and end at map size + more clearance in y direction
            for (int y = -moreClearance + 1; y < TileSize.y + moreClearance; y += 4)
            {
                // find the world coordinates of the main point
                Vector3 startVec = new Vector3(x, 0, y);
                Vector3 mainPoint = startVec+ Vector3.up* heightAtPoint(startVec);

                // find world coordinates of the point to its right and forward of it
                Vector3 rightVec = new Vector3(x + 1, 0, y) + Vector3.up * heightAtPoint(new Vector3(x + 1, 0, y));
                Vector3 upVec = new Vector3(x , 0, y+1) + Vector3.up * heightAtPoint(new Vector3(x , 0, y+1));

                // get the difference between main point and these new points
                // cross product them and we get a normal vector
                Vector3 normal = Vector3.Cross(upVec - mainPoint, rightVec - mainPoint).normalized;

                //add that to the additonal points class and add it to list
                addtionalPoint thisPoint = new addtionalPoint(mainPoint, normal);

                addtionalPoints.Add(thisPoint);
                
            }


        }

        // The rest all work very similarly just in a different area, so for loop
        // parameters slightly different

        // right side
        for (int x = (int)TileSize.x + 1; x < TileSize.x+moreClearance; x += 4)
        {
            for (int y = -moreClearance + 1; y < TileSize.y + moreClearance; y += 3)
            {
                Vector3 startVec = new Vector3(x, 0, y);
                Vector3 mainPoint = startVec + Vector3.up * heightAtPoint(startVec);

                Vector3 rightVec = new Vector3(x + 1, 0, y) + Vector3.up * heightAtPoint(new Vector3(x + 1, 0, y));
                Vector3 upVec = new Vector3(x, 0, y + 1) + Vector3.up * heightAtPoint(new Vector3(x, 0, y + 1));
                Vector3 normal = Vector3.Cross(upVec - mainPoint, rightVec - mainPoint).normalized;

                addtionalPoint thisPoint = new addtionalPoint(mainPoint, normal);

                addtionalPoints.Add(thisPoint);

            }


        }

        // top side
        for (int x = 0; x < (int)TileSize.x; x += 3)
        {
            for (int y = (int)TileSize.y + 1; y < TileSize.y + moreClearance; y += 4)
            {
                Vector3 startVec = new Vector3(x, 0, y);
                Vector3 mainPoint = startVec + Vector3.up * heightAtPoint(startVec);

                Vector3 rightVec = new Vector3(x + 1, 0, y) + Vector3.up * heightAtPoint(new Vector3(x + 1, 0, y));
                Vector3 upVec = new Vector3(x, 0, y + 1) + Vector3.up * heightAtPoint(new Vector3(x, 0, y + 1));
                Vector3 normal = Vector3.Cross(upVec - mainPoint, rightVec - mainPoint).normalized;

                addtionalPoint thisPoint = new addtionalPoint(mainPoint, normal);

                addtionalPoints.Add(thisPoint);

            }


        }

        // bottom side
        for (int x = 0; x < (int)TileSize.x; x += 4)
        {
            for (int y = -moreClearance + 1; y < 0; y += 3)
            {
                Vector3 startVec = new Vector3(x, 0, y);
                Vector3 mainPoint = startVec + Vector3.up * heightAtPoint(startVec);

                Vector3 rightVec = new Vector3(x + 1, 0, y) + Vector3.up * heightAtPoint(new Vector3(x + 1, 0, y));
                Vector3 upVec = new Vector3(x, 0, y + 1) + Vector3.up * heightAtPoint(new Vector3(x, 0, y + 1));
                Vector3 normal = Vector3.Cross(upVec - mainPoint, rightVec - mainPoint).normalized;

                addtionalPoint thisPoint = new addtionalPoint(mainPoint, normal);

                addtionalPoints.Add(thisPoint);

            }


        }


        return addtionalPoints;
    }

    // calculate the index on a map by its vertex
    public static int vertToIndex(Vector3 _pos)
    {
        _pos += new Vector3(1, 0, 1) * blurRad;
        return (int)((int)_pos.z * (TileSize.y + 2 * blurRad + 2 * moreClearance)) + (int)(_pos.x);
    }

    private float heightAtPoint(Vector3 vertex)
    {
        // so it all starts at (0,0,0) needed for referencing from textures/ arrays
        Vector3 correctedVert = vertex + new Vector3(1, 0, 1) * (blurRad + moreClearance);

        Vector3 vertexWorld = vertex + new Vector3(TileSize.x * biomeIndex.x, 0, TileSize.y * biomeIndex.y); //current vertex world position

        // the index that we look at in arrays
        int arrayIndex = (int)((int)correctedVert.z * (TileSize.y + 2 * blurRad + 2 * moreClearance)) + (int)(correctedVert.x);


        //need to find all the biome blur values for the current vertex
        // then store the percentage of each biome in the vector in a dictionary
        // containing only keys where biome blur[vertex]>0

        List<float> biomeValues = new List<float>();
        List<biomeDescription> biomeData = new List<biomeDescription>();
        for (int findBiome = 0; findBiome < biomeInTile.Length; findBiome++) // go through all biomes in blursByID
        {

            // if there is a blur texture for specific biome this means its in scene at least
            if (blursByID.ContainsKey(findBiome))
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

        for (int biomeLook = 0; biomeLook < biomeData.Count; biomeLook++) // we will go through each biome
        {
            // the strength of the biome we're looking at
            float biomeStrength = biomeValues[biomeLook];

            // go through each possible terrain type in said biome
            string[] possTerraintypes = biomeDataArray[biomeLook].terrainGenTypes;
            for (int terrainTypeIndex = 0; terrainTypeIndex < possTerraintypes.Length; terrainTypeIndex++)
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

        /* foreach (KeyValuePair<string, float> dataPair in terrainTypeWeight)
         {
             // part below is commented out because I havent made that script yet but principle works
             heightValue += terrainNoise.GetNoise(vertexWorld, dataPair.Key) * dataPair.Value;
         }*/
        heightValue= terrainNoise.totalNoise(vertexWorld, terrainTypeWeight);
        return heightValue;
    }

    // getter for texByID
    public Dictionary<int, RenderTexture> getRenderTex()
    {
        return texByID;
    }

    // struct for addtional points should contain position and normal vector at a point
    public struct addtionalPoint
    {
        public Vector3 point;
        public Vector3 normal;

        public addtionalPoint(Vector3 _point,Vector3 _norm)
        {
            point = _point;
            normal = _norm;
        }
    }
}
