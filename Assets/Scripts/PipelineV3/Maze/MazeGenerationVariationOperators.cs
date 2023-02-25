using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace PipelineV3.Maze
{
    public class MazeMutation : Mutation
    {
        static bool[,] occupation;

        protected override void MutateImpl(GenericLevel genericLevel)
        {
            List<OccupiedCellMazeDesignElement> occupiedCellDEs = null;
            List<MazeWallDesignElement> wallDEs = null; 
            List<MazeRoomDesignElement> roomDEs = null;


            //TILES
            {
                //mutate overall level structure
                float addTile = UnityEngine.Random.Range(0f,1f);
                float removeTile = UnityEngine.Random.Range(0f,1f);
                while(addTile < MazeBuilderMetrics.TILE_ADD_PROBABILITY)
                {
                    mutationsAttempted++;
                    if(genericLevel.AddDesignElement(new OccupiedCellMazeDesignElement(genericLevel)))
                        mutationsAccepted++;
                    addTile = UnityEngine.Random.Range(0f,1f);
                }

                occupiedCellDEs = genericLevel.GetDesignElementsOfType<OccupiedCellMazeDesignElement>();

                if(occupiedCellDEs != null)
                {
                    if(removeTile < MazeBuilderMetrics.TILE_REMOVE_PROBABILITY)
                    {
                        var selectedDE = occupiedCellDEs[UnityEngine.Random.Range(0, occupiedCellDEs.Count)];
                        genericLevel.RemoveDesignElement(selectedDE);
                        occupiedCellDEs.Remove(selectedDE);
                        AddSuccessfulMutation();
                    }
                    occupiedCellDEs = genericLevel.GetDesignElementsOfType<OccupiedCellMazeDesignElement>();

                    //mutate individual design elements
                    foreach (var de in occupiedCellDEs)
                        EvaluateMutationResult(de.Mutation());
                    
                    occupiedCellDEs = genericLevel.GetDesignElementsOfType<OccupiedCellMazeDesignElement>();

                    //remove duplicates
                    if(occupation == null)
                        occupation = new bool[MazeBuilderMetrics.WIDTH,MazeBuilderMetrics.HEIGHT];
                    
                    for (int y = 0; y < MazeBuilderMetrics.HEIGHT; y++)
                    {
                        for (int x = 0; x < MazeBuilderMetrics.WIDTH; x++)
                        {
                            occupation[x,y] = false;
                        }
                    }

                    foreach (var de in occupiedCellDEs)
                    {
                        if(occupation[de.x,de.y])
                            genericLevel.RemoveDesignElement(de);
                        
                        else
                            occupation[de.x,de.y] = true;
                    }

                    occupiedCellDEs = genericLevel.GetDesignElementsOfType<OccupiedCellMazeDesignElement>();
                }
            }

            //WALLS
            {
                //mutate overall level structure
                float addWall = UnityEngine.Random.Range(0f,1f);
                float removeWall = UnityEngine.Random.Range(0f,1f);

                if(addWall < MazeBuilderMetrics.WALL_ADD_PROBABILITY)
                {
                    for (int i = 0; i < 4000; i++)
                    {
                        var grid = ((MazeSpawnEnvironment)genericLevel.spawnEnvironment).grid;
                        var r = UnityEngine.Random.Range(0, grid.Length);
                        var chosenCell = grid[r];
                        if(!chosenCell.occupied)
                            continue;
                        
                        int2 topOffset = new int2(chosenCell.X, chosenCell.Y + 1);
                        if(!MathHelper.isInBounds(topOffset, MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT))
                            continue;
                        var topNeighbor = MazeBuilderMetrics.GetIndex(topOffset.x, topOffset.y);
                        bool topOccupied = grid[topNeighbor].occupied;

                        int2 rightOffset = new int2(chosenCell.X + 1, chosenCell.Y);
                        if(!MathHelper.isInBounds(rightOffset, MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT))
                            continue;
                        var rightNeighbor = MazeBuilderMetrics.GetIndex(rightOffset.x, rightOffset.y);
                        bool rightOccupied = grid[rightNeighbor].occupied;

                        if(rightOccupied && topOccupied)
                            continue;
                        
                        bool horizontal = topOccupied ? true : (rightOccupied ? true : UnityEngine.Random.Range(0f, 1f) > 0.5f);
                        mutationsAttempted++;
                        if(genericLevel.AddDesignElement(new MazeWallDesignElement(genericLevel, new int2(chosenCell.X, chosenCell.Y), horizontal)))
                            mutationsAccepted++;

                        break;
                    }
                }
                
                wallDEs = genericLevel.GetDesignElementsOfType<MazeWallDesignElement>();
                if(wallDEs != null)
                {
                    if(removeWall < MazeBuilderMetrics.WALL_REMOVE_PROBABILITY)
                    {
                        if(wallDEs.Count > 0)
                        {
                            var selectedDE = wallDEs[UnityEngine.Random.Range(0, wallDEs.Count)];
                            genericLevel.RemoveDesignElement(selectedDE);
                            wallDEs.Remove(selectedDE);
                            AddSuccessfulMutation();
                        }

                    }

                    //mutate individual design elements
                    foreach (var de in wallDEs)
                        EvaluateMutationResult(de.Mutation());
                    

                    wallDEs = genericLevel.GetDesignElementsOfType<MazeWallDesignElement>();
                }
            }

            //ROOMS
            {
                float addRoom = UnityEngine.Random.Range(0f,1f);
                float removeRoom = UnityEngine.Random.Range(0f,1f);

                if(addRoom < MazeBuilderMetrics.ROOM_ADD_PROBABILITY)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        if(genericLevel.AddDesignElement(new MazeRoomDesignElement(genericLevel)))
                        {
                            AddSuccessfulMutation();
                            break;
                        }
                    }
                }
                
                roomDEs = genericLevel.GetDesignElementsOfType<MazeRoomDesignElement>();
                if(roomDEs != null)
                {
                    if(removeRoom < MazeBuilderMetrics.ROOM_REMOVE_PROBABILITY)
                    {
                        if(roomDEs.Count > 0)
                        {
                            var selectedDE = roomDEs[UnityEngine.Random.Range(0, roomDEs.Count)];
                            genericLevel.RemoveDesignElement(selectedDE);
                            roomDEs.Remove(selectedDE);
                            AddSuccessfulMutation();
                        }
                    }

                    //mutate individual design elements
                    foreach (var de in roomDEs)
                        EvaluateMutationResult(de.Mutation());
                    

                    roomDEs = genericLevel.GetDesignElementsOfType<MazeRoomDesignElement>();
                }
            }

            if(mutationsAccepted > 0)
            {
                genericLevel.spawnEnvironment.Clear();

                if(occupiedCellDEs != null)
                    foreach (var de in occupiedCellDEs)
                        de.Spawn();

                if(wallDEs != null)
                    foreach (var de in wallDEs)
                        de.Spawn();
                
                if(roomDEs != null)
                    foreach(var de in roomDEs)
                        de.Spawn();
                
                genericLevel.spawnEnvironment.FinalizeEnvironment();
            }
        }
    }

    public class MazeCrossover : Crossover
    {
        public List<DesignElement> Crossover<T>(GenericLevel lhs, GenericLevel rhs, GenericLevel child) where T : DesignElement
        {
            float r = UnityEngine.Random.Range(0.0f, 1.0f);
            if(r < 0.5f)
            {
                (lhs, rhs) = (rhs, lhs);
            }

            var lhsDEs = lhs.GetDesignElementsOfType<T>(); //first half of this list //also make deep copy
            var rhsDEs = rhs.GetDesignElementsOfType<T>(); //second half of this list //also make deep copy
            

            int lhsHalfLength = lhsDEs == null ? 0 : lhsDEs.Count / 2; //31 - 15
            int rhsHalfLength = rhsDEs == null ? 0 : rhsDEs.Count / 2; //60 - 30
            int newLength = lhsHalfLength + rhsHalfLength; //45


            var newDEs = new List<DesignElement>(newLength);

            if(lhsDEs != null)
            {
                for (int i = 0; i < lhsHalfLength; i++)
                    newDEs.Add(lhsDEs[i].Clone(child));
            }

            if(rhsDEs != null)
            {
                for (int i = 0; i < rhsHalfLength; i++)
                    newDEs.Add(rhsDEs[i + rhsHalfLength].Clone(child));
            }

            return newDEs;
        }

        public List<DesignElement> RandomCrossover<T>(GenericLevel lhs, GenericLevel rhs, GenericLevel child) where T : DesignElement
        {
            var lhsDEs = lhs.GetDesignElementsOfType<T>(); //first half of this list //also make deep copy
            var rhsDEs = rhs.GetDesignElementsOfType<T>(); //second half of this list //also make deep copy
            

            int lhsHalfLength = lhsDEs == null ? 0 : lhsDEs.Count / 2; //31 - 15
            int rhsHalfLength = rhsDEs == null ? 0 : rhsDEs.Count / 2; //60 - 30
            int newLength = lhsHalfLength + rhsHalfLength; //45
            var newDEs = new List<DesignElement>(newLength);
            if(lhsDEs != null)
            {
                for (int i = 0; i < lhsHalfLength; i++)
                {
                    var r = UnityEngine.Random.Range(0, lhsDEs.Count);
                    newDEs.Add(lhsDEs[r].Clone(child));
                }
            }
            if(rhsDEs != null)
            {
                for (int i = 0; i < rhsHalfLength; i++)
                {
                    var r = UnityEngine.Random.Range(0, rhsDEs.Count);
                    newDEs.Add(rhsDEs[r].Clone(child));
                } 
            }

            return newDEs;
        }

        public void CellCrossover(GenericLevel lhs, GenericLevel rhs, GenericLevel child, bool horizontal)
        {
            if(UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f)
                (lhs, rhs) = (rhs, lhs);

            var lhsDEs = lhs.GetDesignElementsOfType<OccupiedCellMazeDesignElement>(); //first half of this list //also make deep copy
            var rhsDEs = rhs.GetDesignElementsOfType<OccupiedCellMazeDesignElement>(); //second half of this list //also make deep copy
            if(horizontal)
            {
                int halfHeight = Mathf.RoundToInt(MazeBuilderMetrics.HEIGHT / (float)2);
                if(lhsDEs != null)
                {
                    foreach (var cell in lhsDEs)
                    {
                        if(cell.y > halfHeight)
                            child.AddDesignElement(cell.Clone(child));
                    }
                }

                if(lhsDEs != null)
                {
                    foreach (var cell in rhsDEs)
                    {
                        if(cell.y <= halfHeight)
                            child.AddDesignElement(cell.Clone(child));
                    }
                }

            }
            else
            {
                int halfWidth = Mathf.RoundToInt(MazeBuilderMetrics.WIDTH / (float)2);
                if(lhsDEs != null)
                {
                    foreach (var cell in lhsDEs)
                    {
                        if(cell.x > halfWidth)
                            child.AddDesignElement(cell.Clone(child));

                    }
                }

                if(rhsDEs != null)
                {
                    foreach (var cell in rhsDEs)
                    {
                        if(cell.x <= halfWidth)
                            child.AddDesignElement(cell.Clone(child));
                    } 
                }
               
            }
        }

        public void WallCrossover(GenericLevel lhs, GenericLevel rhs, GenericLevel child, bool horizontal)
        {
            if(UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f)
                (lhs, rhs) = (rhs, lhs);

            var lhsDEs = lhs.GetDesignElementsOfType<MazeWallDesignElement>();
            var rhsDEs = rhs.GetDesignElementsOfType<MazeWallDesignElement>();

            if(horizontal)
            {
                int halfHeight = Mathf.RoundToInt(MazeBuilderMetrics.HEIGHT / (float)2);
                if(lhsDEs != null)
                {
                    //upper half, always add wall
                    foreach (var wall in lhsDEs)
                    {
                        if(wall.startPosition.y <= halfHeight)
                            continue;

                            child.AddDesignElement(wall.Clone(child));
                    }
                }

                if(rhsDEs != null)
                {
                    foreach (var wall in rhsDEs)
                    {
                        if(wall.startPosition.y > halfHeight)
                            continue;
                        
                        var highPoint = wall.startPosition.y + wall.length;
                        if(wall.horizontal || (highPoint) <= halfHeight)
                        {
                            child.AddDesignElement(wall.Clone(child));
                            continue;
                        }

                        var splitWall = wall.Clone(child);
                        //20
                        //15 + 12 //overlap 7
                        var lengthReduction = highPoint - halfHeight;
                        ((MazeWallDesignElement)splitWall).length -= lengthReduction;
                        child.AddDesignElement(splitWall);
                    }
                }

            }
            else
            {
                int halfWidth = Mathf.RoundToInt(MazeBuilderMetrics.WIDTH / (float)2);
                if(lhsDEs != null)
                {
                    //right half, always add wall
                    foreach (var wall in lhsDEs)
                    {
                        if(wall.startPosition.x <= halfWidth)
                            continue;

                        child.AddDesignElement(wall.Clone(child));
                    }
                }

                if(rhsDEs != null)
                {
                    foreach (var wall in rhsDEs)
                    {
                        if(wall.startPosition.x > halfWidth)
                            continue;
                        
                        var rightPoint = wall.startPosition.x + wall.length;
                        if(!wall.horizontal || (rightPoint) <= halfWidth)
                        {
                            child.AddDesignElement(wall.Clone(child));
                            continue;
                        }

                        var splitWall = wall.Clone(child);
                        var lengthReduction = rightPoint - halfWidth;
                        ((MazeWallDesignElement)splitWall).length -= lengthReduction;
                        child.AddDesignElement(splitWall);
                    }
                }
            }
        }

        public void RoomCrossover(GenericLevel lhs, GenericLevel rhs, GenericLevel child, bool horizontal)
        {
            if(UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f)
                (lhs, rhs) = (rhs, lhs);

            var lhsDEs = lhs.GetDesignElementsOfType<MazeRoomDesignElement>();
            var rhsDEs = rhs.GetDesignElementsOfType<MazeRoomDesignElement>();
            
            if(horizontal)
            {
                int halfHeight = Mathf.RoundToInt(MazeBuilderMetrics.HEIGHT / (float)2);
                if(lhsDEs != null)
                {
                    //upper half, always add room
                    foreach (var room in lhsDEs)
                    {
                        if(room.lLPosition.y + (room.height / 2) > halfHeight)
                            child.AddDesignElement(room.Clone(child));
                    }
                }

                if(rhsDEs != null)
                {
                    foreach (var room in rhsDEs)
                    {                        
                        var halfHighPoint = room.lLPosition.y + (room.height / 2);
                        if(halfHighPoint <= halfHeight)
                            child.AddDesignElement(room.Clone(child));
                    }
                }

            }
            else
            {
                int halfWidth = Mathf.RoundToInt(MazeBuilderMetrics.WIDTH / (float)2);
                if(lhsDEs != null)
                {
                    //right half, always add wall
                    foreach (var room in lhsDEs)
                    {
                        if(room.lLPosition.x + (room.width / 2) > halfWidth)
                            child.AddDesignElement(room.Clone(child));
                    }
                }

                if(rhsDEs != null)
                {
                    foreach (var room in rhsDEs)
                    {                        
                        var halfRightPoint = room.lLPosition.x + (room.width / 2);
                        if(halfRightPoint <= halfWidth)
                            child.AddDesignElement(room.Clone(child));
                    }
                }     
            }
        }

        public override GenericLevel CrossoverFunc(GenericLevel lhs, GenericLevel rhs)
        {
            //do some crossover shenanigans
            var spawnEnvironment = new MazeSpawnEnvironment();
            spawnEnvironment.Initialize();

            var child = new GenericLevel(spawnEnvironment, new MazeMutation(), new MazeCrossover());
            var horizontal = UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f;

            CellCrossover(lhs, rhs, child, horizontal);
            WallCrossover(lhs, rhs, child, horizontal);
            RoomCrossover(lhs, rhs, child, horizontal);

            var newTileDEs = child.GetDesignElementsOfType<OccupiedCellMazeDesignElement>();
            var newWallDEs = child.GetDesignElementsOfType<MazeWallDesignElement>();
            var newRoomDEs = child.GetDesignElementsOfType<MazeRoomDesignElement>();
            
            //recreate the spawn environment
            if(newTileDEs != null)
                foreach (var de in newTileDEs)
                    de.Spawn();
            if(newWallDEs != null)
                foreach (var de in newWallDEs)
                    de.Spawn();
            if(newRoomDEs != null)
                foreach (var de in newRoomDEs)
                    de.Spawn();

            spawnEnvironment.FinalizeEnvironment();

            return child;
        }
    }
}