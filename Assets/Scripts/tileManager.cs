using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Diagnostics;

public class tileManager : MonoBehaviour
{
    [SerializeField] private Vector2 tilePosition; //what index is this tile

    [SerializeField] private RenderTexture debugRT; // for debugging
    [SerializeField] private Texture2D debugT2D; // for debugging

    static private Material terrainMaterial; // static Material for use in all tiles

    // a setter for the terrain material
    public static void setTerrainMaterial(Material _terrainMat)
    {
        terrainMaterial = _terrainMat;
    }

    private BiomeDataCreator BDC;

    private biomeColourCreator BCC;

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

        
        
        // create additional data points for height and gradient around outside of the mesh
        // needed for colouring in accurately
        List<BiomeDataCreator.addtionalPoint> addPoints= BDC.calculateAdditionalPoints();

        // create a colour creator class instance
        BCC = new biomeColourCreator();
        // set important data made by BiomeDataCreator class
        BCC.setAdditionalPoints(addPoints); // add in additional points
        BCC.setBiomeIndexes(BDC.getBiomeIndexes()); // add in array of which point is which biome

        // add colour to the mesh using vertex painting
        tileMesh= BCC.colouriseMesh(tileMesh);
    
        // set the mesh to the gameobject
        GetComponent<MeshRenderer>().material = terrainMaterial;
        GetComponent<MeshFilter>().mesh = tileMesh;
        GetComponent<MeshCollider>().sharedMesh = tileMesh;


    }
}
