using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PipelineV2))]
public class PipelineV2Editor : Editor
{
    PipelineV2 tgt;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if(GUILayout.Button("Generate"))
        {
            tgt.Generate();
        }
    }

    void OnEnable() 
    {
        tgt = target as PipelineV2;
    }
}
