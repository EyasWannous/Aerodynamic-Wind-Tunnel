using Unity.Mathematics;
using static Unity.Mathematics.math;

public static class SpatialHash3D
{

    static int3[] offsets3D = new int3[]
    {
        new int3(-1, -1, -1),
        new int3(-1, -1, 0),
        new int3(-1, -1, 1),
        new int3(-1, 0, -1),
        new int3(-1, 0, 0),
        new int3(-1, 0, 1),
        new int3(-1, 1, -1),
        new int3(-1, 1, 0),
        new int3(-1, 1, 1),
        new int3(0, -1, -1),
        new int3(0, -1, 0),
        new int3(0, -1, 1),
        new int3(0, 0, -1),
        new int3(0, 0, 0),
        new int3(0, 0, 1),
        new int3(0, 1, -1),
        new int3(0, 1, 0),
        new int3(0, 1, 1),
        new int3(1, -1, -1),
        new int3(1, -1, 0),
        new int3(1, -1, 1),
        new int3(1, 0, -1),
        new int3(1, 0, 0),
        new int3(1, 0, 1),
        new int3(1, 1, -1),
        new int3(1, 1, 0),
        new int3(1, 1, 1),
        new int3(new)
    };

    // Constants used for hashing
    static uint hashK1 = 15823;
    static uint hashK2 = 9737333;
    static uint hashK3 = 440817757;

    // Convert floating point position into an integer cell coordinate
    static int3 GetCell3D(float3 position, float radius)
    {
        return (int3)floor(position / radius);
    }

    // Hash cell coordinate to a single unsigned integer
    static uint HashCell3D(int3 cell)
    {
        uint3 ucell = (uint3)cell;
        return (ucell.x * hashK1) + (ucell.y * hashK2) + (ucell.z * hashK3);
    }

    static uint KeyFromHash(uint hash, uint tableSize)
    {
        return hash % tableSize;
    }

}