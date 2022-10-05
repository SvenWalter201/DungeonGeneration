using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

using static MathHelper;

/**
TODO: Somewhere there is an infinite loop. Spatial Partitioning Bug?

Widen can connect regions, but these connections can then be smaller than the defined min passge width
Possible Solution: 
    Do Widen so often until nothing changes anymore. Then it is guaranteed, that the result will be valid

Smoothing can invalidate the dungeon
Post Process Smoth can invalidate the dungeon

Run Widen afterward


Cellular Automata that gets influenced by more cells than just the direct neighbors
Image Kernels???

**/

public struct CaveGridSettings
{
    public int seed;
    public int partitionSize;
    public bool useSpacialPartitioning;
}

[Serializable]
public class CaveGrid : BaseGrid<bool>
{
    public const bool WALL = true, TERRAIN = false;

    public CaveGrid(int width, int height, CaveGridSettings settings) : base(width, height) 
    { 
        Initialize(settings); 
    }

    public CaveGrid(int width, int height, CaveGridSettings attributes, Func<int, int, bool> init) : base(width, height, init) 
    { 
        Initialize(attributes);
    }

    public CaveGrid(bool[,] values, CaveGridSettings attributes) : base(values) 
    { 
        Initialize(attributes);
    }

    public void Initialize(CaveGridSettings settings)
    {
        this.settings = settings;
        AllocatePersistentArrays(); 
    }


    CaveGridSettings settings;
    public int PartitionSize => settings.partitionSize;
    public int minWallSize, passageWayRadius, minTerrainWidth;

    NativeArray<int2> offsets, directOffsets;

    public void AllocatePersistentArrays()
    {
            offsets = new NativeArray<int2>(new int2[]
            {
                new int2(-1,-1),
                new int2(-1,0),
                new int2(-1,+1),
                new int2(0,-1),
                new int2(0,+1),
                new int2(+1,-1),
                new int2(+1,0),
                new int2(+1,+1)
            }, Allocator.Persistent);

            directOffsets = new NativeArray<int2>(new int2[]
            {
                new int2(-1,0),
                new int2(0,-1),
                new int2(0,+1),
                new int2(+1,0),
            }, Allocator.Persistent);
    }

    public void CleanupPersistentArrays()
    {
        if(offsets != default)
        {
            offsets.Dispose();
            offsets = default;
        }
        if(directOffsets != default)
        {
            directOffsets.Dispose();
            directOffsets = default;
        }
    }

    public void ValidatePersistentArrays()
    {
        if (offsets != default && directOffsets != default) 
            return;
        CleanupPersistentArrays();
        AllocatePersistentArrays();
    }

    public void Randomize(float fillPercentage)
    {
        var nativeGrid = new NativeArray<bool>(values, Allocator.TempJob);

        var job = new RandomizeJob()
        {
            grid = nativeGrid,
            fillPercentage = fillPercentage,
            hash = SmallXXHash.Seed(settings.seed)
        };
        job.Schedule(width * height, 64).Complete();
        values = job.grid.ToArray();
        nativeGrid.Dispose();
    }

    [BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]
    public struct RandomizeJob : IJobParallelFor
    {
        public NativeArray<bool> grid;
        public float fillPercentage;
        public SmallXXHash hash;

        public void Execute(int i)
        {
            uint h = hash.Eat(i);
            var hashFloat = 0.00392156862f * ((h >> 16) & 255);
            grid[i] = hashFloat > fillPercentage;
        }
    }

    public void SmoothSingle(int terrainMargin, int wallMargin, bool onlyDirectNeighbors = false)
    {
            ValidatePersistentArrays();
            var original = new NativeArray<bool>(values, Allocator.TempJob);
            var copy = new NativeArray<bool>(values, Allocator.TempJob);

            var smoothJob = new SmoothJob()
            {
                original = original,
                copy = copy,
                offsets = onlyDirectNeighbors ? directOffsets : offsets,
                height = height,
                width = width,
                terrainMargin = terrainMargin,
                wallMargin = wallMargin,
            };
            smoothJob.Schedule(width * height, 64).Complete();

            values = smoothJob.copy.ToArray();
            original.Dispose();
            copy.Dispose();
    }

