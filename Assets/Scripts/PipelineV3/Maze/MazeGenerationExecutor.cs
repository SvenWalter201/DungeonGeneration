using PipelineV3.Maze;
using PipelineV3;

public class MazeGenerationExecutor : EvolutionaryAlgorithmExecutor
{
    public override void Execute()
    {
        var genericParams = new PipelineV3.GenericParameters
        {
            maxCrossovers = MazeBuilderMetrics.MAX_CROSSOVERS,
            populationSize = MazeBuilderMetrics.POP_SIZE,
            maxGenerations = MazeBuilderMetrics.MAX_GENERATIONS,
            selectionStrategy = SelectionStrategy.RouletteSelection,
            insertionStrategy = InsertionStrategy.RouletteReplacement,
            populationTransitionStrategy = InsertionStrategy.RouletteReplacement
        };

        var l1 = new MazeLevelGenerator(genericParams);
        var r1 = l1.Run(null);
        InputHandler.Instance.eaExecuting = false;
    }
	
}
