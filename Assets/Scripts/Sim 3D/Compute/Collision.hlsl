//cbuffer Constants : register(b0)
//{
//	float3 sphereCenter;
//	float sphereRadius;
//};

//bool IsPointInsideSphere(float3 pos, float3 sphereCenter, float sphereRadius)
//{
//	return distance(pos, sphereCenter) <= sphereRadius;
//}
//
//float3 ClosestPointOnSegment(float3 a, float3 b, float3 pos)
//{
//	float3 ab = b - a;
//	float t = dot(pos - a, ab) / dot(ab, ab);
//    t = clamp(t, 0, 1);
//    return a + t * ab;
//}
//
//bool IsSegmentIntersectingSphere(float3 p1, float3 p2, float3 sphereCenter, float sphereRadius)
//{
//	float3 closestPoint = ClosestPointOnSegment(p1, p2, sphereCenter);
//	return IsPointInsideSphere(closestPoint, sphereCenter, sphereRadius);
//}
//
//bool IsPointInTriangle(float3 p, float3 a, float3 b, float3 c)
//{
//	float3 v0 = c - a;
//	float3 v1 = b - a;
//	float3 v2 = p - a;
//
//	float dot00 = dot(v0, v0);
//	float dot01 = dot(v0, v1);
//	float dot02 = dot(v0, v2);
//	float dot11 = dot(v1, v1);
//	float dot12 = dot(v1, v2);
//
//	float invDenom = 1.0 / (dot00 * dot11 - dot01 * dot01);
//	float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
//	float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
//
//	return (u >= 0) && (v >= 0) && (u + v < 1);
//}
//
//bool IsSphereIntersectingTrianglePlane(float3 v0, float3 v1, float3 v2, float3 sphereCenter, float sphereRadius)
//{
//	float3 normal = normalize(cross(v1 - v0, v2 - v0));
//	float distance = dot(sphereCenter - v0, normal);
//
//	if (abs(distance) > sphereRadius)
//	{
//		return false;
//	}
//
//	float3 projection = sphereCenter - distance * normal;
//	return IsPointInTriangle(projection, v0, v1, v2);
//}
//
//bool IsCollided(float3 v0, float3 v1, float3 v2, float3 sphereCenter, float sphereRadius)
//{
//    // Check if any vertex of the triangle is inside the sphere
//	if (IsPointInsideSphere(v0, sphereCenter, sphereRadius) ||
//        IsPointInsideSphere(v1, sphereCenter, sphereRadius) ||
//        IsPointInsideSphere(v2, sphereCenter, sphereRadius))
//	{
//		return true;
//	}
//
//    // Check if the sphere intersects with any of the triangle's edges
//	if (IsSegmentIntersectingSphere(v0, v1, sphereCenter, sphereRadius) ||
//        IsSegmentIntersectingSphere(v1, v2, sphereCenter, sphereRadius) ||
//        IsSegmentIntersectingSphere(v2, v0, sphereCenter, sphereRadius))
//	{
//		return true;
//	}
//
//    // Check if the sphere intersects with the triangle's plane
//	return IsSphereIntersectingTrianglePlane(v0, v1, v2, sphereCenter, sphereRadius);
//}


float DistancePointToTriangle(float3 sphereCenter, float3 v0, float3 v1, float3 v2)
{
    float3 edge0 = v1 - v0;
    float3 edge1 = v2 - v0;
    float3 v0ToPoint = v0 - sphereCenter;

    float a = dot(edge0, edge0);
    float b = dot(edge0, edge1);
    float c = dot(edge1, edge1);
    float d = dot(edge0, v0ToPoint);
    float e = dot(edge1, v0ToPoint);
    float f = dot(v0ToPoint, v0ToPoint);

    float det = a * c - b * b;
    float s = b * e - c * d;
    float t = b * d - a * e;

    if (s + t <= det)
    {
        if (s < 0)
        {
            if (t < 0) // Region 4
            {
                if (d < 0)
                {
                    s = min(max(-d / a, 0.0), 1.0);
                    t = 0.0;
                }
                else
                {
                    s = 0.0;
                    t = min(max(-e / c, 0.0), 1.0);
                }
            }
            else // Region 3
            {
                s = 0.0;
                t = min(max(-e / c, 0.0), 1.0);
            }
        }
        else if (t < 0) // Region 5
        {
            s = min(max(-d / a, 0.0), 1.0);
            t = 0.0;
        }
        else // Region 0
        {
            s /= det;
            t /= det;
        }
    }
    else
    {
        if (s < 0) // Region 2
        {
            s = 0.0;
            t = min(max(-e / c, 0.0), 1.0);
        }
        else if (t < 0) // Region 6
        {
            s = min(max(-d / a, 0.0), 1.0);
            t = 0.0;
        }
        else // Region 1
        {
            float numer = c + e - b - d;
            if (numer <= 0)
            {
                s = 0.0;
                t = 1.0;
            }
            else
            {
                float denom = a - 2.0 * b + c;
                s = min(max(numer / denom, 0.0), 1.0);
                t = 1.0 - s;
            }
        }
    }

    float3 closestPoint = v0 + s * edge0 + t * edge1;
    float3 diff = closestPoint - sphereCenter;
    return sqrt(dot(diff, diff));
}

bool IsCollided(float3 v0, float3 v1, float3 v2, float3 sphereCenter, float sphereRadius)
{
    return DistancePointToTriangle(sphereCenter, v0, v1, v2) <= sphereRadius;
}