    public void Smooth(int smoothingIterations, int terrainMargin, int wallMargin)
    {
        for (var i = 0; i < smoothingIterations; i++)
            SmoothSingle(terrainMargin, wallMargin);                  
    }

    static bool IsInsideGrid(int2 pos, int width, int height) => 
            pos.x >= 0 &&
            pos.y >= 0 &&
            pos.x < width &&
            pos.y < height;


    [BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]
    public struct SmoothJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<bool> original;
        [ReadOnly] public NativeArray<int2> offsets;
        public NativeArray<bool> copy;
        public int width, height, terrainMargin, wallMargin;
        public void Execute(int i)
        {
            var pos = getPos(i, width);
            var wallCount = 0;
            for (int o = 0; o < offsets.Length; o++)
            {
                var offsettedPos = pos + offsets[o];
                if(!CaveGrid.IsInsideGrid(offsettedPos, width, height))
                {
                    ++wallCount;
                    continue;
                }

                if(original[getIndex(offsettedPos, width)])
                    ++wallCount;
            }
            if(wallCount > wallMargin)
                copy[i] = true;
            if(wallCount < terrainMargin)
                copy[i] = false;
        }
    }


    public BaseGrid<int> numbers;

    public void GetDistanceToWallsGrid()
    {
        ValidatePersistentArrays();

        numbers = new BaseGrid<int>(width, height, (int x, int y) => -1);

        var original = new NativeArray<bool>(values, Allocator.TempJob);
        var numbersNative = new NativeArray<int>(numbers.GetCopy(), Allocator.TempJob);
        var numbersNativeCopy = new NativeArray<int>(numbers.GetCopy(), Allocator.TempJob);            

        var numbersJob = new NumbersInitializeJob()
        {
            original = original,
            numbersGrid = numbersNative,
            offsets = offsets,
            height = height,
            width = width
        };
        numbersJob.Schedule(width * height, 64).Complete();

        numbers.SetAllValues(numbersJob.numbersGrid.ToArray());
        numbersNativeCopy.CopyFrom(numbersNative);

        var maxPossible = width / 2;
        for (var i = 0; i < maxPossible; i++)
        {
            var iterJob = new NumbersIterateJob()
            {
                original = original,
                numbersGrid = numbersNative,
                numbersGridCopy = numbersNativeCopy,
                offsets = offsets,
                height = height,
                width = width,
                lastIterNumber = i + 1
            };
            iterJob.Schedule(width * height, 64).Complete();  

            numbersNative.CopyFrom(numbersNativeCopy);
        }

        numbers.SetAllValues(numbersNativeCopy.ToArray());
        numbersNative.Dispose();  
        numbersNativeCopy.Dispose();    
        original.Dispose();      
    }
    
    [BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]    
    public struct NumbersInitializeJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<bool> original;
        public NativeArray<int> numbersGrid;
        [ReadOnly] public NativeArray<int2> offsets;

        public int width, height;

        public void Execute(int i)
        {
            var pos = getPos(i, width);

            if(original[i])
                return;

            for (var o = 0; o < offsets.Length; o++)
            {
                var offsetPos = pos + offsets[o];
                if(!CaveGrid.IsInsideGrid(offsetPos, width, height))
                    continue;
                var index = MathHelper.getIndex(offsetPos.x, offsetPos.y, width);
                if (!original[index]) 
                    continue;
                
                numbersGrid[i] = 1;
                return;
            }
        }
    }

    [BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]    
    public struct NumbersIterateJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<bool> original;
        [ReadOnly] public NativeArray<int> numbersGrid;
        public NativeArray<int> numbersGridCopy;
        [ReadOnly] public NativeArray<int2> offsets;

        public int width, height, lastIterNumber;

        public void Execute(int i)
        {
            var pos = getPos(i, width);

            if(original[i])
                return;

            if(numbersGrid[i] >= 0)
                return;

            for (var o = 0; o < offsets.Length; o++)
            {
                var offsetPos = pos + offsets[o];
                if(!CaveGrid.IsInsideGrid(offsetPos, width, height))
                    continue;
                var index = MathHelper.getIndex(offsetPos.x, offsetPos.y, width);

                if (numbersGrid[index] != lastIterNumber) 
                    continue;
                
                numbersGridCopy[i] = lastIterNumber + 1;
                return;
            }
        }
    }
    

    public void WidenSingleIteration(bool onlyVisualize, int number)
    {
        ValidatePersistentArrays();
        GetDistanceToWallsGrid();

        var original = new NativeArray<bool>(values, Allocator.TempJob);
        var copy = new NativeArray<bool>(values, Allocator.TempJob);
        var numbersNative = new NativeArray<int>(numbers.GetCopy(), Allocator.TempJob);
        var numbersNativeCopy = new NativeArray<int>(numbers.GetCopy(), Allocator.TempJob);
        var widenNumbersJob = new WidenJob()
        {
            //original = original,
            copy = copy,
            numbersGrid = numbersNative,
            numbersGridCopy = numbersNativeCopy,
            directOffsets = directOffsets,
            height = height,
            width = width,
            number = number,
            onlyVisualize = onlyVisualize
        };
        widenNumbersJob.Schedule(width * height, 64).Complete();

        values = widenNumbersJob.copy.ToArray();

        if(onlyVisualize)
            numbers.SetAllValues(numbersNativeCopy.ToArray());

        original.Dispose();
        copy.Dispose();
        numbersNative.Dispose();
        numbersNativeCopy.Dispose();
    }

    [BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]    
    public struct WidenJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> numbersGrid;
        public NativeArray<int> numbersGridCopy;
        [NativeDisableParallelForRestriction] public NativeArray<bool> copy;
        [ReadOnly] public NativeArray<int2> directOffsets;

        public int width, height, number;
        public bool onlyVisualize;

        public void Execute(int i)
        {
            int2 pos = getPos(i, width);
            if(numbersGrid[i] != number)
                return;
        
            int numberOfEqualNeighbors = 0;

            for (int o = 0; o < directOffsets.Length; o++)
            {
                int2 offsettedPos = pos + directOffsets[o];
                if(!CaveGrid.IsInsideGrid(offsettedPos, width, height))
                    return; //for now
                int index = MathHelper.getIndex(offsettedPos.x, offsettedPos.y, width);

                if(numbersGrid[index] > number) //if any direct neighbor has a larger number, don't blow up
                    return;

                else if(numbersGrid[index] == number)
                    ++numberOfEqualNeighbors;
            }

            //0 and 1 equal neighbors do not interest us. 0 equal neighbors can be detected via. flood fill. 
            //1 equal neighbor will be handled by its neighbor
            if(numberOfEqualNeighbors < 2)
            {
                if(number == 1) //if it is a 1 and it has 1 or 0 neighbors, it is a pebble, that will get deleted
                    return;

                Blowup(i,pos);
            } 
            else if(numberOfEqualNeighbors > 2)
            {
                Blowup(i,pos);
            }    
            //check if curve or not, if not blow up, if yes, check if there is a curve adjacent. If yes, blow up
            else if(IsOutsideCurve(pos))
            {
                for (int o = 0; o < directOffsets.Length; o++)
                {
                    int2 offsettedPos = pos + directOffsets[o];
                    if(!CaveGrid.IsInsideGrid(offsettedPos, width, height))
                        break;
                    
                    int index = MathHelper.getIndex(offsettedPos.x, offsettedPos.y, width);

                    if(numbersGrid[index] != number)
                        continue; 
                    
                    if(IsOutsideCurve(offsettedPos))
                    {
                        Blowup(i,pos);
                        return;
                    }
                }
            }
            else 
            {
                Blowup(i,pos);
            }
        }

        bool IsOutsideCurve(int2 pos)
        {
            int2 directionSum = int2.zero;
            int sum = 0;
            for (int o = 0; o < directOffsets.Length; o++)
            {
                int2 offsettedPos = pos + directOffsets[o];
                if(!CaveGrid.IsInsideGrid(offsettedPos, width, height))
                    return false; //for now
                int index = MathHelper.getIndex(offsettedPos.x, offsettedPos.y, width);
                
                if(numbersGrid[index] > number) //this is not an outside curve if one of its neighbors is a larger value
                    return false;
                if(numbersGrid[index] == number)
                {
                    ++sum;
                    directionSum += directOffsets[o];
                }
            }

            if(sum != 2)
                return false;

            return (directionSum.x != 0 && directionSum.y != 0);     
        }

        void Blowup(int i, int2 pos)
        {
            if(onlyVisualize)
            {
                numbersGridCopy[i] = 99; //signal that this node would be blown up
                return;
            }
            for (int x = pos.x - number; x <= pos.x + number; x++)
            {
                for (int y = pos.y - number; y <= pos.y + number; y++)
                {
                    int2 offsettedPos = new int2(x,y);
                    if(!CaveGrid.IsInsideGrid(offsettedPos, width, height))
                        continue;
                    int index = MathHelper.getIndex(offsettedPos.x, offsettedPos.y, width);
                    copy[index] = false;
                }
            }
        }
    }

    //Floodfill from the start to find all nodes belonging to the same region
    public CaveRegion ProcessRegion(int2 start, ref bool[,] visited, int minRegionSize)
    {
        var regionTiles = new List<int2>();
        var edgeTiles = new List<int2>();
        var partitions = new List<int2>();
        var type = GetValue(start);
        var queue = new Queue<int2>();
        queue.Enqueue(start);
        visited[start.x,start.y] = true;
        while(queue.Count > 0)
        {
            int2 current = queue.Dequeue();
            regionTiles.Add(current);

            int2 currentPartition = current / settings.partitionSize;
            if(!partitions.Contains(currentPartition))
                partitions.Add(currentPartition);

            var neighborCoordinates = GetDirectNeighborCoordinates(current.x, current.y);
            bool isEdgeTile = false;
            for (int i = 0; i < neighborCoordinates.Length; i++)
            {
                int2 n = neighborCoordinates[i];
                if(!visited[n.x, n.y] && GetValue(n) == type)
                {
                    visited[n.x,n.y] = true;
                    queue.Enqueue(n);
                }
                if(GetValue(n) != type)
                    isEdgeTile = true;
            }

            if(isEdgeTile)
                edgeTiles.Add(current);
        }

        //Convert region, if it was too small
        if(regionTiles.Count < minRegionSize)
        {
            var fillValue = !type;
            for (int i = 0; i < regionTiles.Count; i++)
            {
                SetValue(fillValue, regionTiles[i]);
            }
            return null; //Don't return the region
        }

        var caveRegion = new CaveRegion(regionTiles, edgeTiles, partitions, this);
        return caveRegion;
    }

    public List<CaveRegion> ProcessRegions(bool type, int minRegionSize)
    {
        var regions = new List<CaveRegion>();
        var visited = new bool[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(!visited[x,y] && GetValue(x,y) == type)
                {
                    var region = ProcessRegion(new int2(x,y), ref visited, minRegionSize);                    
                    if(region != null)
                        regions.Add(region);
                }
            }
        }
        return regions;
    }

    public void ConnectionRegionsSingle(List<CaveRegion> regions, bool type)
    {
        var relevantRegions = regions;
        if(settings.useSpacialPartitioning)
        {
            relevantRegions = OnlyGetRelevantRegions(regions[0], regions);
            if(relevantRegions.Count == 0)
            {
                //Debug.Log("No relevant regions detected");
                return;
            }
        }

        if(ConnectionRegion(regions[0], relevantRegions, type))
            regions.RemoveAt(0);
    }

    const int maxIter = 300;
    public void ConnectRegions(bool type, int minRegionSize, List<CaveRegion> regions = null)
    {
        if(regions == null)
            regions = ProcessRegions(type, minRegionSize);

        //Debug.Log("Initial Region Count: "+ regions.Count);
        int currentIter = 0;
        while(regions.Count > 1 && currentIter < maxIter)
        {
            ConnectionRegionsSingle(regions, type);
            ++currentIter;
        }   
        if(currentIter > maxIter - 2)
        {
            Debug.LogWarning("Infite Loop aborted!");
        }
    }

    //use the spacial partitioning data to avoid comparing against rooms that are farther away
    public List<CaveRegion> OnlyGetRelevantRegions(CaveRegion currentRegion, List<CaveRegion> regions)
    {
        List<CaveRegion> relevantRegions = new List<CaveRegion>();
        int[] closestDistances = new int[regions.Count];
        int closestTotal = 9999;
        //use manhattan distance to check all the occupying clusters of a region against all other regions.
        for (int i = 0; i < regions.Count; i++)
        {
            int closestInOtherRegion = 9999;
            foreach (var partition in currentRegion.partitions)
            {
                foreach (var otherPartition in regions[i].partitions)
                {
                    int manhattan = math.abs(partition.x - otherPartition.x) + math.abs(partition.y - otherPartition.y);
                    if(manhattan < closestInOtherRegion)
                        closestInOtherRegion = manhattan;
                }
            }
            if(closestInOtherRegion < closestTotal)
                closestTotal = closestInOtherRegion;

            closestDistances[i] = closestInOtherRegion;
        }

        for (int i = 0; i < regions.Count; i++)
        {
            if(closestDistances[i] < closestTotal + 2 && regions[i] != currentRegion)
                relevantRegions.Add(regions[i]);    
        }
        return relevantRegions;
    }

    //connects a region with the closest region to it
    public bool ConnectionRegion(CaveRegion currentRegion, List<CaveRegion> regions, bool type)
    {
        NativeArray<int2>[] tempArrays = new NativeArray<int2>[regions.Count * 2];
        NativeArray<JobHandle> handles = new NativeArray<JobHandle>(regions.Count, Allocator.Temp);
        for (int i = 0; i < regions.Count; i++)
        {
            tempArrays[i * 2] = new NativeArray<int2>(currentRegion.edgeTiles.ToArray(), Allocator.TempJob);
            tempArrays[i * 2 + 1] = new NativeArray<int2>(regions[i].edgeTiles.ToArray(), Allocator.TempJob);

           var job = new FindConnectionPointsJob()
            {
                edgeTilesA = tempArrays[i * 2],
                edgeTilesB = tempArrays[i * 2 + 1],
            };
            handles[i] = job.Schedule();
        }

        JobHandle.CompleteAll(handles);

        int bestDistance = 999999;
        CaveRegion bestCandidate = null;
        int2 tileA = int2.zero, tileB = int2.zero;
        for (int i = 0; i < regions.Count; i++)
        {
            if(regions[i] == currentRegion)
                continue;

            NativeArray<int2> result = tempArrays[i*2];
            if(result[0].x < bestDistance)
            {
                bestDistance = result[0].x;
                bestCandidate = regions[i];
                tileA = new int2(result[1]);
                tileB = new int2(result[2]);
            }
        }
        foreach (var temp in tempArrays)
            temp.Dispose();
        handles.Dispose();   

        if(bestCandidate != null)
        {
            var connectionTiles = CreatePassage(tileA, tileB, type);
            bestCandidate.AbsorbRegion(currentRegion, connectionTiles);
            return true;
        }
        return false;    
    }

    [BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]
    public struct FindConnectionPointsJob : IJob
    {
        public NativeArray<int2> edgeTilesA;
        [ReadOnly] public NativeArray<int2> edgeTilesB;

        public void Execute()
        {
            int bestDistance = 999999;
            int2 tileA = int2.zero, tileB = int2.zero;
            for (int i = 0; i < edgeTilesA.Length; i++)
            {
                for (int j = 0; j < edgeTilesB.Length; j++)
                {
                    int distance = sqrMagnitude(edgeTilesA[i], edgeTilesB[j]);
                    if(distance < bestDistance)
                    {
                        bestDistance = distance;
                        tileA = edgeTilesA[i];
                        tileB = edgeTilesB[j];
                    }
                }
            }
            edgeTilesA[0] = new int2(bestDistance,0);
            edgeTilesA[1] = tileA;
            edgeTilesA[2] = tileB;
        }
    }

    public List<int2> CreatePassage(int2 tileA, int2 tileB, bool type)
    {
        List<int2> tiles = new List<int2>();
        int2[] line = GetLine(tileA, tileB);
        for (int i = 0; i < line.Length; i++)
            MakeCircleRegion(line[i], passageWayRadius, type, ref tiles);
        
        return tiles;
    }

    void MakeCircleRegion(int2 center, int radius, bool type, ref List<int2> tiles)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y > radius * radius)
                    continue;
                
                int2 pos = new int2(center.x + x, center.y + y);
                if (!tiles.Contains(pos) && CheckBounds(pos))
                { 
                    SetValue(type, pos);
                    tiles.Add(pos);
                }
            }
        }
    }
}

