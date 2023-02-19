using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CrossoverModule<T, O> where T : Gene<T> where O : SelectionModuleOutput<T>
{
    public abstract List<T> Crossover(O selectionModuleOutput);
}