using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class MeshCombiner : MonoBehaviour
{
    public string meshName = "CombinedMesh";
    public string savePath = "Assets/Meshes";

    void Start()
    {
        CombineMeshes();
    }

    void CombineMeshes()
    {
        // Get all MeshFilter components in child objects
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        // Combine meshes
        for (int i = 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false); // Disable the individual mesh
        }

        // Create a new MeshFilter and MeshRenderer for the combined mesh
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine);

        // Add a MeshRenderer to render the combined mesh
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterials = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterials;

        // Reactivate the parent GameObject
        SaveMesh(meshFilter.mesh, meshName, savePath);

        gameObject.SetActive(true);
    }

    void SaveMesh(Mesh mesh, string name, string path)
    {
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(mesh, path + name + ".asset");
        AssetDatabase.SaveAssets();
#endif
    }
}
