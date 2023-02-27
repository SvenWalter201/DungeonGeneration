using UnityEngine;

namespace PipelineV3.Maze
{
    public partial class MazeLevelGenerator : EvolutionaryAlgorithmLayer
    {
        protected override void ExecutionFinishedCallback(int currentGeneration)
        {
            //AlgoViz.CreatePlot("fPopSize");
            AlgoViz.CreateMultiPlot("maxFitness");
            AlgoViz.CreateMultiPlot("minVio");
            //AlgoViz.CreatePlot("amountDE");
            //AlgoViz.CreatePlot("amountWalls");
            AlgoViz.CreateMultiPlot("invalidationRate");
            AlgoViz.CreateMultiPlot("vRate");
            AlgoViz.CreateMultiPlot("iPopIntegrationRate");
            AlgoViz.CreateMultiPlot("fPopIntegrationRate");
            AlgoViz.CreateMultiPlot("diversityIPop");
            AlgoViz.CreateMultiPlot("diversityFPop");
            AlgoViz.CreateMultiPlot("avgMutF");
            AlgoViz.CreateMultiPlot("avgMutI");
            AlgoViz.CreateMultiPlot("mutValidationRate");
            AlgoViz.CreateMultiPlot("mutInvalidationRate");
            AlgoViz.CreateMultiPlot("fitnessDelta");
            AlgoViz.CreateMultiPlot("constraintDelta");
            AlgoViz.CreateMultiPlot("min Const. Delta");
            AlgoViz.CreateMultiPlot("max Fit. Delta");

        }

        protected override void EndOfGenerationCallback(GenerationInformation generationInformation)
        {
            int countDEs = 0;
            foreach (var member in iPop.members)
                countDEs += member.GetCountOfDesignElementsOfType<OccupiedCellMazeDesignElement>();
            
            foreach (var member in fPop.members)
                countDEs += member.GetCountOfDesignElementsOfType<OccupiedCellMazeDesignElement>();
            
            float averageDEs = countDEs / (float)(fPop.Count + iPop.Count);

            int countWalls = 0;
            foreach (var member in iPop.members)
                countWalls += member.GetCountOfDesignElementsOfType<MazeWallDesignElement>();
            
            foreach (var member in fPop.members)
                countWalls += member.GetCountOfDesignElementsOfType<MazeWallDesignElement>();
            float averageWalls = countWalls / (float)(fPop.Count + iPop.Count);

            //Debug.Log($"Current Generation: {currentGeneration}");
            //AlgoViz.AddPlotPoint("fPopSize", fPop.Count);
            AlgoViz.AddPlotPoint(label + "maxFitness", fPop.Count > 0 ? fPop.members[0].fitness : 0);
            AlgoViz.AddPlotPoint(label + "minVio", iPop.Count > 0 ? iPop.members[0].violatedConstraints : 0);
            //AlgoViz.AddPlotPoint("amountDE", averageDEs);
            //AlgoViz.AddPlotPoint("amountWalls", averageWalls);
            AlgoViz.AddPlotPoint(label + "invalidationRate", generationInformation.invalidationRate);
            AlgoViz.AddPlotPoint(label + "vRate", generationInformation.validationRate);
            AlgoViz.AddPlotPoint(label + "iPopIntegrationRate", generationInformation.iPopIntegrationRate);
            AlgoViz.AddPlotPoint(label + "fPopIntegrationRate", generationInformation.fPopIntegrationRate);
            AlgoViz.AddPlotPoint(label + "avgMutF", generationInformation.averageAcceptedMutationRateFOffspring);
            AlgoViz.AddPlotPoint(label + "avgMutI", generationInformation.averageAcceptedMutationRateIOffspring);
            AlgoViz.AddPlotPoint(label + "mutValidationRate", generationInformation.mutationValidationRate);
            AlgoViz.AddPlotPoint(label + "mutInvalidationRate", generationInformation.mutationInvalidationRate);
            AlgoViz.AddPlotPoint(label + "fitnessDelta", generationInformation.averageMutationFitnessDelta);
            AlgoViz.AddPlotPoint(label + "constraintDelta", generationInformation.averageMutationConstraintViolationDelta);
            AlgoViz.AddPlotPoint(label + "min Const. Delta", generationInformation.minCDelta);
            AlgoViz.AddPlotPoint(label + "max Fit. Delta", generationInformation.maxFDelta);

            DetermineGeneticDiversity();

            if(generationInformation.currentGeneration % 50 == 0)
            {
                AlgoViz.BeginNewStep();
                var sprite = GeneratePopulationTexture(out Vector2 size);
                AlgoViz.AddDrawCommand(DrawCommand.DrawUITexture(Vector3.zero, sprite, size));
            }
        }

        public void DetermineGeneticDiversity()
        {
            var diversityFPop = 0f;
            var diversityIPop = 0f;
            var cellAmount = MazeBuilderMetrics.CellAmount;
            float iPopCellDiversitySum = 0;
            float fPopCellDiversitySum = 0;
            for (int i = 0; i < cellAmount; i++)
            {
                int sum = 0;
                foreach (var member in fPop.members)
                {
                    var sE = ((MazeSpawnEnvironment)member.spawnEnvironment).grid;
                    if(sE[i].occupied)
                        sum++;
                }
                fPopCellDiversitySum += fPop.Count > 0 ? 1 - (Mathf.Abs((sum / (float)fPop.Count) - 0.5f) * 2) : 0;
                sum = 0;
                foreach (var member in iPop.members)
                {
                    var sE = ((MazeSpawnEnvironment)member.spawnEnvironment).grid;
                    if(sE[i].occupied)
                        sum++;
                }
                iPopCellDiversitySum += iPop.Count > 0 ? 1 - (Mathf.Abs((sum / (float)iPop.Count) - 0.5f) * 2) : 0;                
            }
            diversityIPop = iPopCellDiversitySum / (float)cellAmount;
            diversityFPop = fPopCellDiversitySum / (float)cellAmount;

            AlgoViz.AddPlotPoint(label + "diversityIPop", diversityIPop);
            AlgoViz.AddPlotPoint(label + "diversityFPop", diversityFPop);

        }

