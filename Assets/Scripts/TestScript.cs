using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    public ComputeShader biomeComputeShader;
    public ComputeShader biomeSeperatorShader;
    public ComputeShader BBComputeShader;

    public List<biomeDescription> theBiomes;

    private BiomeDataCreator BDC;

    public List<terrainNoisePreset> noisePresets;

    public RenderTexture thisBiomeSet;
    public Vector2 mapCood;
    void Start()
    {
        Stopwatch SW = new Stopwatch();
        SW.Start();
        terrainNoise.setNoisePresets(noisePresets);
        BDC = new BiomeDataCreator(theBiomes, mapCood);
        BDC.setShaders(biomeComputeShader, biomeSeperatorShader, BBComputeShader);
        BDC.createBiome();
      
        

        Mesh newMesh = BDC.ApplyHeights();
       
        
        GetComponent<MeshFilter>().mesh = newMesh;
        SW.Stop();
        UnityEngine.Debug.Log(SW.ElapsedMilliseconds);
        thisBiomeSet = BDC.getBiomeMap();
       
    }
    public float[] temp;
    
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
