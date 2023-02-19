using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvolutionaryAlgorithms.SelectionModules
{
    public class RouletteSelectionModule<T> : SelectionModule<T, SelectionModuleOutput<T>> where T : Gene<T>
    {
        int amount;
        public int Amount => amount;

        bool replacement;
        public bool Replacement => replacement;

        public RouletteSelectionModule(int amount, bool replacement)
        {
            this.amount = amount;
            this.replacement = replacement; //same member can be picked multple times
        }

        public override SelectionModuleOutput<T> Select(Population<T> population)
        {
            List<T> selectedMembers = new List<T>();
            float fitnessSum = 0;
            foreach (var member in population.members)
                fitnessSum += member.fitness;

            for (int m = 0; m < amount; m++)
            {
                if(!replacement)
                {
                    fitnessSum = 0;
                    foreach (var member in population.members)
                        fitnessSum += member.fitness;                
                }
                float r = Random.Range(0, fitnessSum);
                int i = 0;
                for (i = 0; i < population.members.Count; i++)
                {
                    r -= population.members[i].fitness;
                    if(r < 0)
                        break;
                }
                selectedMembers.Add(population.members[i]);    

                if(!replacement)
                    population.members.RemoveAt(i);
            }

            if(!replacement)
                population.members.AddRange(selectedMembers);
            
            SelectionModuleOutput<T> output = new SelectionModuleOutput<T>();
            output.population = population;
            output.selectedMembers = selectedMembers;
            return output;
        }
    }

    public class SingleTournamentSelectionModule<T> : SelectionModule<T, SingleTournamentSelectionOutput<T>> where T : Gene<T>
    {
        public override SingleTournamentSelectionOutput<T> Select(Population<T> population)
        {
            return base.Select(population);
        }
    }

    public class SingleTournamentSelectionOutput<T> : SelectionModuleOutput<T> where T : Gene<T>
    {

    }
}