public class CaveRegion
{
    public List<int2> tiles,edgeTiles, partitions;
    public List<CaveRegion> connectedRegions;
    CaveGrid grid;
    public CaveRegion(List<int2> tiles, List<int2> edgeTiles, List<int2> partitions, CaveGrid grid)
    {
        this.tiles = tiles;
        this.edgeTiles = edgeTiles;
        this.partitions = partitions;
        this.grid = grid;
        connectedRegions = new List<CaveRegion>();
    }

    public void AbsorbRegion(CaveRegion other, List<int2> connectionTiles)
    {
        tiles.AddRange(other.tiles); //add the tiles of the other region into this Region

        tiles.Capacity += connectionTiles.Count;
        foreach (var tile in connectionTiles) //add the tiles of the connection into this region, if they are not already contained
        {
            if(!tiles.Contains(tile))
            {
                tiles.Add(tile);
                int2 partition = tile / grid.PartitionSize;
                if(!partitions.Contains(partition))
                    partitions.Add(partition);
            }
        }
        tiles.TrimExcess();

        foreach (var partition in other.partitions) //add the partitions of the other region into this region
        {
            if(!partitions.Contains(partition))
                partitions.Add(partition);
        }

        RecalculateEdgeTiles(); //recalculate the edgetiles
    }

