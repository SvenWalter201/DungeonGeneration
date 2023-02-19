using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GraphMetrics
{
    public const float nodeSize = 0.5f; 
    public const float lineWidth = 0.1f;
    public const float lineStartOffset = 0.1f;
    public const float arrowWidth = 0.3f;
    public const float drawPlaneY = 0.0f; //where in the world to draw a graph
    public readonly static Color edgeColor = new Color(0.6f, 0.0f, 0.0f, 1.0f);
    public readonly static Color edgeHoveredColor = new Color(0.9f, 0.0f, 0.0f, 1.0f);
    public readonly static Color edgeSelectedColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    public readonly static Color nodeColor = new Color(0.0f, 0.0f, 1.0f, 1.0f);
    public readonly static Color nodeHoveredColor = new Color(0.0f, 1.0f, 0.0f, 1.0f);
    public readonly static Color nodeSelectedColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);

}
