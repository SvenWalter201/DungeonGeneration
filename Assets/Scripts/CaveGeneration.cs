using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Mathematics;
using System.Diagnostics;
using TMPro;
using static UnityEngine.Debug;

[System.Serializable]
public class ConnectRegionsGenerator : Generator
{
    public override string Label => "Connect Regions";
    public override bool HasDetailedStepping => true;
    public override VisualizationType VisualizationType => VisualizationType.NumbersUpdated;
    public int passagewayRadius = 1;
 
    public override void Generate(CaveGrid grid, bool steppingActive = false)
    {
        grid.passageWayRadius = passagewayRadius;
        grid.ConnectRegions(CaveGrid.TERRAIN, 0);
    }

    public override bool InternalStepsRemaining()
    {
        return false;
    }
}

[System.Serializable]
public class WidenGenerator : Generator
{
    public override string Label => "Widen";
    public bool previewExpansionPoints = false;
    public override bool HasDetailedStepping => true;
    public override VisualizationType VisualizationType => VisualizationType.Numbers;

    public int minTerrainWidth = 1;
    bool toggle = false;
    public override void Generate(CaveGrid grid, bool steppingActive = false)
    {
        if(steppingActive && detailedStepping)
        {
            if(previewExpansionPoints)
            {
                toggle = !toggle;
                grid.WidenSingleIteration(toggle, internalStep + 1);

                if(!toggle)
                {
                    grid.GetDistanceToWallsGrid();
                    ++internalStep;
                }
                    
            }
            else
            {
                grid.WidenSingleIteration(false, internalStep + 1);
                ++internalStep;
            }

        }
        else
        {
            for (int i = 0; i < minTerrainWidth; i++)
                grid.WidenSingleIteration(false, i + 1);
        }
    }

    public override bool InternalStepsRemaining()
    {
        return internalStep < minTerrainWidth;
    }

    public override void ClearState()
    {
        base.ClearState();
        toggle = false;
    }
}

[System.Serializable]
public class RemoveBumpsGenerator : Generator
{
    public override string Label => "Remove Bumps";
    public override bool HasDetailedStepping => true;
    public int iterations = 1;
    public override VisualizationType VisualizationType => VisualizationType.NumbersUpdated;

    public override void Generate(CaveGrid grid, bool steppingActive = false)
    {
        if(steppingActive && detailedStepping)
        {
            grid.SmoothSingle(2,2, true);
            ++internalStep;
        }
        else
        {
            for (int i = 0; i < iterations; i++)
                grid.SmoothSingle(2,2, true);
            
        }

    }

    public override bool InternalStepsRemaining()
    {
        return internalStep < iterations;
    }
}

[System.Serializable]
public class ProcessRegionsGenerator : Generator
{
    public override string Label => "Process Region";
    public RegionType type = RegionType.Terrain;
    public int minRegionSize = 10;
    public override VisualizationType VisualizationType => VisualizationType.NumbersUpdated;

    public override void Generate(CaveGrid grid, bool steppingActive = false)
    {
        var typeAsBool = type == RegionType.Wall;
        grid.ProcessRegions(typeAsBool, minRegionSize);
    }
}

[System.Serializable]
public class SmoothGenerator : Generator
{
    public override string Label => "Smooth";
    public override bool HasDetailedStepping => true;
    public override VisualizationType VisualizationType => VisualizationType.NumbersUpdated;
    public int iterations = 3;
    public int terrainMargin = 4;
    public int wallMargin = 4;

    public override void Generate(CaveGrid grid, bool steppingActive = false)
    {
        if(steppingActive && detailedStepping)
        {
            grid.SmoothSingle(terrainMargin, wallMargin);
            ++internalStep;
        }
        else
        {
            for (int i = 0; i < iterations; i++)
                grid.SmoothSingle(terrainMargin, wallMargin);
            
        }
    }

    public override bool InternalStepsRemaining()
    {
        return internalStep < iterations;
    }
}
[System.Serializable]
public class RandomizeGenerator : Generator
{
    public override string Label => "Randomize";
    public override bool HasDetailedStepping => false;
    public float fillPercentage = 0.5f;

