using UnityEngine;
using Unity.Mathematics;

namespace PipelineV3.Maze
{
    public partial class MazeLevelGenerator : EvolutionaryAlgorithmLayer
    {
        public MazeLevelGenerator( GenericParameters genericParams) : base(genericParams){}

        public override GenericLevel CreateMember()
        {
            var startCellAmount = (int)(MazeBuilderMetrics.CellAmount * MazeBuilderMetrics.START_FILL_PERCENTAGE);

            var spawnEnvironment = new MazeSpawnEnvironment();
            spawnEnvironment.Initialize();

            var gL = new GenericLevel(spawnEnvironment, new MazeMutation(), new MazeCrossover());

            for (int j = 0; j < startCellAmount; j++)
            {
                var dE = new OccupiedCellMazeDesignElement(gL);
                if(gL.AddDesignElement(dE))
                    dE.Spawn();
            }
            for (int j = 0; j < MazeBuilderMetrics.START_WALL_AMOUNT; j++)
            {
                var dE = new MazeWallDesignElement(gL);
                if(gL.AddDesignElement(dE))
                    dE.Spawn();
            }    

            for (int j = 0, numRooms = 0; j < 1000 && numRooms < MazeBuilderMetrics.START_ROOM_AMOUNT; j++)
            {
                var dE = new MazeRoomDesignElement(gL);
                if(gL.AddDesignElement(dE))
                {
                    dE.Spawn();
                    numRooms++;
                }
            } 

            spawnEnvironment.FinalizeEnvironment();  

            return gL; 
        }

        public override bool TerminationCondition()
        {
            return false;
        }

        public override float CalculateFitness(GenericLevel gL)
        {
            var fitness = 0f;//CulDeSacLengthFitness(gL);
            //var culDeSacAmount = CulDeSacAmountFitness(gL);
            fitness += LongestPathFitness(gL);
            fitness += OptimalFillFitness(gL);
            fitness += CulDeSacClusterLengthFitness(gL);
            fitness *= 0.333f;
            gL.fitness = fitness; 
            return fitness;      
        }

        float LongestPathFitness(GenericLevel gL)
        {
            var spawnEnvironment = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var grid = spawnEnvironment.grid;
            var pathLength = grid[grid.Length - 1].distanceToStart;
            var maxLength = (MazeBuilderMetrics.CellAmount) / 2;
            var minLength = MazeBuilderMetrics.WIDTH + MazeBuilderMetrics.HEIGHT;

            float score = 0;
            if(pathLength >= MazeBuilderMetrics.OPTIMAL_PATH_LENGTH)
            {
                var t = (pathLength -  MazeBuilderMetrics.OPTIMAL_PATH_LENGTH) / (float)maxLength;
                score = Mathf.Lerp(1f, 0f, t);
            }
            else
            {
                var t = (pathLength - minLength) / (float) ( MazeBuilderMetrics.OPTIMAL_PATH_LENGTH - minLength);
                score = Mathf.Lerp(0f,1f, t);
            }

            return score;
        }

        int CulDeSacAmountFitness(GenericLevel gL)
        {
            var spawnEnvironment = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var grid = spawnEnvironment.grid;
            int count = 0;
            foreach (var cell in grid)
            {
                if(cell.isCulDeSac)
                    count++;
            }
            return count;
        }

        float CulDeSacLengthFitness(GenericLevel gL)
        {
            var spawnEnvironment = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var grid = spawnEnvironment.grid;
            float fitness = 0;

             //at two or more times the optimal length, CulDeSac's don't give any fitness points
            foreach (var cell in grid)
            {
                if(!cell.isCulDeSac)
                    continue;

                fitness += GetCulDeSacScore(cell.culDeSacLength);
            }

            return fitness / (float)CulDeSacAmountFitness(gL);
        }

