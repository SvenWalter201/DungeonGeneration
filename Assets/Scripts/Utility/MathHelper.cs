using Unity.Mathematics;
using UnityEngine;

public static class MathHelper
{
    public static int getIndex(int x, int y, int width) =>
        x + y * width;

    public static int getIndex(int2 pos, int width) =>
        pos.x + pos.y * width;  

    public static int2 getPos(int index, int width)
    {
        int x = index % width;
        int y = index / width;

        return new int2(x, y);
    } 
    public static bool powerOfTwo(int x)
{
    return (x != 0) && ((x & (x - 1)) == 0);
}

    public static int sqrMagnitude(int2 a, int2 b)
    {
        int2 v = a - b;
        return v.x * v.x + v.y * v.y;
    }

    public static int2[] GetLine(int2 start, int2 end)
    {
        int x = start.x;
        int y = start.y;

        int dX = end.x - x;
        int dY = end.y - y;

        bool inverted = false;

        int step = (int)math.sign(dX);
        int gradientStep = (int)math.sign(dY);

        int longest = math.abs(dX);
        int shortest = math.abs(dY);

        if (longest < shortest)
        {
            inverted = true;
            (longest, shortest) = (shortest, longest);
            (step, gradientStep) = (gradientStep, step);
        }

        int gradientAccumulation = longest / 2;
        int2[] line =new int2[longest];
        for (int i = 0; i < longest; i++)
        {
            line[i] = new int2(x, y);
            if (inverted)
                y += step;
            else
                x += step;

            gradientAccumulation += shortest;

            if (gradientAccumulation >= longest)
            {
                if (inverted)
                    x += gradientStep;
                else
                    y += gradientStep;

                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    public static float2 ToXZF2(this Vector3 v3)
    {
        return new float2(v3.x, v3.z);
    }

    public static int2 clampInt2(this int2 i)
    {
        return new int2(math.clamp(i.x, -1,1), math.clamp(i.y, -1,1));
    }
    public static int2 int2M1() => new int2(-1,-1);
    public static Vector3 ToXZV3(this int2 p) => new Vector3(p.x, 0.0f, p.y);
    public static Vector3 ToXZV3Scaled(this int2 p, float scale) => new Vector3(p.x * scale, 0.0f, p.y * scale);

    public static bool isM1(this int2 p) => p.x == -1 && p.y == -1;
    public static bool IsPointInsideConvexPolygon(float2 p, float2[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            float2 p1 = points[i];
            float2 p2 = points[(i + 1) % points.Length];
            float2 edge = p2 - p1;
            float d = edge.x * (p.y - p1.y) - (p.x - p1.x) * edge.y;
            if(d < 0)
                return false;
        }
        return true;
    }

    public static int2[] directNeighborOffsets = new int2[]
    {
        new int2(1,0),
        new int2(-1,0),
        new int2(0,1),
        new int2(0,-1)
    };

    public static int2[] neighborOffsets = new int2[]
    {
        new int2(1,0),
        new int2(-1,0),
        new int2(0,1),
        new int2(0,-1),
        new int2(1,1),
        new int2(-1,1),
        new int2(1,-1),
        new int2(-1,-1)
    };

    public static bool isInBounds(int2 coord, int width, int height)
    {
        return coord.x >= 0 && coord.y >= 0 && coord.x < width && coord.y < height;
    }

    public static float GetAngleFromFloatVector(Vector2 vec)
    {
        vec = vec.normalized;
        float n = Mathf.Atan2(vec.y, vec.x) * Mathf.Rad2Deg;
        
        if (n < 0) 
            n += 360;
        
        return n;    
    }
}
