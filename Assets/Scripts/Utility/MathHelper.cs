using Unity.Mathematics;
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
}
