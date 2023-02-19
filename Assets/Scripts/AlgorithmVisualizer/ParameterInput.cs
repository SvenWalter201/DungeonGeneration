using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PipelineV3;
using UnityEngine.UI;
using TMPro;

public class ParameterInput : MonoBehaviour
{
    List<InputField> inputs;
    [SerializeField] Button executeButton;

    EvolutionaryAlgorithmExecutor executor;

    void Awake() 
    {
        inputs = new List<InputField>();
    }

    void Start()
    {
        
    }

    public void SetupInputFields(EvolutionaryAlgorithmExecutor executor, GenericParameters genericParams) 
    {
        
        executeButton.onClick.AddListener(delegate { Generate(); });

    }

    public void Generate()
    {
        executor.Execute();
        Destroy(gameObject);
    }
}
