using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionModule<T, O> where T : Gene<T> where O : SelectionModuleOutput<T>
{
    public virtual O Select(Population<T> population)
    {
        return null;
    }
}


public class SelectionModuleOutput<T> where T : Gene<T>
{
    public Population<T> population;
    public List<T> selectedMembers;
}

