using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PipelineV3.Dungeon;
using PipelineV3;

public class DungeonGenerationExecutor : EvolutionaryAlgorithmExecutor
{
    public override void Execute()
    {
        var genericParams = new PipelineV3.GenericParameters
        {
            maxCrossovers = DungeonGenerationMetrics.MAX_CROSSOVERS,
            populationSize = DungeonGenerationMetrics.POP_SIZE,
            maxGenerations = DungeonGenerationMetrics.MAX_GENERATIONS,
            selectionStrategy = SelectionStrategy.RouletteSelection,
            insertionStrategy = InsertionStrategy.RouletteReplacement,
            populationTransitionStrategy = InsertionStrategy.RouletteReplacement
        };

        var l1 = new DungeonGenerator(genericParams);
        var r1 = l1.Run(1, descriptor, null);
        InputHandler.Instance.eaExecuting = false;    }
}
