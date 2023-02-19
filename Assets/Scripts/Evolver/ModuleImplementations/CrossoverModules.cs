using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvolutionaryAlgorithms.CrossoverModules
{
    public class CopyCrossoverModule<T> : CrossoverModule<T, SelectionModuleOutput<T>> where T : Gene<T>
    {
        public override List<T> Crossover(SelectionModuleOutput<T> selectionModuleOutput)
        {
            List<T> children = new List<T>(selectionModuleOutput.selectedMembers.Count);
            foreach (var member in selectionModuleOutput.selectedMembers)
            {
                var cpy = member.Copy();
                cpy.Mutate();
                children.Add(cpy);
            }
            return children;
        }
    }
}