    public override void Generate(CaveGrid grid, bool steppingActive = false)
    {
        grid.Randomize(fillPercentage);
    }
}

public enum GeneratorType { Randomize, Smooth, ProcessRegions, ConnectRegions, RemoveBumps, Widen }
public enum RegionType { Terrain, Wall}
public enum VisualizationType { NumbersUpdated, Binary, Numbers }

[System.Serializable]
public class Generator
{
    public virtual string Label => "Generator";
    public virtual string StepLabel(int step) => (detailedStepping) ? $"Step {step}.{internalStep}: {Label}" : $"Step {step}: {Label}";
    public virtual bool HasDetailedStepping => false;
    public bool detailedStepping = false;
    public int internalStep = 0;
    public virtual VisualizationType VisualizationType => VisualizationType.Binary;
    public virtual bool InternalStepsRemaining() => false;
    public virtual void Generate(CaveGrid grid, bool steppingActive = false){}
    public virtual void ClearState() { internalStep = 0; } //if the generator holds some internal state, clear it
}

[System.Serializable]
public class GenerationLayer : ISerializationCallbackReceiver
{
    public GeneratorType type;
    public Generator generator;

    public GenerationLayer(GeneratorType type)
    {
        this.type = type;
        SelectGenerator();
    }

    [HideInInspector] public RandomizeGenerator randomizeGenerator;
    [HideInInspector] public SmoothGenerator smoothGenerator;
    [HideInInspector] public ProcessRegionsGenerator processRegionsGenerator;
    [HideInInspector] public ConnectRegionsGenerator connectRegionsGenerator;
    [HideInInspector] public RemoveBumpsGenerator removeBumpsGenerator;
    [HideInInspector] public WidenGenerator widenGenerator;

    public void SelectGenerator()
    {
        switch(type)
        {
            case GeneratorType.Randomize: 
                if(generator == null || generator.GetType() != typeof(RandomizeGenerator)) 
                    generator = new RandomizeGenerator(); 

                randomizeGenerator = generator as RandomizeGenerator;
                break;
            case GeneratorType.Smooth: 
                if(generator == null || generator.GetType() != typeof(SmoothGenerator)) 
                    generator = new SmoothGenerator(); 

                smoothGenerator = generator as SmoothGenerator;
                break;
            case GeneratorType.ProcessRegions: 
                if(generator == null || generator.GetType() != typeof(ProcessRegionsGenerator)) 
                    generator = new ProcessRegionsGenerator(); 

                processRegionsGenerator = generator as ProcessRegionsGenerator;
                break;
            case GeneratorType.ConnectRegions: 
                if(generator == null || generator.GetType() != typeof(ConnectRegionsGenerator)) 
                    generator = new ConnectRegionsGenerator(); 

                connectRegionsGenerator = generator as ConnectRegionsGenerator;
                break;
            case GeneratorType.RemoveBumps: 
                if(generator == null || generator.GetType() != typeof(RemoveBumpsGenerator)) 
                    generator = new RemoveBumpsGenerator(); 

                removeBumpsGenerator = generator as RemoveBumpsGenerator;
                break;
            case GeneratorType.Widen: 
                if(generator == null || generator.GetType() != typeof(WidenGenerator)) 
                    generator = new WidenGenerator(); 

                widenGenerator = generator as WidenGenerator;
                break;
        }
    }

    public void ValidateType() => SelectGenerator();

