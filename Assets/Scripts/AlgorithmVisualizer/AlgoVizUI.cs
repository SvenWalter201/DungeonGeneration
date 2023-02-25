using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AlgoVizUI : MonoBehaviour
{
    [SerializeField] Button plotButtonPrefab, executorButtonPrefab, toggleCanvasBtn;
    [SerializeField] RectTransform plotContainer, plotSelectionArea, leftHeaderArea, algoVizCanvas;
    [SerializeField] Plot plotPrefab;

    [SerializeField] Image texture2DDisplay;

    List<Button> buttons = new List<Button>();
    List<Plot> plots = new List<Plot>();
    List<Button> executorButtons = new List<Button>();
    List<EvolutionaryAlgorithmExecutor> executors = new List<EvolutionaryAlgorithmExecutor>();
    List<GameObject> currentlyRenderedAlgoVizObjects = new List<GameObject>();

    [SerializeField] Image recordButtonImage;
    [SerializeField] Sprite recordingSprite, beginRecordingSprite;

    [SerializeField] TMP_Text replayStepLabel;
    [SerializeField] Slider replayStepSlider;


    //float interval = 2f;
    //float timeUntilNext = 2f;

    static AlgoVizUI instance;
    public static AlgoVizUI Instance => instance; 

    void Awake() 
    {
        instance = this;
    }

    void Start() 
    {
        replayStepSlider.maxValue = 0;
        replayStepSlider.value = 0;
    }

    void Update() 
    {
        /*
        timeUntilNext -= Time.deltaTime;
        if(timeUntilNext < 0.0f)
        {
            timeUntilNext = interval;
            int r = UnityEngine.Random.Range(3, 50);
            var plot = new List<float>(r);
            for (int i = 0; i < r; i++)
            {
                plot.Add(UnityEngine.Random.Range(-50, 200));
            }
            CreatePlot($"Plot Nr. {plots.Count}", plot);
        }
        */

    }

    public void ResetUI()
    {
        foreach (var button in buttons)
        {
            Destroy(button.gameObject);
        }

        foreach (var plot in plots)
        {
            plot.DeletePlot();
        }

        buttons.Clear();
        plots.Clear();
    }
    
    public void CreateMultiPlot(string plotName, List<List<float>> values)
    {
        int index = plots.Count;

        var plotButtonInstance = Instantiate(plotButtonPrefab);
        plotButtonInstance.transform.SetParent(plotSelectionArea, false);
        plotButtonInstance.onClick = new Button.ButtonClickedEvent();
        plotButtonInstance.onClick.AddListener(delegate { ToggleButton(index); });

        var label = plotButtonInstance.GetComponentInChildren<TMP_Text>();
        label.text = plotName;
        buttons.Add(plotButtonInstance);

        var plotInstance = Instantiate(plotPrefab);
        plotInstance.transform.SetParent(plotContainer, false);
        plotInstance.CreateMultiPlot(plotName, values);
        plots.Add(plotInstance);
    }

    public void CreatePlot(string plotName, List<float> values)
    {
        int index = plots.Count;

        var plotButtonInstance = Instantiate(plotButtonPrefab);
        plotButtonInstance.transform.SetParent(plotSelectionArea, false);
        plotButtonInstance.onClick = new Button.ButtonClickedEvent();
        plotButtonInstance.onClick.AddListener(delegate { ToggleButton(index); });

        var label = plotButtonInstance.GetComponentInChildren<TMP_Text>();
        label.text = plotName;
        buttons.Add(plotButtonInstance);

        var plotInstance = Instantiate(plotPrefab);
        plotInstance.transform.SetParent(plotContainer, false);
        plotInstance.CreatePlot(plotName, values);
        plots.Add(plotInstance);
    }

    public void ToggleButton(int index)
    {
        algoVizCanvas.gameObject.SetActive(false);

        for (int i = 0; i < plots.Count; i++)
        {
            plots[i].gameObject.SetActive(false);  
        }
        plots[index].gameObject.SetActive(true);

    }

    bool currentlyRecording = false;

    public void ToggleRecord()
    {
        var tgt = AlgoViz.Instance;

        currentlyRecording = !currentlyRecording;
        if(currentlyRecording)
        {
            tgt.BeginRecord();
            recordButtonImage.sprite = recordingSprite;
            ResetUI();
        }
        else
        {
            tgt.EndRecord();
            recordButtonImage.sprite = beginRecordingSprite;

            replayStepSlider.maxValue = tgt.steps.Count - 1;
            replayStepSlider.value = 0;

            DisplayCanvasContent();
        }
    }

    public void RegisterExecutor(string name, EvolutionaryAlgorithmExecutor executor)
    {
        var btnInstance = Instantiate(executorButtonPrefab);
        btnInstance.transform.SetParent(leftHeaderArea, false);
        var label = btnInstance.GetComponentInChildren<TMP_Text>();
        label.text = name;

        btnInstance.onClick.AddListener(delegate {  ResetUI(); executor.Execute(); });
    }

    public void TogglePopup()
    {

    }

    bool canvasActive = false;

    public void ToggleCanvas()
    {
        canvasActive = !canvasActive;
        algoVizCanvas.gameObject.SetActive(canvasActive);
        if(canvasActive)
        {
            DisplayCanvasContent();
        }
    }

    public void SetCurrentReplayStep()
    {
        var tgt = AlgoViz.Instance;
        var currentReplayStep = (int)replayStepSlider.value;

        tgt.replayIndex = currentReplayStep;
        DisplayCanvasContent();
    }

    public void DisplayCanvasContent()
    {
        foreach (var item in currentlyRenderedAlgoVizObjects)
            Destroy(item);
        
        currentlyRenderedAlgoVizObjects.Clear();

        for (int i = 0; i < plots.Count; i++)
            plots[i].gameObject.SetActive(false);  
        

        var tgt = AlgoViz.Instance;

        if(tgt.replayIndex == -1)
            return;

        var step = tgt.steps[tgt.replayIndex];
        for (int i = 0; i < step.DrawCommandCount; i++)
        {
            var currentDrawCommand = step.drawCommands[i];
            switch(currentDrawCommand.type)
            {
                case DrawCommand.DrawCommandType.ui:
                {
                    var instance = Instantiate(texture2DDisplay);
                    instance.transform.SetParent(algoVizCanvas, false);
                    currentlyRenderedAlgoVizObjects.Add(instance.gameObject);
                    instance.sprite = currentDrawCommand.sprite;
                    instance.rectTransform.sizeDelta = currentDrawCommand.size;
                    break;
                }
            }
        }    

        replayStepLabel.text = "Current Step: " + tgt.replayIndex;
    }
}

public static class AlgoVizUIMetrics
{
    public static int displaySegments = 10;
    public static float labelXOffset = 30;
    public static float leftPadding = 80, rightPadding = 30;
    public static float topPadding = 50;
    public static float bottomPadding = 25;
}
