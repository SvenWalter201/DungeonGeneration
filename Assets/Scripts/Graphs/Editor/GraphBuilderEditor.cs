using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;

[CustomEditor(typeof(GraphBuilder))]
public class GraphBuilderEditor : Editor
{
    GraphBuilder tgt;
    SelectionInfo selectionInfo;
    bool needsRepaint = false;

    public override void OnInspectorGUI()
    {
        if(selectionInfo.selectedNodeIndex != -1)
        {
            var currentNode = tgt.graph.GetNode(selectionInfo.selectedNodeIndex);
            EditorGUILayout.LabelField(currentNode.InformationText());
        }

        if(GUILayout.Button("Count Components"))
        {
            tgt.CountComponents();
        }

        if(GUILayout.Button("Rasterize"))
        {
            tgt.Rasterize();
        }

        base.OnInspectorGUI();
    }

    void OnSceneGUI() 
    {
        var guiEvent = Event.current;
        if(guiEvent.type == EventType.Repaint)
        {
            Draw();
            //OnInspectorGUI();
        }
        else if (guiEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }
        else
        {
            HandleInput(guiEvent);
            //HandleUtility.Repaint();
            if (needsRepaint)
            {
                EditorUtility.SetDirty(target);
                HandleUtility.Repaint();
            }
            
        }
        
    }

