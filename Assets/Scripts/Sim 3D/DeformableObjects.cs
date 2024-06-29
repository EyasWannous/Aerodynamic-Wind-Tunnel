using System.Collections.Generic;
using UnityEngine;

public class DeformableObjects : MonoBehaviour
{
    public Vector3 windForce = new(0, 0, 0);
    public float alpha = 0.5f;
    public float timeStep = 0.02f;
    public List<float[]> pointMassesList;  // List of masses arrays for each mesh filter
    public float forceThreshold = 1.5f;
    public float massmin = 1f;
    public float massmax = 1.5f;
    public List<MeshFilter> meshFilters;  // Multiple mesh filters
    public ComputeShader computeShader;
    public float deformationDuration = 5.0f;

    private List<Vector3[]> initialPositionsList = new();
    private List<Vector3[]> positionsList = new();
    private List<Vector3[]> velocitiesList = new();
    private List<float[]> massesList = new();
    private List<Vector3> initialCenterOfMassList = new();

    private List<ComputeBuffer> positionsBuffers = new();
    private List<ComputeBuffer> velocitiesBuffers = new();
    private List<ComputeBuffer> initialPositionsBuffers = new();
    private List<ComputeBuffer> massesBuffers = new();

    private ComputeBuffer windForceBuffer;

    private float elapsedTime = 0f;
    private bool isDeforming = true;

    void Start()
    {
        if (meshFilters == null || meshFilters.Count == 0)
        {
            Debug.LogError("No MeshFilters assigned.");
            return;
        }

        windForceBuffer = new ComputeBuffer(1, sizeof(float) * 3);
        windForceBuffer.SetData(new Vector3[] { windForce });

        computeShader.SetBuffer(0, "windForceBuffer", windForceBuffer);
        computeShader.SetFloat("alpha", alpha);
        computeShader.SetFloat("timeStep", timeStep);
        computeShader.SetFloat("forceThreshold", forceThreshold);
        //computeShader.SetFloat("massmin", massmin);
        //computeShader.SetFloat("massmax", massmax);

        for (int j = 0; j < meshFilters.Count; j++)
        {
            var meshFilter = meshFilters[j];
            var mesh = meshFilter.mesh;
            Vector3[] vertices = mesh.vertices;
            int verticesCount = vertices.Length;

            Vector3[] initialPositions = new Vector3[verticesCount];
            Vector3[] positions = new Vector3[verticesCount];
            Vector3[] velocities = new Vector3[verticesCount];
            float[] masses = new float[verticesCount];

            float[] pointMasses = (pointMassesList != null && pointMassesList.Count > j) ? pointMassesList[j] : null;

            if (pointMasses != null && pointMasses.Length != verticesCount)
            {
                Debug.LogError("pointMasses array length does not match the number of vertices. Ignoring provided masses.");
                pointMasses = null;
            }

            for (int i = 0; i < verticesCount; i++)
            {
                masses[i] = (pointMasses != null) ? pointMasses[i] : Random.Range(massmin, massmax);
                //initialPositions[i] = meshFilter.transform.TransformPoint(vertices[i]);
                initialPositions[i] = vertices[i];
                velocities[i] = Vector3.zero;
                positions[i] = initialPositions[i];
            }

            Vector3 initialCenterOfMass = ComputeCenterOfMass(initialPositions, masses);

            initialPositionsList.Add(initialPositions);
            positionsList.Add(positions);
            velocitiesList.Add(velocities);
            massesList.Add(masses);
            initialCenterOfMassList.Add(initialCenterOfMass);

            ComputeBuffer positionsBuffer = new(verticesCount, sizeof(float) * 3);
            ComputeBuffer velocitiesBuffer = new(verticesCount, sizeof(float) * 3);
            ComputeBuffer initialPositionsBuffer = new(verticesCount, sizeof(float) * 3);
            ComputeBuffer massesBuffer = new(verticesCount, sizeof(float));

            positionsBuffer.SetData(positions);
            velocitiesBuffer.SetData(velocities);
            initialPositionsBuffer.SetData(initialPositions);
            massesBuffer.SetData(masses);

            positionsBuffers.Add(positionsBuffer);
            velocitiesBuffers.Add(velocitiesBuffer);
            initialPositionsBuffers.Add(initialPositionsBuffer);
            massesBuffers.Add(massesBuffer);
        }
    }
    void Update()
    {
        if (isDeforming)
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime >= deformationDuration)
            {
                isDeforming = false;
                return;
            }

            int kernelHandle = computeShader.FindKernel("Deforme");

            for (int i = 0; i < positionsList.Count; i++)
            {
                // Ensure indices are valid
                if (i >= positionsList.Count || i >= positionsBuffers.Count || i >= meshFilters.Count)
                {
                    Debug.LogError("Index out of range.");
                    continue;
                }

                computeShader.SetBuffer(kernelHandle, "positions", positionsBuffers[i]);
                computeShader.SetBuffer(kernelHandle, "velocities", velocitiesBuffers[i]);
                computeShader.SetBuffer(kernelHandle, "initialPositions", initialPositionsBuffers[i]);
                computeShader.SetBuffer(kernelHandle, "masses", massesBuffers[i]);

                int threadGroups = Mathf.CeilToInt(positionsList[i].Length / 256.0f);
                computeShader.Dispatch(kernelHandle, threadGroups, 1, 1);

                positionsBuffers[i].GetData(positionsList[i]);

                var meshFilter = meshFilters[i];
                if (meshFilter != null)
                {
                    var mesh = meshFilter.mesh;
                    mesh.vertices = positionsList[i];
                    mesh.RecalculateNormals();
                }
            }
        }
    }

    void OnDestroy()
    {
        positionsBuffers.ForEach(x => x.Dispose());
        velocitiesBuffers.ForEach(x => x.Dispose());
        initialPositionsBuffers.ForEach(x => x.Dispose());
        massesBuffers.ForEach(x => x.Dispose());

        windForceBuffer?.Release();
    }

    private Vector3 ComputeCenterOfMass(Vector3[] points, float[] masses)
    {
        Vector3 centerOfMass = Vector3.zero;
        float totalMass = 0f;
        for (int i = 0; i < points.Length; i++)
        {
            centerOfMass += points[i] * masses[i];
            totalMass += masses[i];
        }
        return centerOfMass / totalMass;
    }
}
