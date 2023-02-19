using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System;

public static class GraphLibrary
{
    public static void BreathFirstSearch(AdjacencyList graph, int startNode)
    {
        Queue<int> nodeQueue = new Queue<int>();
        bool[] visited = new bool[graph.NodeCount];

        nodeQueue.Enqueue(startNode);
        visited[startNode] = true;

        while(nodeQueue.Count > 0)
        {
            var current = nodeQueue.Dequeue();
            var connectedNodes = graph.GetNode(current);
            foreach(var nodeIndex in connectedNodes)
            {
                if(!visited[nodeIndex])
                {
                    nodeQueue.Enqueue(nodeIndex);
                    visited[nodeIndex] = true;
                }
            }
        }
    }

    public static List<int> BreathFirstSearchShortestPath(AdjacencyList graph, int startNode, int goalNode)
    {
        Queue<int> queue = new Queue<int>();
        bool[] visited = new bool[graph.NodeCount];
        int[] prev = new int[graph.NodeCount];

        for (int i = 0; i < graph.NodeCount; i++)
        {
            visited[i] = false;
            prev[i] = -1;
        }

        queue.Enqueue(startNode);
        visited[startNode] = true;

        while(queue.Count > 0)
        {
            var current = queue.Dequeue();
            var connectedNodes = graph.GetNode(current);
            foreach(var nodeIndex in connectedNodes)
            {
                if(!visited[nodeIndex])
                {
                    queue.Enqueue(nodeIndex);
                    visited[nodeIndex] = true;
                    prev[nodeIndex] = current;
                }
            }
        } 

        List<int> path = new List<int>();
        for (int at = goalNode; at != -1; at = prev[at])
            path.Add(at);
              

        path.Reverse();
        return path[0] == startNode ? path : null;
    }


    public static void DepthFirstSearch(AdjacencyList graph, int startNode)
    {
        bool[] visited = new bool[graph.NodeCount];

        dfs(startNode);

        void dfs(int index)
        {
            visited[index] = true;

            var connectedNodes = graph.GetNode(index);
            foreach(var nodeIndex in connectedNodes)
            {
                if(!visited[nodeIndex])
                    dfs(nodeIndex);
                
            }
        }
    }

    public static ComponentInformation CountComponents(AdjacencyList graph)
    {
        bool[] visited = new bool[graph.NodeCount];
        int[] componentIds = new int[graph.NodeCount];
        int currentComponentId = 0;

        for (int i = 0; i < graph.NodeCount; i++)
        {
            if(!visited[i])
            {
                dfs(i);
                currentComponentId++;
            }

        }

        void dfs(int index)
        {
            visited[index] = true;
            componentIds[index] = currentComponentId;

            var connectedNodes = graph.GetNode(index);
            foreach(var nodeIndex in connectedNodes)
            {
                if(!visited[nodeIndex])
                    dfs(nodeIndex);
                
            }
        }
        return new ComponentInformation { ids = componentIds, count = currentComponentId };
    }

    public static int[] TopologicalSort(AdjacencyList graph)
    {
        bool[] visited = new bool[graph.NodeCount];
        int[] ordering = new int[graph.NodeCount];
        int orderingIndex = graph.NodeCount - 1;

        for (int i = 0; i < graph.NodeCount; i++)
        {
            if(!visited[i])
                dfs(i);
        }

        return ordering;    

        void dfs(int index)
        {
            visited[index] = true;

            var connectedNodes = graph.GetNode(index);
            foreach(var nodeIndex in connectedNodes)
            {
                if(!visited[nodeIndex])
                    dfs(nodeIndex);
            }

            ordering[orderingIndex] = index;
            orderingIndex--;
        }    
    }

    //Single Source Shortest Path on Directed Acyclic Graph
    public static int[] SSSPDAG(int[] ordering, WeightedAdjacencyList graph)
    {
        int[] distances = new int[graph.NodeCount];
        for (int i = 0; i < graph.NodeCount; i++)
            distances[i] = int.MaxValue;

        distances[0] = 0;

        for (int i = 0; i < graph.NodeCount; i++)
        {
            var connectedNodes = graph.GetConnectedNodes(ordering[i]);
            for (int j = 0; j < connectedNodes.Count; j++)
            {
                var currentEdge = connectedNodes[j];
                int newDistance = distances[i] + currentEdge.edgeWeight;
                if(newDistance < distances[currentEdge.toNode])
                    distances[currentEdge.toNode] = newDistance;
            }
        }
        
        return distances;
    }

