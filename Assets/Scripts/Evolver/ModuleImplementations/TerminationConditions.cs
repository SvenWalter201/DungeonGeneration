namespace EvolutionaryAlgorithms.TerminationConditions
{
    public class MaxGenerationTerminationCondition<T> : TerminationCondition<T> where T : Gene<T>
    {
        public int maxGenerations = 0, currentGeneration = 0;

        public MaxGenerationTerminationCondition(int maxGenerations)
        {
            this.maxGenerations = maxGenerations;
        }

        public override bool Terminate(Population<T> population)
        {
            if(currentGeneration >= maxGenerations) 
                return true;

            currentGeneration++;
            return false;
        }
    }

    public class StringMatchFoundTerminationCondition : TerminationCondition<StringEvolverGene>
    {
        public override bool Terminate(Population<StringEvolverGene> population)
        {

            return base.Terminate(population);
        }
    }

}