    public void RecalculateEdgeTiles()
    {
        edgeTiles.Clear();

        foreach (var tile in tiles)
        {
            bool[] directNeighbors = grid.GetDirectNeighbors(tile.x, tile.y);
            for (int i = 0; i < directNeighbors.Length; i++)
            {
                if (directNeighbors[i])
                {
                    edgeTiles.Add(tile);
                    break;
                }
            }
        }
    }
}




/*

    public void WidenNarrowWalkways()
    {
            ValidatePersistentArrays();

            NativeArray<bool> original = new NativeArray<bool>(values, Allocator.TempJob);
            NativeArray<bool> copy = new NativeArray<bool>(values, Allocator.TempJob);

            WidenJob widenJob = new WidenJob()
            {
                original = original,
                copy = copy,
                offsets = offsets,
                directOffsets = directOffsets,
                height = height,
                width = width
            };
            widenJob.Schedule(width * height, 64).Complete();

            values = widenJob.copy.ToArray();
            original.Dispose();
            copy.Dispose();
    }

    [BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]
    public struct WidenJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<bool> original;
        [ReadOnly] public NativeArray<int2> offsets;
        [ReadOnly] public NativeArray<int2> directOffsets;
        [NativeDisableParallelForRestriction] public NativeArray<bool> copy;
        public int width, height;
        public void Execute(int i)
        {
            int2 pos = getPos(i, width);
            if(original[i])
                return;

            int2 accumulatedDir = int2.zero;
            int wallAmount = 0;
            for (int o = 0; o < offsets.Length; o++)
            {
                int2 offsettedPos = pos + offsets[o];
                if(!CaveGrid.IsInsideGrid(offsettedPos, width, height))
                    continue;
                
                if(original[getIndex(offsettedPos, width)])
                {
                    accumulatedDir += offsets[o];
                    ++wallAmount;
                }
            }

            if(wallAmount <= 1)
                return;
                
            int absSum = math.abs(accumulatedDir.x) + math.abs(accumulatedDir.y);
            if(absSum <= 2)
            {
                for (int o = 0; o < offsets.Length; o++)
                {
                    int2 offsettedPos = pos + offsets[o];
                    if(!CaveGrid.IsInsideGrid(offsettedPos, width, height))
                        continue;
                    
                    copy[getIndex(offsettedPos, width)] = false;
                }                
            }

        }
    }

    public void WidenArbitrary()
    {
        ValidatePersistentArrays();

        for (int i = 0; i < minPassageWidth; i++)
        {
            NativeArray<bool> original = new NativeArray<bool>(values, Allocator.TempJob);
            NativeArray<bool> copy = new NativeArray<bool>(values, Allocator.TempJob);

            WidenArbitraryJob widenJob = new WidenArbitraryJob()
            {
                original = original,
                copy = copy,
                directOffsets = directOffsets,
                height = height,
                width = width,
                radius = i + 1
            };
            widenJob.Schedule(width * height, 64).Complete();

            values = widenJob.copy.ToArray();
            original.Dispose();
            copy.Dispose();
            PostProcessSmoothSingle();  
        }
            
    }
    [BurstCompile(FloatPrecision.High, FloatMode.Fast, CompileSynchronously = true)]
    public struct WidenArbitraryJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<bool> original;
        public NativeArray<bool> copy;
        [ReadOnly] public NativeArray<int2> directOffsets;

        public int width, height, radius;

        bool IsInsideRadius(int2 pos, int2 originalPos)
        {
            int a = math.abs(pos.x - originalPos.x);
            int b =  math.abs(pos.y - originalPos.y);
            return a * a + b * b <= radius * radius;
        }

        public void Execute(int i)
        {
            //only examine walls
            if(!original[i])
                return;

            int2 pos = getPos(i, width);

            for (int o = 0; o < directOffsets.Length; o++)
            {
                bool foundTerrain = false;
                for (int r = 1; r <= radius; r++)
                {
                    int2 offsettedPos = pos + directOffsets[o] * r;
                    if(!CaveGrid.IsInsideGrid(offsettedPos, width, height))
                        break;

                    int index = getIndex(offsettedPos, width);
                    if(!original[index]) //check if value is not a wall
                    {
                        foundTerrain = true;
                    }
                    else 
                    {
                        if(foundTerrain) //found wall after terrain!
                        {
                            copy[i] = false;
                            return;
                        }
                    }
                }
            }
            {

            return;
            int iterationsRemaining = (radius * 2 + 1) * (radius * 2 + 1);

            NativeList<int2> region = new NativeList<int2>(iterationsRemaining, Allocator.Temp);
            NativeList<int2> queue = new NativeList<int2>(iterationsRemaining, Allocator.Temp);
            
            queue.Add(pos);
            region.Add(pos);

            while(queue.Length > 0 && iterationsRemaining >= 0)
            {
                int last = queue.Length - 1;
                int2 current = queue[last];
                queue.RemoveAtSwapBack(last);

                for (int o = 0; o < directOffsets.Length; o++)
                {
                    int2 offsettedPos = current + directOffsets[o];
                    if(
                        !IsInsideGrid(offsettedPos) || 
                        !IsInsideRadius(offsettedPos, pos) ||
                        region.Contains(offsettedPos) //check if value is already visited
                    )
                        continue;
                    
                    if(!original[getIndex(offsettedPos, width)]) //check if value is not a wall
                        continue;

                    queue.Add(offsettedPos);
                    region.Add(offsettedPos);
                }

                --iterationsRemaining;
            }


            bool foundUncontained = false;
            for (int x = pos.x - radius; x <= pos.x + radius; x++)
            {
                for (int y = pos.y - radius; y <= pos.y + radius; y++)
                {
                    int2 offsettedPos = new int2(x,y);
                    if(!IsInsideGrid(offsettedPos) || !IsInsideRadius(offsettedPos, pos))
                        continue;
                    
                    int index = getIndex(offsettedPos, width);
                    if(!original[index]) //check if value is not a wall
                        continue;    

                    if(!region.Contains(offsettedPos))
                    {
                        copy[i] = false;
                        foundUncontained = true;
                        break;
                    }            
                }

                if(foundUncontained)
                    break;
            }
            queue.Dispose();
            region.Dispose();
            
            }
            
        }
    }



*/