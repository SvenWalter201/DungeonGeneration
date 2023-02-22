using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace PipelineV3.Maze
{
    public class MazeMutation : Mutation
    {
        static bool[,] occupation;

        public override void Mutate(GenericLevel genericLevel)
        {
            List<OccupiedCellMazeDesignElement> occupiedCellDEs = null;
            List<MazeWallDesignElement> wallDEs = null; 

            //TILES
            {
                //mutate overall level structure
                float addTile = UnityEngine.Random.Range(0f,1f);
                float removeTile = UnityEngine.Random.Range(0f,1f);
                while(addTile < MazeBuilderMetrics.TILE_ADD_PROPABILITY)
                {
                    genericLevel.AddDesignElement(new OccupiedCellMazeDesignElement(genericLevel));
                    addTile = UnityEngine.Random.Range(0f,1f);
                }

                occupiedCellDEs = genericLevel.GetDesignElementsOfType<OccupiedCellMazeDesignElement>();

                if(occupiedCellDEs != null)
                {
                    if(removeTile < MazeBuilderMetrics.TILE_REMOVE_PROPABILITY)
                    {
                        var selectedDE = occupiedCellDEs[UnityEngine.Random.Range(0, occupiedCellDEs.Count)];
                        genericLevel.RemoveDesignElement(selectedDE);
                        occupiedCellDEs.Remove(selectedDE);
                    }
                    occupiedCellDEs = genericLevel.GetDesignElementsOfType<OccupiedCellMazeDesignElement>();

                    //mutate individual design elements
                    foreach (var de in occupiedCellDEs)
                        de.Mutation();

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

                if(addWall < MazeBuilderMetrics.WALL_ADD_PROPABILITY)
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
                        genericLevel.AddDesignElement(new MazeWallDesignElement(genericLevel, chosenCell.X, chosenCell.Y, horizontal));
                        break;
                    }
                }
                
                wallDEs = genericLevel.GetDesignElementsOfType<MazeWallDesignElement>();
                if(wallDEs != null)
                {
                    if(removeWall < MazeBuilderMetrics.WALL_REMOVE_PROPABILITY)
                    {
                        if(wallDEs.Count > 0)
                        {
                            var selectedDE = wallDEs[UnityEngine.Random.Range(0, wallDEs.Count)];
                            genericLevel.RemoveDesignElement(selectedDE);
                            wallDEs.Remove(selectedDE);
                        }

                    }

                    //mutate individual design elements
                    foreach (var de in wallDEs)
                        de.Mutation();

                    wallDEs = genericLevel.GetDesignElementsOfType<MazeWallDesignElement>();
                }
            }

            genericLevel.spawnEnvironment.Clear();
            if(occupiedCellDEs != null)
            {
                foreach (var de in occupiedCellDEs)
                    de.Spawn();
            }

            if(wallDEs != null)
            {
                foreach (var de in wallDEs)
                    de.Spawn();
            }

            genericLevel.spawnEnvironment.FinalizeEnvironment();
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

        public List<DesignElement> CellCrossover(GenericLevel lhs, GenericLevel rhs, GenericLevel child)
        {
            if(UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f)
                (lhs, rhs) = (rhs, lhs);

            var lhsDEs = lhs.GetDesignElementsOfType<OccupiedCellMazeDesignElement>(); //first half of this list //also make deep copy
            var rhsDEs = rhs.GetDesignElementsOfType<OccupiedCellMazeDesignElement>(); //second half of this list //also make deep copy
            var newDEs = new List<DesignElement>();
            bool horizontal = UnityEngine.Random.Range(0.0f, 1.0f) < 0.5f;
            if(horizontal)
            {
                int halfHeight = Mathf.RoundToInt(MazeBuilderMetrics.HEIGHT / (float)2);
                if(lhsDEs != null)
                {
                    foreach (var cell in lhsDEs)
                    {
                        if(cell.y > halfHeight)
                            newDEs.Add(cell.Clone(child));
                    }
                }

                if(lhsDEs != null)
                {
                    foreach (var cell in rhsDEs)
                    {
                        if(cell.y <= halfHeight)
                            newDEs.Add(cell.Clone(child));
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
                            newDEs.Add(cell.Clone(child));

                    }
                }

                if(rhsDEs != null)
                {
                    foreach (var cell in rhsDEs)
                    {
                        if(cell.x <= halfWidth)
                            newDEs.Add(cell.Clone(child)); 
                    } 
                }
               
            }
            return newDEs;
        }

        public override GenericLevel CrossoverFunc(GenericLevel lhs, GenericLevel rhs)
        {
            //do some crossover shenanigans
            var spawnEnvironment = new MazeSpawnEnvironment();
            spawnEnvironment.Initialize();

            var child = new GenericLevel(spawnEnvironment, new MazeMutation(), new MazeCrossover());

            var newTileDEs = CellCrossover(lhs, rhs, child);
            var newWallDEs = Crossover<MazeWallDesignElement>(lhs, rhs, child);
            
            var newDEDictionary = new Dictionary<System.Type, List<DesignElement>>();
            if(newTileDEs.Count > 0)
                newDEDictionary.Add(newTileDEs[0].GetType(), newTileDEs);

            if(newWallDEs.Count > 0)
                newDEDictionary.Add(newWallDEs[0].GetType(), newWallDEs);

            child.SetDesignElements(newDEDictionary);

            //recreate the spawn environment
            foreach (var de in newTileDEs)
                de.Spawn();
            foreach (var de in newWallDEs)
                de.Spawn();

            spawnEnvironment.FinalizeEnvironment();

            return child;
        }
    }
}