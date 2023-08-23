using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Diagnostics;

public class tileManager : MonoBehaviour
{
    [SerializeField] private Vector2 tilePosition; //what index is this tile

    [SerializeField] private RenderTexture debugRT; // for debugging
    [SerializeField] private Texture2D debugT2D; // for debugging

    private static List<biomeDescription> theBiomes;
    static private Material terrainMaterial; // static Material for use in all tiles

    // a setter for the terrain material
    public static void setTerrainMaterial(Material _terrainMat)
    {
        terrainMaterial = _terrainMat;
    }

    private BiomeDataCreator BDC;

    private biomeColourCreator BCC;
    private objectPlacement OP;



    public void createTile(Vector2 _tilePos)
    { 

        // Add the required components to the tile
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<MeshCollider>();

        //Set up Vector2 index and BiomeDataCreator class for this tile
        tilePosition = _tilePos;
        BDC = new BiomeDataCreator(tilePosition);

        // create the biomes for tile
        BDC.createBiome();

        // create the tile mesh

        // apply heights to the mesh
        Mesh tileMesh= BDC.ApplyHeights();
        biomeArray = BDC.getBiomeIndexes();
        
        
        // create additional data points for height and gradient around outside of the mesh
        // needed for colouring in accurately
        List<BiomeDataCreator.addtionalPoint> addPoints= BDC.calculateAdditionalPoints();

        // create a colour creator class instance
        BCC = new biomeColourCreator();
        // set important data made by BiomeDataCreator class
        BCC.setAdditionalPoints(addPoints); // add in additional points
        BCC.setBiomeIndexes(biomeArray); // add in array of which point is which biome

        // add colour to the mesh using vertex painting
        tileMesh= BCC.colouriseMesh(tileMesh);
    
        // set the mesh to the gameobject
        GetComponent<MeshRenderer>().material = terrainMaterial;
        GetComponent<MeshFilter>().mesh = tileMesh;
        GetComponent<MeshCollider>().sharedMesh = tileMesh;


        // create object holder
        GameObject objHolder = new GameObject();
        objHolder.transform.parent = transform;
        objHolder.name = "Object Holder";


        OP = new objectPlacement(BCC.getAcceptables());
        OP.setTileIndex(tilePosition);
        OP.setObjectHolder(objHolder.transform);
        OP.setBiomeArray(BDC.getBiomeIndexes());
        OP.createObjects();
        debugRT = BDC.getBiomeMap();

        setupWeather();

    }


    private int[] biomeArray; // hold indexes of biomes in this tile


    // getter for finding the biome at a specific place in this tile
    public biomeDescription getBiomeAt(Vector2 pos)
    {

        // biome Array is different size to the tile so calculate offsets here
        int arrWidth = (int)Mathf.Sqrt(biomeArray.Length);
        int offset =(int)( 0.5f * (arrWidth - 100));

        // index of biome in the array
        int biomeIndx = (((int)pos.y + offset) * arrWidth) + (int)pos.x + offset;
        return theBiomes[biomeArray[biomeIndx]]; // return biome

    }

    // setter for setting private list of biomes in world
    public static void setBiomes(List<biomeDescription> _biomes)
    {

        theBiomes = _biomes;
    }


    private static GameObject weatherPrebab; // prefab for weather tile

    public static void setWeatherPrebab(GameObject _wp) // setter for above 
    {
        weatherPrebab = _wp;
    }


    private GameObject weatherTileHolder; // transform holds the weather tiles in heirachy

    // array to reference all weather tiles
    private weatherTile[] weatherTiles = new weatherTile[100]; 


    // set up the weather tiles on start
    private void setupWeather()
    {

        int weatherTileSize = 10; // size of the particle system

        // create the weather tile holder to be organised
        weatherTileHolder = new GameObject();
        weatherTileHolder.name = "Weather Holder";
        weatherTileHolder.transform.position= new Vector3( tilePosition.x * 100, 0, tilePosition.y * 100);
        weatherTileHolder.transform.parent = transform;

        int count = 0; // keep a count so we can easily put the 2d items in a 1d array
        for(int x = 0; x < 100; x += weatherTileSize)
        {
            for (int y = 0; y < 100; y += weatherTileSize)
            {

                GameObject temp = Instantiate(weatherPrebab); //instantiate prefab
                temp.transform.parent = weatherTileHolder.transform; // assign to weatherTileHolder heirachy

                //calculate and set postion for it
                Vector3 pos = new Vector3(x + tilePosition.x * 100, 60, y + tilePosition.y * 100);
                temp.transform.position = pos;

                // initialise its script
                temp.GetComponent<weatherTile>().newLocation(pos);
                weatherTiles[count]=temp.GetComponent<weatherTile>();

                count++;
            }

        }
    }


    // functions to return whether its raining/ snowing or not

    // this one returns the average for the whole tile
    public bool getPrecipating()
    {
        float prec = 0;
        for(int i = 0; i < weatherTiles.Length; i++) // go through each weatherTile
        {
            prec += (weatherTiles[i].getState() ? 1 : 0);

        }
        prec /= (float)weatherTiles.Length; // average if its raining or not

        return prec>0.5f;
    }

    // get whether its raining in exact part of tile
    public bool getPrecipating(Vector2 pos) 
    {
        int ind = ((int)pos.y) + (int)(pos.x / 10);
        return weatherTiles[ind].getState();

    }

    private static Transform playerTransform; // reference to player transform

    public static void setPlayerTransform(Transform _p) // setter for above
    {
        playerTransform = _p;
    }

    private float lastCheck; // so we can check efficiently every n sec

    private void Update()
    {
        // check each 2s or on start as long as weatherTiles have been made
        if ((lastCheck<2 || Time.time - lastCheck > 2) && weatherTileHolder != null)
        {
            lastCheck = Time.time;

            // This part is renderDist essentailly particle systems can be
            // expensive so we only want them when theyre near

            // check if the tile is within range then enable/ disable
            if (Vector3.Distance(playerTransform.position, transform.position + new Vector3(50, 15, 50)) < 200)
            {
                weatherTileHolder.SetActive(true);
            }
            else
                weatherTileHolder.SetActive(false);
        }
    }
}
