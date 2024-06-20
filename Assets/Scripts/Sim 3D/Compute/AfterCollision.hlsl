// Function to calculate the reflection of the sphere's direction based on collision with a plane
float3 ReflectDirection(float3 direction, float3 normal)
{
    return direction - 2 * dot(direction, normal) * normal;
}

// Main function that takes all inputs and returns the new direction of the sphere
float3 CalculateNewDirection(float3 planePoint1, float3 planePoint2, float3 planePoint3, float3 sphereDirection)
{
    // Calculate plane normal
    float3 planeNormal = normalize(cross(planePoint2 - planePoint1, planePoint3 - planePoint1));

    // Reflect the sphere's direction based on the collision angle
    float3 newDirection = ReflectDirection(sphereDirection, planeNormal);

    return newDirection;
}
