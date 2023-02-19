using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace PipelineV3.Maze
{
    public class MazeLevelGenerator : EvolutionaryAlgorithmLayer
    {
        public MazeLevelGenerator( GenericParameters genericParams) : base(genericParams){}

        protected override void ExecutionFinishedCallback(int currentGeneration)
        {
            AlgoViz.CreatePlot("fPopSize");
            AlgoViz.CreatePlot("maxFitness");
            AlgoViz.CreatePlot("minVio");
            AlgoViz.CreatePlot("amountDE");
            AlgoViz.CreatePlot("amountWalls");

        }

        protected override void EndOfGenerationCallback(int currentGeneration)
        {
            int countDEs = 0;
            foreach (var member in iPop.members)
                countDEs += member.GetCountOfDesignElementsOfType<OccupiedCellMazeDesignElement>();
            
            foreach (var member in fPop.members)
                countDEs += member.GetCountOfDesignElementsOfType<OccupiedCellMazeDesignElement>();
            
            float averageDEs = countDEs / (float)(fPop.Count + iPop.Count);

            int countWalls = 0;
            foreach (var member in iPop.members)
                countWalls += member.GetCountOfDesignElementsOfType<MazeWallDesignElement>();
            
            foreach (var member in fPop.members)
                countWalls += member.GetCountOfDesignElementsOfType<MazeWallDesignElement>();
            float averageWalls = countWalls / (float)(fPop.Count + iPop.Count);

            //Debug.Log($"Current Generation: {currentGeneration}");
            AlgoViz.AddPlotPoint("fPopSize", fPop.Count);
            AlgoViz.AddPlotPoint("maxFitness", fPop.Count > 0 ? fPop.members[0].fitness : 0);
            AlgoViz.AddPlotPoint("minVio", iPop.Count > 0 ? iPop.members[0].violatedConstraints : 0);
            AlgoViz.AddPlotPoint("amountDE", averageDEs);
            AlgoViz.AddPlotPoint("amountWalls", averageWalls);

            if(currentGeneration % 50 == 0)
            {
                AlgoViz.BeginNewStep();
                AlgoViz.AddDrawCommand(DrawCommand.DrawUITexture(Vector3.zero, GenerateTexture()));
            }
        }

        Color iPopColor = Color.red, fPopColor = Color.green;
        const int marginPxl = 3;
        public Sprite GenerateTexture()
        {
            var totalCount = MazeBuilderMetrics.POP_SIZE * 2;
            var sqrt = Mathf.CeilToInt(Mathf.Sqrt(totalCount));
            var marginPixels = (sqrt + 1) * marginPxl;
            var pixelWidth = MazeBuilderMetrics.WIDTH * sqrt + marginPixels;
            var pixelHeight = MazeBuilderMetrics.HEIGHT * sqrt + marginPixels;

            var tex = new Texture2D(pixelWidth, pixelHeight);
            var colors = new Color[pixelWidth * pixelHeight];

            var unitSizeY = MazeBuilderMetrics.HEIGHT + marginPxl;
            var unitSizeX = MazeBuilderMetrics.WIDTH + marginPxl;

            for (int y = 0, idx = 0; y < pixelHeight; y++)
            {
                var unitY = y / unitSizeY;
                var inUnitY = y % unitSizeY;

                inUnitY -= marginPxl;

                for (int x = 0; x < pixelWidth; x++, idx++)
                {
                    var unitX = x / unitSizeX;
                    var inUnitX = x % unitSizeX;

                    inUnitX -= marginPxl;
                    if(inUnitX < 0 || inUnitY < 0)
                    {
                        colors[idx] = Color.grey;
                    }
                    else
                    {
                        int levelIdx = unitX + (sqrt * unitY);
                        GenericLevel level = null;
                        Color occupiedColor = Color.black;
                        if(levelIdx >= iPop.Count)
                        {
                            levelIdx -= iPop.Count;
                            if(levelIdx >= fPop.Count)
                            {
                                colors[idx] = Color.black;
                                continue;
                            }
                            else
                            {
                                level = fPop.members[levelIdx];
                                occupiedColor = new Color(0.05f,0.5f,0.1f,1f);
                            }
                        }
                        else
                        {
                            level = iPop.members[levelIdx];
                            occupiedColor = new Color(0.5f,0.05f,0.1f,1f);
                        }

                        var castEnv = (MazeSpawnEnvironment)level.spawnEnvironment;
                        var occupied = castEnv.occupation[inUnitX, inUnitY];
                        colors[idx] = occupied ? occupiedColor : Color.white;
                    }
                }
            }
            var rect = new Rect();
            rect.center = new Vector2(0,0);
            rect.height = pixelHeight;
            rect.width = pixelWidth;
            tex.SetPixels(colors);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();
            return Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f));
        }

        bool[,] visitedArr;
        int[,] distancesArr;

        void ResetVisited()
        {
            for (int y = 0; y < MazeBuilderMetrics.HEIGHT; y++)
            {
                for (int x = 0; x < MazeBuilderMetrics.WIDTH; x++)
                {
                    visitedArr[x,y] = false;
                }
            }
        }

        public override void InitializePopulation(GenericLevel previousLayerSolution)
        {
            visitedArr = new bool[MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT];
            distancesArr = new int[MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT];
            ResetVisited();

            var startCellAmount = (int)(MazeBuilderMetrics.WIDTH * MazeBuilderMetrics.HEIGHT * MazeBuilderMetrics.START_FILL_PERCENTAGE);
            iPop = new Population(genericParams, MazeBuilderMetrics.POP_SIZE, false);
            fPop = new Population(genericParams, MazeBuilderMetrics.POP_SIZE, true);
            
            for (int i = 0; i < MazeBuilderMetrics.POP_SIZE; i++)
            {
                var spawnEnvironment = new MazeSpawnEnvironment();
                spawnEnvironment.Initialize();

                var gL = new GenericLevel(spawnEnvironment, new MazeMutation(), new MazeCrossover());

                for (int j = 0; j < startCellAmount; j++)
                {
                    var dE = new OccupiedCellMazeDesignElement(gL);
                    gL.AddDesignElement(dE);
                    dE.Spawn();
                }
                for (int j = 0; j < MazeBuilderMetrics.START_WALL_AMOUNT; j++)
                {
                    var dE = new MazeWallDesignElement(gL);
                    gL.AddDesignElement(dE);
                    dE.Spawn();
                }

                iPop.members.Add(gL);
            }
        }

        public override bool TerminationCondition()
        {
            return false;
        }

        //do a breath first search from the start (0,0)
        public int LongestPathFitness(GenericLevel gL)
        {
            for (int y = 0; y < MazeBuilderMetrics.HEIGHT; y++)
            {
                for (int x = 0; x < MazeBuilderMetrics.WIDTH; x++)
                {
                    distancesArr[x,y] = 0;
                }
            }

            var spawnEnvironment = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var occupiedSpaces = spawnEnvironment.occupation;

            var queued = new Queue<int2>();
            ResetVisited();

            queued.Enqueue(new int2(0,0));

            while(queued.Count > 0)
            {
                var current = queued.Dequeue();
                var offsets = MathHelper.directNeighborOffsets;
                var currentDistance = distancesArr[current.x, current.y];

                for (int i = 0; i < offsets.Length; i++)
                {
                    var neighborCoord = current + offsets[i];
                    if(MathHelper.isInBounds(neighborCoord, MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT))
                    {
                        if(occupiedSpaces[neighborCoord.x, neighborCoord.y])
                            continue;

                        if(visitedArr[neighborCoord.x, neighborCoord.y])
                            continue; 

                        if(neighborCoord.x == MazeBuilderMetrics.WIDTH - 1 && neighborCoord.y == MazeBuilderMetrics.HEIGHT - 1)
                            return currentDistance + 1;

                        queued.Enqueue(neighborCoord);
                        visitedArr[neighborCoord.x, neighborCoord.y] = true;
                        distancesArr[neighborCoord.x, neighborCoord.y] = currentDistance + 1;
                    }
                }
            }

            return 0;
        }

        public int LargestAreaFitness(GenericLevel gL)
        {
            var spawnEnvironment = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var occupiedSpaces = spawnEnvironment.occupation;
            var biggestAreaSize = 0;
            ResetVisited();

            for (int x = 0; x < MazeBuilderMetrics.WIDTH; x++)
            {
                for (int y = 0; y < MazeBuilderMetrics.HEIGHT; y++)
                {
                    if(visitedArr[x,y])
                        continue;

                    if(occupiedSpaces[x,y])   
                        continue;

                    var currentAreaSize = 1;
                    var area = new Queue<int2>();
                    area.Enqueue(new int2(x,y));
                    visitedArr[x,y] = true;
                    while(area.Count > 0)
                    {
                        var coord = area.Dequeue();
                        var offsets = MathHelper.directNeighborOffsets;
                        for (int i = 0; i < offsets.Length; i++)
                        {
                            var neighborCoord = coord + offsets[i];
                            if(MathHelper.isInBounds(neighborCoord, MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT))
                            {
                                if(visitedArr[neighborCoord.x, neighborCoord.y])
                                    continue;

                                if(occupiedSpaces[neighborCoord.x, neighborCoord.y])
                                    continue;
                                
                                currentAreaSize++;
                                area.Enqueue(neighborCoord);
                                visitedArr[neighborCoord.x, neighborCoord.y] = true;
                            }
                        }
                    }

                    if(currentAreaSize > biggestAreaSize)
                        biggestAreaSize = currentAreaSize;
                }
            }

            return biggestAreaSize;
        }

        public override int CalculateFitness(GenericLevel gL)
        {
            var fitness = LongestPathFitness(gL);

            gL.fitness = fitness; 
            return fitness;      
        }

        public override int CalculateConstraintViolations(GenericLevel gL)
        {
            var violatedConstraints = CheckConnectedConstraint(gL);
            violatedConstraints += CheckPOIConstraint(gL);

            gL.violatedConstraints = violatedConstraints;
            return violatedConstraints;
        }

        public int CheckPOIConstraint(GenericLevel gL)
        {
            var cas = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var occupiedSpaces = cas.occupation; 
            int violations = 0;

            if(occupiedSpaces[0,0])
                violations+=20;

            if(occupiedSpaces[MazeBuilderMetrics.WIDTH - 1, MazeBuilderMetrics.HEIGHT - 1])
                violations+=20;

            return violations;
        }

        public int CheckConnectedConstraint(GenericLevel gL)
        {
            var cas = (MazeSpawnEnvironment)(gL.spawnEnvironment);
            var occupiedSpaces = cas.occupation;
            var areas = 0;
            ResetVisited();

            for (int x = 0; x < MazeBuilderMetrics.WIDTH; x++)
            {
                for (int y = 0; y < MazeBuilderMetrics.HEIGHT; y++)
                {
                    if(visitedArr[x,y])
                        continue;

                    if(occupiedSpaces[x,y])   
                        continue;

                    var area = new Queue<int2>();
                    area.Enqueue(new int2(x,y));
                    visitedArr[x,y] = true;
                    while(area.Count > 0)
                    {
                        var coord = area.Dequeue();
                        var offsets = MathHelper.directNeighborOffsets;
                        for (int i = 0; i < offsets.Length; i++)
                        {
                            var neighborCoord = coord + offsets[i];
                            if(MathHelper.isInBounds(neighborCoord, MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT))
                            {
                                if(visitedArr[neighborCoord.x, neighborCoord.y])
                                    continue;

                                if(occupiedSpaces[neighborCoord.x, neighborCoord.y])
                                    continue;
                                
                                area.Enqueue(neighborCoord);
                                visitedArr[neighborCoord.x, neighborCoord.y] = true;
                            }
                        }
                    }

                    areas++;
                }
            }

            if(areas > 1)
                return areas;

            return 0;  
        }
    }

    public class MazeSpawnEnvironment : SpawnEnvironment
    {
        public bool[,] occupation;

        public override void Initialize()
        {
            occupation = new bool[MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT];
        }

        public override void Clear()
        {
            for (int x = 0; x < MazeBuilderMetrics.WIDTH; x++)
            {
                for (int y = 0; y < MazeBuilderMetrics.HEIGHT; y++)
                {
                    occupation[x,y] = false;
                }
            }
        }

        public override void Spawn(string type, DesignElement dE)
        {
            //switch over possible dE types
            switch(type)
            {
                case "OccupiedCell":
                {
                    var castDE = (OccupiedCellMazeDesignElement)dE;
                    occupation[castDE.x,castDE.y] = true;
                    break;
                }
                case "Wall":
                {
                    var castDE = (MazeWallDesignElement)dE;
                    var offset = castDE.horizontal ? new int2(1,0) : new int2(0,1);
                    var start = new int2(castDE.startX, castDE.startY);
                    for (int i = 0; i < castDE.length; i++)
                    {
                        var offsetCoordinate = start + offset * i;
                        if(!MathHelper.isInBounds(offsetCoordinate, MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT))
                            break;

                        occupation[offsetCoordinate.x, offsetCoordinate.y] = true;
                    }

                    break;
                }
            }
        }

        public override SpawnEnvironment Clone()
        {
            var clone = new MazeSpawnEnvironment();
            var occupationClone = new bool[MazeBuilderMetrics.WIDTH, MazeBuilderMetrics.HEIGHT];
            for (int x = 0; x < MazeBuilderMetrics.WIDTH; x++)
            {
                for (int y = 0; y < MazeBuilderMetrics.HEIGHT; y++)
                {
                    occupationClone[x,y] = occupation[x,y];
                }
            }            
            clone.occupation = occupationClone;
            return clone;
        }

    }

    public class MazeWallDesignElement : DesignElement
    {
        public MazeWallDesignElement(MazeWallDesignElement original, GenericLevel levelReference) : base(levelReference)
        {
            startX = original.startX;
            startY = original.startY;
            length = original.length;
            horizontal = original.horizontal;
        }

        public MazeWallDesignElement(GenericLevel levelReference) : base(levelReference)
        {
            startX = UnityEngine.Random.Range(0, MazeBuilderMetrics.WIDTH);
            startY = UnityEngine.Random.Range(0, MazeBuilderMetrics.HEIGHT);
            length = UnityEngine.Random.Range(0, MazeBuilderMetrics.MAX_WALL_LENGTH);
            horizontal = UnityEngine.Random.Range(0f, 1f) > 0.5f;
        }

        public int startX, startY, length; //Coordinates
        public bool horizontal; //true = horizontal, false = vertical

        public override void Spawn()
        {
            levelReference.spawnEnvironment.Spawn("Wall", this);
        }

        public override void Mutate()
        {
            float r = UnityEngine.Random.Range(0f, 1f);
            if(r < MazeBuilderMetrics.WALL_SHIFT_PROBABILITY)
            {
                int rX = UnityEngine.Random.Range(startX - 1, startX + 2); //max exclusive
                int rY = UnityEngine.Random.Range(startY - 1, startY + 2);
                if(rX >= 0 && rX < MazeBuilderMetrics.WIDTH)
                    startX = rX;
                if(rY >= 0 && rY < MazeBuilderMetrics.HEIGHT)
                    startY = rY;	
            }
            r = UnityEngine.Random.Range(0f, 1f);
            if(r < MazeBuilderMetrics.WALL_FLIP_PROBABILITY)
                horizontal = !horizontal;

            r = UnityEngine.Random.Range(0f, 1f);
            if(r < MazeBuilderMetrics.WALL_CHANGE_LENGTH_PROBABILITY)
            {
                length = UnityEngine.Random.Range(length - 1, length + 2);
                if(length <= 0)
                    length = 1;
            }
        }

        public override DesignElement Clone(GenericLevel newOwner)
        {
            return new MazeWallDesignElement(this, newOwner);
        }        
    }

    public class OccupiedCellMazeDesignElement : DesignElement
    {
        public OccupiedCellMazeDesignElement(OccupiedCellMazeDesignElement original, GenericLevel levelReference) : base(levelReference)
        {
            x = original.x;
            y = original.y;
        }

        public OccupiedCellMazeDesignElement(GenericLevel levelReference) : base(levelReference)
        {
            x = UnityEngine.Random.Range(0, MazeBuilderMetrics.WIDTH);
            y = UnityEngine.Random.Range(0, MazeBuilderMetrics.HEIGHT);
        }

        public int x, y; //Coordinates

        public override void Spawn()
        {
            levelReference.spawnEnvironment.Spawn("OccupiedCell", this);
        }

        public override void Mutate()
        {
            float rShift = UnityEngine.Random.Range(0f, 1f);
            if(rShift > MazeBuilderMetrics.TILE_SHIFT_PROPABILITY)
                return;
            int rX = UnityEngine.Random.Range(x - 1, x + 2); //max exclusive
            int rY = UnityEngine.Random.Range(y - 1, y + 2);
            if(rX >= 0 && rX < MazeBuilderMetrics.WIDTH)
                x = rX;
            if(rY >= 0 && rY < MazeBuilderMetrics.HEIGHT)
                y = rY;		

            //e.g. shift x and/or y coordinates by +1 or -1
        }

        public override DesignElement Clone(GenericLevel newOwner)
        {
            return new OccupiedCellMazeDesignElement(this, newOwner);
        }
    }

    public class MazeMutation : Mutation
    {
        static bool[,] occupation;

        public override void Mutate(GenericLevel genericLevel)
        {
            List<OccupiedCellMazeDesignElement> occupiedCellDEs = null;
            List<MazeWallDesignElement> wallDEs = null; 

            //TILES
            {
                //mutate overall level structure
                float addTile = UnityEngine.Random.Range(0f,1f);
                float removeTile = UnityEngine.Random.Range(0f,1f);

                if(addTile < MazeBuilderMetrics.TILE_ADD_PROPABILITY)
                    genericLevel.AddDesignElement(new OccupiedCellMazeDesignElement(genericLevel));

                occupiedCellDEs = genericLevel.GetDesignElementsOfType<OccupiedCellMazeDesignElement>();

                if(occupiedCellDEs != null)
                {
                    if(removeTile < MazeBuilderMetrics.TILE_REMOVE_PROPABILITY)
                    {
                        var selectedDE = occupiedCellDEs[UnityEngine.Random.Range(0, occupiedCellDEs.Count)];
                        genericLevel.RemoveDesignElement(selectedDE);
                        occupiedCellDEs.Remove(selectedDE);
                    }

                    //mutate individual design elements
                    foreach (var de in occupiedCellDEs)
                        de.Mutate();

                    //remove duplicates
                    if(occupation == null)
                        occupation = new bool[MazeBuilderMetrics.WIDTH,MazeBuilderMetrics.HEIGHT];
                    
                    for (int y = 0; y < MazeBuilderMetrics.HEIGHT; y++)
                    {
                        for (int x = 0; x < MazeBuilderMetrics.WIDTH; x++)
                        {
                            occupation[x,y] = false;
                        }
                    }

                    foreach (var de in occupiedCellDEs)
                    {
                        if(occupation[de.x,de.y])
                            genericLevel.RemoveDesignElement(de);
                        
                        else
                            occupation[de.x,de.y] = true;
                    }

                    occupiedCellDEs = genericLevel.GetDesignElementsOfType<OccupiedCellMazeDesignElement>();
                }
            }

            //WALLS
            {
                //mutate overall level structure
                float addWall = UnityEngine.Random.Range(0f,1f);
                float removeWall = UnityEngine.Random.Range(0f,1f);

                if(addWall < MazeBuilderMetrics.WALL_ADD_PROPABILITY)
                    genericLevel.AddDesignElement(new MazeWallDesignElement(genericLevel));
                
                wallDEs = genericLevel.GetDesignElementsOfType<MazeWallDesignElement>();
                if(wallDEs != null)
                {
                    if(removeWall < MazeBuilderMetrics.WALL_REMOVE_PROPABILITY)
                    {
                        if(wallDEs.Count > 0)
                        {
                            var selectedDE = wallDEs[UnityEngine.Random.Range(0, wallDEs.Count)];
                            genericLevel.RemoveDesignElement(selectedDE);
                            wallDEs.Remove(selectedDE);
                        }

                    }

                    //mutate individual design elements
                    foreach (var de in wallDEs)
                        de.Mutate();
                }
            }

            genericLevel.spawnEnvironment.Clear();
            if(occupiedCellDEs != null)
            {
                foreach (var de in occupiedCellDEs)
                    de.Spawn();
            }

            if(wallDEs != null)
            {
                foreach (var de in wallDEs)
                    de.Spawn();
            }
        }
    }

    public class MazeCrossover : Crossover
    {
        public List<DesignElement> Crossover<T>(GenericLevel lhs, GenericLevel rhs, GenericLevel child) where T : DesignElement
        {
            float r = UnityEngine.Random.Range(0.0f, 1.0f);
            if(r < 0.5f)
            {
                (lhs, rhs) = (rhs, lhs);
            }

            var lhsDEs = lhs.GetDesignElementsOfType<T>(); //first half of this list //also make deep copy
            var rhsDEs = rhs.GetDesignElementsOfType<T>(); //second half of this list //also make deep copy
            

            int lhsHalfLength = lhsDEs == null ? 0 : lhsDEs.Count / 2; //31 - 15
            int rhsHalfLength = rhsDEs == null ? 0 : rhsDEs.Count / 2; //60 - 30
            int newLength = lhsHalfLength + rhsHalfLength; //45


            var newDEs = new List<DesignElement>(newLength);

            if(lhsDEs != null)
            {
                for (int i = 0; i < lhsHalfLength; i++)
                    newDEs.Add(lhsDEs[i].Clone(child));
            }

            if(rhsDEs != null)
            {
                for (int i = 0; i < rhsHalfLength; i++)
                    newDEs.Add(rhsDEs[i + rhsHalfLength].Clone(child));
            }

            return newDEs;
        }

        public override GenericLevel CrossoverFunc(GenericLevel lhs, GenericLevel rhs)
        {
            //do some crossover shenanigans
            var spawnEnvironment = new MazeSpawnEnvironment();
            spawnEnvironment.Initialize();

            var child = new GenericLevel(spawnEnvironment, new MazeMutation(), new MazeCrossover());

            var newTileDEs = Crossover<OccupiedCellMazeDesignElement>(lhs, rhs, child);
            var newWallDEs = Crossover<MazeWallDesignElement>(lhs, rhs, child);
            
            var newDEDictionary = new Dictionary<System.Type, List<DesignElement>>();
            if(newTileDEs.Count > 0)
                newDEDictionary.Add(newTileDEs[0].GetType(), newTileDEs);

            if(newWallDEs.Count > 0)
                newDEDictionary.Add(newWallDEs[0].GetType(), newWallDEs);

            child.SetDesignElements(newDEDictionary);

            //recreate the spawn environment
            foreach (var de in newTileDEs)
                de.Spawn();
            foreach (var de in newWallDEs)
                de.Spawn();

            return child;
        }
    }

    //METRICS
    public static class MazeBuilderMetrics
    {
        public static int WIDTH = 30, HEIGHT = 30;
        public static int POP_SIZE = 40;
        public static int MAX_GENERATIONS = 2000;
        public static int MAX_CROSSOVERS = 10;
        public static float 
            START_FILL_PERCENTAGE = 0.0f,//0.5f, 
            TILE_SHIFT_PROPABILITY = 0.0f,//0.002f,
            TILE_ADD_PROPABILITY = 0.0f,//0.1f,
            TILE_REMOVE_PROPABILITY = 0.0f;//0.02f;


        public static int START_WALL_AMOUNT = 10;
        public static int MAX_WALL_LENGTH = 30;
        public static float 
            WALL_ADD_PROPABILITY = 0.1f,//0.2f, 
            WALL_REMOVE_PROPABILITY = 0.0f,//0.02f, 
            WALL_CHANGE_LENGTH_PROBABILITY = 0.0f,//0.01f,
            WALL_FLIP_PROBABILITY = 0.0f,//0.01f, 
            WALL_SHIFT_PROBABILITY = 0.0f;//0.01f;
    }
    /**
    USE LEVEL AS ABSTRACT CLASS OR OPERATORS?
    **/
}
