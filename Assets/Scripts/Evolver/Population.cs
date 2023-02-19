using System.Collections.Generic;
using UnityEngine;

public class Population<T> where T : Gene<T>
{
    public List<T> members;
    public Population()
    {
        members = new List<T>();
    }

    public void AddMember(T member)
    {
        members.Add(member);
    }

    public override string ToString()
    {
        string s = "Population: \n";
        for (int i = 0; i < members.Count; i++)
        {
            s += "{" + i + "}: {" + members[i].ToString() + "}\n";
        }
        return s;
    }
}

public abstract class DesignElement
{
    public abstract void Mutate();
    public abstract void Spawn();
    public abstract DesignElement Clone();
}

public abstract class Constraint
{
    public abstract int CheckConstraint(GenericLevel population);
}



public class GenericLevel : System.IComparable<GenericLevel>
{
    public Dictionary<System.Type, List<DesignElement>> sortedList;
    public List<Constraint> constraints; //does this need to be here?
    public int fitness, summedError;

    public GenericLevel()
    {
        sortedList = new Dictionary<System.Type, List<DesignElement>>();
        constraints = new List<Constraint>();
    }

    public List<T> GetDesignElementsOfType<T>() where T : DesignElement
    {
        var list = sortedList[typeof(T)];
        List<T> castList = new List<T>(list.Count);
        foreach (var item in list)
        {
            var castItem = (T)item;
            //Debug.Log(castItem.ToString());
            castList.Add(castItem);
        }
        return castList;
    }

    public void AddConstraint(Constraint constraint)
    {
        constraints.Add(constraint);
    }

    public void AddDesignElement(DesignElement designElement)
    {
        //Debug.Log(designElement.GetType().ToString());
        if(sortedList.ContainsKey(designElement.GetType()))
        {
            sortedList[designElement.GetType()].Add(designElement);
            return;
        }
        sortedList.Add(designElement.GetType(), new List<DesignElement>{ designElement });

    }

    public int CheckConstraints()
    {
        int summedError = 0;
        foreach (var constraint in constraints)
        {
            summedError += constraint.CheckConstraint(this);
        }

        this.summedError = summedError;
        return summedError;
    }

    public bool TryGetDesignElementsOfType<T>(out List<DesignElement> l) where T : DesignElement
    {
        if(sortedList.ContainsKey(typeof(T)))
        {
            l = sortedList[typeof(T)];
            return true;
        }
        l = null;
        return false;
    } 

    public int CompareTo(GenericLevel other)
    {
        return -1 * fitness.CompareTo(other.fitness);
    }

    public GenericLevel Clone()
    {
        var gL = new GenericLevel();
        gL.fitness = fitness;
        gL.summedError = summedError;
        var clonedSortedList = new Dictionary<System.Type, List<DesignElement>>();
        foreach(var key in sortedList.Keys)
        {
            var value = sortedList[key];
            List<DesignElement> clonedValue = new List<DesignElement>(value.Count);
            foreach (var item in value)
                clonedValue.Add(item.Clone());
            
            clonedSortedList.Add(key, clonedValue);
        }
        gL.sortedList = clonedSortedList;
        gL.constraints = constraints;
        return gL;
    }

    public void Mutate()
    {
        foreach(var key in sortedList.Keys)
        {
            var value = sortedList[key];
            foreach (var item in value)
            {
                item.Mutate();
            }
        }
    }
}