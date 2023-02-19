using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvolutionaryAlgorithmExecutor : MonoBehaviour
{
    [SerializeField] public KeyCode keyBinding;
    [SerializeField] public string descriptor = "";
    void Start() 
    {
        InputHandler.Instance.AddEAExecutor(this, keyBinding);
        AlgoVizUI.Instance.RegisterExecutor(descriptor, this);
    }

    public virtual void Execute()
    {
        
    }
}
