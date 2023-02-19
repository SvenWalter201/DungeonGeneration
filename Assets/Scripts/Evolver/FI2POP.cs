using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class FI2POP : MonoBehaviour
{
    [SerializeField] GameObject asset;
    public static FI2POP instance;

    void Awake() 
    {
        instance = this;
        Run();
    }

    void Run()
    {
        var iPop = new List<GenericLevel>(SimpleCaseMetrics.popSize);
        var fPop = new List<GenericLevel>(SimpleCaseMetrics.popSize);
        var rouletteSelectionModule = new RouletteSelectionModule(10, true);
        var rouletteInsertionModule = new RouletteInsertionModule();

        for (int i = 0; i < SimpleCaseMetrics.popSize; i++)
        {
            var gL = new GenericLevel();
            for (int n = 0; n < SimpleCaseMetrics.startNodes; n++)
            {
                var ts = new TraversibleSpace
                {
                    x = UnityEngine.Random.Range(0, SimpleCaseMetrics.maxWidth),
                    y = UnityEngine.Random.Range(0, SimpleCaseMetrics.maxHeight)
                };

                gL.AddDesignElement(ts);
            }

            gL.fitness = 0;
            gL.AddConstraint(new ConnectedConstraint());
            var summedError = gL.CheckConstraints();
            if(summedError > 0)
            {
                iPop.Add(gL);
            }
            else
            {
                fPop.Add(gL);
            }
        }

        AlgoViz.Instance.BeginRecord();

        for (int i = 0; i < fPop.Count; i++)
        {
            var gL = fPop[i];
            EvaluateFitness(gL);
        }   


        
        for (int currentGeneration = 0; currentGeneration < SimpleCaseMetrics.maxGenerations; currentGeneration++)
        {
            {
                iPop.Sort((x,y) => x.summedError.CompareTo(y.summedError));
                int offset = iPop.Count / 2;
                for (int i = 0; i < offset; i++)
                {
                    var clone = iPop[i].Clone();
                    clone.Mutate();
                    iPop[i + offset] = clone;
                }

                int smallestError = int.MaxValue;
                for (int i = iPop.Count - 1; i >= 0; i--)
                {
                    var gL = iPop[i];
                    int c = gL.CheckConstraints();
                    if(c == 0) //if no constraints are violated, trasition to fPop
                    {
                        iPop.RemoveAt(i);
                        EvaluateFitness(gL);
                        fPop.Add(gL);
                    }
                    if(c < smallestError)
                        smallestError = c;
                }    
                if(currentGeneration % SimpleCaseMetrics.logInterval == 0)
                {
                    Debug.Log($"Gen {currentGeneration}: Smallest Error: {smallestError}");
                }            
            }
            {
                fPop.Sort((x,y) => -(x.fitness.CompareTo(y.fitness)));
                int offset = fPop.Count / 2;
                for (int i = 0; i < offset; i++)
                {
                    var clone = fPop[i].Clone();
                    clone.Mutate();
                    fPop[i + offset] = clone;
                }

                var children = rouletteSelectionModule.Select(fPop);
                
                foreach (var child in children)
                    child.Mutate();
                
                rouletteInsertionModule.Insert(children, fPop);

                int highestFitness = 0;
                for (int i = 0; i < fPop.Count; i++)
                {
                    var gL = fPop[i];
                    EvaluateFitness(gL);
                    int f = gL.fitness;
                    if(f > highestFitness)
                        highestFitness = f;
                }

                //AlgoViz
                if(currentGeneration % SimpleCaseMetrics.logInterval == 0)
                {   
                    for (int i = 0, x = 0, y = 0; i < fPop.Count; i++, x++)
                    {
                        var gL = fPop[i];
                        var tsElements = gL.GetDesignElementsOfType<TraversibleSpace>();
                        if(x >= 5)
                        {
                            x = 0;
                            y++;
                        }

                        for (int j = 0; j < tsElements.Count; j++)
                        {
                            var tsElement = tsElements[j];
                            Vector3 position = new Vector3(tsElement.x + x * (SimpleCaseMetrics.maxWidth + 3), 0, tsElement.y + y * (SimpleCaseMetrics.maxHeight + 3));
                            AlgoViz.AddDrawCommand(DrawCommand.DrawSquare(position, 1, Color.black), false);
                        }
                    }

                    //Debug.Log($"Gen {currentGeneration}: Highest Fitness: {highestFitness}");
                    AlgoViz.BeginNewStep();
                }
            }

           
        }

        AlgoViz.Instance.EndRecord();
        
    }

    //fitness for now will be the biggest connected area
    void EvaluateFitness(GenericLevel gL)
    {
        var tsElements = gL.GetDesignElementsOfType<TraversibleSpace>();
        bool[,] occupiedSpaces = new bool[SimpleCaseMetrics.maxWidth, SimpleCaseMetrics.maxHeight];
        foreach (var ts in tsElements)
            occupiedSpaces[ts.x, ts.y] = true;
        
        int biggestAreaSize = 0;
        bool[,] visited = new bool[SimpleCaseMetrics.maxWidth, SimpleCaseMetrics.maxHeight];
        for (int x = 0; x < SimpleCaseMetrics.maxWidth; x++)
        {
            for (int y = 0; y < SimpleCaseMetrics.maxHeight; y++)
            {
                if(visited[x,y])
                    continue;

                if(!occupiedSpaces[x,y])   
                    continue;

                int currentAreaSize = 1;
                Queue<int2> area = new Queue<int2>();
                area.Enqueue(new int2(x,y));
                visited[x,y] = true;
                while(area.Count > 0)
                {
                    var coord = area.Dequeue();
                    var offsets = MathHelper.directNeighborOffsets;
                    for (int i = 0; i < offsets.Length; i++)
                    {
                        var neighborCoord = coord + offsets[i];
                        if(MathHelper.isInBounds(neighborCoord, SimpleCaseMetrics.maxWidth, SimpleCaseMetrics.maxHeight))
                        {
                            if(visited[neighborCoord.x, neighborCoord.y])
                                continue;

                            if(!occupiedSpaces[neighborCoord.x, neighborCoord.y])
                                continue;
                            
                            currentAreaSize++;
                            area.Enqueue(neighborCoord);
                            visited[neighborCoord.x, neighborCoord.y] = true;
                        }
                    }
                }

                if(currentAreaSize > biggestAreaSize)
                    biggestAreaSize = currentAreaSize;
            }
        }

        gL.fitness = biggestAreaSize;
        //Debug.Log($"Fitness: {biggestAreaSize}");
    }

    public T SpawnAsset<T>(T original) where T : Object
    {
        T instance = Instantiate<T>(original);
        return instance;
    }
}

