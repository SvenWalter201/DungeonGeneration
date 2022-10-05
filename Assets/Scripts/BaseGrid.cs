using System;
using UnityEngine;
using Unity.Mathematics;

using static MathHelper;

[Serializable]
public class BaseGrid<T>
{
    protected int width, height;
    public int Width => width;
    public int Height => height; 
    protected T[] values;
    public string name = "BaseGrid";

    public BaseGrid(){}

    public BaseGrid(int width, int height, Func<int, int, T> init)
    {
        this.width = width;
        this.height = height;

        values = new T[width * height];

        for (int y = 0, i = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++, i++)
            {
                values[i] = init(x, y);
            }
        }
    }

    public BaseGrid(int width, int height)
    {
        this.width = width;
        this.height = height;

        values = new T[width * height];
    }

    public BaseGrid(T[,] values)
    {
        width = values.GetLength(0);
        height = values.GetLength(1);

        this.values = new T[width * height];

        for (int y = 0, i = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++, i++)
            {
                this.values[i] = values[x, y];
            }
        }
    }

    public BaseGrid(int width, int height, T[] values)
    {
        this.width = width;
        this.height = height;
        this.values = values;
    }

    /*
    //TODO: Make safe for edge cases
    public BaseGrid<T> GetNeighborGrid(int2 position)
    {
        T[] values = new T[3 * 3];
        for (int y = position.y - 1, index = 0; y <= position.y + 1; y++)
        {
            for (int x = position.x - 1; x <= position.x + 1; x++, index++)
            {
                if (CheckBounds(x, y))
                    values[index] = GetValue(x, y);
            }
        }

        return new BaseGrid<T>(3, 3, values);
    }
    */

    public static int2[] directNeighborOffsets = new int2[]
    {
        new int2(0,1),
        new int2(0,-1),
        new int2(1,0),
        new int2(-1,0)
    };

    public static int2[] neighborOffsets = new int2[]
    {
        new int2(0,1),
        new int2(0,-1),
        new int2(1,0),
        new int2(-1,0),
        new int2(1,1),
        new int2(-1,1),
        new int2(1,-1),
        new int2(-1,-1),
    };

    T[] GetNeighbors(int2 pos, int2[] offsets, int neighborCount)
    {
        T[] neighbors = new T[neighborCount];
        for (int i = 0, index = 0; i < offsets.Length; i++)
        {
            int2 offsettedPos = pos + offsets[i];
            if(CheckBounds(offsettedPos))
                neighbors[index++] = GetValue(offsettedPos);
        }
        return neighbors;
    }

    public T[] GetAllNeighbors(int x, int y) => GetNeighbors(new int2(x,y), neighborOffsets, GetNeighborCount(new int2(x,y)));

    public T[] GetDirectNeighbors(int x, int y) => GetNeighbors(new int2(x,y), directNeighborOffsets, GetDirectNeighborCount(new int2(x,y)));

    int2[] GetNeighborCoordinates(int2 pos, int2[] offsets, int neighborCount)
    {
        int2[] coordinates = new int2[neighborCount];
        for (int i = 0, index = 0; i < offsets.Length; i++)
        {
            int2 offsettedPos = pos + offsets[i];
            if(CheckBounds(offsettedPos))
                coordinates[index++] = offsettedPos;
            
        }
        return coordinates;        
    }

    public int2[] GetAllNeighborsCoordinates(int x, int y) => 
        GetNeighborCoordinates(new int2(x,y), neighborOffsets, GetNeighborCount(new int2(x,y)));

    public int2[] GetDirectNeighborCoordinates(int x, int y) => 
        GetNeighborCoordinates(new int2(x,y), directNeighborOffsets, GetDirectNeighborCount(new int2(x,y)));
    

    public bool CheckBounds(int2 pos) =>
        CheckBounds(pos.x, pos.y);

    public bool CheckBounds(int x, int y) =>
        x >= 0 && x < Width && y >= 0 && y < Height;

    public int GetDirectNeighborCount(int2 pos)
    {
        int neighborCount = 4;
        if(pos.x == 0 || pos.x == width - 1)
        {
            --neighborCount;
            if(pos.y == 0 || pos.y == height - 1)
                --neighborCount;
        }
        else if(pos.y == 0 || pos.y == height - 1)
        {
            --neighborCount;
            if(pos.x == 0 || pos.x == width - 1)
               --neighborCount;
        }
        return neighborCount;
    }

    public int GetNeighborCount(int2 pos)
    {
        int neighborCount = 8;
        if(pos.x == 0 || pos.x == width - 1)
        {
            neighborCount-=3;
            if(pos.y == 0 || pos.y == height - 1)
                neighborCount--;
        }
        else if(pos.y == 0 || pos.y == height - 1)
        {
            neighborCount-=3;
            if(pos.x == 0 || pos.x == width - 1)
                neighborCount--;
        }
        return neighborCount;
    }

    public T GetValue(int x, int y) =>
        values[getIndex(new int2(x, y), width)];

    public T GetValue(int2 xy) =>
        values[getIndex(xy, width)];

    public void SetValue(T value, int x, int y) =>
        values[getIndex(new int2(x, y), width)] = value;

    public void SetValue(T value, int2 xy) =>
        values[getIndex(xy, width)] = value;

    public T[] GetCopy() =>
        (T[])values.Clone();

    public T[,] GetCopy2Dim()
    {
        T[,] copy = new T[width, height];
        for (int y = 0, i = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++, i++)
            {
                copy[x, y] = values[i];
            }
        }
        return copy;
    }

    public void SetAllValues(T[] _values) =>
        values = _values;

    public void Print()
    {
        Debug.Log(name);
        for (int y = 0; y < height; y++)
        {
            string s = "";
            for (int x = 0; x < width; x++)
            {
                s += GetValue(x, y).ToString();
                
                if(x < width - 1)
                    s += "|";                
            }
            Debug.Log(s);
        }
    }
}
