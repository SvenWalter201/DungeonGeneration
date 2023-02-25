using PipelineV3;
using PipelineV3.Maze;

public class MazeWallLayoutExecutor : EvolutionaryAlgorithmExecutor
{
    public override void Execute()
    {
        var genericParams = Layout2();

        var l1 = new MazeLevelGenerator(genericParams);
        var r1 = l1.Run(GetSeed(), descriptor, null);
        InputHandler.Instance.eaExecuting = false;
    }

    public GenericParameters Layout2()
    {
        MazeBuilderMetrics.WIDTH = 30;
        MazeBuilderMetrics.HEIGHT = 30;
        MazeBuilderMetrics.POP_SIZE = 40;
        MazeBuilderMetrics.MAX_GENERATIONS = 1000;
        MazeBuilderMetrics.MAX_CROSSOVERS = 10;

        MazeBuilderMetrics.START_WALL_AMOUNT = 20;
        MazeBuilderMetrics.MAX_WALL_LENGTH = 30;
        MazeBuilderMetrics.WALL_ADD_PROBABILITY = 0.6f;
        MazeBuilderMetrics.WALL_REMOVE_PROBABILITY = 0.2f; 
        MazeBuilderMetrics.WALL_CHANGE_LENGTH_PROBABILITY = 0.03f;
        MazeBuilderMetrics.WALL_FLIP_PROBABILITY = 0.01f;
        MazeBuilderMetrics.WALL_SHIFT_PROBABILITY = 0.03f;

       return new GenericParameters
        {
            maxCrossovers = MazeBuilderMetrics.MAX_CROSSOVERS,
            populationSize = MazeBuilderMetrics.POP_SIZE,
            maxGenerations = MazeBuilderMetrics.MAX_GENERATIONS,
            selectionStrategy = SelectionStrategy.RankSelection,
            insertionStrategy = InsertionStrategy.RankReplacement,
            populationTransitionStrategy = InsertionStrategy.RandomEliteReplacement
        };
    }	   
}
