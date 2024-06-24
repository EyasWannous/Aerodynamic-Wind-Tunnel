#pragma kernel CSMain

//struct Vertex
//{
//    float3 position;
//    float3 initialPosition;
//    float3 velocity;
//    float mass;
//};

//RWStructuredBuffer<Vertex> vertices;

float3 windForce;
float alpha;
float timeStep;
float forceThreshold;
float3 initialCenterOfMass;
float3 currentCenterOfMass;
int numVertices;

float4x4 Inverse(float4x4 m)
{
    float4x4 inv;
    float det;

    inv[0][0] = m[1][1] * m[2][2] * m[3][3] -
                m[1][1] * m[2][3] * m[3][2] -
                m[2][1] * m[1][2] * m[3][3] +
                m[2][1] * m[1][3] * m[3][2] +
                m[3][1] * m[1][2] * m[2][3] -
                m[3][1] * m[1][3] * m[2][2];

    inv[1][0] = -m[1][0] * m[2][2] * m[3][3] +
                 m[1][0] * m[2][3] * m[3][2] +
                 m[2][0] * m[1][2] * m[3][3] -
                 m[2][0] * m[1][3] * m[3][2] -
                 m[3][0] * m[1][2] * m[2][3] +
                 m[3][0] * m[1][3] * m[2][2];

    inv[2][0] = m[1][0] * m[2][1] * m[3][3] -
                m[1][0] * m[2][3] * m[3][1] -
                m[2][0] * m[1][1] * m[3][3] +
                m[2][0] * m[1][3] * m[3][1] +
                m[3][0] * m[1][1] * m[2][3] -
                m[3][0] * m[1][3] * m[2][1];

    inv[3][0] = -m[1][0] * m[2][1] * m[3][2] +
                 m[1][0] * m[2][2] * m[3][1] +
                 m[2][0] * m[1][1] * m[3][2] -
                 m[2][0] * m[1][2] * m[3][1] -
                 m[3][0] * m[1][1] * m[2][2] +
                 m[3][0] * m[1][2] * m[2][1];

    inv[0][1] = -m[0][1] * m[2][2] * m[3][3] +
                 m[0][1] * m[2][3] * m[3][2] +
                 m[2][1] * m[0][2] * m[3][3] -
                 m[2][1] * m[0][3] * m[3][2] -
                 m[3][1] * m[0][2] * m[2][3] +
                 m[3][1] * m[0][3] * m[2][2];

    inv[1][1] = m[0][0] * m[2][2] * m[3][3] -
                m[0][0] * m[2][3] * m[3][2] -
                m[2][0] * m[0][2] * m[3][3] +
                m[2][0] * m[0][3] * m[3][2] +
                m[3][0] * m[0][2] * m[2][3] -
                m[3][0] * m[0][3] * m[2][2];

    inv[2][1] = -m[0][0] * m[2][1] * m[3][3] +
                 m[0][0] * m[2][3] * m[3][1] +
                 m[2][0] * m[0][1] * m[3][3] -
                 m[2][0] * m[0][3] * m[3][1] -
                 m[3][0] * m[0][1] * m[2][3] +
                 m[3][0] * m[0][3] * m[2][1];

    inv[3][1] = m[0][0] * m[2][1] * m[3][2] -
                m[0][0] * m[2][2] * m[3][1] -
                m[2][0] * m[0][1] * m[3][2] +
                m[2][0] * m[0][2] * m[3][1] +
                m[3][0] * m[0][1] * m[2][2] -
                m[3][0] * m[0][2] * m[2][1];

    inv[0][2] = m[0][1] * m[1][2] * m[3][3] -
                m[0][1] * m[1][3] * m[3][2] -
                m[1][1] * m[0][2] * m[3][3] +
                m[1][1] * m[0][3] * m[3][2] +
                m[3][1] * m[0][2] * m[1][3] -
                m[3][1] * m[0][3] * m[1][2];

    inv[1][2] = -m[0][0] * m[1][2] * m[3][3] +
                 m[0][0] * m[1][3] * m[3][2] +
                 m[1][0] * m[0][2] * m[3][3] -
                 m[1][0] * m[0][3] * m[3][2] -
                 m[3][0] * m[0][2] * m[1][3] +
                 m[3][0] * m[0][3] * m[1][2];

    inv[2][2] = m[0][0] * m[1][1] * m[3][3] -
                m[0][0] * m[1][3] * m[3][1] -
                m[1][0] * m[0][1] * m[3][3] +
                m[1][0] * m[0][3] * m[3][1] +
                m[3][0] * m[0][1] * m[1][3] -
                m[3][0] * m[0][3] * m[1][1];

    inv[3][2] = -m[0][0] * m[1][1] * m[3][2] +
                 m[0][0] * m[1][2] * m[3][1] +
                 m[1][0] * m[0][1] * m[3][2] -
                 m[1][0] * m[0][2] * m[3][1] -
                 m[3][0] * m[0][1] * m[1][2] +
                 m[3][0] * m[0][2] * m[1][1];

    inv[0][3] = -m[0][1] * m[1][2] * m[2][3] +
                 m[0][1] * m[1][3] * m[2][2] +
                 m[1][1] * m[0][2] * m[2][3] -
                 m[1][1] * m[0][3] * m[2][2] -
                 m[2][1] * m[0][2] * m[1][3] +
                 m[2][1] * m[0][3] * m[1][2];

    inv[1][3] = m[0][0] * m[1][2] * m[2][3] -
                m[0][0] * m[1][3] * m[2][2] -
                m[1][0] * m[0][2] * m[2][3] +
                m[1][0] * m[0][3] * m[2][2] +
                m[2][0] * m[0][2] * m[1][3] -
                m[2][0] * m[0][3] * m[1][2];

    inv[2][3] = -m[0][0] * m[1][1] * m[2][3] +
                 m[0][0] * m[1][3] * m[2][1] +
                 m[1][0] * m[0][1] * m[2][3] -
                 m[1][0] * m[0][3] * m[2][1] -
                 m[2][0] * m[0][1] * m[1][3] +
                 m[2][0] * m[0][3] * m[1][1];

    inv[3][3] = m[0][0] * m[1][1] * m[2][2] -
                m[0][0] * m[1][2] * m[2][1] -
                m[1][0] * m[0][1] * m[2][2] +
                m[1][0] * m[0][2] * m[2][1] +
                m[2][0] * m[0][1] * m[1][2] -
                m[2][0] * m[0][2] * m[1][1];

    det = m[0][0] * inv[0][0] + m[0][1] * inv[1][0] + m[0][2] * inv[2][0] + m[0][3] * inv[3][0];

    if (det == 0)
        return m; // Return the original matrix if it is not invertible

    det = 1.0 / det;

    for (int i = 0; i < 4; i++)
    {
        for (int j = 0; j < 4; j++)
        {
            inv[i][j] *= det;
        }
    }

    return inv;
}


