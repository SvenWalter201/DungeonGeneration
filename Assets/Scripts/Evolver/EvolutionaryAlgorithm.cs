using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace EvolutionaryAlgorithms
{
    public class EvolutionaryAlgorithm<T, SO> where T : Gene<T> where SO : SelectionModuleOutput<T>
    {
        public EvolutionaryAlgorithm()
        {

        }

        public void Run(
            int populationSize,
            TerminationCondition<T> terminationCondition, 
            SelectionModule<T, SO> selectionModule, 
            CrossoverModule<T, SO> crossoverModule,
            InsertionModule<T, SO> insertionModule,
            FitnessModule<T> fitnessModule)
        {
            Population<T> population = new Population<T>();

            while(!terminationCondition.Terminate(population))
            {
                population = fitnessModule.Evaluate();
                var selectionParams = selectionModule.Select(population);
                var selectedMembers = selectionParams.selectedMembers;
                var children = crossoverModule.Crossover(selectionParams);
                population = insertionModule.Insert(children, selectionParams);
            }
        }
    }
}