    void HandleInput(Event guiEvent)
    {
        var mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        float distanceToDrawPlane = (GraphMetrics.drawPlaneY - mouseRay.origin.y) / mouseRay.direction.y;
        Vector3 mousePosition = mouseRay.GetPoint(distanceToDrawPlane);

        UpdateMouseOverInformation(mousePosition);

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.Shift)
            HandleShiftLeftMouseDown(mousePosition);
                
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
            HandleLeftMouseDown(mousePosition);

        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0)
            HandleLeftMouseUp(mousePosition);
        
        
        if (guiEvent.type == EventType.MouseDrag && guiEvent.button == 0 && guiEvent.modifiers == EventModifiers.None)
            HandleLeftMouseDrag(mousePosition);
        
        
    }

    void HandleShiftLeftMouseDown(Vector3 mousePosition)
    {
        DeleteNodeOrEdge();
    }

    void DeleteNodeOrEdge()
    {
        var graph = tgt.graph;
        if(selectionInfo.hoveredNodeIndex  != -1)
        {
            graph.DeleteNode(selectionInfo.hoveredNodeIndex);
            selectionInfo.hoveredNodeIndex = -1;
            needsRepaint = true;
        }
        else if(selectionInfo.selectedNodeIndex != -1)
        {
            graph.DeleteNode(selectionInfo.selectedNodeIndex);
            selectionInfo.selectedNodeIndex = -1;
            needsRepaint = true;
        }
        else if(selectionInfo.hoveredEdgeIndex != -1)
        {
            graph.DeleteEdge(selectionInfo.hoveredEdgeIndex);
            selectionInfo.hoveredEdgeIndex = -1;
            //selectionInfo.selectedEdgeIndex = -1;
            needsRepaint = true;
        }
        else if(selectionInfo.selectedEdgeIndex != -1)
        {
            graph.DeleteEdge(selectionInfo.selectedEdgeIndex);
            selectionInfo.selectedEdgeIndex = -1;
            needsRepaint = true;
        }
    }

    void HandleLeftMouseDown(Vector3 positionWS)
    {
        if(
            selectionInfo.hoveredNodeIndex != -1 && 
            selectionInfo.selectedNodeIndex != -1 && 
            !tgt.graph.AreNodesConnected(selectionInfo.hoveredNodeIndex, selectionInfo.selectedNodeIndex)
        )
            ConnectSelectedNodeAndHoveredNode();
        else if(selectionInfo.hoveredNodeIndex != -1)
            SelectHoveredNode();
        else if(selectionInfo.hoveredEdgeIndex != -1)
            SelectHoveredEdge();
        else if(selectionInfo.selectedNodeIndex != -1)
            DeselectSelectedNode();
        else if(selectionInfo.selectedEdgeIndex != -1)
            DeselectSelectedEdge();
        else
            CreateNode(positionWS);
    }

    void HandleLeftMouseDrag(Vector3 mousePosition)
    {
        if (selectionInfo.selectedNodeIndex != -1)
        {
            tgt.graph.GetNode(selectionInfo.selectedNodeIndex).positionWS = mousePosition;
            selectionInfo.dragging = true;
            needsRepaint = true;
        }
    }

    void HandleLeftMouseUp(Vector3 mousePosition)
    {
        if (selectionInfo.selectedNodeIndex != -1)
        {
            if (selectionInfo.dragging)
            {
                selectionInfo.dragging = false;
                selectionInfo.selectedNodeIndex = -1;
            }

            needsRepaint = true;

        }
    }

    void ConnectSelectedNodeAndHoveredNode()
    {
        tgt.graph.ConnectNodes(selectionInfo.selectedNodeIndex, selectionInfo.hoveredNodeIndex);
        selectionInfo.selectedNodeIndex = -1;
    }

    void DeselectSelectedNode()
    {
        selectionInfo.selectedNodeIndex = -1;
        needsRepaint = true;
    }

    void DeselectSelectedEdge()
    {
        selectionInfo.selectedEdgeIndex = -1;
        needsRepaint = true;
    }

    void SelectHoveredNode()
    {
        selectionInfo.selectedNodeIndex = selectionInfo.hoveredNodeIndex;
        selectionInfo.hoveredNodeIndex = -1;
        selectionInfo.selectedEdgeIndex = -1;
        needsRepaint = true;
    }

    void SelectHoveredEdge()
    {
        selectionInfo.selectedEdgeIndex = selectionInfo.hoveredEdgeIndex;
        selectionInfo.hoveredEdgeIndex = -1;
        selectionInfo.selectedNodeIndex = -1;
        needsRepaint = true;
    }

    void CreateNode(Vector3 positionWS)
    {
        tgt.graph.AddNode(0, positionWS);
        selectionInfo.selectedNodeIndex = -1;
        needsRepaint = true;
    }

    void UpdateMouseOverInformation(Vector3 mousePositionWS)
    {
        var graph = tgt.graph;
        
        for (int i = 0; i < graph.NodeCount; i++)
        {
            if(Vector3.Distance(graph.GetNode(i).positionWS, mousePositionWS) <= GraphMetrics.nodeSize &&
            selectionInfo.selectedNodeIndex != i)
            {
                selectionInfo.hoveredNodeIndex = i;
                return;
            }
        }

        for (int i = 0; i < graph.EdgeCount; i++)
        {
            var currentEdge = graph.GetEdge(i);
            var fromNode = graph.GetNode(currentEdge.fromIndex);
            var goalNode = graph.GetNode(currentEdge.toIndex);
            Vector3 edgeDirection = (goalNode.positionWS - fromNode.positionWS).normalized;
            Vector3 perpendicular = Vector3.Cross(edgeDirection, Vector3.up);
            Vector3 p1 = fromNode.positionWS + perpendicular * GraphMetrics.lineWidth + edgeDirection * GraphMetrics.nodeSize;
            Vector3 p2 = fromNode.positionWS - perpendicular * GraphMetrics.lineWidth + edgeDirection * GraphMetrics.nodeSize;
            Vector3 p3 = goalNode.positionWS + perpendicular * GraphMetrics.lineWidth - edgeDirection * GraphMetrics.nodeSize;
            Vector3 p4 = goalNode.positionWS - perpendicular * GraphMetrics.lineWidth - edgeDirection * GraphMetrics.nodeSize;
            bool mouseOnEdge = MathHelper.IsPointInsideConvexPolygon(mousePositionWS.ToXZF2(), new float2[]
            {
                p1.ToXZF2(),
                p2.ToXZF2(), 
                p4.ToXZF2(), 
                p3.ToXZF2()
                
            });    

            if(mouseOnEdge)
            {
                selectionInfo.hoveredEdgeIndex = i;
                return;
            }
        }
        
        needsRepaint = true;
        selectionInfo.hoveredNodeIndex = -1;
        selectionInfo.hoveredEdgeIndex = -1;
    }

    void Draw()
    {
        var graph = tgt.graph;
        for (int i = 0; i < graph.NodeCount; i++)
        {
            var currentNode = graph.GetNode(i);

            if(selectionInfo.selectedNodeIndex == i)
                Handles.color = GraphMetrics.nodeSelectedColor;
            else if(selectionInfo.hoveredNodeIndex == i)
                Handles.color = GraphMetrics.nodeHoveredColor;
            else
                Handles.color = GraphMetrics.nodeColor;

            Handles.DrawSolidDisc(currentNode.positionWS, Vector3.up, GraphMetrics.nodeSize);

            if(selectionInfo.selectedNodeIndex == i || selectionInfo.hoveredNodeIndex == i)  
            {
                GUIContent content = new GUIContent();
                content.text = currentNode.InformationText();
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 20;
                style.fontStyle = FontStyle.Bold;
                
                Handles.Label(currentNode.positionWS, content, style);
            }          
        }

        for (int i = 0; i < graph.EdgeCount; i++)
        {
            var currentEdge = graph.GetEdge(i);
            var fromNode = graph.GetNode(currentEdge.fromIndex);
            var goalNode = graph.GetNode(currentEdge.toIndex);

            if(selectionInfo.selectedEdgeIndex == i)
                Handles.color = GraphMetrics.edgeSelectedColor;
            else if(selectionInfo.hoveredEdgeIndex == i)
                Handles.color = GraphMetrics.edgeHoveredColor;
            else
                Handles.color = GraphMetrics.edgeColor;

            Vector3 edgeDirection = (goalNode.positionWS - fromNode.positionWS).normalized; 
            Vector3 perpendicular = Vector3.Cross(edgeDirection, Vector3.up);
            Vector3 p1 = fromNode.positionWS + perpendicular * GraphMetrics.lineWidth + edgeDirection * (GraphMetrics.nodeSize + GraphMetrics.lineStartOffset);
            Vector3 p2 = fromNode.positionWS - perpendicular * GraphMetrics.lineWidth + edgeDirection * (GraphMetrics.nodeSize + GraphMetrics.lineStartOffset);
            Vector3 p3, p4;
            p3 = p4 = Vector3.zero;

            if(graph.isDirected)
            {
                p3 = goalNode.positionWS + perpendicular * GraphMetrics.lineWidth - edgeDirection * (GraphMetrics.nodeSize + GraphMetrics.lineStartOffset + GraphMetrics.arrowWidth * 2f);
                p4 = goalNode.positionWS - perpendicular * GraphMetrics.lineWidth - edgeDirection * (GraphMetrics.nodeSize + GraphMetrics.lineStartOffset + GraphMetrics.arrowWidth * 2f);

                Vector3 arrowTipFront = goalNode.positionWS - edgeDirection * (GraphMetrics.nodeSize + GraphMetrics.lineStartOffset);
                Vector3 arrowTipP3 = p3 + perpendicular * (GraphMetrics.arrowWidth - GraphMetrics.lineWidth);
                Vector3 arrowTipP4 = p4 - perpendicular * (GraphMetrics.arrowWidth - GraphMetrics.lineWidth);
                Handles.DrawAAConvexPolygon(new Vector3[]{arrowTipFront, arrowTipP3, arrowTipP4});
            }
            else
            {
                p3 = goalNode.positionWS + perpendicular * GraphMetrics.lineWidth - edgeDirection * (GraphMetrics.nodeSize + GraphMetrics.lineStartOffset);
                p4 = goalNode.positionWS - perpendicular * GraphMetrics.lineWidth - edgeDirection * (GraphMetrics.nodeSize + GraphMetrics.lineStartOffset);
            }

            Handles.DrawAAConvexPolygon(new Vector3[]{p1,p2,p4,p3});
            
        }
    }



    void OnEnable() 
    {
        tgt = target as GraphBuilder;
        selectionInfo = new SelectionInfo();
    }

    void OnDisable() 
    {
        
    }
}


public class SelectionInfo
{
    public int hoveredNodeIndex = -1;
    public int selectedNodeIndex = -1;
    public int hoveredEdgeIndex = -1;
    public int selectedEdgeIndex = -1;
    public bool dragging = false;
}