// Computes the outer product of two vectors
float4x4 OuterProduct(float3 a, float3 b)
{
    return float4x4(
        a.x * b.x, a.x * b.y, a.x * b.z, 0.0,
        a.y * b.x, a.y * b.y, a.y * b.z, 0.0,
        a.z * b.x, a.z * b.y, a.z * b.z, 0.0,
        0.0, 0.0, 0.0, 0.0
    );
}

// Adds two 4x4 matrices
float4x4 AddMatrices(float4x4 a, float4x4 b)
{
    return a + b;
}

// Multiplies a 4x4 matrix by a scalar
float4x4 MultiplyMatrixByScalar(float4x4 myMatrix, float scalar)
{
    return myMatrix * scalar;
}

// Computes the center of mass for the given vertices
float3 ComputeCenterOfMass()
{
    float3 centerOfMass = float3(0, 0, 0);
    float totalMass = 0.0;

    for (int i = 0; i < numVertices; i++)
    {
        centerOfMass += vertices[i].position * vertices[i].mass;
        totalMass += vertices[i].mass;
    }

    return centerOfMass / totalMass;
}

// Applies wind force to each vertex
void ApplyWindForce()
{
    for (int i = 0; i < numVertices; i++)
    {
        float3 acceleration = windForce / vertices[i].mass;
        if (length(acceleration) > forceThreshold)
        {
            vertices[i].velocity += acceleration * timeStep;
        }
    }
}

// Integrates the positions based on velocities
void TimeIntegration()
{
    for (int i = 0; i < numVertices; i++)
    {
        if (length(vertices[i].velocity) * vertices[i].mass > forceThreshold)
        {
            vertices[i].position += vertices[i].velocity * timeStep;
        }
    }
}

// Computes shape matching
void ShapeMatching()
{
    currentCenterOfMass = ComputeCenterOfMass();

    float3 p[1024]; // Assuming max 1024 vertices, adjust as needed
    float3 q[1024];
    float4x4 Apq = float4x4(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
    float4x4 Aqq = float4x4(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

    for (int i = 0; i < numVertices; i++)
    {
        p[i] = vertices[i].position - currentCenterOfMass;
        q[i] = vertices[i].initialPosition - initialCenterOfMass;
        Apq = AddMatrices(Apq, MultiplyMatrixByScalar(OuterProduct(p[i], q[i]), vertices[i].mass));
        Aqq = AddMatrices(Aqq, MultiplyMatrixByScalar(OuterProduct(q[i], q[i]), vertices[i].mass));
    }

    float4x4 AqqInv = Inverse(Aqq);
    float4x4 A = mul(Apq, AqqInv);

    // For polar decomposition, assuming A is already a rotation matrix
    float4x4 R = A;

    for (uint j = 0; j < numVertices; j++)
    {
        float3 goalPosition = mul(R, float4(q[j], 1.0)).xyz + currentCenterOfMass;
        vertices[j].position = goalPosition * alpha + vertices[j].position * (1 - alpha);
    }
}