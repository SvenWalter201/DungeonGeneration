using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class StringEvolverGene : Gene<StringEvolverGene>, System.IComparable<StringEvolverGene>
{
    public string s;

    public StringEvolverGene(StringEvolverGene g)
    {
        s = g.s;
        fitness = g.fitness;
    }

    public StringEvolverGene(string s) : base()
    {
        this.s = s;
    }

    public StringEvolverGene(int stringLength) : base()
    {
        s = StringEvolver.RandomString(stringLength);
    }

    public override StringEvolverGene Copy()
    {
        return new StringEvolverGene(this);
    }

    public int CompareTo(StringEvolverGene other)
    {
        return -1 * fitness.CompareTo(other.fitness);
    }

    public override StringEvolverGene Crossover(StringEvolverGene other)
    {
        char[] crossed = new char[s.Length];
        for (int i = 0; i < s.Length; i++)
            crossed[i] = Random.Range(0,2) == 1 ? s[i] : other.s[i];
        
        return new StringEvolverGene(new string(crossed));
    }

    public override void Mutate()
    {
        var charArray = s.ToCharArray();
        charArray[Random.Range(0, charArray.Length)] = StringEvolver.chars[Random.Range(0, StringEvolver.chars.Length)];
        s = new string(charArray);   
    }

    public override List<StringEvolverGene> Crossover(List<StringEvolverGene> members)
    {
        throw new System.NotImplementedException();
    }

    public override string ToString()
    {
        return s;
    }
}