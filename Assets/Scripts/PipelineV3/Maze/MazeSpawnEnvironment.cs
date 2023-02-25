using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace PipelineV3.Maze
{
    public class MazeSpawnEnvironment : SpawnEnvironment
    {
        public Cell[] grid;
        public int blockedCellsAmount = 0; 
        public int AvailableCellsAmount => MazeBuilderMetrics.CellAmount - blockedCellsAmount;

        public override void Initialize()
        {
            grid = new Cell[MazeBuilderMetrics.CellAmount];
            for (int y = 0, idx = 0; y < MazeBuilderMetrics.HEIGHT; y++)
            {
                for (int x = 0; x < MazeBuilderMetrics.WIDTH; x++, idx++)
                {
                    grid[idx] = new Cell(x,y);
                }
            }
            blockedCellsAmount = 0;
        }

        public override void Clear()
        {
            foreach (var cell in grid)
                cell.occupied = false;
            
            blockedCellsAmount = 0;
        }

        public override void Spawn(string type, DesignElement dE)
        {
            //switch over possible dE types
            switch(type)
            {
                case "OccupiedCell":
                {
                    var cellDE = (OccupiedCellMazeDesignElement)dE;
                    var cell = grid[MazeBuilderMetrics.GetIndex(cellDE.x, cellDE.y)];
                    if(!cell.occupied)
                    {
                        cell.occupied = true;
                        blockedCellsAmount++;
                    }
                    break;
                }
                case "Wall":
                {
                    var wallDE = (MazeWallDesignElement)dE;
                    var offset = wallDE.horizontal ? new int2(1,0) : new int2(0,1);
                    for (int i = 0; i < wallDE.length; i++)
                    {
                        var offsetCoordinate = wallDE.startPosition + offset * i;
                        if(!MathHelper.isInBounds(offsetCoordinate, MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT))
                            break;
                        var cell = grid[MazeBuilderMetrics.GetIndex(offsetCoordinate.x, offsetCoordinate.y)];

                        if(!cell.occupied)
                        {
                            cell.occupied = true;
                            blockedCellsAmount++;
                        }
                    }

                    break;
                }
                case "Room":
                {
                    var roomDE = (MazeRoomDesignElement)dE;
                    for (int x = 0; x < roomDE.width; x++)
                    {
                        for (int y = 0; y < roomDE.height; y++)
                        {
                            var currentPosition = new int2(x,y) + roomDE.lLPosition;
                            var cell = grid[MazeBuilderMetrics.GetIndex(currentPosition.x, currentPosition.y)];

                            //WALL
                            if(x == 0 || y == 0 || x == roomDE.width - 1 || y == roomDE.height - 1)
                            {
                                if(!cell.occupied)
                                    blockedCellsAmount++;

                                cell.occupied = true;                                
                                continue;
                            }

                            if(cell.occupied)
                                blockedCellsAmount--;

                            cell.occupied = false;
                        }
                    }
                    for (int i = 0; i < roomDE.doorPositions.Count; i++)
                    {
                        var currentDoorPosition = roomDE.doorPositions[i];
                        var cell = grid[MazeBuilderMetrics.GetIndex(currentDoorPosition.x, currentDoorPosition.y)];

                        if(cell.occupied)
                            blockedCellsAmount--;

                        cell.occupied = false;
                    }
                    break;
                }
            }
        }

        public override SpawnEnvironment Clone()
        {
            var clone = new MazeSpawnEnvironment();
            var gridClone = new Cell[MazeBuilderMetrics.CellAmount];

            for (int i = 0; i < grid.Length; i++)
                gridClone[i] = grid[i].Clone();
                    
            clone.grid = gridClone;
            clone.blockedCellsAmount = blockedCellsAmount;
            return clone;
        }

        public override void FinalizeEnvironment()
        {
            CalculateDistanceToStartCell();
            if(DetermineOptimalPath())
                DetermineCulDeSacs();        
        }
        const int maxIter = 10000;

        public void CalculateDistanceToStartCell()
        {
            foreach(var cell in grid)
            {
                cell.distanceToStart = -1;
                cell.visited = false;
                cell.partOfOptimalPath = false;
                cell.isCulDeSac = false;
            }
            var offsets = MathHelper.directNeighborOffsets;
            var queued = new Queue<Cell>();
            var startCell = grid[0];
            startCell.distanceToStart = 0;
            startCell.visited = true;
            queued.Enqueue(grid[0]);

            int it = 0;
            while(queued.Count > 0 && it < maxIter)
            {
                var current = queued.Dequeue();
                var currentDistance = current.distanceToStart;

                for (int i = 0; i < offsets.Length; i++)
                {
                    if(MathHelper.isInBounds(new int2(current.X + offsets[i].x, current.Y + offsets[i].y), MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT))
                    {
                        var neighborIdx = MazeBuilderMetrics.GetIndex(current.X + offsets[i].x, current.Y + offsets[i].y);
                        var neighborCell = grid[neighborIdx];
                        if(neighborCell.occupied)
                            continue;

                        if(neighborCell.visited)
                            continue;

                        queued.Enqueue(neighborCell);
                        neighborCell.visited = true;
                        neighborCell.distanceToStart = currentDistance + 1;
                    }
                }

                it++;
            }
            if(it >= maxIter)
                Debug.LogWarning("Max Iter Reached: CalculateDistanceToStartCell");
        }

        public bool DetermineOptimalPath()
        {
            var cell = grid[grid.Length - 1];
            if(cell.distanceToStart == -1)
                return false; //no path exists

            var offsets = MathHelper.directNeighborOffsets;

            int it = 0;
            while(cell.distanceToStart > 0 && it < maxIter)
            {
                for (int i = 0; i < offsets.Length; i++)
                {
                    if(!MathHelper.isInBounds(new int2(cell.X + offsets[i].x, cell.Y + offsets[i].y), MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT))
                    continue;
                    
                    var neighborIdx = MazeBuilderMetrics.GetIndex(cell.X + offsets[i].x, cell.Y + offsets[i].y);
                    var neighborCell = grid[neighborIdx]; 
                    if(neighborCell.occupied)
                        continue;

                    if(neighborCell.distanceToStart < cell.distanceToStart)     
                    {
                        neighborCell.partOfOptimalPath = true;
                        cell = neighborCell;
                    }      
                }
            }

            if(it >= maxIter)
                Debug.LogWarning("Max Iter Reached: DetermineOptimalPath");

            return true;
        }

        public void DetermineCulDeSacs()
        {
            var offsets = MathHelper.directNeighborOffsets;

            foreach(var cell in grid)
            {
                if(cell.distanceToStart == -1)
                    continue;

                if(cell.X == MazeBuilderMetrics.WIDTH - 1 && cell.Y == MazeBuilderMetrics.HEIGHT - 1)
                    continue;

                bool neighborWithWorseDistanceExists = false, nearbyWallExists = false;
                for (int i = 0; i < offsets.Length; i++)
                {
                    if(!MathHelper.isInBounds(new int2(cell.X + offsets[i].x, cell.Y + offsets[i].y), MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT))
                        continue;
                    
                    var neighborIdx = MazeBuilderMetrics.GetIndex(cell.X + offsets[i].x, cell.Y + offsets[i].y);
                    var neighborCell = grid[neighborIdx];
                    if(neighborCell.distanceToStart > cell.distanceToStart)
                    {
                        neighborWithWorseDistanceExists = true;
                        break;
                    }
                    if(neighborCell.occupied)
                        nearbyWallExists = true;
                }

                if(!neighborWithWorseDistanceExists && nearbyWallExists)
                {
                    cell.isCulDeSac = true;
                    if(cell.partOfOptimalPath)
                    {
                        cell.culDeSacLength = 0;
                        continue;
                    }

                    int distanceToOptimalPath = 0;

                    var current = cell;
                    
                    int it = 0;
                    while(it < maxIter)
                    {
                        bool foundOptimalPath = false;
                        for (int i = 0; i < offsets.Length; i++)
                        {
                            if(!MathHelper.isInBounds(new int2(current.X + offsets[i].x, current.Y + offsets[i].y), MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT))
                            continue;
                            
                            var neighborIdx = MazeBuilderMetrics.GetIndex(current.X + offsets[i].x, current.Y + offsets[i].y);
                            var neighborCell = grid[neighborIdx]; 
                            if(neighborCell.occupied)
                                continue;


                            if(neighborCell.distanceToStart < current.distanceToStart)     
                            {
                                current = neighborCell;
                                distanceToOptimalPath++;
                                if(neighborCell.partOfOptimalPath)
                                {
                                    foundOptimalPath = true;
                                    break;
                                }
                            }      
                        }

                        if(foundOptimalPath)
                        {
                            cell.culDeSacLength = distanceToOptimalPath;
                            break;
                        }

                        it++;
                    }

                    if(it >= maxIter)
                        Debug.LogWarning("maxIter reached: DetermineCulDeSacs");                
                }
            } 
        }     
    }
}