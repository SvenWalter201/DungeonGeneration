using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Gene<T> where T : Gene<T>
{
    public Gene(){}

    public int fitness = 0;
    //public Action<T> MutationEvent;
    //public Action<T> FitnessEvent;
    public abstract T Crossover(T other);

    public abstract void Mutate();

    public abstract List<T> Crossover(List<T> members);

    public abstract T Copy();

    public static Gene<T> Create(){return null;}
}
