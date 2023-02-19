using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EvolutionaryAlgorithms;
using EvolutionaryAlgorithms.TerminationConditions;
using EvolutionaryAlgorithms.SelectionModules;
using EvolutionaryAlgorithms.CrossoverModules;
using EvolutionaryAlgorithms.InsertionModules;

public class TestEA : MonoBehaviour
{
    [SerializeField] int populationSize = 20, maxGenerations = 100, parentsPerGeneration = 4;
    [SerializeField] bool selectionWithReplacement = true;
    void Start()
    {
        var ea = new EvolutionaryAlgorithm<StringEvolverGene, SelectionModuleOutput<StringEvolverGene>>();
        ea.Run(
            populationSize, 
            new MaxGenerationTerminationCondition<StringEvolverGene>(maxGenerations), 
            new RouletteSelectionModule<StringEvolverGene>(parentsPerGeneration, selectionWithReplacement),
            new CopyCrossoverModule<StringEvolverGene>(),
            new RandomReplacementInsertion<StringEvolverGene>(true), 
            null);
    }
}











