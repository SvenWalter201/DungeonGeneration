using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericCrossoverLayer<T> : Layer where T : Gene<T>
{
    public override VariableParams SetupNecessaryInputs()
    {
        var varParams = new VariableParams();
        varParams.Add<Population<T>>("population");
        varParams.Add<List<T>>("selectedMembers");
        return varParams;
    }

    public override VariableParams Process(VariableParams input)
    {
        var population = input.Get<Population<T>>("population");
        var selectedMembers = input.Get<List<T>>("selectedMembers");
        var children = new List<T>();

        for (int i = 0; i < selectedMembers.Count; i++)
        {
            var child = selectedMembers[i].Copy();
            child.Mutate();
            children.Add(child);
        }

        var varParams = new VariableParams();
        varParams.Add<Population<T>>("population", population);
        varParams.Add<List<T>>("children", children);
        return varParams;
    }    
}

public class AbsoluteFitnessReplacement<T> : Layer where T : Gene<T>
{
    public override VariableParams SetupNecessaryInputs()
    {
        var varParams = new VariableParams();
        varParams.Add<Population<T>>("population");
        varParams.Add<List<T>>("children");      
        return varParams;
    }

    /*
    5,3,2 = 1/2 3/10 1/5 Sum = 10;
    5,3,2 = 1/5 + 1/3 + 1/2 = 6/30 + 10/30 + 15/30 = 31/30 16/30

    */

    public override VariableParams Process(VariableParams input)
    {
        var population = input.Get<Population<T>>("population");
        var children = input.Get<List<T>>("children");

        int i = 1;
        for (int c = 0; c < children.Count; c++)
        {
            population.members[population.members.Count - i] = children[c];
            i++;
            //int i = Random.Range(0, population.members.Count);
            //population.members[i] = children[c];
        }  
        
        var varParams = new VariableParams();
        varParams.Add<Population<T>>("population", population);
        return varParams;
    }       
}

public class RouletteInsertionLayer<T> : Layer where T : Gene<T>
{
    public override VariableParams SetupNecessaryInputs()
    {
        var varParams = new VariableParams();
        varParams.Add<Population<T>>("population");
        varParams.Add<List<T>>("children");      
        return varParams;
    }

    /*
    5,3,2 = 1/2 3/10 1/5 Sum = 10;
    5,3,2 = 1/5 + 1/3 + 1/2 = 6/30 + 10/30 + 15/30 = 31/30 16/30

    */

    public override VariableParams Process(VariableParams input)
    {
        var population = input.Get<Population<T>>("population");
        var children = input.Get<List<T>>("children");



        for (int c = 0; c < children.Count; c++)
        {
            float fitnessSum = 0;

            foreach (var member in population.members)
                fitnessSum += 1.0f / (float)member.fitness;

            float r = Random.Range(0, fitnessSum);
            int i = 0;
            for (i = 0; i < population.members.Count; i++)
            {
                r -= 1.0f / (float)population.members[i].fitness;
                if(r < 0)
                {
                    population.members[i] = children[c];
                    break;
                }
            }
        }  
        
        var varParams = new VariableParams();
        varParams.Add<Population<T>>("population", population);
        return varParams;
    }   
}

public class StringEvolverFitnessLayer : Layer
{
    public override VariableParams SetupNecessaryInputs()
    {
        var varParams = new VariableParams();
        varParams.Add<Population<StringEvolverGene>>("population");
        return varParams;
    }

    public override VariableParams Process(VariableParams input)
    {
        var referenceString = model.globals.Get<string>("reference");
        var stringLength = model.globals.Get<int>("stringLength");
        var population = input.Get<Population<StringEvolverGene>>("population");

        foreach (var member in population.members)
        {     
            int fitness = 0;
            for (int i = 0; i < stringLength; i++)
                fitness += referenceString[i] == member.s[i] ? 1 : 0;
            
            member.fitness = fitness;     
            Debug.Log("F: " + member.fitness);  
        }

        population.members.Sort();

        var varParams = new VariableParams();
        varParams.Add<Population<StringEvolverGene>>("population", population);
        return varParams;
    }
}

public class FirstSelectionLayer<T> : Layer where T : Gene<T>
{
    int amount;

    public FirstSelectionLayer(int amount) : base()
    {
        this.amount = amount;
    }

    public override VariableParams SetupNecessaryInputs()
    {
        var varParams = new VariableParams();
        varParams.Add<Population<T>>("population");
        return varParams;
    }

    public override VariableParams Process(VariableParams input)
    {
        var population = input.Get<Population<T>>("population");

        List<T> selectedMembers = new List<T>();

        for (int i = 0; i < amount; i++)
            selectedMembers.Add(population.members[i]);
   
        var varParams = new VariableParams();
        varParams.Add<Population<T>>("population", population);
        varParams.Add<List<T>>("selectedMembers", selectedMembers);
        return varParams;
    }   
}

public class RouletteSelectionLayer<T> : Layer where T : Gene<T>
{
    int amount;
    bool replacement;

    public RouletteSelectionLayer(int amount, bool replacement) : base()
    {
        this.amount = amount;
        this.replacement = replacement;
    }

    public override VariableParams SetupNecessaryInputs()
    {
        var varParams = new VariableParams();
        varParams.Add<Population<T>>("population");
        return varParams;
    }

    public override VariableParams Process(VariableParams input)
    {
        var population = input.Get<Population<T>>("population");

        List<T> selectedMembers = new List<T>();
        float fitnessSum = 0;

        foreach (var member in population.members)
            fitnessSum += member.fitness;

        for (int m = 0; m < amount; m++)
        {
            float r = Random.Range(0, fitnessSum);
            int i = 0;
            for (i = 0; i < population.members.Count; i++)
            {
                r -= population.members[i].fitness;
                if(r < 0)
                {
                    selectedMembers.Add(population.members[i]);    
                    if(!replacement)
                    {
                        population.members.RemoveAt(i);

                        fitnessSum = 0;
                        foreach (var member in population.members)
                            fitnessSum += member.fitness;                
                    }
                    break;
                }
            }
        }

        if(!replacement)
            population.members.AddRange(selectedMembers); 
   
        var varParams = new VariableParams();
        varParams.Add<Population<T>>("population", population);
        varParams.Add<List<T>>("selectedMembers", selectedMembers);
        return varParams;
    }
}
