using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CompletelyGenericTest : MonoBehaviour
{
    void Awake() 
    {
        int popSize = 10, stringLength = 10, maxGen = 200;
        var ea = new GenericEA();
        ea.AddLayer(new StringEvolverFitnessLayer());
        ea.AddLayer(new FirstSelectionLayer<StringEvolverGene>(popSize/2));
        ea.AddLayer(new GenericCrossoverLayer<StringEvolverGene>());
        ea.AddLayer(new AbsoluteFitnessReplacement<StringEvolverGene>());

        ea.AddGlobal<string>("reference", "HELLOWORLD");
        ea.AddGlobal<int>("stringLength", stringLength);

        VariableParams arguments = new VariableParams();
        var population = new Population<StringEvolverGene>();
        for (int i = 0; i < popSize; i++)
            population.AddMember(new StringEvolverGene(stringLength));

        arguments.Add<Population<StringEvolverGene>>("population", population);

        ea.Run(arguments, maxGen);
    }
}

public class PopulationConstructionLayer<T> : Layer where T : Gene<T>
{
    public override VariableParams SetupNecessaryInputs()
    {
        var varParams = new VariableParams();
        varParams.Add<int>("amount");
        varParams.Add<string>("name");   
        return varParams;      
    }

    public override VariableParams Process(VariableParams input)
    {
        var amount = input.Get<int>("amount");
        var name = input.Get<string>("name");

        var population = new Population<T>();

        var vParams = new VariableParams();
        vParams.Add<Population<T>>("population", population);
        vParams.Add<string>("name", name);
        return vParams;
    }
}

public class SplitPopulationLayer : Layer
{
    public SplitPopulationLayer(VariableParams necessaryInputs) : base(necessaryInputs){}

    public override VariableParams Process(VariableParams input)
    {
        var population = input.Get<Population<StringEvolverGene>>("population");
        

        var vParams = new VariableParams();    
        vParams.Add<int>("amountL2", population.members.Capacity);    

        if(input.UnsafeGet<string>(out string s, "name"))
            vParams.Add<string>("name", s);
        
        return vParams;
    }
}

public abstract class Layer
{
    VariableParams necessaryInputs, optionalInputs;
    public GenericEA model;

    public virtual VariableParams SetupNecessaryInputs(){return null;}

    public Layer()
    {
        this.necessaryInputs = SetupNecessaryInputs();
    }

    public Layer(VariableParams necessaryInputs)
    {
        this.necessaryInputs = necessaryInputs;
    }

    public VariableParams ProcessF(VariableParams input)
    {
        if(!VariableParams.AContainsB(input, necessaryInputs))
        {
            Debug.Log("There was no match!");
            return null;
        }
        return Process(input);
    }

    public abstract VariableParams Process(VariableParams input);
}

public class VariableParams
{
    Dictionary<string, TypedObject> data;

    public VariableParams()
    {
        this.data = new Dictionary<string, TypedObject>();
    }
    public VariableParams(Dictionary<string, TypedObject> data)
    {
        this.data = data;
    }
    public void Add<T>(string name)
    {
        data.Add(name, new TypedObject(typeof(T)));
    }
    public void Add<T>(string name, T value)
    {
        data.Add(name, new TypedObject(typeof(T),value));
    }

    public override string ToString()
    {
        string s = "";
        foreach (var param in data)
        {
            s += param.Key + " : {" + param.Value.data.ToString() + "}\n";
        }
        
        return s;
    }

    public static bool AContainsB(VariableParams a, VariableParams b)
    {
        foreach(var bd in b.data)
        {
            if(!a.data.ContainsKey(bd.Key))
                return false;
            if(a.data[bd.Key].type != bd.Value.type)
                return false;
        }
        return true;
    }

    public T Get<T>(string key)
    {
        return (T)data[key].data;
    }

    public bool UnsafeGet<T>(out T d, string key)
    {
        d = default;
        if(!data.ContainsKey(key))
            return false;
        if(data[key].type != typeof(T))
            return false;
        d = (T)data[key].data;
        return true;
    }
    public T CastObject<T>(object input) 
    {   
        return (T) input;   
    }
}

public class TypedObject
{
    public TypedObject(Type t)
    {
        type = t;
    }

    public TypedObject(Type t, object data)
    {
        type = t;
        this.data = data;
    }

    public Type type;
    public object data;

    public override string ToString() => 
        type.ToString() + ": " + data.ToString();
    
}

public abstract class Mutation<T> where T : Gene<T>
{
    public abstract T Mutate(T gene);
}


