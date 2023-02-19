using System.Collections.Generic;

public class TerminationCondition<T> where T : Gene<T>
{
    public virtual bool Terminate(Population<T> population)
    {
        return true;
    }
}