using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChunkLibrary", menuName = "ScriptableObjects/ChunkLibrary")]
public class ChunkLibrary : ScriptableObject
{
    public List<Chunk> chunks;


    void SetupCompatabilities()
    {
        foreach (var chunkA in chunks)
        {
            List<int> compatibleChunks = new List<int>();

            for (int b = 0; b < chunks.Count; b++)
            {
                var chunkB = chunks[b];
                if(chunkA == chunkB)
                    continue;

                if(Chunk.IsCompatible(chunkA, chunkB))
                    compatibleChunks.Add(b);
            }

            chunkA.CompatibleChunks = compatibleChunks;            
        }
    }
}
