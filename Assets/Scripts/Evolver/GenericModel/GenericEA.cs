using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GenericEA
{
    List<Layer> layerStack;
    public VariableParams globals;

    public GenericEA()
    {
        layerStack = new List<Layer>();
        globals = new VariableParams();
    }

    public void AddLayer(Layer l)
    {
        l.model = this;
        layerStack.Add(l);
    }

    public void AddGlobal<T>(string name, T value)
    {
        globals.Add<T>(name, value);
    }

    public void Run(VariableParams arguments, int maxGenerations)
    {
        for (int i = 0; i < maxGenerations; i++)
        {
            arguments = ProcessLayerStack(arguments);
            if(arguments == null)
                return;      
        }
        Debug.Log(arguments.ToString());
    }

    VariableParams ProcessLayerStack(VariableParams arguments)
    {

        for (int i = 0; i < layerStack.Count; i++)
        {
            var result = layerStack[i].ProcessF(arguments);
            if(result == null)
                return null;
            arguments = result;
        }            
       
        return arguments;
    }    
}