        Color 
            occupiedColor = Color.black,
            culDeSacColor = new Color(1,0.33f, 0.8f),
            optimalPathColor = new Color(0.6f, 1f,1f),
            freeColor = Color.white;

        const int marginPxl = 3;
        const int maxImageSize = 800;

        public Vector2 GetImageSize(int width, int height)
        {
            if(width > maxImageSize || height > maxImageSize)
            {
                if(width > height)
                {
                    return new Vector2(maxImageSize, (height / (float)width) * maxImageSize);
                }
                else
                {
                   return new Vector2((width / (float)height) * maxImageSize, maxImageSize);
                }
            }
            return new Vector2(width, height);
        }
        public Sprite GenerateSolutionTexture(out Vector2 size)
        {
            GenericLevel bestSolution = null;
            if(fPop.Count > 0)
                bestSolution = fPop.members[0];
            else
                bestSolution = iPop.members[0];

            var sE = (MazeSpawnEnvironment)bestSolution.spawnEnvironment;

            var tex = new Texture2D(MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT);

            int width = MazeBuilderMetrics.WIDTH * 100;
            int height = MazeBuilderMetrics.HEIGHT * 100;
            size = GetImageSize(width, height);

            var colors = new Color[MazeBuilderMetrics.CellAmount];
            for (int y = 0, idx = 0; y < MazeBuilderMetrics.HEIGHT; y++)
            {
                for (int x = 0; x < MazeBuilderMetrics.WIDTH; x++, idx++)
                {
                    Color color = Color.white;
                    var cell = sE.grid[MazeBuilderMetrics.GetIndex(x, y)];
                    if(cell.occupied)
                        color = occupiedColor;
                    else if(cell.partOfOptimalPath)
                        color = optimalPathColor;
                    //else if(cell.isCulDeSac)
                    //    color = culDeSacColor;
                    else 
                        color = freeColor;
                            
                    colors[idx] = color;
                }
            }
            var rect = new Rect();
            rect.center = new Vector2(0,0);
            rect.width = MazeBuilderMetrics.WIDTH;
            rect.height = MazeBuilderMetrics.HEIGHT;
            tex.SetPixels(colors);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f));
        }

        public Sprite GeneratePopulationTexture(out Vector2 size)
        {
            var totalCount = MazeBuilderMetrics.POP_SIZE * 2;
            var sqrt = Mathf.CeilToInt(Mathf.Sqrt(totalCount));
            var marginPixels = (sqrt + 1) * marginPxl;
            var pixelWidth = MazeBuilderMetrics.WIDTH * sqrt + marginPixels;
            var pixelHeight = MazeBuilderMetrics.HEIGHT * sqrt + marginPixels;

            int width = pixelWidth * 20;
            int height = pixelHeight * 20;
            size = GetImageSize(width, height);

            var tex = new Texture2D(pixelWidth, pixelHeight);
            var colors = new Color[pixelWidth * pixelHeight];

            var unitSizeY = MazeBuilderMetrics.HEIGHT + marginPxl;
            var unitSizeX = MazeBuilderMetrics.WIDTH + marginPxl;

            for (int y = 0, idx = 0; y < pixelHeight; y++)
            {
                var unitY = y / unitSizeY;
                var inUnitY = y % unitSizeY;

                inUnitY -= marginPxl;

                for (int x = 0; x < pixelWidth; x++, idx++)
                {
                    var unitX = x / unitSizeX;
                    var inUnitX = x % unitSizeX;

                    inUnitX -= marginPxl;
                    if(inUnitX < 0 || inUnitY < 0)
                    {
                        colors[idx] = Color.grey;
                    }
                    else
                    {
                        int levelIdx = unitX + (sqrt * unitY);
                        GenericLevel level = null;
                        var color = Color.black;
                        if(levelIdx >= iPop.Count)
                        {
                            levelIdx -= iPop.Count;
                            if(levelIdx >= fPop.Count)
                            {
                                colors[idx] = Color.black;
                                continue;
                            }
                            else
                            {
                                level = fPop.members[levelIdx];
                                var sE = (MazeSpawnEnvironment)level.spawnEnvironment;
                                var cell = sE.grid[MazeBuilderMetrics.GetIndex(inUnitX, inUnitY)];
                                if(cell.occupied)
                                    color = occupiedColor;
                                else if(cell.partOfOptimalPath)
                                    color = optimalPathColor;
                                //else if(cell.isCulDeSac)
                                //    color = culDeSacColor;
                                else 
                                    color = freeColor;
                            }
                        }
                        else
                        {
                            level = iPop.members[levelIdx];
                            var sE = (MazeSpawnEnvironment)level.spawnEnvironment;
                            var cell = sE.grid[MazeBuilderMetrics.GetIndex(inUnitX, inUnitY)];   
                            color = cell.occupied ? occupiedColor : freeColor;                     
                        }


                        colors[idx] =color;
                    }
                }
            }
            var rect = new Rect();
            rect.center = new Vector2(0,0);
            rect.height = pixelHeight;
            rect.width = pixelWidth;
            tex.SetPixels(colors);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f));
        }

    }

}