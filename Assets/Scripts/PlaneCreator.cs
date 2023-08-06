using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;// used for stopwatch

using TriangleNet.Geometry;// TriangleNet used for triangulation
using TriangleNet.Topology;


public class PlaneCreator : MonoBehaviour
{
    
    Stopwatch SW = new Stopwatch(); // create a stopwatch so we can time things for efficiency


    void Start()
    {
        SW.Start();// stopwatch start timing
    
        SW.Stop();

        UnityEngine.Debug.Log(SW.ElapsedMilliseconds); // print to console time elapsed


    }

    void createPlane()
    {
        Vector2[] pointsIn = getPoints();

      
        Polygon polygon = new Polygon();

        // Add uniformly-spaced points

        for (int sampleNum = 0; sampleNum < pointsIn.Length; sampleNum++)
        {
            polygon.Add(new Vertex((double)pointsIn[sampleNum].x, (double)pointsIn[sampleNum].y));
        }


    }

    private Vector2[] getPoints() // takes ~15ms on MBPro in editor
    {

        List<Vector2> pdsPoints= FastPoissonDiskSampling.Sampling(new Vector2(0, 0), new Vector2(100, 100), 4); // creates list of points sampled with PDS

        // need to add outline, add points at an interval of 4m along the edge
        for(int along = 0; along <= 100; along += 4)
        {
            pdsPoints.Add(new Vector2(0, along));
            pdsPoints.Add(new Vector2(along, 0));
            pdsPoints.Add(new Vector2(100, along));
            pdsPoints.Add(new Vector2(along, 100));

        }


        Vector2[]pdsArray = pdsPoints.ToArray(); // convert the list to an array for better speed

        return pdsArray;
    }
}
