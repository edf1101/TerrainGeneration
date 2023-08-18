using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tileManager : MonoBehaviour
{
    [SerializeField] private Vector2 tilePosition; //what index is this tile

    [SerializeField] private RenderTexture debugRT; // for debugging

    static private Material terrainMaterial; // static Material for use in all tiles

    // a setter for the terrain material
    public static void setTerrainMaterial(Material _terrainMat)
    {
        terrainMaterial = _terrainMat;
    }

    private BiomeDataCreator BDC;

    public void createTile(Vector2 _tilePos)
    {
        // Add the required components to the tile
        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        //Set up Vector2 index and BiomeDataCreator class for this tile
        tilePosition = _tilePos;
        BDC = new BiomeDataCreator(tilePosition);

        // create the biomes for tile
        BDC.createBiome();

        // create the tile mesh
        Mesh tileMesh= BDC.ApplyHeights();

        //Apply mesh and material to tile Components
        GetComponent<MeshRenderer>().material = terrainMaterial;
        GetComponent<MeshFilter>().mesh = tileMesh;

        //debug stuff
        debugRT = BDC.getRenderTex()[0];
    }
}
