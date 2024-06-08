using UnityEngine;

public class AirParticleDisplay3D : MonoBehaviour
{
    public Shader shader;
    public float scale;
    Mesh mesh;
    Material mat;

    ComputeBuffer argsBuffer;
    Bounds bounds;

    public Gradient colourMap;
    public int gradientResolution;
    public float velocityDisplayMax;
    Texture2D gradientTexture;
    bool needsUpdate;

    public int meshResolution;
    public int debug_MeshTriCount;

    public Color tintColor;
    public float softness;

    // Reference to Simulation3D
    public Simulation3D simulation;

    void Start()
    {
        if (simulation == null)
        {
            Debug.LogError("Simulation reference is not assigned!");
            return;
        }

        if (shader == null)
        {
            Debug.LogError("Shader is not assigned!");
            return;
        }

        Init(simulation);
    }

    public void Init(Simulation3D sim)
    {
        mat = new Material(shader);
        mat.SetBuffer("Positions", sim.PositionBuffer);
        mat.SetBuffer("Velocities", sim.VelocityBuffer);

        mesh = SebStuff.SphereGenerator.GenerateSphereMesh(meshResolution);
        debug_MeshTriCount = mesh.triangles.Length / 3;
        argsBuffer = ComputeHelper.CreateArgsBuffer(mesh, sim.PositionBuffer.count);
        bounds = new Bounds(Vector3.zero, Vector3.one * 10000);
    }

    void LateUpdate()
    {
        if (mat == null)
        {
            Debug.LogError("Material is not assigned! Did you call Init()?");
            return;
        }

        UpdateSettings();
        Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, argsBuffer);
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
        mat.SetColor("TintColor", tintColor);
        mat.SetFloat("Softness", softness);
        mat.SetFloat("velocityMax", velocityDisplayMax);

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
