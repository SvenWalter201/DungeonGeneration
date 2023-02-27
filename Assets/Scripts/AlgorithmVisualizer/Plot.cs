using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plot : MonoBehaviour
{
    RectTransform plotContainer;

    void Awake() 
    {
        plotContainer = GetComponent<RectTransform>();
    }

    [SerializeField] PlotPoint plotPointPrefab;
    [SerializeField] PlotPointConnection plotPointConnectionPrefab;
    [SerializeField] DisplaySegment displaySegmentPrefab;

    List<PlotPoint> plotPoints = new List<PlotPoint>();
    List<PlotPointConnection> plotPointConnections = new List<PlotPointConnection>();
    List<DisplaySegment> displaySegments = new List<DisplaySegment>();


    public void DeletePlot()
    {
        foreach (var plotPoint in plotPoints)
            Destroy(plotPoint.gameObject);
        
        foreach (var plotPointConnection in plotPointConnections)
            Destroy(plotPointConnection.gameObject);
        
        foreach (var displaySegment in displaySegments)
            Destroy(displaySegment.gameObject);
        
        Destroy(gameObject);
    }

    public void CreateMultiPlot(string plotName, List<string> plotLabels, List<List<float>> plots) => 
        StartCoroutine(CreateMultiPlotDelayed(plotName, plotLabels, plots));
    

    IEnumerator CreateMultiPlotDelayed(string plotName, List<string> plotLabels, List<List<float>> plots)
    {
        int delayFrames = 3;
        while(delayFrames > 0)
        {
            delayFrames--;
            yield return null;
        }
        float paddedPlotWidth = PlotWidth - (AlgoVizUIMetrics.leftPadding + AlgoVizUIMetrics.rightPadding);

        int maxLength = 0;

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        foreach (var plot in plots)
        {
            if(plot.Count > maxLength)
                maxLength = plot.Count;

            foreach (var value in plot)
            {
                if(value > maxValue)
                    maxValue = value;
                if(value < minValue)
                    minValue = value;                
            }
        }

        float spacePerPoint = paddedPlotWidth / (maxLength - 1);
        float range = maxValue - minValue;

        float rangeSegment = range / (float)AlgoVizUIMetrics.displaySegments;
        float amountSegment = maxLength / (float)(AlgoVizUIMetrics.displaySegments);
        float paddedPlotHeight = PlotHeight - (AlgoVizUIMetrics.bottomPadding + AlgoVizUIMetrics.topPadding);

        float rangeSegmentSpacing = paddedPlotHeight / (float) AlgoVizUIMetrics.displaySegments;
        float amountSegmentSpacing = paddedPlotWidth / (float) AlgoVizUIMetrics.displaySegments;

        float verticalSpacePerUnit = paddedPlotHeight / range;

        CreateHeader(plotName);
        CreateDisplaySegmentsVertical(range, minValue, maxLength, rangeSegmentSpacing);
        CreateDisplaySegmentsHorizontal(amountSegment, amountSegmentSpacing);     
        int idx = 0;
        foreach (var plot in plots)
        {
            PlotValues(plot, spacePerPoint, verticalSpacePerUnit, minValue, colors[idx]);
            idx++;
        }

        //Labels
        for (int i = 0; i < plots.Count; i++)
        {
            float xPosition = PlotWidth - (AlgoVizUIMetrics.rightPadding - 30);
            float yPosition = AlgoVizUIMetrics.bottomPadding + i * 35;// + (rangeSegmentSpacing * 0.5f);

            var colorBubble = Instantiate(plotPointConnectionPrefab);
            colorBubble.transform.SetParent(plotContainer, false);
            colorBubble.PlaceConnection(new Vector2(xPosition, yPosition), new Vector2(xPosition + 50f, yPosition), colors[i], 20f);
            plotPointConnections.Add(colorBubble);

            var displaySegment = Instantiate(displaySegmentPrefab);
            displaySegment.transform.SetParent(plotContainer, false);
            var rT = displaySegment.GetComponent<RectTransform>();
            rT.anchorMin = Vector2.zero;
            rT.anchorMax = Vector2.zero;
            rT.anchoredPosition = new Vector2(xPosition + 87f, yPosition);
            displaySegment.SetText(plotLabels[i]);
            displaySegments.Add(displaySegment);


        }


        gameObject.SetActive(false);
    }

    Color[] colors = new Color[]{Color.red, Color.blue, Color.magenta, Color.green};
    public void CreatePlot(string plotName, List<float> values) =>     
        StartCoroutine(CreatePlotDelayed(plotName, values));

    float PlotWidth => plotContainer.sizeDelta.x;
    float PlotHeight => plotContainer.sizeDelta.y;
    float PaddedWidth => PlotWidth - AlgoVizUIMetrics.leftPadding - AlgoVizUIMetrics.rightPadding;
    float PaddedHeight => PlotHeight - AlgoVizUIMetrics.topPadding - AlgoVizUIMetrics.bottomPadding;

    IEnumerator CreatePlotDelayed(string plotName, List<float> values)
    {
        int delayFrames = 3;
        while(delayFrames > 0)
        {
            delayFrames--;
            yield return null;
        }
        float spacePerPoint = PaddedWidth / (values.Count - 1);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < values.Count; i++)
        {
            if(values[i] > maxValue)
                maxValue = values[i];
            if(values[i] < minValue)
                minValue = values[i];
        }

        float range = maxValue - minValue;

        float rangeSegment = range / (float)AlgoVizUIMetrics.displaySegments;
        float amountSegment = values.Count / (float)(AlgoVizUIMetrics.displaySegments);

        float rangeSegmentSpacing = PaddedHeight / (float) AlgoVizUIMetrics.displaySegments;
        float amountSegmentSpacing = PaddedWidth / (float) AlgoVizUIMetrics.displaySegments;

        float verticalSpacePerUnit = PaddedHeight / range;

        CreateHeader(plotName);
        CreateDisplaySegmentsVertical(range, minValue, values.Count, rangeSegmentSpacing);
        CreateDisplaySegmentsHorizontal(amountSegment, amountSegmentSpacing);       
        PlotValues(values, spacePerPoint, verticalSpacePerUnit, minValue, Color.red);
        gameObject.SetActive(false);
    }

    void CreateHeader(string plotName)
    {
        var plotLabel = Instantiate(displaySegmentPrefab);
        plotLabel.transform.SetParent(plotContainer, false);
        var rT = plotLabel.GetComponent<RectTransform>();
        rT.anchorMin = Vector2.zero;
        rT.anchorMax = Vector2.zero;
        var yPosition = (PaddedHeight + AlgoVizUIMetrics.bottomPadding) * 0.45f;
        rT.anchoredPosition = new Vector2(AlgoVizUIMetrics.leftPadding * 0.1f, yPosition);// - AlgoVizUIMetrics.topPadding * 0.3f);   
        rT.rotation = Quaternion.Euler(0,0,90);     
        plotLabel.SetText(plotName);

        displaySegments.Add(plotLabel);
    }

    void CreateDisplaySegmentsVertical(float range, float minValue, float numValues, float rangeSegmentSpacing)
    {
        float rangeSegment = range / (float)AlgoVizUIMetrics.displaySegments;
        float amountSegment = numValues / (float)(AlgoVizUIMetrics.displaySegments);

        int decimalPoints = 3;
        if(range > 0.1f)
            decimalPoints = 2;
        if(range > 1f)
            decimalPoints = 1;
        if(range > 10f)
            decimalPoints = 0;

        //DISPLAY SEGMENTS VERTICAL
        for (int i = 0; i < AlgoVizUIMetrics.displaySegments + 1; i++)
        {
            var displaySegment = Instantiate(displaySegmentPrefab);
            displaySegment.transform.SetParent(plotContainer, false);
            var rT = displaySegment.GetComponent<RectTransform>();
            float yPosition = i * rangeSegmentSpacing + AlgoVizUIMetrics.bottomPadding;// + (rangeSegmentSpacing * 0.5f);
            rT.anchorMin = Vector2.zero;
            rT.anchorMax = Vector2.zero;
            rT.anchoredPosition = new Vector2(AlgoVizUIMetrics.leftPadding * 0.8f, yPosition);
            var number = minValue + rangeSegment * i;
            var rounded = System.Math.Round(number, decimalPoints);
            displaySegment.SetText((float)rounded);

            displaySegments.Add(displaySegment);

            var connection = Instantiate(plotPointConnectionPrefab);
            connection.transform.SetParent(plotContainer, false);
            connection.PlaceConnection(new Vector2(AlgoVizUIMetrics.leftPadding, yPosition), new Vector2(PlotWidth - AlgoVizUIMetrics.rightPadding, yPosition), Color.grey, 1f);
            plotPointConnections.Add(connection);
        }
    }

    void CreateDisplaySegmentsHorizontal(float amountSegment, float amountSegmentSpacing)
    {
        for (int i = 0; i < AlgoVizUIMetrics.displaySegments + 1; i++)
        {
            var displaySegment = Instantiate(displaySegmentPrefab);
            displaySegment.transform.SetParent(plotContainer, false);
            var rT = displaySegment.GetComponent<RectTransform>();
            float xPosition = i * amountSegmentSpacing + AlgoVizUIMetrics.leftPadding;// + (rangeSegmentSpacing * 0.5f);
            rT.anchorMin = Vector2.zero;
            rT.anchorMax = Vector2.zero;
            rT.anchoredPosition = new Vector2(xPosition, AlgoVizUIMetrics.bottomPadding * 0.8f);
            var number = amountSegment * i;
            var rounded = System.Math.Round(number, 1);
            displaySegment.SetText((float)rounded);

            displaySegments.Add(displaySegment);

            var connection = Instantiate(plotPointConnectionPrefab);
            connection.transform.SetParent(plotContainer, false);
            connection.PlaceConnection(new Vector2(xPosition, AlgoVizUIMetrics.bottomPadding), new Vector2(xPosition, PlotHeight - AlgoVizUIMetrics.topPadding), Color.grey, 1f);
            plotPointConnections.Add(connection);
        }  
        {
            var axisDescription = Instantiate(displaySegmentPrefab);
            axisDescription.transform.SetParent(plotContainer, false);
            var rT = axisDescription.GetComponent<RectTransform>(); 
            float xPosition = (PaddedWidth + AlgoVizUIMetrics.leftPadding) * 0.5f;
            float yPosition = AlgoVizUIMetrics.bottomPadding * 0.45f;
            rT.anchorMin = Vector2.zero;
            rT.anchorMax = Vector2.zero;
            rT.anchoredPosition = new Vector2(xPosition, yPosition);
            axisDescription.SetTextCenterAligned("Generations");
        }
    }

    void PlotValues(List<float> values, float spacePerPoint, float verticalSpacePerUnit, float minValue, Color color)
    {
        PlotPoint lastPoint = null;

        for (int i = 0; i < values.Count; i++)
        {
            var currentPoint = Instantiate(plotPointPrefab);

            float xPosition = i * spacePerPoint + AlgoVizUIMetrics.leftPadding;
            float yPosition = values[i] * verticalSpacePerUnit - minValue * verticalSpacePerUnit + AlgoVizUIMetrics.bottomPadding;
            currentPoint.transform.SetParent(plotContainer, false);
            currentPoint.PlacePoint(new Vector2(xPosition, yPosition));

            plotPoints.Add(currentPoint);

            if(i > 0)
                CreateConnection(lastPoint, currentPoint, color);
            
            lastPoint = currentPoint;
        }        
    }

    public void CreateConnection(PlotPoint pointA, PlotPoint pointB, Color color)
    {
        var connection = Instantiate(plotPointConnectionPrefab);
        connection.transform.SetParent(plotContainer, false);
        connection.PlaceConnection(pointA.AnchoredPosition, pointB.AnchoredPosition, color, 1.7f);

        plotPointConnections.Add(connection);
    }

    
}
