using System.Collections.Generic;
using EvolutionaryAlgorithms.SelectionModules;

namespace EvolutionaryAlgorithms.InsertionModules
{
    public class RandomReplacementInsertion<T> : InsertionModule<T, SelectionModuleOutput<T>> where T : Gene<T>
    {
        public RandomReplacementInsertion(bool withReplacement)
        {

        }

        public override Population<T> Insert(List<T> children, SelectionModuleOutput<T> input)
        {
            foreach (var child in children)
            {
                int r = UnityEngine.Random.Range(0, children.Count);
                input.population.members[r] = child;
            }
            return input.population;
        }
    }

    public class SingleTournamentInsertion<T> : InsertionModule<T, SingleTournamentSelectionOutput<T>> where T : Gene<T>
    {
        public override Population<T> Insert(List<T> children, SingleTournamentSelectionOutput<T> input)
        {
            return null;
        }
    }

}