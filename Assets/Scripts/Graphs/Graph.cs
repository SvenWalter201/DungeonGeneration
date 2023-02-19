using UnityEngine;
using System.Collections.Generic;

//A datastructure representing a graph. Supports options for converting it into other representations, aswell as options to enforce
//a certain topology (directed/undirected), (cyclic/acyclic)
[System.Serializable]
public class Graph<T>
{
    public bool isDirected = false;
    public int NodeCount => nodes.Count;
    public int EdgeCount => edges.Count;
    [SerializeField] List<Node<T>> nodes;
    [SerializeField] List<Edge> edges;
    [SerializeField] GraphLibrary.ComponentInformation componentInformation, sscInformation;



    //TODO: Update this so it works with directed Graphs
    public bool AreNodesConnected(int a, int b)
    {
        for (int i = 0; i < edges.Count; i++)
        {
            if((edges[i].fromIndex == a && edges[i].toIndex == b) || (edges[i].fromIndex == b && edges[i].toIndex == a))
                return true;
        }
        return false;        
    }

    public void AddNode(T data, Vector3 position)
    {
        int index = nodes.Count;
        nodes.Add(new Node<T>(data, position, index));
        OnGraphTopologyUpdated();
    }

    public void ConnectNodes(int a, int b)
    {
        if(!AreNodesConnected(a,b))
        {
            int index = edges.Count;
            edges.Add(new Edge(a, b, 0));
            nodes[a].AddEdge(index);
            nodes[b].AddEdge(index);
        }
        OnGraphTopologyUpdated();
    }

    public void DeleteNode(int index)
    {
        for (int i = edges.Count - 1; i >= 0; i--)
        {
            var currentEdge = edges[i];
            if(currentEdge.fromIndex == index || currentEdge.toIndex == index)
                {
                    edges.RemoveAt(i);
                    UpdateEdgeReferences(i);
                    continue;
                }
            if(currentEdge.fromIndex > index)
                currentEdge.fromIndex--;

            if(currentEdge.toIndex > index)
                currentEdge.toIndex--; 
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            var current = nodes[i];

            if(current.index > index)
                current.index--;
        }
        nodes.RemoveAt(index);
        OnGraphTopologyUpdated();
    }

    public void DeleteEdge(int index)
    {
        edges.RemoveAt(index);
        UpdateEdgeReferences(index);
        OnGraphTopologyUpdated();
    }

    public void UpdateEdgeReferences(int deletedEdgeIndex)
    {
        for (int i = 0; i < nodes.Count; i++)
            nodes[i].UpdateEdgeReferences(deletedEdgeIndex);
        
    }

    //run some algorithms to get information about the graphs topology
    void OnGraphTopologyUpdated()
    {
        var adjacencyList = GetAdjacencyList();
        componentInformation = GraphLibrary.CountComponents(adjacencyList);
        if(isDirected)
        {
            sscInformation = GraphLibrary.FindSCCs(adjacencyList);
        }
    }

#region GETTERS
    public Node<T> GetNode(int index) =>
        index < nodes.Count ? nodes[index] : null;
    
    public Edge GetEdge(int index) =>
        index < edges.Count ? edges[index] : null;
    
    public AdjacencyList GetAdjacencyList()
    {
        List<List<int>> adjacencyList = new();
        for (int i = 0; i < nodes.Count; i++)
        {
            List<int> adjacentVertices = new List<int>();
            foreach (var edgeIndex in nodes[i].edges)
            {
                var currentEdge = edges[edgeIndex];
                if(isDirected)
                {
                    if(currentEdge.fromIndex == i)
                        adjacentVertices.Add(currentEdge.toIndex);
                }
                else
                    adjacentVertices.Add(currentEdge.fromIndex == i ? currentEdge.toIndex : currentEdge.fromIndex);
            }            
            adjacencyList.Add(adjacentVertices);
        }

        return new AdjacencyList { adjacencyList = adjacencyList};
    }

    public WeightedAdjacencyList GetWeightedAdjacencyList()
    {
        List<List<WeightedAdjacencyList.EdgeInformation>> adjacencyList = new();
        for (int i = 0; i < nodes.Count; i++)
        {
            List<WeightedAdjacencyList.EdgeInformation> adjacentVertices = new();
            foreach (var edgeIndex in nodes[i].edges)
            {
                var currentEdge = edges[edgeIndex];
                if(isDirected)
                {
                    if(currentEdge.fromIndex == i)
                        adjacentVertices.Add(new WeightedAdjacencyList.EdgeInformation
                        {
                            toNode = currentEdge.toIndex, 
                            edgeWeight = currentEdge.cost
                        });
                }
                else
                {
                    int toIndex = currentEdge.fromIndex == i ? currentEdge.toIndex : currentEdge.fromIndex;
                    adjacentVertices.Add(new WeightedAdjacencyList.EdgeInformation
                        {
                            toNode = toIndex, 
                            edgeWeight = currentEdge.cost
                        });

                }
            }            
            adjacencyList.Add(adjacentVertices);
        }

        return new WeightedAdjacencyList { adjacencyList = adjacencyList};        
    }
#endregion GETTERS
}

[System.Serializable]
public class AdjacencyList
{
    public int NodeCount => adjacencyList.Count;
    public List<List<int>> adjacencyList;
    public List<int> GetNode(int index) => adjacencyList[index];
}

[System.Serializable]
public class WeightedAdjacencyList
{
    public int NodeCount => adjacencyList.Count;
    public List<List<EdgeInformation>> adjacencyList;
    public List<EdgeInformation> GetConnectedNodes(int index) => adjacencyList[index];

    [System.Serializable]
    public class EdgeInformation
    {
        public int toNode;
        public int edgeWeight;
    }
}



[System.Serializable]
public class Node<T>
{
    public T data;
    public Vector3 positionWS;
    public int index;
    public List<int> edges;

    public Node(T data, Vector3 positionWS, int index)
    {
        edges = new List<int>();
        this.data = data;
        this.positionWS = positionWS;
        this.index = index;
    }

    public string InformationText() 
    {
        return "Node " + index;
    }
    
    public void AddEdge(int edgeIndex)
    {
        edges.Add(edgeIndex);
    }

    public void UpdateEdgeReferences(int deletedEdgeIndex)
    {
        for (int i = edges.Count - 1; i >= 0; i--)
        {
            if(edges[i] == deletedEdgeIndex)
                edges.RemoveAt(i);
            else if(edges[i] > deletedEdgeIndex)
                edges[i]--;
        }
    }

}

[System.Serializable]
public class Edge
{
    public int fromIndex, toIndex;
    public int cost;

    public Edge(int fromIndex, int toIndex, int cost = 1)
    {
        this.fromIndex = fromIndex;
        this.toIndex = toIndex;
        this.cost = cost;
    }
}