public class TraversibleSpace : DesignElement
{
    public int x, y;
    public Object prefab;

    public override void Mutate()
    {
        //SHIFT TILE
        var r = UnityEngine.Random.Range(0f,1f);
        if(r <SimpleCaseMetrics.tileMutationChance)
        {
            int xMutation = UnityEngine.Random.Range(-1,2);
            int yMutation = UnityEngine.Random.Range(-1,2);

            int altX = x + xMutation;
            int altY = y + yMutation;
            if(altX >= 0 && altX < SimpleCaseMetrics.maxWidth)
                x = altX;
            if(altY >= 0 && altY < SimpleCaseMetrics.maxHeight)
                y = altY;           
        }

        //COMPLETELY RANDOMIZE TILE
        r = UnityEngine.Random.Range(0f,1f);
        if(r < 0.01f)
        {
            x = UnityEngine.Random.Range(0,SimpleCaseMetrics.maxWidth);
            y = UnityEngine.Random.Range(0,SimpleCaseMetrics.maxHeight);
        }
    }

    public override void Spawn()
    {
        var instance = FI2POP.instance.SpawnAsset(prefab);
    }

    public override string ToString()
    {
        return $"TS: [{x}:{y}]";
    }

    public override DesignElement Clone()
    {
        var ts = new TraversibleSpace();
        ts.x = x;
        ts.y = y;
        return ts;
    }


}

