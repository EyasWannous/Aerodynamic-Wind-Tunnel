using System;
using System.Collections.Generic;
// using System.Diagnostics;
using System.Linq;
using MIConvexHull;
//using Unity.VisualScripting;
using UnityEngine;

public class TriangulationScript : MonoBehaviour
{
    public Mesh mesh;
    public Vector3[] vertices;
    public List<Triangle> trianglesToDraw;
    public int[] triangles;

    void Start()
    {
        Debug.Log("Delaunay Triangulation Example started.");

        mesh = GetCombinedMesh(gameObject);

        if (mesh is null)
        {
            Debug.LogError("No MeshFilter or SkinnedMeshRenderer found on the model.");
            return;
        }

        vertices = mesh.vertices;

        Debug.Log($"Mesh found with {mesh.vertexCount} vertices.");
        Debug.Log($"Mesh found with {mesh.triangles.Count()} triangles.");

        mesh.triangles = Triangulate(vertices);
        triangles = mesh.triangles;
        Debug.Log($"Mesh changed to have {mesh.triangles.Count()} triangles.");
        
        mesh.RecalculateNormals();
    }

    public void Init()
    {
        Debug.Log("Delaunay Triangulation Example started.");

        mesh = GetCombinedMesh(gameObject);

        if (mesh is null)
        {
            Debug.LogError("No MeshFilter or SkinnedMeshRenderer found on the model.");
            return;
        }

        vertices = mesh.vertices;

        Debug.Log($"Mesh found with {mesh.vertexCount} vertices.");
        Debug.Log($"Mesh found with {mesh.triangles.Count()} triangles.");

        mesh.triangles = Triangulate(vertices);
        triangles = mesh.triangles;
        Debug.Log($"Mesh changed to have {mesh.triangles.Count()} triangles.");

        mesh.RecalculateNormals();
    }

    int[] Triangulate(Vector3[] vertices)
    {
        List<int> triangles = new List<int>();

        var tetrahedrons = Tetrahedralize(vertices.ToList());

        var allTriangles = tetrahedrons.SelectMany(x => GetTriangles(x)).ToList();

        var uniqueTriangles = GetNotSharedTrianglesOnly(allTriangles);

        trianglesToDraw = uniqueTriangles;

        return uniqueTriangles.SelectMany(t => t.VerticesIndexes).ToArray();
        //return allTriangles.SelectMany(t => t.VerticesIndexes).ToArray();
    }

    public static Mesh GetCombinedMesh(GameObject root)
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

    public static List<Tetrahedron> Tetrahedralize(List<Vector3> vertices)
    {
        var customVertices = vertices.Select(v => new Vertex(v.x, v.y, v.z)).ToList();

        double planeDistanceTolerance = 1e-5;

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
        return Array.FindIndex(vertices, v => v.x == vertex.x && v.y == vertex.y && v.z == vertex.z);
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

    public static List<Triangle> GetAllCombos(List<int> list)
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

    public static List<Triangle> GetNotSharedTrianglesOnly(List<Triangle> allTriangles)
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

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;

        if (trianglesToDraw is null)
            return;

        if (trianglesToDraw.Count is not 0)
        {
            foreach (var triangle in trianglesToDraw)
            {
                Gizmos.DrawLine(vertices[triangle.VerticesIndexes[0]], vertices[triangle.VerticesIndexes[1]]);
                Gizmos.DrawLine(vertices[triangle.VerticesIndexes[0]], vertices[triangle.VerticesIndexes[2]]);
                Gizmos.DrawLine(vertices[triangle.VerticesIndexes[1]], vertices[triangle.VerticesIndexes[2]]);
            }
        }
    }
}
public class Vertex : IVertex
{
    public double[] Position { get; set; }

    public Vertex(double x, double y, double z)
    {
        Position = new double[] { x, y, z };
    }
}

public class Tetrahedron : TriangulationCell<Vertex, Tetrahedron>
{
}

public class Triangle
{
    public List<int> VerticesIndexes { get; set; }

    public Triangle(List<int> vertices)
    {
        if (vertices.Count != 3)
            throw new ArgumentException($"A triangle must have exactly 3 vertices. provided: {vertices.Count}");
        VerticesIndexes = vertices;
    }
}

public class TriangleEqualityComparer : IEqualityComparer<Triangle>
{
    public bool Equals(Triangle t1, Triangle t2)
    {
        if (t1 == null && t2 == null)
            return true;
        if (t1 == null || t2 == null)
            return false;

        var sortedT1 = t1.VerticesIndexes.OrderBy(v => v).ToList();
        var sortedT2 = t2.VerticesIndexes.OrderBy(v => v).ToList();

        return sortedT1.SequenceEqual(sortedT2);
    }

    public int GetHashCode(Triangle t)
    {
        if (t == null)
            throw new ArgumentNullException(nameof(t));

        var sortedVertices = t.VerticesIndexes.OrderBy(v => v).ToList();
        int hash = 17;
        foreach (var vertex in sortedVertices)
        {
            hash = hash * 31 + vertex.GetHashCode();
        }
        return hash;
    }
}
