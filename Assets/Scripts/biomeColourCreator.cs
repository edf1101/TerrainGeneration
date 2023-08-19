using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class biomeColourCreator 
{

    // Threshhold data for what we consider to be special terrain
    private const float highThresh = 40;
    private const float lowThresh = 1;
    private const float steepThresh = 0.5f;
    private const float slightThresh = 0.2f;

    // this is a colour map of the special colours eg steep slopes, high point
    // basically anything except normal ground
    private Color[] specialPartialMap;

    // the max blur radius we can use, the limit is 
    private int blurRadius; 

    //how large the texture is in each dimension ,calculated later
    private int mapSize;

    static private List<biomeDescription> theBiomes; // list of all biome types

    // a setter for private attribute theBiomes
    public static void setBiomes(List<biomeDescription> _biomes)
    {
        theBiomes = _biomes;
    }

    // a list of all the additional points, ie points sampled outside of the tile
    // manually 
    private List<BiomeDataCreator.addtionalPoint> additionalPoints;

    // a setter for additional pointss
    public void setAdditionalPoints(List<BiomeDataCreator.addtionalPoint> _addPoints)
    {
        additionalPoints = _addPoints;
    }


    private int[] biomeIndexes; // what biome each tile is calculated in BiomeDataCreator
    public void setBiomeIndexes(int[] _biomeIndexes) // a setter for above
    {
        biomeIndexes = _biomeIndexes;
    }

    // this creates Actual colors for the special points( high ,steep etc) in the map
    // although it only creates the points at a vertex or addtional point so lots of
    // blank space in color map hence only partial
    private void createSpecialPartials(Mesh _tileMesh)
    {

        // get the mesh data from the old mesh
        Vector3[] verts = _tileMesh.vertices;
        int[] tris = _tileMesh.triangles;
        Vector3[] normals = _tileMesh.normals;

        // get mapsize and blurRadius from biomeDataCreator and set our variables to them
        mapSize = (int)BiomeDataCreator.getTileSize().x;
        blurRadius = BiomeDataCreator.getMaxDelaunayWarp();


        int blurTexSize = mapSize + 2 * blurRadius; // calculate the size of our color maps

        //initialise the color map as all black
        specialPartialMap = new Color[blurTexSize * blurTexSize];




        // go through each vertex
        for (int vertIndex = 0; vertIndex < verts.Length; vertIndex++)
        {
            Vector3 currentVert = verts[vertIndex];
            // add slight offset so the (0,0) points align
            Vector3 actualPos = currentVert + new Vector3(1, 0, 1) * (blurRadius);

            // get the data required to decide what kind of colour it should be
            float HeightVal = currentVert.y;

            //this is really the gradient done by dotting Up vector with normal
            // higher values are steeper, between (0,1) as normalised
            float normalVal = 1 - Mathf.Abs(Vector3.Dot(Vector3.up, normals[vertIndex]));

            // get the colour from the data provided
            Color biomeCol = colorFromData(HeightVal, normalVal, actualPos);

            // and set that colour in the array
            specialPartialMap[(Mathf.RoundToInt(actualPos.z) * blurTexSize) + Mathf.RoundToInt(actualPos.x)] = biomeCol;

        }

        // convert the additional points into an array for speed
        BiomeDataCreator.addtionalPoint[] addtionalPointsArray = additionalPoints.ToArray();

        // go through each additonal point
        for (int i = 0; i < addtionalPointsArray.Length; i++)
        {
            BiomeDataCreator.addtionalPoint currPoint = addtionalPointsArray[i];

            // Get the position and add correcting offset
            Vector3 mainPoint = currPoint.point + new Vector3(1, 0, 1) * blurRadius;

            // get height + gradient data
            float HeightVal = mainPoint.y;
            float normalVal = 1 - Mathf.Abs(Vector3.Dot(Vector3.up, currPoint.normal));

            //decide colour for pixel and set that in the map
            Color biomeCol = colorFromData(HeightVal, normalVal, mainPoint);
            specialPartialMap[(Mathf.RoundToInt(mainPoint.z) * blurTexSize) + Mathf.RoundToInt(mainPoint.x)] = biomeCol;
         
        }

    }


    // creates a backing texture of all the normal colours
    // based on biome solely
    private Color[] normalBackingColors() 
    {
        int blurTexSize = mapSize + 2 * blurRadius; // calculate texture size
       
        Color[] cols = new Color[blurTexSize * blurTexSize]; // blank array

        for(int y=0;y<blurTexSize;y++)
        {

            for (int x = 0; x < blurTexSize; x++) // go through each xy coordinate
            {
                // get the position currently and add offset correction
                Vector3 _pos = new Vector3(x,0,y) + new Vector3(1, 0, 1) * blurRadius;

                //get the biome we must be in
                biomeDescription myBiome = theBiomes[biomeIndexes[BiomeDataCreator.vertToIndex(_pos)]];

                //add that current biome to the color map
                cols[(y * blurTexSize) + x] = myBiome.normalColour.Evaluate(Random.Range(0,1));

            }
        }
     
        return cols; // return made color map
    }


    // this combines the two colour maps for normal backing and additonal colours
    private Color[] combine(Color[] backing, Color[] addtional)
    {
        for(int i = 0; i < backing.Length; i++) // go through each pixel in map
        {

            // if a additional pixel isnt empty this gets priority and we should
            // swap them over
            if ((addtional[i].r==0&& addtional[i].g==0 && addtional[i].b==0)==false)
            {
                backing[i] = addtional[i]; // do the swap
            }
        }
        return backing; // return combined colour map
    }


    /* returns the mesh with correct vertex colours based on height/ gradient/ biome
     *  The challenge is that we want very large radius blur edges between biomes 
     *  and we want to have visible colour changes for steep, high triangles too
     * but we want these to be blurred but only slightly
     * 
     * So 
     * 1. we create a base biome texture
     * 2. blur it lots
     * 3. add special points on top making them prevalent
     * 4. blur it again just a little
     */

    public Mesh colouriseMesh(Mesh _tileMesh) {

        createSpecialPartials(_tileMesh); // create the partial map of only special colours

        // This compute shader will make the partial map more prevalent
        // ie expand the pixels of special areas take up more space
        FillGapsComputeHelper FCG = new FillGapsComputeHelper();

        Color[]  specialPartials = FCG.FillGaps(specialPartialMap); // returns the more prevalent map

        // create a colour texture of Backing colours
        Color[] backings = normalBackingColors();

        // This compute shader blurs color maps
        rgbBlurComputeHelper RGBlur = new rgbBlurComputeHelper();

        // blur the backing map with a 21 radius
        backings = RGBlur.rbgBlur(backings, 21);

        // combine the new blurred backings with special partials
        Color[] combined =combine(backings, specialPartials);


        // blur the combined map slightly
        Color[] finalMap = RGBlur.rbgBlur(combined, 9);

        // make it so no vertices are shared this makes the vertex shading look 
        // much nicer as the whole triangle is same colour flat shaded
        _tileMesh = doubleVerts(_tileMesh);

        // add the final Colours to the new mesh
        _tileMesh = addVertexCols(_tileMesh, finalMap);

        // return the colourised mesh
        return _tileMesh;
    }


    // Add vertex colours to the mesh
    private Mesh addVertexCols(Mesh oldMesh, Color[] _cols)
    {
        int mapSizeTex = mapSize + 2 * blurRadius; // calculate map size


        Vector3[] verts = oldMesh.vertices; // get the old vertices
        Color[] vertCols = new Color[oldMesh.vertexCount]; // create new array for colours

        for(int i = 0; i < vertCols.Length/3; i++)  // go through each triangle
        {

            // this is the first vertex of each triangle
            Vector3 currentVert = verts[i*3]+ new Vector3(1,0,1)*(blurRadius);

            // calculate the colour map index of that vertex
            int index = (mapSizeTex * (int)currentVert.z) + (int)currentVert.x;

            Color col = _cols[index]; // get the colour in that index

            vertCols[(i * 3) + 0] = col; // apply that colour to whole triangle
            vertCols[(i * 3) + 1] = col;
            vertCols[(i * 3) + 2] = col;
        }

        oldMesh.colors = vertCols; // update colours and return mesh
        return oldMesh;

    }


    // To make all triangles have all vertices the same colour we need to make
    // sure there are no shared vertices. This makes sure that happens.

    private Mesh doubleVerts(Mesh orgMesh)
    {
        // create blank arrays for new Data, as each triangle has 3 unique vertices
        // tris will be same length as verts
        int[] tris = new int[orgMesh.triangles.Length];
        Vector3[] verts = new Vector3[orgMesh.triangles.Length];

        // get original mesh data
        Vector3[] oldVerts = orgMesh.vertices;
        int[] oldTris = orgMesh.triangles;

        // go through old tris array
        for(int i = 0; i < oldTris.Length; i++)
        {
            Vector3 lookVert = oldVerts[oldTris[i]]; // find the vertex its referring to
            verts[i] = lookVert; // put it as its own unique vertex in new mesh
            tris[i] = i;
        }

        // put new tri and vert data into new array
        Mesh newMesh = new Mesh();
        newMesh.vertices = verts;
        newMesh.triangles = tris;
        
        newMesh.RecalculateNormals();

        return newMesh; // return new mesh
    }

    // calculate from given data whether a position is special, if so what colour
    // should it be , if not special return black
    // The evaluate stuff is because we feed it a gradient of acceptable colours
    // and it chooses a random one of those 
    private Color colorFromData(float _height,float _gradient, Vector3 _pos)
    {
        //get biome pixel is currently in
         biomeDescription myBiome =theBiomes[ biomeIndexes[BiomeDataCreator.vertToIndex(_pos)]] ;

        if (_gradient > steepThresh) // steep terrain
            return myBiome.steepColour.Evaluate(Random.Range(0f, 1f));

        else if (_height > highThresh) // high terrain
            return myBiome.highColour.Evaluate(Random.Range(0f, 1f));

        else if (_height < lowThresh) // low terrain
            return myBiome.lowColour.Evaluate(Random.Range(0f, 1f));

        else if (_gradient > slightThresh) // slightly steep terrain
            return myBiome.steepSomewhatColour.Evaluate(Random.Range(0f, 1f));

        else
            return Color.black;
     }


}
