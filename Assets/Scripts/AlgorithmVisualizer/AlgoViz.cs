using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgoViz : MonoBehaviour
{
    [HideInInspector] public bool recording = false;
    [HideInInspector] public List<AlgorithmStep> steps = new List<AlgorithmStep>();
    [HideInInspector] public AlgorithmStep currentStep;
    [HideInInspector] public int replayIndex = -1;
    [HideInInspector] public int drawCommandCount = 0;
    public Dictionary<string, List<float>> plots = new Dictionary<string, List<float>>();
    public AlgoVizUI uiReference;
    
    static AlgoViz instance;
    public static AlgoViz Instance 
    {
        get 
        {
            if(instance == null)
            {
                instance = GameObject.FindObjectOfType<AlgoViz>();
            }
            return instance;
        }
    }

    public void BeginRecord()
    {
        replayIndex = -1;
        recording = true;
        Clear();
    }

    public void Clear()
    {
        steps.Clear();
        drawCommandCount = 0;
        currentStep = new AlgorithmStep();
    }

    public void EndRecord()
    {
        steps.Add(currentStep.FinishStep());
        recording = false;
    }

    public void RestartReplay()
    {
        if(steps.Count == 0)
            return;
        replayIndex = 0;      
    }

    public void StepReplay()
    {
        if(steps.Count == 0)
            return;
        if(replayIndex == -1)
        {
            replayIndex = 0;
            return;
        }
        replayIndex++;
        if(replayIndex >= steps.Count)
        {
            replayIndex = 0;
            return;
        }
    }

    public static void BeginNewStep()
    {
        if(!Instance.recording)
            return;

        if(Instance.drawCommandCount >= AlgoVizMetrics.maximumDrawCommands)
            return;

        Instance.steps.Add(Instance.currentStep.FinishStep());
        Instance.currentStep = new AlgorithmStep();
    }

    public static void AddPlotPoint(string plotIdentifier, float plotPoint)
    {
        if(!Instance.recording)
            return;

        if(Instance.plots.TryGetValue(plotIdentifier, out List<float> plot))
        {
            plot.Add(plotPoint);
        }
        else
        {
            Instance.plots.Add(plotIdentifier, new List<float>(){plotPoint});
        }
    }

    public static void CreateMultiPlot(string suffix)
    {
        if(!Instance.recording)
            return;

        var plots = new List<List<float>>();
        var labels = new List<string>();
        foreach(var key in Instance.plots.Keys)
        {
            if(key.Contains(suffix))
            {
                plots.Add(Instance.plots[key]);
                var to = key.IndexOf(suffix);
                string prefix = key.Substring(0, to);
                labels.Add(prefix);
            }
        }

        if(plots.Count > 0)
        {
            Instance.uiReference.CreateMultiPlot(suffix, labels, plots);
        }
        else
        {
            Debug.LogWarning($"No Plots with Suffix: {suffix} exist");
        }
    }

    public static void CreatePlot(string plotIdentifier)
    {
        if(!Instance.recording)
            return;

        if(Instance.plots.TryGetValue(plotIdentifier, out List<float> plot))
        {
            Instance.uiReference.CreatePlot(plotIdentifier, plot);
        }
        else
        {
            Debug.LogWarning($"Plot {plotIdentifier} does not exist");
        }
    }

    public static void AddInspectorCommand(InspectorCommand ic)
    {
        if(!Instance.recording)
            return;        

        if(Instance.drawCommandCount >= AlgoVizMetrics.maximumDrawCommands)
            return;

        Instance.currentStep.AddInspectorCommand(ic);
    }

    public static void AddDrawCommand(DrawCommand dc, bool overdraw = false)
    {
        if(!Instance.recording)
            return;

        if(Instance.drawCommandCount >= AlgoVizMetrics.maximumDrawCommands)
            return;

        if(overdraw)
        {
            var dcs = Instance.currentStep.drawCommands;
            bool foundOverridable = false;
            for (int i = 0; i < dcs.Count; i++)
            {
                if(dcs[i].position == dc.position && dcs[i].type == dc.type)
                {
                    dcs[i] = dc;
                    foundOverridable = true;
                    break;
                }
            }
            if(!foundOverridable)
            {
                Instance.currentStep.AddDrawCommand(dc);
                Instance.drawCommandCount++;
            }
        }
        else
        {
            Instance.currentStep.AddDrawCommand(dc);
            Instance.drawCommandCount++;
        }

        
    }
}

[System.Serializable]
public class AlgorithmStep
{
    public int DrawCommandCount => drawCommands.Count;
    public int InspectorCommandCount => inspectorCommands.Count;

    public List<DrawCommand> drawCommands = new List<DrawCommand>();
    public List<InspectorCommand> inspectorCommands = new List<InspectorCommand>();

    public void AddDrawCommand(DrawCommand dc)
    {
        drawCommands.Add(dc);
    }

    public void AddInspectorCommand(InspectorCommand ic)
    {
        inspectorCommands.Add(ic);
    }

    public AlgorithmStep FinishStep()
    {
        return new AlgorithmStep { drawCommands = drawCommands, inspectorCommands = inspectorCommands };
    }
}
