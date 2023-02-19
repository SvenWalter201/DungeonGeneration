using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public static class GraphRasterizer
{

    [System.Serializable]
    public class RasterizationResult
    {
        public bool success;
        public List<Var> nodes;
    }

    const int minPathLength = 1, maxPathLength = 3;
    const int minRoomSize = 1, maxRoomSize = 3;
    const int maximumIterations = 100000;


    public static void CreateDetailGrid()
    {
    }


    public static RasterizationResult Rasterize(AdjacencyList graph, int seed)
    {
        UnityEngine.Random.InitState(seed);

        List<Var> nodes = new List<Var>();
        int[] processingOrder = new int[graph.NodeCount];

        for (int i = 0; i < graph.NodeCount; i++)
        {
            nodes.Add(new Var
            {
                neighbors = graph.GetNode(i),
                distanceToRoot = int.MaxValue,
                nodeIndex = i
            });
        }

        GetDistancesToRoot();

        nodes.Sort((Var a, Var b) => a.distanceToRoot.CompareTo(b.distanceToRoot)); //sort by distance for processing order

        for (int i = 0; i < graph.NodeCount; i++)
            processingOrder[i] = nodes[i].nodeIndex;

        //check if the CSP is possible
/*
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
                        return new RasterizationResult
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
*/


        int halfResolution = nodes[nodes.Count - 1].distanceToRoot * (maxPathLength + (maxRoomSize - 1));
        
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

        bool[] assignments = new bool[grid.Length];

        for (int i = 0; i < grid.Length; i++)
            assignments[i] = false;

        int2[] offsets = new int2[4 * maxPathLength];
        int[] offsetOrdering = new int[4 * maxPathLength];
        for (int i = 0; i < offsets.Length; i+=4)
        {
            int distance = i / 4 + 1;
            offsets[i] = new int2(distance,0);
            offsets[i+1] = new int2(-distance,0);
            offsets[i+2] = new int2(0,distance);
            offsets[i+3] = new int2(0,-distance);
        }

        for (int i = 0; i < offsetOrdering.Length; i++)
            offsetOrdering[i] = i;

        int2[] roomSizes = new int2[maxRoomSize * maxRoomSize];
        int[] roomSizeOrdering = new int[maxRoomSize * maxRoomSize];
        for (int x = 1, i = 0; x <= maxRoomSize; x++)
        {
            for (int y = 1; y <= maxRoomSize; y++, i++)
            {
                roomSizes[i] = new int2(x,y);
            }
        }
        for (int i = 0; i < roomSizeOrdering.Length; i++)
            roomSizeOrdering[i] = i;

        for (int i = 0; i < nodes.Count; i++)
        {
            List<int> offsetPickOrder = new List<int>((int[])offsetOrdering.Clone());
            int[] uniqueOffsetOrdering = new int[4 * maxPathLength];
            for (int j = 0; j < 4 * maxPathLength; j++)
            {
                int rand = UnityEngine.Random.Range(0, offsetPickOrder.Count);
                uniqueOffsetOrdering[j] = offsetPickOrder[rand];
                offsetPickOrder.RemoveAt(rand);
            }
            nodes[i].offsetOrdering = uniqueOffsetOrdering;

            List<int> roomSizePickOrder = new List<int>((int[])roomSizeOrdering.Clone());
            int[] uniqueRoomSizeOrdering = new int[maxRoomSize * maxRoomSize];
            for (int j = 0; j < maxRoomSize * maxRoomSize; j++)
            {
                int rand = UnityEngine.Random.Range(0, roomSizePickOrder.Count);
                uniqueRoomSizeOrdering[j] = roomSizePickOrder[rand];
                roomSizePickOrder.RemoveAt(rand);
            }
            nodes[i].roomSizeOrdering = uniqueRoomSizeOrdering;
        }

        nodes[0].position = new int2(halfResolution, halfResolution);          
        assignments[MathHelper.getIndex(nodes[0].position, resolution)] = true;

        int currentIt = 0, currentDepth = 0;
        bool result = Solve(0);

        if(currentIt >= maximumIterations)
            Debug.LogWarning("Rasterization terminated early because maximum iteration count was reached");

        return new RasterizationResult
        {
            success = result,
            nodes = nodes
        };


        bool Solve(int index)
        {
            currentIt++;
            if(currentIt >= maximumIterations)
                return false;

            int nodeIndex = processingOrder[index];
            var current = nodes[nodeIndex];
            currentDepth++;

            if(current.parentNodeIndex == -1)
            {
                if(Solve(index + 1))
                {
                    currentDepth--;
                    return true;
                }
            }
            else
            {
                var parentNode = nodes[current.parentNodeIndex];
                for (int o = 0; o < offsets.Length; o++)
                {
                    int2 currentPosition = parentNode.position + offsets[current.offsetOrdering[o]];
                    int gridIndex = MathHelper.getIndex(currentPosition, resolution);

                    if(assignments[gridIndex])//position is occupied
                    {
                        continue;
                    }

                    bool positionValid = true;

                    for (int n = 0; n < current.neighbors.Count; n++)
                    {


                        var neighbor = nodes[current.neighbors[n]];
                        if(neighbor.position.isM1()) //ignore neighbors that have not been placed yet
                        {
                            continue;
                        }
                        else
                        {
                            AlgoViz.BeginNewStep();
                            DrawCommonInformation(current);
                            AlgoViz.AddInspectorCommand(InspectorCommand.WriteLine("Checking neighbor [" + neighbor.nodeIndex + "] of node [" + nodeIndex + "]"));

                            int2 neighborPosition = neighbor.position;

                            AlgoViz.AddDrawCommand(DrawCommand.DrawSquare(neighborPosition.ToXZV3Scaled(10), 10, Color.red), true);
                            AlgoViz.AddDrawCommand(DrawCommand.DrawLabel(
                                neighborPosition.ToXZV3Scaled(10), 
                                "Nei: " + neighbor.nodeIndex.ToString(), 
                                Color.black), true);

                            //connection needs to be straight
                            bool xSame = currentPosition.x == neighborPosition.x;
                            bool ySame = currentPosition.y == neighborPosition.y;
                            if(!xSame && !ySame)
                            {
                                positionValid = false;
                                break;                              
                            }

                            int2 dir = neighborPosition - currentPosition;
                            
                            int length = math.max(math.abs(dir.x), math.abs(dir.y));
                            dir = dir.clampInt2();
                            if(length > maxPathLength)
                            {
                                positionValid = false;
                                break;
                            }

                            bool pathOccupied = false;
                            for (int j = 0; j < length; j++)
                            {
                                int2 p = currentPosition + dir * j;
                                AlgoViz.AddDrawCommand(DrawCommand.DrawSquare(p.ToXZV3Scaled(10), 10, Color.blue), true);
                                int idx = MathHelper.getIndex(p, resolution);
                                if(assignments[idx])
                                {
                                    pathOccupied = true;
                                    break;
                                }
                            }
                            if(pathOccupied)
                            {
                                positionValid = false;
                                break;
                            }
                        }
                    }

                    if(positionValid)
                    {             
                        AlgoViz.BeginNewStep();
                        DrawCommonInformation(current);
                        AlgoViz.AddInspectorCommand(InspectorCommand.WriteLine("Generating Path from [" + nodeIndex + "] to [" + parentNode.nodeIndex + "]"));

                        int2 connectionDir = currentPosition - parentNode.position;
                        int connectionLength = math.max(math.abs(connectionDir.x), math.abs(connectionDir.y));
                        connectionDir = connectionDir.clampInt2();
                        for (int i = 0; i < connectionLength; i++)
                        {
                            int2 p = parentNode.position + connectionDir * (i + 1);
                            int idx = MathHelper.getIndex(p, resolution);
                            AlgoViz.AddDrawCommand(DrawCommand.DrawSquare(p.ToXZV3Scaled(10), 10, Color.magenta));
                            assignments[idx] = true;
                        }
                        
                        current.position = currentPosition;

                        AlgoViz.AddDrawCommand(DrawCommand.DrawSquare(current.position.ToXZV3Scaled(10), 10, Color.green));
                        AlgoViz.AddDrawCommand(DrawCommand.DrawLabel(
                        current.position.ToXZV3Scaled(10), 
                        "N: " + current.nodeIndex.ToString(), 
                        Color.black));

                        if(index == graph.NodeCount - 1) //reached the last node
                        {
                            currentDepth--;
                            return true;
                        }

                        if(Solve(index + 1))
                        {
                            currentDepth--;
                            return true;
                        }

                        for (int i = 0; i < connectionLength; i++)
                        {
                            int2 p = parentNode.position + connectionDir * (i + 1);
                            int idx = MathHelper.getIndex(p, resolution);
                            assignments[idx] = false;
                        }
                        current.position = MathHelper.int2M1();
                        
                    }
                }
            }

            currentDepth--;
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
                        nodes[n].parentNodeIndex = nodes[current].nodeIndex;
                    }
                }
            }
        }

        void DrawCommonInformation(Var currentNode)
        {
            AlgoViz.AddInspectorCommand(InspectorCommand.WriteLine("Common Information: \n"));
            AlgoViz.AddInspectorCommand(InspectorCommand.WriteLine($"Current Depth: {currentDepth}"));
            AlgoViz.AddInspectorCommand(InspectorCommand.WriteLine($"Currently looking at node {currentNode.nodeIndex}"));

            //Draw all nodes tha are already placed
            for (int i = 0; i < nodes.Count; i++)
            {
                if(!nodes[i].position.isM1())
                {
                    AlgoViz.AddDrawCommand(DrawCommand.DrawSquare(nodes[i].position.ToXZV3Scaled(10), 10, Color.grey));
                    AlgoViz.AddDrawCommand(DrawCommand.DrawLabel(
                        nodes[i].position.ToXZV3Scaled(10), 
                        "Idx: " + nodes[i].nodeIndex.ToString(), 
                        Color.black));                    
                }
            }
        }
    }

    public class RasterizedNode
    {
        public int nodeIndex;
        public int2 position;
        public byte openings; //in which direction the node has to connect to adjacent nodes
    }

    public class RasterizedPath
    {
    }

    public class Var
    {
        public int nodeIndex = -1;
        public int2 position = new int2(-1,-1);
        public int parentNodeIndex = -1;
        public int distanceToRoot = 0;
        public List<int> neighbors;
        public int[] offsetOrdering;
        public int[] roomSizeOrdering;
        public RasterizedNode GenerateRasterizedNode()
        {
            return new RasterizedNode{ nodeIndex = nodeIndex, position = position };
        }
    }
}
