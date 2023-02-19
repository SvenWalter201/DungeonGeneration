using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class GraphBuilder : MonoBehaviour
{
    public bool reseed = true;
    public string seed = "";
    public Graph<int> graph = new Graph<int>();

    public int CountComponents()
    {
        var info = GraphLibrary.CountComponents(graph.GetAdjacencyList());
        Debug.Log(info.count);
        return info.count;
    }

    public void Rasterize()
    {
        var result = GraphRasterizer.Rasterize(graph.GetAdjacencyList(), seed.GetHashCode());
        if(!result.success)
        {
            Debug.Log("Could not find solution");
            return;
        }

        int xMin, xMax, yMin, yMax;
        xMin = yMin = int.MaxValue;
        xMax = yMax = 0;
        for (int i = 0; i < result.nodes.Count; i++)
        {
            var current = result.nodes[i];
            var p = current.position;

            if(p.x < xMin)
                xMin = p.x;

            if(p.x > xMax)
                xMax = p.x;

            if(p.y < yMin)
                yMin = p.y;

            if(p.y > yMax)
                yMax = p.y;        
        }  

        for (int i = 0; i < result.nodes.Count; i++)
        {
            var current = result.nodes[i];
            current.position -= new int2(xMin, yMin);
            var node = graph.GetNode(current.nodeIndex);
            node.positionWS = new Vector3(current.position.x * 10f, 0f, current.position.y * 10f) + new Vector3(5f, 0f, 5f);
        }
    }
}

