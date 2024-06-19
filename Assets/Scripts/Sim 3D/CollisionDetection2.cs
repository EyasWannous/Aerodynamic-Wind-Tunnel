using System;

public class PointTriangleDistance
{
    public static double Dot(double[] v1, double[] v2)
    {
        return v1[0] * v2[0] + v1[1] * v2[1] + v1[2] * v2[2];
    }

    public static double[] Subtract(double[] v1, double[] v2)
    {
        return new double[] { v1[0] - v2[0], v1[1] - v2[1], v1[2] - v2[2] };
    }

    public static double[] Add(double[] v1, double[] v2)
    {
        return new double[] { v1[0] + v2[0], v1[1] + v2[1], v1[2] + v2[2] };
    }

    public static double[] Multiply(double[] v, double scalar)
    {
        return new double[] { v[0] * scalar, v[1] * scalar, v[2] * scalar };
    }

    public static (double, double[]) PointTriangleDistance(double[,] TRI, double[] P)
    {
        double[] B = { TRI[0, 0], TRI[0, 1], TRI[0, 2] };
        double[] E0 = Subtract(new double[] { TRI[1, 0], TRI[1, 1], TRI[1, 2] }, B);
        double[] E1 = Subtract(new double[] { TRI[2, 0], TRI[2, 1], TRI[2, 2] }, B);
        double[] D = Subtract(B, P);
        double a = Dot(E0, E0);
        double b = Dot(E0, E1);
        double c = Dot(E1, E1);
        double d = Dot(E0, D);
        double e = Dot(E1, D);
        double f = Dot(D, D);
        double det = a * c - b * b;
        double s = b * e - c * d;
        double t = b * d - a * e;

        double sqrdistance;

        if (s + t <= det)
        {
            if (s < 0.0)
            {
                if (t < 0.0)
                {
                    if (d < 0)
                    {
                        t = 0.0;
                        if (-d >= a)
                        {
                            s = 1.0;
                            sqrdistance = a + 2.0 * d + f;
                        }
                        else
                        {
                            s = -d / a;
                            sqrdistance = d * s + f;
                        }
                    }
                    else
                    {
                        s = 0.0;
                        if (e >= 0.0)
                        {
                            t = 0.0;
                            sqrdistance = f;
                        }
                        else
                        {
                            if (-e >= c)
                            {
                                t = 1.0;
                                sqrdistance = c + 2.0 * e + f;
                            }
                            else
                            {
                                t = -e / c;
                                sqrdistance = e * t + f;
                            }
                        }
                    }
                }
                else
                {
                    s = 0.0;
                    if (e >= 0.0)
                    {
                        t = 0.0;
                        sqrdistance = f;
                    }
                    else
                    {
                        if (-e >= c)
                        {
                            t = 1.0;
                            sqrdistance = c + 2.0 * e + f;
                        }
                        else
                        {
                            t = -e / c;
                            sqrdistance = e * t + f;
                        }
                    }
                }
            }
            else
            {
                if (t < 0.0)
                {
                    t = 0.0;
                    if (d >= 0.0)
                    {
                        s = 0.0;
                        sqrdistance = f;
                    }
                    else
                    {
                        if (-d >= a)
                        {
                            s = 1.0;
                            sqrdistance = a + 2.0 * d + f;
                        }
                        else
                        {
                            s = -d / a;
                            sqrdistance = d * s + f;
                        }
                    }
                }
                else
                {
                    double invDet = 1.0 / det;
                    s = s * invDet;
                    t = t * invDet;
                    sqrdistance = s * (a * s + b * t + 2.0 * d) + t * (b * s + c * t + 2.0 * e) + f;
                }
            }
        }
        else
        {
            if (s < 0.0)
            {
                double tmp0 = b + d;
                double tmp1 = c + e;
                if (tmp1 > tmp0)
                {
                    double numer = tmp1 - tmp0;
                    double denom = a - 2.0 * b + c;
                    if (numer >= denom)
                    {
                        s = 1.0;
                        t = 0.0;
                        sqrdistance = a + 2.0 * d + f;
                    }
                    else
                    {
                        s = numer / denom;
                        t = 1.0 - s;
                        sqrdistance = s * (a * s + b * t + 2.0 * d) + t * (b * s + c * t + 2.0 * e) + f;
                    }
                }
                else
                {
                    s = 0.0;
                    if (tmp1 <= 0.0)
                    {
                        t = 1.0;
                        sqrdistance = c + 2.0 * e + f;
                    }
                    else
                    {
                        if (e >= 0.0)
                        {
                            t = 0.0;
                            sqrdistance = f;
                        }
                        else
                        {
                            t = -e / c;
                            sqrdistance = e * t + f;
                        }
                    }
                }
            }
            else
            {
                if (t < 0.0)
                {
                    double tmp0 = b + e;
                    double tmp1 = a + d;
                    if (tmp1 > tmp0)
                    {
                        double numer = tmp1 - tmp0;
                        double denom = a - 2.0 * b + c;
                        if (numer >= denom)
                        {
                            t = 1.0;
                            s = 0.0;
                            sqrdistance = c + 2.0 * e + f;
                        }
                        else
                        {
                            t = numer / denom;
                            s = 1.0 - t;
                            sqrdistance = s * (a * s + b * t + 2.0 * d) + t * (b * s + c * t + 2.0 * e) + f;
                        }
                    }
                    else
                    {
                        t = 0.0;
                        if (tmp1 <= 0.0)
                        {
                            s = 1.0;
                            sqrdistance = a + 2.0 * d + f;
                        }
                        else
                        {
                            if (d >= 0.0)
                            {
                                s = 0.0;
                                sqrdistance = f;
                            }
                            else
                            {
                                s = -d / a;
                                sqrdistance = d * s + f;
                            }
                        }
                    }
                }
                else
                {
                    double numer = c + e - b - d;
                    if (numer <= 0.0)
                    {
                        s = 0.0;
                        t = 1.0;
                        sqrdistance = c + 2.0 * e + f;
                    }
                    else
                    {
                        double denom = a - 2.0 * b + c;
                        if (numer >= denom)
                        {
                            s = 1.0;
                            t = 0.0;
                            sqrdistance = a + 2.0 * d + f;
                        }
                        else
                        {
                            s = numer / denom;
                            t = 1.0 - s;
                            sqrdistance = s * (a * s + b * t + 2.0 * d) + t * (b * s + c * t + 2.0 * e) + f;
                        }
                    }
                }
            }
        }

        if (sqrdistance < 0.0)
        {
            sqrdistance = 0.0;
        }

        double dist = Math.Sqrt(sqrdistance);
        double[] PP0 = Add(Add(B, Multiply(E0, s)), Multiply(E1, t));
        return (dist, PP0);
    }

    public static void Main()
    {
        double[,] TRI = { { 0.0, -1.0, 0.0 }, { 1.0, 0.0, 0.0 }, { 0.0, 0.0, 0.0 } };
        double[] P = { 0.5, -0.3, 0.5 };
        var (dist, pp0) = PointTriangleDistance(TRI, P);
        Console.WriteLine(dist);
        Console.WriteLine(string.Join(", ", pp0));
    }
}