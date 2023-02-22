using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

[RequireComponent(typeof(BoxCollider))]
public class MazeTesting : MonoBehaviour
{
    [SerializeField]
    [Range(20,100)]
    int optimalPathLength = 100;
    [SerializeField]
    [Range(1,30)]
    int optimalCulDeSacLength = 15;
    [HideInInspector] [SerializeField] public int width = 20, height = 20;
    [HideInInspector] [SerializeField] public Cell[] grid;

    [HideInInspector] public BoxCollider boxCollider;

    void Awake() 
    {
        GetComps();
    }

    public void InitializeGrid()
    {
        for (int y = 0, idx = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++, idx++)
            {
                grid[idx] = new Cell(x,y);
            }
        }
    }

    public void CalculateGridMetrics()
    {
        CalculateDistanceToStartCell();
        DetermineOptimalPath();
        DetermineCulDeSacs();
        CalculateFitness();
    }

    public void DetermineOptimalPath()
    {
        var cell = grid[grid.Length - 1];
        if(cell.distanceToStart == -1)
            return; //no path exists

        var offsets = MathHelper.directNeighborOffsets;

        while(cell.distanceToStart > 0)
        {
            for (int i = 0; i < offsets.Length; i++)
            {
                if(!MathHelper.isInBounds(new int2(cell.X + offsets[i].x, cell.Y + offsets[i].y), width, height))
                continue;
                
                var neighborIdx = GetIndex(cell.X + offsets[i].x, cell.Y + offsets[i].y);
                var neighborCell = grid[neighborIdx]; 
                if(neighborCell.occupied)
                    continue;

                if(neighborCell.distanceToStart < cell.distanceToStart)     
                {
                    neighborCell.partOfOptimalPath = true;
                    cell = neighborCell;
                }      
            }
        }

    }

    public void DetermineCulDeSacs()
    {
        var offsets = MathHelper.directNeighborOffsets;

        foreach(var cell in grid)
        {
            if(cell.distanceToStart == -1)
                continue;

            var neighborWithWorseDistanceExists = false;
            for (int i = 0; i < offsets.Length; i++)
            {
                if(!MathHelper.isInBounds(new int2(cell.X + offsets[i].x, cell.Y + offsets[i].y), width, height))
                continue;
                
                var neighborIdx = GetIndex(cell.X + offsets[i].x, cell.Y + offsets[i].y);
                var neighborCell = grid[neighborIdx];
                if(neighborCell.distanceToStart > cell.distanceToStart)
                {
                    neighborWithWorseDistanceExists = true;
                    break;
                }
            }

            if(!neighborWithWorseDistanceExists)
            {
                cell.isCulDeSac = true;
                if(cell.partOfOptimalPath)
                {
                    cell.culDeSacLength = 0;
                    continue;
                }

                int distanceToOptimalPath = 0;

                var current = cell;
                
                while(true)
                {
                    bool foundOptimalPath = false;
                    for (int i = 0; i < offsets.Length; i++)
                    {
                        if(!MathHelper.isInBounds(new int2(current.X + offsets[i].x, current.Y + offsets[i].y), width, height))
                        continue;
                        
                        var neighborIdx = GetIndex(current.X + offsets[i].x, current.Y + offsets[i].y);
                        var neighborCell = grid[neighborIdx]; 
                        if(neighborCell.occupied)
                            continue;


                        if(neighborCell.distanceToStart < current.distanceToStart)     
                        {
                            current = neighborCell;
                            distanceToOptimalPath++;
                            if(neighborCell.partOfOptimalPath)
                            {
                                foundOptimalPath = true;
                                break;
                            }
                        }      
                    }

                    if(foundOptimalPath)
                    {
                        cell.culDeSacLength = distanceToOptimalPath;
                        break;
                    }
                }                
            }
        } 
    }

    public void CalculateDistanceToStartCell()
    {
        foreach(var cell in grid)
        {
            cell.distanceToStart = -1;
            cell.visited = false;
            cell.partOfOptimalPath = false;
            cell.isCulDeSac = false;
        }
        var offsets = MathHelper.directNeighborOffsets;
        var queued = new Queue<Cell>();
        var startCell = grid[0];
        startCell.distanceToStart = 0;
        startCell.visited = true;
        queued.Enqueue(grid[0]);

        while(queued.Count > 0)
        {
            var current = queued.Dequeue();
            var currentDistance = current.distanceToStart;

            for (int i = 0; i < offsets.Length; i++)
            {
                if(MathHelper.isInBounds(new int2(current.X + offsets[i].x, current.Y + offsets[i].y), width, height))
                {
                    var neighborIdx = GetIndex(current.X + offsets[i].x, current.Y + offsets[i].y);
                    var neighborCell = grid[neighborIdx];
                    if(neighborCell.occupied)
                        continue;

                    if(neighborCell.visited)
                        continue;

                    queued.Enqueue(neighborCell);
                    neighborCell.visited = true;
                    neighborCell.distanceToStart = currentDistance + 1;
                }
            }
        }
    }

    public void CalculateFitness()
    {
        var longestPathFitness = LongestPathFitness();
        Debug.Log($"LongestPathFitness: {longestPathFitness}");
        var culDeSacAmount = CulDeSacAmountFitness();
        Debug.Log($"CulDeSacAmountFitness: {culDeSacAmount}");
        var culDeSacLength = CulDeSacLengthFitness();
        Debug.Log($"CulDeSacLengthFitness: {culDeSacLength}");
    }

    float CulDeSacLengthFitness()
    {
        float fitness = 0;
        var maxCulDeSac = optimalCulDeSacLength * 2; //at two or more times the optimal length, CulDeSac's don't give any fitness points
        foreach (var cell in grid)
        {
            if(!cell.isCulDeSac)
                continue;

            if(cell.culDeSacLength >= optimalCulDeSacLength)
            {
                //20 optimum 10 max 30
                //15 optimum 5 max 30
                float t = (cell.culDeSacLength - optimalCulDeSacLength) / (float)(maxCulDeSac - optimalCulDeSacLength);
                fitness += Mathf.Lerp(100, 0, t);
            }
            else
            {
                //5 optimum 15 max 30
                float t = cell.culDeSacLength / (float)optimalCulDeSacLength;
                fitness += Mathf.Lerp(0,100,t);
            }
        }

        return fitness;
    }

    int CulDeSacAmountFitness()
    {
        int count = 0;
        foreach (var cell in grid)
        {
            if(cell.isCulDeSac)
                count++;
        }
        return count;
    }

    float LongestPathFitness()
    {
        var pathLength = grid[grid.Length - 1].distanceToStart;
        var maxLength = (width * height) / 2;
        var minLength = width + height;

        float score = 0;
        if(pathLength >= optimalPathLength)
        {
            var t = (pathLength - optimalPathLength) / (float)maxLength;
            score = Mathf.Lerp(100f, 0f, t);
        }
        else
        {
            var t = (pathLength - minLength) / (float) (optimalPathLength - minLength);
            score = Mathf.Lerp(0f,100f, t);
        }

        return score;
    }

    void Update() 
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 100f))
            {
                if (raycastHit.transform != null)
                {
                    var x = (int)(raycastHit.point.x + width * 0.5f);
                    var y = (int)(raycastHit.point.z + height * 0.5f);

                    ShowInformationForCell(x,y);
                }
            }
        }
    }

    public void ShowInformationForCell(int x, int y)
    {

    }

    public void GetComps()
    {
        boxCollider = GetComponent<BoxCollider>();
    }   

    public int GetIndex(int x, int y) => y * width + x;
    

    void OnDrawGizmos() 
    {
        if(grid == null)
            return;

        for (int y = 0, idx = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++, idx++)
            {
                var cell = grid[idx];
                Gizmos.color = Color.white;
                if(cell.occupied)
                    Gizmos.color = Color.black;
                if(x == 0 && y == 0)
                    Gizmos.color = Color.green;
                else if(x == width - 1 && y == height - 1)
                    Gizmos.color = Color.red;
                else if(cell.isCulDeSac)
                    Gizmos.color = Color.magenta;
                else if(cell.partOfOptimalPath)
                    Gizmos.color = Color.cyan;

                Gizmos.DrawCube(new Vector3(x,0,y) - new Vector3(width * 0.5f, 0f, height * 0.5f) + new Vector3(0.5f, 0f, 0.5f), new Vector3(1f,0.01f,1f));
            }
        }
    }
}

[System.Serializable]
public class Cell
{
    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    [SerializeField] int x = 0, y = 0;
    public int X => x;
    public int Y => y;
    public bool occupied = false, visited = false, isCulDeSac = false, partOfOptimalPath = false;
    public int distanceToStart = 0;
    public int culDeSacLength = 0;


    public Cell Clone()
    {
        var clone = new Cell(x,y);
        clone.occupied = occupied;
        clone.visited = visited;
        clone.isCulDeSac = isCulDeSac;
        clone.partOfOptimalPath = partOfOptimalPath;
        clone.distanceToStart = distanceToStart;
        clone.culDeSacLength = culDeSacLength;
        return clone;
    }
}
