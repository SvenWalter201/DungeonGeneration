using Unity.Mathematics;

namespace PipelineV3.Dungeon
{
    public class DungeonGenerator : EvolutionaryAlgorithmLayer
    {

        public override GenericLevel CreateMember()
        {
            throw new System.NotImplementedException();
        }
        public DungeonGenerator(GenericParameters genericParams) : base(genericParams){}

        protected override void EndOfGenerationCallback(GenerationInformation currentGeneration)
        {
        }

        protected override void ExecutionFinishedCallback(int currentGeneration)
        {
        }

        public override bool TerminationCondition()
        {
            return false;
        }

        public override float CalculateConstraintViolations(GenericLevel gL)
        {
            return 0;
        }

        public override float CalculateFitness(GenericLevel gL)
        {
            return 0;
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
        protected override void MutateImpl(GenericLevel genericLevel)
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

DesignElements: 

Room Elements: 

- LL Corner
- Width, Length
- Door positions along the edges (up to m doors)




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
