using PipelineV3.Maze;
using PipelineV3;

public class MazeRoomLayoutExecutor : EvolutionaryAlgorithmExecutor
{
    public override void Execute()
    {
        var genericParams = RoomLayout();

        var l1 = new MazeLevelGenerator(genericParams);
        var r1 = l1.Run(GetSeed(), descriptor, null);
        InputHandler.Instance.eaExecuting = false;
    }

    public GenericParameters RoomLayout()
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

        MazeBuilderMetrics.START_ROOM_AMOUNT = 10;
        MazeBuilderMetrics.MIN_ROOM_SIZE = 3;
        MazeBuilderMetrics.MAX_ROOM_SIZE = 8;
        MazeBuilderMetrics.MAX_DOOR_AMOUNT = 3;

        MazeBuilderMetrics.ROOM_ADD_PROBABILITY = 0.3f;
        MazeBuilderMetrics.ROOM_REMOVE_PROBABILITY = 0.3f;
        MazeBuilderMetrics.ROOM_CHANGE_SIZE_PROBABILITY = 0.1f;
        MazeBuilderMetrics.ROOM_SHIFT_PROBABILITY = 0.1f;
        MazeBuilderMetrics.ROOM_CHANGE_DOOR_PROBABILITY = 0.1f;


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
