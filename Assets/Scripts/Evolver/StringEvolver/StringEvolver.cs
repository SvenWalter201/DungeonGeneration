using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringEvolver : MonoBehaviour
{
    const int stringLength = 10, runs = 50;
    public const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    static System.Random random = new System.Random();

    void Awake() 
    {
        for (int tournamentSize = 2; tournamentSize < 16; tournamentSize++)        
        {
            var result = RunMultiple(new EvolveParams{tournamentSize = tournamentSize}, 50);
            Debug.Log($"Tournament size {tournamentSize}: {result.meanGen} generations");
        }
    }

    public EvolveMultipleResult RunMultiple(EvolveParams para, int runs, bool verbose = false)
    {
        int sum = 0;
        int min = int.MaxValue;
        int max = 0;
        List<int> results = new List<int>(runs);
        for (int i = 0; i < runs; i++)
        {
            var result = Run(para);  
            if(result.solutionFound)
            {
                var r = result.requiredGenerations;
                results.Add(r);
                sum += r;

                if(r < min)
                    min = r;
                if(r > max)
                    max = r;
            }
        }

        float mean = sum / (float)results.Count;
        float ssd = 0;

        for (int i = 0; i < results.Count; i++)
        {
            float dist = results[i] - mean;
            ssd += dist * dist;
        }

        float standardDeviation = Mathf.Sqrt(ssd/(float)results.Count);
        if(verbose)
            Debug.Log($"\n Mean: {mean} \n SD: {standardDeviation} \n Min: {min} \n Max: {max}"); 

        return new EvolveMultipleResult
        {
            solvePercentage = results.Count / (float)runs,
            meanGen = mean,
            sd = standardDeviation,
            minGen = min,
            maxGen = max
        };
    }

    public static string RandomString(int length)
    {
        var stringChars = new char[length];

        for (int i = 0; i < length; i++)
            stringChars[i] = chars[random.Next(chars.Length)];
        
        return new string(stringChars);
    }

    public EvolveResult Run(EvolveParams para)
    {
        string reference = "HELLOWORLD";
        List<StringEvolverGene> population = new List<StringEvolverGene>(para.populationSize);
        for (int i = 0; i < para.populationSize; i++)
        {
            population.Add(new StringEvolverGene(stringLength));
        }


        for (int currentGeneration = 0; currentGeneration < para.maxGenerations; currentGeneration++)
        {
            Fitness(reference, population);
            population.Sort();
            //Debug.Log("Gen " + currentGeneration + ": "+ population[0].s + "| Fitness: " + population[0].fitness);

            if(population[0].fitness == stringLength)
            {
                return new EvolveResult{ solutionFound = true, requiredGenerations = currentGeneration};
            }
            population = SingleTournamentSelection(para, population);
        }

        return new EvolveResult{ solutionFound = false, requiredGenerations = -1};
    }

    public void Fitness(string referenceString, List<StringEvolverGene> population)
    {
        foreach (var member in population)
        {     
            int fitness = 0;
            for (int i = 0; i < stringLength; i++)
                fitness += referenceString[i] == member.s[i] ? 1 : 0;
            
            member.fitness = fitness;     
        }
    }

    public List<StringEvolverGene> SingleTournamentSelection(EvolveParams para, List<StringEvolverGene> population)
    {
        int tournamentAmount = para.populationSize / para.tournamentSize;
        List<StringEvolverGene> newPopulation = new List<StringEvolverGene>(para.populationSize);
        for (int i = 0; i < tournamentAmount; i++)
        {
            var currentTournament = new List<StringEvolverGene>(para.tournamentSize);
            for (int j = 0; j < para.tournamentSize; j++)
            {
                int r = random.Next(population.Count);
                currentTournament.Add(population[r]);
                population.RemoveAt(r);
            }

            currentTournament.Sort();
            //Debug.Log("First: " + currentTournament[0].fitness + " | Last: " + currentTournament[tournamentSize - 1].fitness);
            var child = currentTournament[0].Copy();
            child.Mutate();
            currentTournament[para.tournamentSize - 1] = child;

            for (int j = 0; j < para.tournamentSize; j++)
            {
                newPopulation.Add(currentTournament[j]);
            }
        }

        newPopulation.AddRange(population);

        return newPopulation;
    }
}

public class EvolveParams
{
    const int s_maxGenerations = 1000, s_populationSize = 60, s_tournamentSize = 4;
    public int maxGenerations = s_maxGenerations;
    public int tournamentSize = s_tournamentSize;
    public int populationSize = s_populationSize;
}

public class EvolveResult
{
    public bool solutionFound;
    public int requiredGenerations;
}

public class EvolveMultipleResult
{
    public float meanGen;
    public float solvePercentage;
    public float sd;
    public int minGen;
    public int maxGen;
}


