using System.Collections.Generic;

public class InsertionModule<T, I> where T : Gene<T> where I : SelectionModuleOutput<T>
{
    public virtual Population<T> Insert(List<T> children, I input)
    {
        return input.population;
    }
}