using MIConvexHull;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
//using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class CustomTriagulation
{
    GameObject GameObject;
    Vector3[] Vertices;
    Mesh mesh;

    public CustomTriagulation(GameObject gameObject)
    {
        GameObject = gameObject;

        //mesh = GetCombinedMesh(GameObject);
        mesh = GameObject.GetComponent<MeshFilter>().mesh;
        
        if (mesh is null)
        {
            Debug.LogError("No MeshFilter or SkinnedMeshRenderer found on the model.");
            return;
        }

        Vertices = mesh.vertices;
    }

    // returns the triangles conatianing the vertices in the world space
    public MeshFilter Triangulate()
    {
        try
        {
            var tetrahedrons = Tetrahedralize(Vertices.ToList());

            var allTriangles = tetrahedrons.SelectMany(x => GetTriangles(x)).ToList();

            var uniqueTriangles = GetNotSharedTrianglesOnly(allTriangles);

            mesh.triangles = uniqueTriangles.SelectMany(t => t.VerticesIndexes).ToArray();

            var result = GameObject.GetComponent<MeshFilter>();

            result.mesh = mesh;

            return result;
        }catch(Exception ex)
        {
            Debug.Log($"Delaunay Triangulation Error: {ex.ToString()}");
            return GameObject.GetComponent<MeshFilter>();
        }
    }

    private static Mesh GetCombinedMesh(GameObject root)
    {
        // Collect all MeshFilter components in the root and its children
        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();

        // Create a CombineInstance array with the same length as the number of MeshFilters
        CombineInstance[] combineInstances = new CombineInstance[meshFilters.Length];

        // Iterate through each MeshFilter and set up the CombineInstance
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combineInstances[i].mesh = meshFilters[i].sharedMesh;
            combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        // Create a new mesh to hold the combined mesh
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances);

        return combinedMesh;
    }

    private static List<Tetrahedron> Tetrahedralize(List<Vector3> vertices)
    {
        var customVertices = vertices.Select(v => new Vertex(v.x, v.y, v.z)).ToList();

        double planeDistanceTolerance = 1e-1;

        var tetrahedrons = DelaunayTriangulation<Vertex, Tetrahedron>
            .Create(customVertices, planeDistanceTolerance);

        List<Tetrahedron> result = new List<Tetrahedron>();
        foreach (var cell in tetrahedrons.Cells)
        {
            result.Add(cell);
        }

        return result;
    }

    int FindVertex(Vector3 vertex)
    {
        return Array.FindIndex(Vertices, v => v.x == vertex.x && v.y == vertex.y && v.z == vertex.z);
    }

    Triangle[] GetTriangles(Tetrahedron tetrahedron)
    {
        List<int> vertices = new List<int>();

        foreach (var x in tetrahedron.Vertices)
        {
            vertices.Add(
                FindVertex(new Vector3((float)x.Position[0], (float)x.Position[1], (float)x.Position[2]))
            );
        }

        return GetAllCombos(vertices).ToArray();
    }

    private static List<Triangle> GetAllCombos(List<int> list)
    {
        int comboCount = (int)Math.Pow(2, list.Count) - 1;
        List<List<int>> result = new List<List<int>>();
        for (int i = 1; i < comboCount + 1; i++)
        {
            // make each combo here
            result.Add(new List<int>());
            for (int j = 0; j < list.Count; j++)
            {
                if ((i >> j) % 2 != 0)
                    result.Last().Add(list[j]);
            }
        }

        return result.Where(x => x.Count is 3).Select(x => new Triangle(x)).ToList();
    }

    private static List<Triangle> GetNotSharedTrianglesOnly(List<Triangle> allTriangles)
    {
        // Count occurrences of each triangle
        var triangleCounts = new Dictionary<Triangle, int>(new TriangleEqualityComparer());

        foreach (var triangle in allTriangles)
        {
            if (triangleCounts.ContainsKey(triangle))
            {
                triangleCounts[triangle]++;
            }
            else
            {
                triangleCounts[triangle] = 1;
            }
        }

        // Filter triangles that occur exactly once
        var uniqueTriangles = new List<Triangle>();

        foreach (var kvp in triangleCounts)
        {
            if (kvp.Value == 1)
            {
                uniqueTriangles.Add(kvp.Key);
            }
        }

        return uniqueTriangles;
    }
}