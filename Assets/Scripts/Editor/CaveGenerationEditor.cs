using UnityEngine;
using UnityEditor;
using static UnityEditor.EditorGUILayout;

[CustomEditor(typeof(CaveGeneration))]
public class CaveGenerationEditor : Editor
{
    CaveGeneration _tgt;

    readonly GUILayoutOption[] buttonStyling = new GUILayoutOption[]
    {
        GUILayout.Height(30.0f), GUILayout.Width(30.0f)
    };

    GUILayoutOption[] maxWidthButton = new GUILayoutOption[]
    {
        GUILayout.Height(20.0f), GUILayout.Width(250.0f)
    };

    public override void OnInspectorGUI()
    {
        GUILayout.Label("General", EditorStyles.boldLabel);
        base.OnInspectorGUI();
        _tgt.autoUpdate = Toggle("Auto Update", _tgt.autoUpdate);
        _tgt.reseed = Toggle("Reseed", _tgt.reseed);
        Space(15);

        GUILayout.Label("Grid Params", EditorStyles.boldLabel);
        _tgt.resolution = IntSlider("Resolution", _tgt.resolution, 16, 512);
        _tgt.partitionSize = IntSlider("Partition Size", _tgt.partitionSize, 2, 64);
        _tgt.seed = TextField("Seed", _tgt.seed);
        _tgt.useSpacialPartitioning = Toggle("Spacial Partitioning", _tgt.useSpacialPartitioning);
        _tgt.ValidateDimensions();
        Space(15);
        GUILayout.Label("Generator Stack", EditorStyles.boldLabel);
        for (var i = 0; i < _tgt.generatorStack.Count; i++)
        {
            var currentLayer = _tgt.generatorStack[i];

            BeginVertical(EditorStyles.helpBox);
                EditorGUI.indentLevel++;
                _tgt.foldoutGroups[i] = Foldout(_tgt.foldoutGroups[i], $"Layer {(i+1)}: {currentLayer.generator.Label}", EditorStyles.foldout);
               EditorGUI.indentLevel--;
                if(_tgt.foldoutGroups[i])
                {
                    BeginHorizontal(EditorStyles.helpBox);
                        BeginVertical();
                        
                            currentLayer.type = (GeneratorType)EnumPopup("Generator Type", currentLayer.type);
                            currentLayer.ValidateType();

                            if(currentLayer.generator.HasDetailedStepping)
                                currentLayer.generator.detailedStepping = Toggle("Detailed Stepping", currentLayer.generator.detailedStepping);

                            //switch out this generator!
                            switch(currentLayer.type)
                            {
                                case GeneratorType.Randomize:
                                {
                                    var randomizeGenerator = currentLayer.generator as RandomizeGenerator;
                                    if(randomizeGenerator == null) 
                                        break;
                                    randomizeGenerator.fillPercentage = Slider("Fill Percentage",randomizeGenerator.fillPercentage, 0f, 1f);
                                    break;
                                }
                                case GeneratorType.Smooth:
                                {
                                    var smoothGenerator = currentLayer.generator as SmoothGenerator;
                                    if(smoothGenerator == null) 
                                        break;
                                    smoothGenerator.iterations = IntSlider("Iterations",smoothGenerator.iterations, 0, 10);
                                    smoothGenerator.terrainMargin = IntSlider("Terrain Margin", smoothGenerator.terrainMargin, 0, 8);
                                    smoothGenerator.wallMargin = IntSlider("Wall Margin", smoothGenerator.wallMargin, 0, 8);
                                    break;
                                }
                                case GeneratorType.ProcessRegions:
                                {
                                    var processRegionsGenerator = currentLayer.generator as ProcessRegionsGenerator;
                                    if(processRegionsGenerator == null) 
                                        break;
                                    processRegionsGenerator.type = (RegionType)EnumPopup("Region Type", processRegionsGenerator.type);
                                    processRegionsGenerator.minRegionSize = IntSlider("Minimum Region Size", processRegionsGenerator.minRegionSize, 0, 100);                
                                    break;    
                                }
                                case GeneratorType.ConnectRegions:
                                {
                                    var connectRegionsGenerator = currentLayer.generator as ConnectRegionsGenerator;
                                    if(connectRegionsGenerator == null) 
                                        break;

                                    connectRegionsGenerator.passagewayRadius = IntSlider("Passageway Radius", connectRegionsGenerator.passagewayRadius, 1, 5);
                                    break;
                                }
                                case GeneratorType.RemoveBumps:
                                {
                                    var removeBumpsGenerator = currentLayer.generator as RemoveBumpsGenerator;
                                    if(removeBumpsGenerator == null) 
                                        break;

                                    removeBumpsGenerator.iterations = IntSlider("Iterations", removeBumpsGenerator.iterations, 0, 6);
                                    break;
                                }
                                case GeneratorType.Widen:
                                {
                                    var widenGenerator = currentLayer.generator as WidenGenerator;
                                    if(widenGenerator == null) 
                                        break;

                                    if((widenGenerator.detailedStepping))
                                        widenGenerator.previewExpansionPoints = Toggle("Preview Expansion Points", widenGenerator.previewExpansionPoints);
                                    
                                    widenGenerator.minTerrainWidth = IntSlider("Minimum Terrain Width", widenGenerator.minTerrainWidth, 1, 5);
                                    break;
                                }
                            }
                        EndVertical();
                        BeginVertical();
                            if(i > 0)
                            {
                                if(GUILayout.Button("^", buttonStyling))
                                    _tgt.SwapElements(i, i-1);
                            }
                            if(i < _tgt.generatorStack.Count - 1)
                            {
                                if(GUILayout.Button("v", buttonStyling))
                                    _tgt.SwapElements(i, i+1);
                            }
                        EndVertical();
                        BeginVertical();
                            if(GUILayout.Button("+", buttonStyling))
                                _tgt.Insert(i);                    
                            if(GUILayout.Button("-", buttonStyling))
                                _tgt.Remove(i);
                        EndVertical();
                    EndHorizontal(); 
                }
                EndVertical();  
        }

        if(GUILayout.Button("+"))
            _tgt.Add();  

        Space(15);

        if(GUI.changed)
        {
            _tgt.ResetStepThroughStack();
            if(_tgt.autoUpdate)
                _tgt.GenerateFromStack();
        }
            

        if(GUILayout.Button("Generate from Stack"))
            _tgt.GenerateFromStack();

        BeginHorizontal();
            if(GUILayout.Button($"Step (Current: {_tgt.step})"))
                _tgt.StepThroughStack();
            if(GUILayout.Button("Restart Stepping"))
                _tgt.RestartStepThroughStack();
        EndHorizontal();

        //if(GUI.changed && tgt.autoUpdate)
        //    tgt.GenerateFromStack();

        Space(15);
        GUILayout.Label("Display Information", EditorStyles.boldLabel);
        GUILayout.Button("Display Grid as Binary");
        GUILayout.Button("Display Distance to Walls Grid");
        GUILayout.Button("Display Regions as Colors");
        GUILayout.Button("Display occupying Partitions");
    }


    void OnEnable() 
    {
        _tgt = target as CaveGeneration;
    }

    public override bool RequiresConstantRepaint()
    {
        return true;
    }
}
