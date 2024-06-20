using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter))]
public class TestedObject : MonoBehaviour
{
    private Mesh mesh;
    public Vector3[] vertices;
    public int[] triangles;
    //public Dictionary<int, float3[]> pointsTriangles;

    //void Start()
    //{
    //    InitializeMesh();
    //}

    public void InitializeMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter component not found on this GameObject.");
            return;
        }

        mesh = meshFilter.mesh;
        
        vertices = mesh.vertices;
        
        for (int i = 0; i < vertices.Length; i++) 
        {
            vertices[i] = transform.TransformPoint(vertices[i]);
        }

        triangles = mesh.triangles;

        //PrintVerticesAndTriangles();
    }

    void PrintVerticesAndTriangles()
    {
        Debug.Log("Vertices:");
        for (int i = 0; i < vertices.Length; i++)
        {
            Debug.Log("Vertex " + i + ": " + vertices[i]);
        }

        Debug.Log("Triangles:");
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Debug.Log("Triangle " + (i / 3) + ": " + triangles[i] + ", " + triangles[i + 1] + ", " + triangles[i + 2]);
        }
    }

    //void VerticesTriangles()
    //{
    //    for (int i = 0; i < triangles.Length; i += 3)
    //    {

    //    }
    //}
}
