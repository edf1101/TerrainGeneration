using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/* 
 * Code by Ed F
 * www.github.com/edf1101
 */

public class objectPlacement 
{
    // array containing all the distributed points we'll make it static so we
    // only need to calculate once
    private static Vector2[] treePoints; // array for trees
    private static Vector2[] objectPoints; // array for other object, rocks, flowers, grass

    private Vector2 tileSize = new Vector2(100, 100); // size of the map

    private static Vector2 heightRange = new Vector2(1.5f, 30);
    private static float maxGradient = 0.2f;

    // array saying where I can place objects green = good , red = bad
    private Color[] acceptablePositions;

    // need an array to store what biome each tile is
    private int[] biomeArray;

    // So we can find out what biome each index refers 
    private static List<biomeDescription> theBiomes;

    public static void setBiomes(List<biomeDescription> _theBiomes) // setter for above
    {
        theBiomes = _theBiomes;
    }


    private Transform objectHolder; // holds the objects 

    public void setObjectHolder(Transform _objHolder) // setter for above
    {
        objectHolder = _objHolder;
    }


    // sizes and offset as this isnt 100*100 as it has clearance so we can use this
    // data to convert to what we want
    private int biomeMapOffset;
    private int biomeMapSize;

    public void setBiomeArray(int[] _biomeArray) // setter for above private variable
    {
        biomeArray = _biomeArray;
        biomeMapSize = (int)Mathf.Sqrt(biomeArray.Length);
        biomeMapOffset = (biomeMapSize - (int)tileSize.x)/2;
    }

    private Vector2 tileIndex;

    public void setTileIndex(Vector2 _tileIndex)
    {
        tileIndex = _tileIndex;
    }

    private RenderTexture debugTex; // debug texture

    private static LayerMask onlyTerrainLayerMask;

    public objectPlacement(Color[] _acceptablePos) // constructor for class
    {
        acceptablePositions = _acceptablePos;

        // There will be a lot of gaps in this map so we can use our fill gaps
        // shader to help us out and fill it in
        FillGapsComputeHelper FCG = new FillGapsComputeHelper();
        acceptablePositions= FCG.FillGaps(acceptablePositions);
        debugTex= FCG.returnTexture();

        onlyTerrainLayerMask = LayerMask.GetMask(LayerMask.LayerToName(TerrainManager.getTerrainLayer()));
       
    }

    // generate these points will get run on start then reused
    public static void createPoissonPoints() { 
        treePoints=FastPoissonDiskSampling.Sampling(Vector2.zero, 100 * Vector2.one, 2).ToArray();
        objectPoints=FastPoissonDiskSampling.Sampling(Vector2.zero, 100 * Vector2.one, 0.5f,3).ToArray();

        // shuffle the arrays so we can look at 1st 30% of points and still random
        treePoints = shuffle(treePoints);
        objectPoints = shuffle(objectPoints);

        // set the threshold data to the biomeColourCreator
        biomeColourCreator.setObjectPlacementThresholds(heightRange, maxGradient);

    }

    // Shuffle an array of Vector2's
    private static Vector2[] shuffle(Vector2[] _input)
    {
        // store the array length so we dont have to keep going back
        int inpLength = _input.Length;

        // Knuth shuffle algorithm
        for(int index = 0; index < inpLength; index++)
        {
            int swap = Random.Range(0, inpLength);
            Vector2 temp = _input[index];
            _input[index] = _input[swap];
            _input[swap] = temp;

        }

        return _input; // return the shuffled array
    }

    public RenderTexture getDebugTex()
    {
        return debugTex;
    }


    public void createObjects()
    {
        // Tree objects first
        for(int i = 0; i < treePoints.Length; i++) // go through each point
        {
            // get biome array index at this point and acceptable array index
            // first need to calculate position in vector form
            Vector2 orgPoint = treePoints[i];
            Vector2 point = orgPoint + new Vector2(biomeMapOffset, biomeMapOffset);
           
            int index = (int)((int)point.y * biomeMapSize) + (int)point.x;
            
            int acceptablesIndex = (int)((int)orgPoint.y * tileSize.x) + (int)orgPoint.x;
            Color acceptable = acceptablePositions[acceptablesIndex];

            biomeDescription myBiome = theBiomes[biomeArray[index]]; // get current biome
            Vector3 searchPos = new Vector3(orgPoint.x + tileIndex.x * tileSize.x, 100, orgPoint.y + tileSize.y * tileIndex.y);

            // if acceptable object position, and its not in clearing and then have random chance of using position
            if (acceptable == Color.green && Random.value < myBiome.treePercentage && terrainNoise.GetNoise(searchPos, "Tree Map")>myBiome.clearingThresh)
            {
                RaycastHit hit; // holds hit data
               
                if( Physics.Raycast(searchPos, Vector3.down, out hit, 110, onlyTerrainLayerMask))
                {
                    Vector3 actualPoint = hit.point;

                    // create object of where this tree is
                    objectDataLight thisObject= new objectDataLight(actualPoint, myBiome.treeObjects[Random.Range(0,myBiome.treeObjects.Length-1)].name);
                    GameObject temp = TerrainManager.doInstanstiate(myBiome.treeObjects[Random.Range(0, myBiome.treeObjects.Length - 1)]);
                    temp.transform.position = thisObject.objectPosition;
                    // have extra tree height so we can make forests for example have taller trees
                    temp.transform.localScale = thisObject.objectScale+Vector3.up* myBiome.extraTreeHeight;
                    temp.transform.localEulerAngles = thisObject.rotation;
                    temp.transform.parent = objectHolder;
                }
            }
        }
    }

    // lighter version of transform class that holds pos, rot ,scale
    private struct objectDataLight
    {
        public Vector3 objectPosition;
        public Vector3 objectScale;
        public Vector3 rotation;
        public string objectName;

        public objectDataLight(Vector3 _pos,string _objectName)
        {
            objectPosition = _pos;
            objectScale = new Vector3(Random.Range(0.9f, 1.1f), Random.Range(0.9f, 1.1f), Random.Range(0.9f, 1.1f));
            rotation = new Vector3(0,Random.Range(0, 360),0);
            objectName = _objectName;
        }
    }

}