    public void OnAfterDeserialize()
    {
        switch(type)
        {
            case GeneratorType.Randomize:
                if(GetType() != typeof(RandomizeGenerator) && randomizeGenerator != null)
                    generator = randomizeGenerator;
                break;
            case GeneratorType.Smooth:
                if(GetType() != typeof(SmoothGenerator) && smoothGenerator != null)
                    generator = smoothGenerator;
                break;
            case GeneratorType.ProcessRegions:
                if(GetType() != typeof(ProcessRegionsGenerator) && processRegionsGenerator != null)
                    generator = processRegionsGenerator;
                break;
            case GeneratorType.ConnectRegions:
                if(GetType() != typeof(ConnectRegionsGenerator) && connectRegionsGenerator != null)
                    generator = connectRegionsGenerator;
                break;
            case GeneratorType.RemoveBumps:
                if(GetType() != typeof(RemoveBumpsGenerator) && removeBumpsGenerator != null)
                    generator = removeBumpsGenerator;
                break;
            case GeneratorType.Widen:
                if(GetType() != typeof(WidenGenerator) && widenGenerator != null)
                    generator = widenGenerator;
                break;
        }  
    }

    public void OnBeforeSerialize()
    {
    }

}

[ExecuteAlways]
public class CaveGeneration : MonoBehaviour
{
    [HideInInspector] public List<GenerationLayer> generatorStack;
    [HideInInspector] public List<bool> foldoutGroups;
    [SerializeField] RawImage uiImageTarget;
    [SerializeField] TMP_Text label;
    [HideInInspector] public int partitionSize = 16;
    [HideInInspector] public int resolution = 128;
    [HideInInspector] public string seed = "";
    [HideInInspector] public bool reseed = false, useSpacialPartitioning = true, autoUpdate = false;
    [HideInInspector, Range(0, 100)] public int minTerrainSize = 10, minWallSize = 10;
    CaveGrid _grid;

#region STACK_MANAGEMENT

    public void SwapElements(int a, int b)
    {
        (generatorStack[a], generatorStack[b]) = (generatorStack[b], generatorStack[a]);
        (foldoutGroups[a], foldoutGroups[b]) = (foldoutGroups[b], foldoutGroups[a]);
    }
    public void Remove(int i) 
    {
        generatorStack.RemoveAt(i);
        foldoutGroups.RemoveAt(i);
    }
    public void Insert(int i) 
    {
        generatorStack.Insert(i, new GenerationLayer(GeneratorType.Randomize));
        foldoutGroups.Insert(i, true);
    }

    public void Add() 
    {
        generatorStack.Add(new GenerationLayer(GeneratorType.Randomize));
        foldoutGroups.Add(true);
    }

    void Initialize()
    {
        Log("Initialize");
        if(reseed)
            Reseed();

        ValidateDimensions();

        _grid = new CaveGrid(resolution, resolution, new CaveGridSettings 
            {
                partitionSize = partitionSize, 
                seed = SeedHash(), 
                useSpacialPartitioning = useSpacialPartitioning
            } 
        ); 

        foreach (var layer in generatorStack)
        {
            layer.generator.ClearState();
        }
    }

    public void GenerateFromStack()
    {
        var sw = new Stopwatch();
        sw.Start();
        Initialize();

        for (int i = 0; i < generatorStack.Count; i++)
        {
            var currentLayer = generatorStack[i];
            currentLayer.generator.Generate(_grid);
        }

        sw.Stop();
        LogTime(sw.Elapsed, "Generation Time");

        DisplayGrid();
    }

    [HideInInspector] public int step = 0;
    public void StepThroughStack()
    {
        if(step >= generatorStack.Count)
            return;

        if(generatorStack.Count == 0)
            return;

        if(step == 0)
        {
            Initialize();
        }
        var generator = generatorStack[step].generator;
        generator.Generate(_grid, true);
        var stepLabel = generator.StepLabel(step + 1);
        switch(generator.VisualizationType)
        {
            case VisualizationType.Binary: DisplayGrid(stepLabel); break;
            case VisualizationType.Numbers: DisplayNumbersGrid(stepLabel, false); break;
            case VisualizationType.NumbersUpdated: DisplayNumbersGrid(stepLabel, true); break;

        }
        if(!generator.InternalStepsRemaining() || !generator.detailedStepping)
            ++step;
    }

    public void ResetStepThroughStack()
    {
        step = 0;
    }
    public void RestartStepThroughStack()
    {
        step = 0;
        StepThroughStack();
    }


#endregion STACK_MANAGEMENT


    public int SeedHash() => (int)(seed.GetHashCode() * 0.00001f);

