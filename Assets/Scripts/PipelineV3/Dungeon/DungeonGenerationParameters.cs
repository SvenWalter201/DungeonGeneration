using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PipelineV3;

namespace PipelineV3.Dungeon
{
    public class DungeonGenerator : EvolutionaryAlgorithmLayer
    {
        public DungeonGenerator(GenericParameters genericParams) : base(genericParams){}

        protected override void EndOfGenerationCallback(int currentGeneration)
        {
        }

        protected override void ExecutionFinishedCallback(int currentGeneration)
        {
        }

        public override bool TerminationCondition()
        {
            return false;
        }

        public override int CalculateConstraintViolations(GenericLevel gL)
        {
            return 0;
        }

        public override int CalculateFitness(GenericLevel gL)
        {
            return 0;
        }



        public override void InitializePopulation(GenericLevel previousLayerSolution)
        {
            iPop = new Population(genericParams, DungeonGenerationMetrics.POP_SIZE, false);
            fPop = new Population(genericParams, DungeonGenerationMetrics.POP_SIZE, true);
        }


    }

    public class DungeonCrossover : Crossover
    {
        public override GenericLevel CrossoverFunc(GenericLevel lhs, GenericLevel rhs)
        {
            return null;
        }
    }   

    public class DungeonMutation : Mutation
    {
        public override void Mutate(GenericLevel genericLevel)
        {
            
        }
    }

    public static class DungeonGenerationMetrics
    {
        public static int POP_SIZE = 40;
        public static int MAX_GENERATIONS = 2000;
        public static int MAX_CROSSOVERS = 10;
    }
}


/*
Rooms with Indizes 
Doors in Rooms 

n Lock Elements: 
 - lock index
 - Reference to their Key
 - Reference to the door they occupy (doorindex)

n Key Elements:
 - lock index
 - Reference to their Lock
 - Reference to the square they occupy (coordinate)

Constraints: 
- key has to be placed on a square in a room and not in a door
- keys must be reachable before their locks. 
    -> pathfinding from key to start cant go over a lock with higher index.
    (treat locks with higher indizes as unavailable squares)

    -> pathfinding from lock to start needs to go over all locks with lower indizes in reverse order 4,3,2,1
    -> if any other path exist -> error

    for lock 3 treat lock 2 as unavailable -> no path should be found
    if a path can be found -> error
    2nd run: treat lock 2 as available -> if still no path can be found -> error


XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXOOOOOXXOOOOOXXXXXXXXXXXXXXX
XXXXXXXOOOOOOOOOOOOXXXXXXXXXXXXXXX
XXXXXXXOOOOOXXOOOOOXXXXXXXXXXXXXXX
XXXXXXXXXOXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/
