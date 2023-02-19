using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericFramework : MonoBehaviour
{

    //FI-2POP-GA
    //Feasible Individuals get optimized -> Optimization Problem
    //Infeasible Individuals become more feasible -> CSP

    public const int generations = 50, populationSize = 20, crossoverPerGeneration = 2;

    public List<Genotype> feasiblePopulation = new List<Genotype>();
    public List<Genotype> infeasiblePopulation = new List<Genotype>();

    public void Run()
    {
        feasiblePopulation.Clear();
        infeasiblePopulation.Clear();

        for (int i = 0; i < populationSize; i++)
        {
            var g = new Genotype();
            if(ConstraintsViolated(g) > 0)
                infeasiblePopulation.Add(g);
            else
                feasiblePopulation.Add(g);
        }


        for (int gn = 0; gn < generations; gn++)
        {
            
        }
    }


    public Genotype Crossover(Genotype a, Genotype b)
    {
        return a;
    }

    public void Mutate(Genotype g)
    {

    }

    //Fitness Function for Infeasible Population
    public int ConstraintsViolated(Genotype g)
    {
        return 0;
    }

    //Fitness Function for Feasible Population
    public int Fitness(Genotype g)
    {
        return 0;
    }

    public class Genotype
    {
        public Genotype()
        {

        }
    }

}