    void OnDisable()
    {
        _grid?.CleanupPersistentArrays();
    }

    public void ValidateDimensions()
    {
        //partition needs to be a power of 2
        partitionSize = math.ceilpow2(partitionSize);

        //gridSize needs to be a multiple of partition size
        if(resolution % partitionSize != 0)
            resolution = ((int)math.ceil(resolution / (float)partitionSize)) * partitionSize;
    }

    public void Reseed() => seed = (Time.unscaledTime * Time.time).ToString();
    
    void DisplayGrid(string stepLabel = "")
    {
        uiImageTarget.texture = TextureGenerator.TextureFromBoolMap(_grid.GetCopy(), resolution, resolution);
        this.label.text = stepLabel;
    }

    void DisplayNumbersGrid(string stepLabel = "", bool updateRequired = true)
    {
        if(updateRequired)
            _grid.GetDistanceToWallsGrid();

        if(_grid.numbers == null)
            return;

        var values = _grid.numbers.GetCopy();
        var colors = new Color[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            var v = values[i];
            Color c = default;
            if(v == 99)
                c = Color.red;
            else 
            {
                float greyScale = (v + 1) * 0.1f;
                c = new Color(greyScale,greyScale,greyScale,1);
            }
            colors[i] = c;
        }

        uiImageTarget.texture = TextureGenerator.TextureFromColorMap(colors, resolution, resolution);
        label.text = stepLabel;
    }

    void LogTime(System.TimeSpan time, string header)
    {
        // Format and display the TimeSpan value.
        var elapsedTime = $"{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}.{time.Milliseconds:00}";
        Log(header + elapsedTime);
    }

/*
    public IEnumerator GenerateStep()
    {
        stepToPerform = 0;
        currentStep = 0;

        ValidateGrid();
        grid.Randomize(fillPercentage);
        DisplayGrid("Randomize");  
        ++currentStep; 
        yield return new WaitUntil(NextStepTriggered);

        for (int i = 0; i < smoothingIterations; i++)
        {
            grid.SmoothSingle(terrainMargin, wallMargin);
            DisplayGrid("Smoothing It: " + (i+1));
            ++currentStep;
            yield return new WaitUntil(NextStepTriggered);
        }

        grid.ProcessRegions(CaveGrid.WALL, minWallSize);
        DisplayGrid("Cull small Walls");
        ++currentStep; 
        yield return new WaitUntil(NextStepTriggered);

        var regions = grid.ProcessRegions(CaveGrid.TERRAIN, minTerrainSize);  
        DisplayGrid("Cull small Regions");
        ++currentStep;
        yield return new WaitUntil(NextStepTriggered);

        int originalRegionCount = regions.Count - 1;
        while(regions.Count > 1)
        {
            grid.ConnectionRegionsSingle(regions, CaveGrid.TERRAIN);
            DisplayGrid("Connection Regions (" + (originalRegionCount - (regions.Count - 1)) + " of " + originalRegionCount + ")");
            ++currentStep;
            yield return new WaitUntil(NextStepTriggered);
        }  
        for (int j = 0; j < 3; j++)
        {
            for (int i = 0; i < minPassageWidth; i++)
            {
                grid.WidenSingleIteration(true, i + 1);
                DisplayNumbersGrid($"Widen Viz It {(i+1)}");
                ++currentStep;
                yield return new WaitUntil(NextStepTriggered);

                grid.WidenSingleIteration(false, i + 1);
                DisplayGrid($"Widen It {(i+1)}");
                ++currentStep;
                yield return new WaitUntil(NextStepTriggered);
            }

            grid.ProcessRegions(CaveGrid.WALL, minWallSize);
            DisplayGrid("Cull small Walls");
            ++currentStep; 
            yield return new WaitUntil(NextStepTriggered);

            grid.ProcessRegions(CaveGrid.TERRAIN, minTerrainSize);  
            DisplayGrid("Cull small Regions");
            ++currentStep;
            yield return new WaitUntil(NextStepTriggered);

            grid.SmoothSingle(2,2, true);
            DisplayGrid("Post Smooth");
            ++currentStep;
            yield return new WaitUntil(NextStepTriggered);            
        }

        /*
        for (int i = 0; i < 2; i++) //TODO: Investigate if 2 is always enough
        {
            grid.PostProcessSmoothSingle();
            DisplayGrid("Post Smooth It: " + (i+1));
            ++currentStep;
            yield return new WaitUntil(NextStepTriggered);
        }
        */
        /*
        for (int j = 0; j < 2; j++) //TODO: Investigate if 2 is always enough
        {
        grid.PostProcessSmoothSingle();
        DisplayGrid("Post Smooth It: " + (j+1));
        ++currentStep;
        yield return new WaitUntil(NextStepTriggered);
        }
        */