        const int clusterAmount = 5;
        float CulDeSacClusterLengthFitness(GenericLevel gL)
        {
            var spawnEnvironment = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var grid = spawnEnvironment.grid;

            int[,] clusters = new int[clusterAmount,clusterAmount];
            int xDivisor = MazeBuilderMetrics.WIDTH / clusterAmount;
            int yDivisor = MazeBuilderMetrics.HEIGHT / clusterAmount;
            float fitness = 0;
             //at two or more times the optimal length, CulDeSac's don't give any fitness points
            foreach (var cell in grid)
            {
                if(!cell.isCulDeSac)
                    continue;

                int2 clusterCoordinate = new int2(cell.X / xDivisor, cell.Y / yDivisor);
                if(cell.culDeSacLength > clusters[clusterCoordinate.x, clusterCoordinate.y])
                    clusters[clusterCoordinate.x, clusterCoordinate.y] = cell.culDeSacLength;
            }

            for (int x = 0; x < clusterAmount; x++)
            {
                for (int y = 0; y < clusterAmount; y++)
                {
                    fitness += GetCulDeSacScore(clusters[x,y]);

                }
            }
            return fitness / (float)(clusterAmount * clusterAmount);
        }

        float GetCulDeSacScore(int culDeSacLength)
        {
            var maxCulDeSac = MazeBuilderMetrics.OPTIMAL_CUL_DE_SAC_LENGTH * 2;
                if(culDeSacLength >= MazeBuilderMetrics.OPTIMAL_CUL_DE_SAC_LENGTH)
                {
                    //20 optimum 10 max 30
                    //15 optimum 5 max 30
                    float t = (culDeSacLength - MazeBuilderMetrics.OPTIMAL_CUL_DE_SAC_LENGTH) / (float)(maxCulDeSac - MazeBuilderMetrics.OPTIMAL_CUL_DE_SAC_LENGTH);
                    return Mathf.Lerp(1f, 0f, t);
                }
                else
                {
                    //5 optimum 15 max 30
                    float t = culDeSacLength / (float)MazeBuilderMetrics.OPTIMAL_CUL_DE_SAC_LENGTH;
                    return Mathf.Lerp(0f,1f,t);
                }
        }

        float OptimalFillFitness(GenericLevel gL)
        {
            var spawnEnvironment = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var grid = spawnEnvironment.grid;            

            var totalCount = MazeBuilderMetrics.CellAmount;
            var fillAmount = 0;
            foreach (var cell in grid)
            {
                if(cell.occupied)
                    fillAmount++;
            }

            var fillPercentage = fillAmount / (float)totalCount;
            if(fillPercentage >= MazeBuilderMetrics.OPTIMAL_FILL_PERCENTAGE)
            {
                var t = (fillPercentage - MazeBuilderMetrics.OPTIMAL_FILL_PERCENTAGE) / (float)(1 - MazeBuilderMetrics.OPTIMAL_FILL_PERCENTAGE);
                return Mathf.Lerp(1f,0f,t);
            }
            else
            {
                var t = fillPercentage / MazeBuilderMetrics.OPTIMAL_FILL_PERCENTAGE;
                return Mathf.Lerp(0f,1f,t);
            }
        }

        public override float CalculateConstraintViolations(GenericLevel gL)
        {
            var violatedConstraints = ReachablePercentageConstraint(gL);
            violatedConstraints += KeyPointsReachableConstraint(gL);
            gL.violatedConstraints = violatedConstraints;
            return violatedConstraints;
        }

        public int KeyPointsReachableConstraint(GenericLevel gL)
        {
            var spawnEnvironment = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var grid = spawnEnvironment.grid;
            var violation = 0;
            if(grid[0].occupied)
                violation += 10;
            if(grid[grid.Length - 1].occupied)
                violation += 10;
            if(violation > 0)
                Debug.Log("!!!");

            return violation;            
        }
        
        //What percentage of the unoccupied cells is rechable from the starting cell?
        public int ReachablePercentageConstraint(GenericLevel gL)
        {
            var spawnEnvironment = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var grid = spawnEnvironment.grid;

            var walkableCells = 0;
            var reachableCells = 0;
            foreach (var cell in grid)
            {
                if(cell.occupied)
                    continue;

                walkableCells++;

                if(cell.distanceToStart != -1)
                    reachableCells++;
            }

            float percentage = (float)reachableCells / (float)walkableCells;
            return 100 - (int)(percentage * 100);
        }
    }
}
