using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [SerializeField] List<int> compatibleChunks;
    [SerializeField] List<ChunkConstraint> chunkConstraints;

    public List<int> CompatibleChunks
    {
        get
        {
            if(compatibleChunks == null)
                compatibleChunks = new List<int>();

            return compatibleChunks;
        }
        set 
        {
            compatibleChunks = value;
        }
    }


    public static bool IsCompatible(Chunk lhs, Chunk rhs)
    {
        return false;
    }
}

public abstract class ChunkConstraint
{
    public abstract bool ConstraintSatisfied();
}

public class DirectionalChunkConstraint : ChunkConstraint
{
    public byte openings;
    public override bool ConstraintSatisfied()
    {
        return false;
    }

    
}
