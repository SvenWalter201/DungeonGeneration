using PipelineV3.Maze;
using PipelineV3;

public class MazeRoomCellLayoutExecutor : EvolutionaryAlgorithmExecutor
{
    public override void Execute()
    {
        var genericParams = Layout3();

        var l1 = new MazeLevelGenerator(genericParams);
        var r1 = l1.Run(GetSeed(), descriptor, null);
        InputHandler.Instance.eaExecuting = false;
    }

    public GenericParameters Layout3()
    {
        MazeBuilderMetrics.ResetMetrics();
        MazeBuilderMetrics.WIDTH = 30;
        MazeBuilderMetrics.HEIGHT = 30;
        MazeBuilderMetrics.POP_SIZE = 40;
        MazeBuilderMetrics.MAX_GENERATIONS = 1000;
        MazeBuilderMetrics.MAX_CROSSOVERS = 10;

        MazeBuilderMetrics.OPTIMAL_PATH_LENGTH = 120;
        MazeBuilderMetrics.OPTIMAL_CUL_DE_SAC_LENGTH = 16;
        MazeBuilderMetrics.OPTIMAL_FILL_PERCENTAGE = 0.4f;

        MazeBuilderMetrics.START_FILL_PERCENTAGE = 0.1f;
        MazeBuilderMetrics.TILE_SHIFT_PROBABILITY = 0.01f;
        MazeBuilderMetrics.TILE_ADD_PROBABILITY = 0.3f;
        MazeBuilderMetrics.TILE_REMOVE_PROBABILITY = 0.3f;

        MazeBuilderMetrics.START_ROOM_AMOUNT = 8;
        MazeBuilderMetrics.MIN_ROOM_SIZE = 4;
        MazeBuilderMetrics.MAX_ROOM_SIZE = 8;
        MazeBuilderMetrics.MAX_DOOR_AMOUNT = 3;

        MazeBuilderMetrics.ROOM_ADD_PROBABILITY = 0.6f;
        MazeBuilderMetrics.ROOM_REMOVE_PROBABILITY = 0.6f;
        MazeBuilderMetrics.ROOM_CHANGE_SIZE_PROBABILITY = 0.18f;
        MazeBuilderMetrics.ROOM_SHIFT_PROBABILITY = 0.18f;
        MazeBuilderMetrics.ROOM_CHANGE_DOOR_PROBABILITY = 0.14f;


        return new GenericParameters
        {
            maxCrossovers = MazeBuilderMetrics.MAX_CROSSOVERS,
            populationSize = MazeBuilderMetrics.POP_SIZE,
            maxGenerations = MazeBuilderMetrics.MAX_GENERATIONS,
            selectionStrategy = SelectionStrategy.RankSelection,
            insertionStrategy = InsertionStrategy.RankReplacement
        };
    }
}
