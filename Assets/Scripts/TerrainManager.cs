using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [Header("Transform References")]
    // reference to the player's transform so we can get its location and create
    // nearby tiles
    [SerializeField] private Transform playerTransform;

    // Transform so we know which gameobject to put the new tiles into 
    // in the heirachy
    [SerializeField] private Transform tileHolder;

    // A reference to each of the compute shaders neeeded;
    [Header("Compute Shader References")]
    [SerializeField] private ComputeShader biomeComputeShader;
    [SerializeField] private ComputeShader biomeSeperatorShader;
    [SerializeField] private ComputeShader BBComputeShader;
    [SerializeField] private ComputeShader FillGapsShader;
    [SerializeField] private ComputeShader rgbBlurShader;

    [Header("Biome/Noise References")]
    // Also reference each biome Type
    [SerializeField] private List<biomeDescription> theBiomes;

    // And reference each noise type required
    [SerializeField] private List<terrainNoisePreset> noisePresets;


    [Header("Other Settings")]
    // The terrain needs a specific material that uses vertex colours
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private bool debugging;

    // This dictionary holds whether a tile is made for each vector 2
    // no point in the bool really as if the key exists it will be true
    // we use it by doing tilesFound.ContainsKey(pos)
    private Dictionary<Vector2, bool> tilesFound = new Dictionary<Vector2, bool>();

    // this shouldnt ever be changed but nice to be a variable in case it is!
    private const int mapSize=100;

    private Vector2 lastTile; // the last tile the player was in

    // This dictionary holds a reference to each tile by its index
    // useful so we can find them all quickly at a later point
    private Dictionary<Vector2, GameObject> tileReferences = new Dictionary<Vector2, GameObject>();

    //Gets run first
    private void Start()
    {
        //set up rgb blur shader
        rgbBlurComputeHelper.setShader(rgbBlurShader);

        //Set up fillGaps shader
        FillGapsComputeHelper.setShader(FillGapsShader);

        // set the noise presets for the static class firstly
        terrainNoise.setNoisePresets(noisePresets);

        // set the default shaders + biomes statically for biomeDataCreatorClass
        BiomeDataCreator.setShaders(biomeComputeShader, biomeSeperatorShader, BBComputeShader);
        BiomeDataCreator.setBiomes(theBiomes);
        //set the biomes for the colour script too
        biomeColourCreator.setBiomes(theBiomes);

        //set the terrain material statically
        tileManager.setTerrainMaterial(terrainMaterial);
    }


    // Update is called once per frame
    private void Update()
    {
        // calculate the current tile we are in
        Vector2 currentTile = new Vector2(Mathf.FloorToInt(playerTransform.position.x / mapSize), Mathf.FloorToInt( playerTransform.position.z / mapSize));

        // if the tile has changed recently or if its (0,0) as this doesnt register on start
        if (currentTile != lastTile || currentTile== Vector2.zero) 
        {
            lastTile = currentTile;
            if (!tilesFound.ContainsKey(currentTile)) 
            {
                // we found an unmade tile lets make it
                createTile(currentTile);
                tilesFound.Add(currentTile, true);
            }
        }
    }

    private void createTile(Vector2 _mapPosition)
    {
        // little debugging thing to show when creatign new tile from scratch
        if (debugging) 
            Debug.Log("Generating Tile: " + _mapPosition);

        // create a new gameobject for the new tile
        GameObject newTile = new GameObject();

        string tileName= "Tile: (" + _mapPosition.x.ToString() + "," + _mapPosition.y.ToString() + ")";
        newTile.name = tileName; // set name of new tile so we can understand easily

        newTile.transform.parent = tileHolder; // assign its parent
        // set it to its new world position
        newTile.transform.position = new Vector3(_mapPosition.x, 0, _mapPosition.y) * mapSize;

        //add then run tileManager component
        newTile.AddComponent<tileManager>();
        newTile.GetComponent<tileManager>().createTile(_mapPosition);
        tileReferences.Add(_mapPosition, newTile);
    }

}
