using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class PipelineV2 : MonoBehaviour
{
    const int levelSizeX = 4, levelSizeZ = 4;

    [SerializeField] Texture2D oneOpening, twoOpeningStraight, twoOpeningCurved, threeOpening, fourOpening;
    [SerializeField] Material material;
    [SerializeField] GameObject chunkPrefab;
    ModifiableChunk[] chunks;
    
    void Start() 
    {
        Generate();
    }

    public void Generate()
    {
        chunks = new ModifiableChunk[levelSizeX * levelSizeZ];

        for (int z = 0, i = 0; z < levelSizeZ; z++)
        {
            for (int x = 0; x < levelSizeX; x++, i++)
            {
                chunks[i] = new ModifiableChunk(new int2(x,z), 
                    z > 0 ? true : false,
                    x > 0 ? true : false,
                    z < levelSizeZ - 1 ? true : false,
                    x < levelSizeX - 1 ? true : false
                );
            }
        }

        
        for (int i = 0; i < chunks.Length; i++)
        {
            SpawnChunkMesh(chunks[i]);
        }
        
    }

    public void SpawnChunkMesh(ModifiableChunk chunk)
    {
        float eulerRotation = 0;
        Texture2D texture = null;
        switch(chunk.openings)
        {
            case 0: break;
            case 1: texture = oneOpening; eulerRotation = 0; break;
            case 2: texture = oneOpening; eulerRotation = 90; break;
            case 3: texture = twoOpeningCurved; eulerRotation = 180; break;
            case 4: texture = oneOpening; eulerRotation = 180; break;
            case 5: texture = twoOpeningStraight; eulerRotation = 0; break;
            case 6: texture = twoOpeningCurved; eulerRotation = 270; break;
            case 7: texture = threeOpening; eulerRotation = 270; break;
            case 8: texture = oneOpening; eulerRotation = 270; break;
            case 9: texture = twoOpeningCurved; eulerRotation = 90; break;
            case 10: texture = twoOpeningStraight; eulerRotation = 0; break;
            case 11: texture = threeOpening; eulerRotation = 180; break;
            case 12: texture = twoOpeningCurved; eulerRotation = 0; break;
            case 13: texture = threeOpening; eulerRotation = 90; break;
            case 14: texture = threeOpening; eulerRotation = 0; break;
            case 15: texture = fourOpening; eulerRotation = 0; break;
        }

        var instance = Instantiate(chunkPrefab);
        instance.transform.position = new Vector3(chunk.position.x * 10, 0.0f, chunk.position.y * 10);
        instance.transform.rotation = Quaternion.Euler(0, eulerRotation, 0);
        var meshRenderer = instance.GetComponent<MeshRenderer>();
        var materialInstance = Instantiate(material);
        materialInstance.SetTexture("_OpeningMask", texture);
        meshRenderer.material = materialInstance;
    }
}

public class ModifiableChunk
{
    public ModifiableChunk(int2 position, bool north, bool east, bool south, bool west)
    {
        int openings = 0;
        openings += north ? 1 : 0;
        openings += east ? 2 : 0;
        openings += south ? 4 : 0;
        openings += west ? 8 : 0;

        this.openings = openings;
        this.position = position;
    }

    public int2 position;
    public int openings;
}