    //Single Source Longest Path on Directed Acyclic Graph
    public static int[] SSLPDAG(int[] ordering, WeightedAdjacencyList graph)
    {
        int[] distances = new int[graph.NodeCount];
        for (int i = 0; i < graph.NodeCount; i++)
            distances[i] = int.MaxValue;

        distances[0] = 0;

        for (int i = 0; i < graph.NodeCount; i++)
        {
            var connectedNodes = graph.GetConnectedNodes(ordering[i]);
            for (int j = 0; j < connectedNodes.Count; j++)
            {
                var currentEdge = connectedNodes[j];
                int newDistance = distances[i] - currentEdge.edgeWeight;
                if(newDistance < distances[currentEdge.toNode])
                    distances[currentEdge.toNode] = newDistance;
            }
        }

        for (int i = 0; i < graph.NodeCount; i++)
            distances[i] *= -1;      

        return distances;
    }

    //Tarjan's Algorithm for finding strongly cnnected components
    public static ComponentInformation FindSCCs(AdjacencyList graph)
    {
        int n = graph.NodeCount;
        int id = 0;
        int sscCount = 0;
        int[] ids = new int[n];
        int[] lowlinkValues = new int[n];
        bool[] onStack = new bool[n];
        Stack<int> stack = new();

        for (int i = 0; i < n; i++)
            ids[i] = -1; //mark as unvisited
        
        for (int i = 0; i < n; i++)
        {
            if(ids[i] == -1)
                dfs(i);
        }

        return new ComponentInformation
        {
            ids = lowlinkValues,
            count = sscCount
        };

        void dfs(int index)
        {
            stack.Push(index);
            onStack[index] = true;
            ids[index] = lowlinkValues[index] = id;
            id++;

            var connectedNodes = graph.GetNode(index);
            foreach(var nodeIndex in connectedNodes)
            {
                if(ids[nodeIndex] == -1)
                    dfs(nodeIndex);

                if(onStack[nodeIndex])
                    lowlinkValues[index] = Mathf.Min(lowlinkValues[index], lowlinkValues[nodeIndex]);
            }

            if(ids[index] == lowlinkValues[index])
            {
                for (int node = stack.Pop();; node = stack.Pop())
                {
                    onStack[node] = false;
                    lowlinkValues[node] = ids[index];
                    if(node == index)
                        break;
                }
                sscCount++;
            }
        } 
    }

