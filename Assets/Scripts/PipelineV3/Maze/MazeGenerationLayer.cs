using UnityEngine;

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

            spawnEnvironment.FinalizeEnvironment();  

            return gL; 
        }

        public override bool TerminationCondition()
        {
            return false;
        }

        public override int CalculateFitness(GenericLevel gL)
        {
            var fitness = CulDeSacLengthFitness(gL);
            var culDeSacAmount = CulDeSacAmountFitness(gL);
            fitness += LongestPathFitness(gL) * culDeSacAmount;
            //fitness = OptimalFillFitness(gL);
            gL.fitness = fitness; 
            return fitness;      
        }

        const int optimalPathLength = 150, optimalCulDeSacLength = 15;
        const float optimalFillPercentage = 0.8f;

        int LongestPathFitness(GenericLevel gL)
        {
            var spawnEnvironment = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var grid = spawnEnvironment.grid;
            var pathLength = grid[grid.Length - 1].distanceToStart;
            var maxLength = (MazeBuilderMetrics.CellAmount) / 2;
            var minLength = MazeBuilderMetrics.WIDTH + MazeBuilderMetrics.HEIGHT;

            float score = 0;
            if(pathLength >= optimalPathLength)
            {
                var t = (pathLength - optimalPathLength) / (float)maxLength;
                score = Mathf.Lerp(100f, 0f, t);
            }
            else
            {
                var t = (pathLength - minLength) / (float) (optimalPathLength - minLength);
                score = Mathf.Lerp(0f,100f, t);
            }

            return Mathf.RoundToInt(score);
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

        int CulDeSacLengthFitness(GenericLevel gL)
        {
            var spawnEnvironment = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var grid = spawnEnvironment.grid;
            float fitness = 0;
            var maxCulDeSac = optimalCulDeSacLength * 2; //at two or more times the optimal length, CulDeSac's don't give any fitness points
            foreach (var cell in grid)
            {
                if(!cell.isCulDeSac)
                    continue;

                if(cell.culDeSacLength >= optimalCulDeSacLength)
                {
                    //20 optimum 10 max 30
                    //15 optimum 5 max 30
                    float t = (cell.culDeSacLength - optimalCulDeSacLength) / (float)(maxCulDeSac - optimalCulDeSacLength);
                    fitness += Mathf.Lerp(100, 0, t);
                }
                else
                {
                    //5 optimum 15 max 30
                    float t = cell.culDeSacLength / (float)optimalCulDeSacLength;
                    fitness += Mathf.Lerp(0,100,t);
                }
            }

            return Mathf.RoundToInt(fitness);
        }

        int OptimalFillFitness(GenericLevel gL)
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
            if(fillPercentage >= optimalFillPercentage)
            {
                var t = (fillPercentage - optimalFillPercentage) / (float)(1 - optimalFillPercentage);
                return Mathf.RoundToInt(Mathf.Lerp(100,0,t));
            }
            else
            {
                var t = fillPercentage / optimalFillPercentage;
                return Mathf.RoundToInt(Mathf.Lerp(0,100,t));
            }
        }

        public override int CalculateConstraintViolations(GenericLevel gL)
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