        //Post Process Steps
        /*
        for (int i = 0; i < 3; i++) //TODO: Investigate if 2 is always enough
        {
            grid.WidenArbitrary();
            DisplayGrid("Widen Arbitrary It: " + (i+1));
            ++currentStep;
            yield return new WaitUntil(NextStepTriggered);

            for (int j = 0; j < 2; j++) //TODO: Investigate if 2 is always enough
            {
            grid.PostProcessSmoothSingle();
            DisplayGrid("Post Smooth It: " + (j+1));
            ++currentStep;
            yield return new WaitUntil(NextStepTriggered);
            }
        }
        */
 
 /*
    const float INV256 = 0.00392156862f;

    [HideInInspector] public int roomIndex = 0;

    public void ShowPartitionsForRoom()
    {
        BaseGrid<Color> colorGrid = new BaseGrid<Color>(resolution, resolution, (int x, int y) => Color.black);
        var regions = grid.ProcessRegions(CaveGrid.TERRAIN, minTerrainSize);
        if(roomIndex >= regions.Count)
            roomIndex = regions.Count - 1;

        CaveRegion region = regions[roomIndex];
        for (int i = 0; i < regions.Count; i++)
        {
            Color color = (regions[i] == region) ? new Color(0.6f,0.6f,0.6f,1f) : new Color(0.2f,0.2f,0.2f,1f);
            
            foreach (var coordinate in regions[i].tiles)
                colorGrid.SetValue(color, coordinate);    
        } 
        List<int2> partitions = region.partitions;
        foreach (var partition in partitions)
        {
            for (int x = 0; x < partitionSize; x++)
            {
                for (int y = 0; y < partitionSize; y++)
                {
                    int2 pixel = partition * partitionSize + new int2(x,y);
                    Color color = colorGrid.GetValue(pixel);
                    color += new Color(0.4f,0f,0f,1f);
                    colorGrid.SetValue(color, pixel);
                }
            }
        }     
        uiImageTarget.texture = TextureGenerator.TextureFromColorMap(colorGrid.GetCopy(), resolution, resolution);

    }

    public void ColorRegions()
    {
        BaseGrid<Color> colorGrid = new BaseGrid<Color>(resolution, resolution, (int x, int y) => Color.black);
        var regions = grid.ProcessRegions(CaveGrid.TERRAIN, minTerrainSize);
        for (int i = 0; i < regions.Count; i++)
        {
            uint hash = SmallXXHash.Seed(SeedHash()).Eat(i);
            Color color = new Color(
            INV256 * (hash >> 24 & 255),
            INV256 * (hash >> 16 & 255),
            INV256 * (hash >> 8 & 255),
            1.0f);
            Color edgeColor = color;
            edgeColor *= 0.7f;
            edgeColor.a = 1.0f;
            foreach (var coordinate in regions[i].tiles)
                colorGrid.SetValue(color, coordinate);
            
            foreach (var coordinate in regions[i].edgeTiles)
                colorGrid.SetValue(edgeColor, coordinate);
            
        }
         uiImageTarget.texture = TextureGenerator.TextureFromColorMap(colorGrid.GetCopy(), resolution, resolution);
    }

/*
    void OnValidate() 
    {
        ValidateGrid();
        if(autoUpdate)
            Generate();
    }
*/
}
