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

    public void CreatePlot(string plotName, List<float> values)
    {
        StartCoroutine(CreatePlotDelayed(plotName, values));
    }

    IEnumerator CreatePlotDelayed(string plotName, List<float> values)
    {
        int d = 3;
        while(d > 0)
        {
            d--;
            yield return null;
        }
        PlotPoint lastPoint = null;
        float plotHeight = plotContainer.sizeDelta.y;
        float plotWidth = plotContainer.sizeDelta.x;
        float paddedPlotWidth = plotWidth - (AlgoVizUIMetrics.leftPadding + AlgoVizUIMetrics.rightPadding);
        float spacePerPoint = paddedPlotWidth / (values.Count - 1);

        
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
        float amountSegment = values.Count / (float)AlgoVizUIMetrics.displaySegments;
        float paddedPlotHeight = plotHeight - (AlgoVizUIMetrics.bottomPadding + AlgoVizUIMetrics.topPadding);

        float rangeSegmentSpacing = paddedPlotHeight / (float) AlgoVizUIMetrics.displaySegments;
        float amountSegmentSpacing = paddedPlotWidth / (float) AlgoVizUIMetrics.displaySegments;

        float verticalSpacePerUnit = paddedPlotHeight / range;

        //HEADER
        {
            var plotLabel = Instantiate(displaySegmentPrefab);
            plotLabel.transform.SetParent(plotContainer, false);
            var rT = plotLabel.GetComponent<RectTransform>();
            rT.anchorMin = Vector2.zero;
            rT.anchorMax = Vector2.zero;
            rT.anchoredPosition = new Vector2(plotWidth / 2.0f, plotHeight - AlgoVizUIMetrics.topPadding * 0.3f);        
            plotLabel.SetText(plotName);

            displaySegments.Add(plotLabel);
        }


        //DISPLAY SEGMENTS VERTICAL
        for (int i = 0; i < AlgoVizUIMetrics.displaySegments + 1; i++)
        {
            var displaySegment = Instantiate(displaySegmentPrefab);
            displaySegment.transform.SetParent(plotContainer, false);
            var rT = displaySegment.GetComponent<RectTransform>();
            float yPosition = i * rangeSegmentSpacing + AlgoVizUIMetrics.bottomPadding;// + (rangeSegmentSpacing * 0.5f);
            rT.anchorMin = Vector2.zero;
            rT.anchorMax = Vector2.zero;
            rT.anchoredPosition = new Vector2(AlgoVizUIMetrics.labelXOffset, yPosition);
            var number = minValue + rangeSegment * i;
            var rounded = System.Math.Round(number, 1);
            displaySegment.SetText((float)rounded);

            displaySegments.Add(displaySegment);

            var connection = Instantiate(plotPointConnectionPrefab);
            connection.transform.SetParent(plotContainer, false);
            connection.PlaceConnection(new Vector2(AlgoVizUIMetrics.labelXOffset, yPosition), new Vector2(plotWidth - AlgoVizUIMetrics.rightPadding, yPosition), Color.white, 1f);
            plotPointConnections.Add(connection);
        }

        //DISPLAY SEGMENTS HORIZONTAL
        for (int i = 0; i < AlgoVizUIMetrics.displaySegments + 1; i++)
        {
            var displaySegment = Instantiate(displaySegmentPrefab);
            displaySegment.transform.SetParent(plotContainer, false);
            var rT = displaySegment.GetComponent<RectTransform>();
            float xPosition = i * amountSegmentSpacing;// + (rangeSegmentSpacing * 0.5f);
            rT.anchorMin = Vector2.zero;
            rT.anchorMax = Vector2.zero;
            rT.anchoredPosition = new Vector2(xPosition, AlgoVizUIMetrics.bottomPadding / 2f);
            var number = amountSegment * i;
            var rounded = System.Math.Round(number, 1);
            displaySegment.SetText((float)rounded);

            displaySegments.Add(displaySegment);

            var connection = Instantiate(plotPointConnectionPrefab);
            connection.transform.SetParent(plotContainer, false);
            connection.PlaceConnection(new Vector2(xPosition, AlgoVizUIMetrics.bottomPadding), new Vector2(xPosition, plotHeight - AlgoVizUIMetrics.topPadding), Color.white, 1f);
            plotPointConnections.Add(connection);
        }        

        for (int i = 0; i < values.Count; i++)
        {
            var currentPoint = Instantiate(plotPointPrefab);

            float xPosition = i * spacePerPoint + AlgoVizUIMetrics.leftPadding;
            float yPosition = values[i] * verticalSpacePerUnit - minValue * verticalSpacePerUnit + AlgoVizUIMetrics.bottomPadding;
            currentPoint.transform.SetParent(plotContainer, false);
            currentPoint.PlacePoint(new Vector2(xPosition, yPosition));

            plotPoints.Add(currentPoint);

            if(i > 0)
            {
                CreateConnection(lastPoint, currentPoint);
            }
            lastPoint = currentPoint;
        }
        gameObject.SetActive(false);
    }

    public void CreateConnection(PlotPoint pointA, PlotPoint pointB)
    {
        var connection = Instantiate(plotPointConnectionPrefab);
        connection.transform.SetParent(plotContainer, false);
        connection.PlaceConnection(pointA.AnchoredPosition, pointB.AnchoredPosition, Color.red, 2.2f);

        plotPointConnections.Add(connection);
    }
}
