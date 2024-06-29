using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Triangulation : MonoBehaviour
{
    public Vector3[] vertices;
    public int[] triangles;
    public Mesh mesh;
    public Material material; // Add this line to reference a material

    void Start()
    {
        Init();
    }

    public void Init()
    {
        var children = GetAllChildren(gameObject);

        var meshFilters = new List<MeshFilter>();

        foreach (var child in children)
        {
            var triangulation = new CustomTriagulation(child);

            meshFilters.Add(triangulation.Triangulate());
        }

        mesh = MergeMeshFilters(meshFilters)?.mesh;
        vertices = mesh.vertices;
        triangles = mesh.triangles;
        mesh.RecalculateNormals();
    }

    public MeshFilter MergeMeshFilters(List<MeshFilter> meshFilters)
    {
        if (meshFilters is null || meshFilters.Count == 0)
        {
            Debug.LogError("The list of MeshFilters is null or empty.");
            return null;
        }

        // Create a list of CombineInstance
        List<CombineInstance> combineInstances = new List<CombineInstance>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null)
            {
                Debug.LogWarning($"MeshFilter on GameObject {meshFilter.gameObject.name} does not have a shared mesh.");
                continue;
            }

            CombineInstance combineInstance = new CombineInstance();
            combineInstance.mesh = meshFilter.sharedMesh;
            combineInstance.transform = meshFilter.transform.localToWorldMatrix;
            combineInstances.Add(combineInstance);
        }

        // Create a new mesh and combine
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combineInstances.ToArray(), true, true);

        // Create a new GameObject to hold the combined mesh
        GameObject combinedMeshObject = new GameObject("CombinedMesh");
        MeshFilter combinedMeshFilter = combinedMeshObject.AddComponent<MeshFilter>();
        MeshRenderer combinedMeshRenderer = combinedMeshObject.AddComponent<MeshRenderer>();
        combinedMeshFilter.mesh = combinedMesh;

        return combinedMeshFilter;
    }

    public List<GameObject> GetAllChildren(GameObject parent)
    {
        List<GameObject> children = new List<GameObject>();

        // Use a queue to process all children iteratively
        Queue<Transform> queue = new Queue<Transform>();

        // Enqueue the parent transform
        queue.Enqueue(parent.transform);

        // Process the queue
        while (queue.Count > 0)
        {
            // Dequeue the next transform
            Transform current = queue.Dequeue();

            // Iterate through all children of the current transform
            foreach (Transform child in current)
            {
                // Add the child GameObject to the list
                children.Add(child.gameObject);

                // Enqueue the child's transform
                queue.Enqueue(child);
            }
        }

        return children;
    }

    void OnDrawGizmos()
    {
        if (mesh is null || vertices is null || triangles is null)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 v0 = (vertices[triangles[i]]);
            Vector3 v1 = (vertices[triangles[i + 1]]);
            Vector3 v2 = (vertices[triangles[i + 2]]);

            Gizmos.DrawLine(v0, v1);
            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v2, v0);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Save Mesh To Asset")]
    public void SaveMeshToAsset()
    {
        if (mesh == null)
        {
            Debug.LogError("No mesh to save.");
            return;
        }

        string meshPath = "Assets/CombinedMesh.asset";
        string prefabPath = "Assets/CombinedMesh.prefab";

        // Save the mesh asset
        AssetDatabase.CreateAsset(mesh, meshPath);
        AssetDatabase.SaveAssets();

        // Create a new GameObject to hold the combined mesh
        GameObject combinedMeshObject = new GameObject("CombinedMesh");
        MeshFilter combinedMeshFilter = combinedMeshObject.AddComponent<MeshFilter>();
        MeshRenderer combinedMeshRenderer = combinedMeshObject.AddComponent<MeshRenderer>();
        combinedMeshFilter.mesh = mesh;

        // Apply the material
        if (material != null)
        {
            combinedMeshRenderer.material = material;
        }
        else
        {
            Debug.LogWarning("No material assigned. Default material will be used.");
        }

        // Save the GameObject as a prefab
        PrefabUtility.SaveAsPrefabAsset(combinedMeshObject, prefabPath);

        // Clean up the temporary GameObject
        DestroyImmediate(combinedMeshObject);

        Debug.Log("Mesh, MeshRenderer, and Material saved to " + prefabPath);
    }
#endif
}