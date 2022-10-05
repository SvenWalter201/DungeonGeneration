using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CaveGenerationCellularAutomata))]
public class CaveGenerationCellularAutomataEditor : Editor
{
    CaveGenerationCellularAutomata tgt;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Generate"))
            tgt.Generate();

        if(tgt.renderTexture != null)
            GUILayout.Label(tgt.renderTexture, GUILayout.MinWidth(256), GUILayout.MinHeight(256), GUILayout.MaxHeight(512), GUILayout.MaxHeight(512));
    }

    void OnEnable() 
    {
        tgt = target as CaveGenerationCellularAutomata;
    }
}