public class ConnectedConstraint : Constraint
{
    public override int CheckConstraint(GenericLevel population)
    {
        var tsElements = population.GetDesignElementsOfType<TraversibleSpace>();

        bool[,] occupiedSpaces = new bool[SimpleCaseMetrics.maxWidth, SimpleCaseMetrics.maxHeight];
        foreach (var ts in tsElements)
            occupiedSpaces[ts.x, ts.y] = true;
        
        int areas = 0;
        bool[,] visited = new bool[SimpleCaseMetrics.maxWidth, SimpleCaseMetrics.maxHeight];
        for (int x = 0; x < SimpleCaseMetrics.maxWidth; x++)
        {
            for (int y = 0; y < SimpleCaseMetrics.maxHeight; y++)
            {
                if(visited[x,y])
                    continue;

                if(!occupiedSpaces[x,y])   
                    continue;

                Queue<int2> area = new Queue<int2>();
                area.Enqueue(new int2(x,y));
                visited[x,y] = true;
                while(area.Count > 0)
                {
                    var coord = area.Dequeue();
                    var offsets = MathHelper.directNeighborOffsets;
                    for (int i = 0; i < offsets.Length; i++)
                    {
                        var neighborCoord = coord + offsets[i];
                        if(MathHelper.isInBounds(neighborCoord, SimpleCaseMetrics.maxWidth, SimpleCaseMetrics.maxHeight))
                        {
                            if(visited[neighborCoord.x, neighborCoord.y])
                                continue;

                            if(!occupiedSpaces[neighborCoord.x, neighborCoord.y])
                                continue;
                            
                            area.Enqueue(neighborCoord);
                            visited[neighborCoord.x, neighborCoord.y] = true;
                        }
                    }
                }

                areas++;
            }
        }

        //Debug.Log($"Areas: {areas}");

        if(areas > 1)
            return areas;

        return 0;
    }
}

public class RouletteInsertionModule
{
    public void Insert(List<GenericLevel> children, List<GenericLevel> population)
    {
        for (int c = 0; c < children.Count; c++)
        {
            float fitnessSum = 0;

            foreach (var member in population)
                fitnessSum += 1.0f / (float)member.fitness;

            float r = UnityEngine.Random.Range(0, fitnessSum);
            int i = 0;
            for (i = 0; i < population.Count; i++)
            {
                r -= 1.0f / (float)population[i].fitness;
                if(r < 0)
                {
                    population[i] = children[c];
                    break;
                }
            }
        }  
    }
}

public class RouletteSelectionModule
    {
        int amount;
        public int Amount => amount;

        bool replacement;
        public bool Replacement => replacement;

        public RouletteSelectionModule(int amount, bool replacement)
        {
            this.amount = amount;
            this.replacement = replacement; //same member can be picked multple times
        }

        public List<GenericLevel> Select(List<GenericLevel> population)
        {
            List<GenericLevel> selectedMembers = new List<GenericLevel>();
            float fitnessSum = 0;
            foreach (var member in population)
                fitnessSum += member.fitness;

            for (int m = 0; m < amount; m++)
            {
                if(!replacement)
                {
                    fitnessSum = 0;
                    foreach (var member in population)
                        fitnessSum += member.fitness;                
                }
                float r = UnityEngine.Random.Range(0, fitnessSum);
                int i = 0;
                for (i = 0; i < population.Count; i++)
                {
                    r -= population[i].fitness;
                    if(r < 0)
                        break;
                }
                selectedMembers.Add(population[i]);    

                if(!replacement)
                    population.RemoveAt(i);
            }

            if(!replacement)
                population.AddRange(selectedMembers);
            
            return selectedMembers;
        }
    }