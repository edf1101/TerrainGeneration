using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    public ComputeShader biomeComputeShader;
    private BiomeComputeHelper BCHelper;

    void Start()
    {
        BCHelper = new BiomeComputeHelper();
        BCHelper.setComputeShader(biomeComputeShader);

    }


    private void generateBiomes()
    {

    }
}