    [System.Serializable]
    public class ComponentInformation
    {
        public int[] ids;
        public int count;
    }


/*
    public static CSPResult CSPOptimized(AdjacencyList graph)
    {
        List<Var> nodes = new List<Var>();
        int[] processingOrder = new int[graph.NodeCount];

        for (int i = 0; i < graph.NodeCount; i++)
        {
            nodes.Add(new Var
            {
                position = -1,
                neighbors = graph.GetNode(i),
                distanceToRoot = int.MaxValue,
                nodeIndex = i
            });
        }

        GetDistancesToRoot();

        nodes.Sort((Var a, Var b) => a.distanceToRoot.CompareTo(b.distanceToRoot)); //sort by distance for processing order
        for (int i = 0; i < graph.NodeCount; i++)
            processingOrder[i] = nodes[i].nodeIndex;

        int halfResolution = nodes[nodes.Count - 1].distanceToRoot;


        //check if the CSP is possible
        if(nodes.Count > 5)
        {
            int maxAmount = 4;
            int currentDistance = 1;
            int count = 0;
            for (int i = 1; i < nodes.Count; i++)
            {
                int d = nodes[i].distanceToRoot;
                if(d == currentDistance)
                {
                    count++;
                    if(count > maxAmount)
                    {
                        Debug.LogWarning("This Graph can not be rasterized. Only " + maxAmount + 
                        " nodes are allowed at distance " + currentDistance + " away from the root node");
                        return new CSPResult
                        {
                            success = false
                        };
                    }
                }
                else
                {
                    count = 1;
                    currentDistance = d;
                    maxAmount += 4;
                }
            }
        }

        
        nodes.Sort((Var a, Var b) => a.nodeIndex.CompareTo(b.nodeIndex)); //sort by index for correct neighbor references

        int resolution = halfResolution * 2 + 1;
        int2[] grid = new int2[resolution * resolution];
        for (int y = 0, i = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++, i++)
            {
                grid[i] = new int2(x,y);
            }
        }        

        for (int i = 0; i < graph.NodeCount; i++)
        {
            int distanceToRoot = nodes[i].distanceToRoot;
            int domainResolution = distanceToRoot * 2 + 1;
            int distanceToGridWalls = halfResolution - distanceToRoot;
            int2[] domain = new int2[domainResolution * domainResolution];
            for (int y = 0, index = 0; y < domainResolution; y++)
            {
                for (int x = 0; x < domainResolution; x++, index++)
                {
                    domain[index] = new int2(x + distanceToGridWalls,y + distanceToGridWalls);
                }
            }
            nodes[i].domain = domain;
        }

        int[] assignments = new int[grid.Length];
        for (int i = 0; i < grid.Length; i++)
            assignments[i] = -1;

        bool result = Solve(0);

        return new CSPResult
        {
            success = result,
            nodes = nodes
        };

        bool Solve(int index)
        {
            int nodeIndex = processingOrder[index];
            var current = nodes[nodeIndex];
            for (int i = 0; i < current.domain.Length; i++)
            {
                int2 currentPosition = current.domain[i];
                int gridIndex = MathHelper.getIndex(currentPosition, resolution);
                if(assignments[gridIndex] != -1)//position is occupied
                    continue;

                bool positionValid = true;
                for (int n = 0; n < current.neighbors.Count; n++)
                {
                    var neighbor = nodes[current.neighbors[n]];
                    if(neighbor.position.isM1()) //ignore neighbors that have not been placed yet
                        continue;
                    else
                    {
                        int2 neighborPosition = neighbor.position;
                        //if the neighbors are already placed and too far away
                        if(!Var.ConnectionPossible(currentPosition, neighborPosition)) 
                        {
                            positionValid = false;
                            break;
                        }
                    }
                }


                if(positionValid)
                {
                    assignments[gridIndex] = nodeIndex;
                    current.position = gridIndex;

                    if(index == graph.NodeCount - 1) //reached the last node
                        return true;

                    if(Solve(index + 1))
                        return true;

                    assignments[gridIndex] = -1;
                    current.position = -1;
                    
                }
            }

            return false;
        }

        void GetDistancesToRoot()
        {
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(0);
            nodes[0].distanceToRoot = 0;

            while(queue.Count > 0)
            {
                var current = queue.Dequeue();
                var neighbors = graph.GetNode(current);
                foreach(var n in neighbors)
                {
                    if(nodes[n].distanceToRoot == int.MaxValue)
                    {
                        queue.Enqueue(n);
                        nodes[n].distanceToRoot = nodes[current].distanceToRoot + 1;
                    }
                }
            }
        }
    }
*/
/*
    public static CSPResult CSP(AdjacencyList graph, int2[] grid)
    {
        Var[] nodes = new Var[graph.NodeCount];
        int[] assignments = new int[grid.Length];
        for (int i = 0; i < graph.NodeCount; i++)
        {
            nodes[i] = new Var
            {
                assignment = -1,
                //domain = (int2[])possiblePositions.Clone(),
                neighbors = graph.GetNode(i)
            };
        }

        for (int i = 0; i < grid.Length; i++)
            assignments[i] = -1;

        bool result = Solve(0);

        return new CSPResult
        {
            success = result,
            nodes = nodes
        };
        bool Solve(int nodeIndex)
        {
            var current = nodes[nodeIndex];
            for (int i = 0; i < grid.Length; i++)
            {
                int2 currentPosition = grid[i];
                if(assignments[i] != -1)//position is free
                    continue;

                bool positionValid = true;
                for (int n = 0; n < current.neighbors.Count; n++)
                {
                    var neighbor = nodes[current.neighbors[n]];
                    if(neighbor.assignment == -1) //ignore neighbors that have not been placed yet
                        continue;
                    else
                    {
                        int2 neighborPosition = grid[neighbor.assignment];
                        //if the neighbors are already placed and too far away
                        if(!Var.ConnectionPossible(currentPosition, neighborPosition)) 
                        {
                            positionValid = false;
                            break;
                        }
                    }
                }

                if(positionValid)
                {
                    assignments[i] = nodeIndex;
                    current.assignment = i;

                    if(nodeIndex == graph.NodeCount - 1) //reached the last node
                        return true;
                    

                    if(Solve(nodeIndex + 1))
                        return true;
                    else
                    {
                        assignments[i] = -1;
                        current.assignment = -1;
                    }
                }


            }

            return false;
        }  
    }
*/

}
