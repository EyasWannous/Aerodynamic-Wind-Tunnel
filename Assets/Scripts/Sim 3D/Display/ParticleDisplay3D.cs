using UnityEngine;

public class ParticleDisplay3D : MonoBehaviour
{

    public Shader shader;
    public float scale;
    Mesh mesh;
    public Color col;
    Material mat;

    ComputeBuffer argsBuffer;
    Bounds bounds;

    //// Tested Object Compute Buffer
    //ComputeBuffer testedObjectArgsBuffer;
    //Mesh testedObjectMesh;
    //Material testedObjectMat;
    //Bounds testedObjectBounds;


    public Gradient colourMap;
    public int gradientResolution;
    public float velocityDisplayMax;
    Texture2D gradientTexture;
    bool needsUpdate;

    public float speedThreshold; // Added speedThreshold


    public int meshResolution;
    public int debug_MeshTriCount;

    //// Tested Object Compute Buffer
    //public int tested_object_debug_MeshTriCount;
    
    public void Init(Simulation3D sim)
    {
        mat = new Material(shader);
        mat.SetBuffer("Positions", sim.PositionBuffer);
        mat.SetBuffer("Velocities", sim.VelocityBuffer);

        mesh = SebStuff.SphereGenerator.GenerateSphereMesh(meshResolution);
        debug_MeshTriCount = mesh.triangles.Length / 3;
        argsBuffer = ComputeHelper.CreateArgsBuffer(mesh, sim.PositionBuffer.count);
        bounds = new Bounds(Vector3.zero, Vector3.one * 10000);

        //// Tested Object
        //testedObjectMat = new Material(shader);
        //testedObjectMat.SetBuffer("Points", sim.PointsBuffer);

        //testedObjectMesh = SebStuff.SphereGenerator.GenerateSphereMesh(meshResolution);
        //tested_object_debug_MeshTriCount = mesh.triangles.Length / 3;
        //testedObjectArgsBuffer = ComputeHelper.CreateArgsBuffer(mesh, sim.PointsBuffer.count);
        //testedObjectBounds = new Bounds(Vector3.zero, Vector3.one * 10000);
    }

    void LateUpdate()
    {

        UpdateSettings();
        Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, argsBuffer);

        // Tested Object
        //Graphics.DrawMeshInstancedIndirect(testedObjectMesh, 0, mat, testedObjectBounds, testedObjectArgsBuffer);
    }

    void UpdateSettings()
    {
        if (needsUpdate)
        {
            needsUpdate = false;
            ParticleDisplay2D.TextureFromGradient(ref gradientTexture, gradientResolution, colourMap);
            mat.SetTexture("ColourMap", gradientTexture);
        }
        mat.SetFloat("scale", scale);
        mat.SetColor("colour", col);
        mat.SetFloat("velocityMax", velocityDisplayMax);

        mat.SetFloat("speedThreshold", speedThreshold); // Pass the speedThreshold to the shader
        
        Vector3 s = transform.localScale;
        transform.localScale = Vector3.one;
        var localToWorld = transform.localToWorldMatrix;
        transform.localScale = s;

        mat.SetMatrix("localToWorld", localToWorld);
    }

    private void OnValidate()
    {
        needsUpdate = true;
    }

    void OnDestroy()
    {
        ComputeHelper.Release(argsBuffer);
    }
}
