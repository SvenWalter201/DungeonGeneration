using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionaryAlgorithmExecutor : MonoBehaviour
{
    [SerializeField] public KeyCode keyBinding;
    [SerializeField] public string descriptor = "";

    [SerializeField] protected bool randomSeed = true;
    [SerializeField] protected int seed = 100;
    void Start() 
    {
        InputHandler.Instance.AddEAExecutor(this, keyBinding);
        AlgoVizUI.Instance.RegisterExecutor(descriptor, this);
    }

    public virtual void Execute()
    {
        
    }

    public int GetSeed()
    {
        if(randomSeed)
            seed = UnityEngine.Random.Range(0, 1000000000);
        return seed;
    }
}
