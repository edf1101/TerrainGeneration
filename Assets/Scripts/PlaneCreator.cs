using System.Collections.Generic;
using UnityEngine;
//using System.Diagnostics;// used for stopwatch
using TriangleNet.Geometry;// TriangleNet used for triangulation
using TriangleNet.Topology;


public class PlaneCreator 
{

    static UnityEngine.Mesh preMesh;


    public static UnityEngine.Mesh createPlane() 
    {
        if (preMesh == null)
        {
            Vector2[] pointsIn = getPoints(); // get PDS points 
            Debug.Log("remake");
            Polygon polygon = new Polygon(); // create a polygon for Triangle Library and add PDS points to it
            for (int sampleNum = 0; sampleNum < pointsIn.Length; sampleNum++)
            {
                polygon.Add(new Vertex((double)pointsIn[sampleNum].x, (double)pointsIn[sampleNum].y));
            }

            // turn the polygon struct into a triangulation Type: TriangleNet.Mesh
            TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
            TriangleNet.Mesh triangleMesh = (TriangleNet.Mesh)polygon.Triangulate(options);

            TriangleBin bin = new TriangleBin(triangleMesh, 100, 100, pointsIn.Length);

            // need to convert TriangleNetMesh into Unity Mesh
            UnityEngine.Mesh unityMesh = MakeMesh(triangleMesh);
            preMesh = unityMesh;
        }

        return preMesh;
        


    }


    private static UnityEngine.Mesh MakeMesh(TriangleNet.Mesh _mesh)
    {
        // code taken and modified from https://github.com/Chaosed0/DelaunayUnity/

        IEnumerator<Triangle> triangleEnumerator = _mesh.Triangles.GetEnumerator();

       // create lists to add mesh tris and verts to before we put them into a unity mesh
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

        for (int chunkStart = 0; chunkStart < _mesh.Triangles.Count; chunkStart += 1)
        {
            if (triangleEnumerator.MoveNext())// if theres another triangle to look at then move to it
            {

                // look at the current triangle and add it to unity format
                Triangle triangle = triangleEnumerator.Current;

                // For the triangles to be right-side up, they need
                // to be wound in the opposite direction

                Vector3 v0 = ApplyWaves(GetPoint3D(_mesh, triangle.vertices[2].id));
                Vector3 v1 = ApplyWaves(GetPoint3D(_mesh, triangle.vertices[1].id));
                Vector3 v2 = ApplyWaves(GetPoint3D(_mesh, triangle.vertices[0].id));

                triangles.Add(vertices.Count);
                triangles.Add(vertices.Count + 1);
                triangles.Add(vertices.Count + 2);

                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);


            }
        }

        UnityEngine.Mesh unityMesh = new UnityEngine.Mesh(); // convert to unity format
        unityMesh.vertices = vertices.ToArray();
        unityMesh.triangles = triangles.ToArray();
        unityMesh.RecalculateNormals();

        return unityMesh;
    }


    private static Vector3 GetPoint3D(TriangleNet.Mesh _mesh,int index)
    {
        // code taken and modified from https://github.com/Chaosed0/DelaunayUnity/

        Vertex vertex = _mesh.vertices[index];
        float elevation = 0;    //elevations[index];
        return new Vector3((float)vertex.x, elevation, (float)vertex.y);
    }

   
   

    private static Vector2[] getPoints() 
    {

        List<Vector2> pdsPoints= FastPoissonDiskSampling.Sampling(new Vector2(1, 1), new Vector2(99, 99), 2); // creates list of points sampled with PDS
        // these poitns are at least 2m from the edge so they dont interfere with edges


        // need to add outline, add points at an interval of 4m along the edge
        for(int along = 0; along <= 100; along += 2)
        {
            pdsPoints.Add(new Vector2(0, along));
            pdsPoints.Add(new Vector2(along, 0));
            pdsPoints.Add(new Vector2(100, along));
            pdsPoints.Add(new Vector2(along, 100));

        }


        Vector2[]pdsArray = pdsPoints.ToArray(); // convert the list to an array for better speed

        return pdsArray;
    }

    private static Vector3 ApplyWaves(Vector3 vert) 
    {
        float freq = 50f;
        float amp = 1f;
        // not quite working atm

        // we want solid edges on plane but we dont want them to be noticable so we apply sine wave to bump it up a little
        if((vert.x==0 || vert.x==100)&&vert.z<99 && vert.z>1)
          return new Vector3(vert.x + amp*Mathf.Sin(vert.z/100f * freq), vert.y, vert.z);
        else if ((vert.z == 0 || vert.z == 100) && vert.x < 99 && vert.x > 1)
            return new Vector3(vert.x , vert.y, vert.z + amp * Mathf.Sin(vert.x / 100f * freq));
        return vert;
    }


}




/*Timing Data
 * 
 * Data from M2 Pro 12 core Macbook Pro in editor on battery
 * 
 * 15 ms for createPlane(); when minDis for PDS is 4m
 * 39ms when minDis is 2m
 * 
 */
