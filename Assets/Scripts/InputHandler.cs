using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [SerializeField] float speed = 10.0f;

    [SerializeField] 
    bool algoVizActive = false;

    [SerializeField] AlgoVizUI algoVizUI;

    static InputHandler instance;
    public static InputHandler Instance => instance;
    Dictionary<KeyCode, EvolutionaryAlgorithmExecutor> activeEAExecutors;
    public bool eaExecuting = false;

    void Awake() 
    {
        instance = this;
        activeEAExecutors = new Dictionary<KeyCode, EvolutionaryAlgorithmExecutor>();
    }

    void Start()
    {
        algoVizActive = false; 
        algoVizUI.gameObject.SetActive(false);
    }

    public void AddEAExecutor(EvolutionaryAlgorithmExecutor executor, KeyCode keyBinding)
    {
        activeEAExecutors.Add(keyBinding, executor);
    }


    void Update()
    {
        if(!algoVizActive)
            HandleMovement();
        
        //Toggle AlgoViz
        if(Input.GetKeyDown(KeyCode.Space))
        {
            algoVizUI.gameObject.SetActive(!algoVizUI.gameObject.activeInHierarchy);
            algoVizActive = !algoVizActive;
        }

        if(eaExecuting)
            return;

        foreach (var kVPair in activeEAExecutors)
        {
            if(Input.GetKeyDown(kVPair.Key))
            {
                algoVizUI.ResetUI();
                eaExecuting = true;
                kVPair.Value.Execute(); //needs to be multithreaded
                return;
            }
        }
    }

    void HandleMovement()
    {
        if(Input.GetKey(KeyCode.W))
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.S))
        {
            transform.position += -transform.forward * speed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.A))
        {
            transform.position += -transform.right * speed * Time.deltaTime;
        }
        if(Input.GetKey(KeyCode.D))
        {
            transform.position += transform.right * speed * Time.deltaTime;            
        }

        Vector3 mouse = Input.mousePosition;
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, transform.position.y));
        Vector3 forward = (mouseWorld - transform.position).normalized;
        if(Vector3.Dot(forward, transform.forward) < 0.96f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forward, Vector3.up), Time.deltaTime * 1.0f);
        }
    }

}
