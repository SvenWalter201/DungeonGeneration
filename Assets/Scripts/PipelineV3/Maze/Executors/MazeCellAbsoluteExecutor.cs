using PipelineV3;
using PipelineV3.Maze;

public class MazeCellAbsoluteExecutor : EvolutionaryAlgorithmExecutor
{
    public override void Execute()
    {
        var genericParams = Layout1();

        var l1 = new MazeLevelGenerator(genericParams);
        var r1 = l1.Run(GetSeed(), descriptor, null);
        InputHandler.Instance.eaExecuting = false;
    }

    public GenericParameters Layout1()
    {
        MazeBuilderMetrics.ResetMetrics();
        MazeBuilderMetrics.WIDTH = 30;
        MazeBuilderMetrics.HEIGHT = 30;
        MazeBuilderMetrics.POP_SIZE = 40;
        MazeBuilderMetrics.MAX_GENERATIONS = 1000;
        MazeBuilderMetrics.MAX_CROSSOVERS = 10;

        MazeBuilderMetrics.OPTIMAL_PATH_LENGTH = 120;
        MazeBuilderMetrics.OPTIMAL_CUL_DE_SAC_LENGTH = 16;
        MazeBuilderMetrics.OPTIMAL_FILL_PERCENTAGE = 0.46f;

        MazeBuilderMetrics.START_FILL_PERCENTAGE = 0.5f;
        MazeBuilderMetrics.TILE_SHIFT_PROBABILITY = 0.02f;
        MazeBuilderMetrics.TILE_ADD_PROBABILITY = 0.8f;
        MazeBuilderMetrics.TILE_REMOVE_PROBABILITY = 0.2f;

        return new GenericParameters
        {
            maxCrossovers = MazeBuilderMetrics.MAX_CROSSOVERS,
            populationSize = MazeBuilderMetrics.POP_SIZE,
            maxGenerations = MazeBuilderMetrics.MAX_GENERATIONS,
            selectionStrategy = SelectionStrategy.AbsoluteFitnessSelection,
            insertionStrategy = InsertionStrategy.AbsoluteFitnessReplacement
        };
    }
}
