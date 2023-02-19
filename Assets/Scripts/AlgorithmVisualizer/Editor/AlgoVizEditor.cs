using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AlgoViz))]
public class AlgoVizEditor : Editor
{
    AlgoViz tgt;
    bool needsRepaint = false;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        return; //For now only playmode is supported
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Begin Record"))
        {
            tgt.BeginRecord();
            needsRepaint = true;
        }
        if(GUILayout.Button("End Record"))
        {
            tgt.EndRecord();
            needsRepaint = true;
        }
        GUILayout.EndHorizontal();


        GUILayout.Label("DrawCommandCount [" +tgt.drawCommandCount + "]");

        if(tgt.steps.Count != 0 && tgt.replayIndex != -1)
        {
            GUILayout.Label("Step [" + (tgt.replayIndex + 1) + "/" + tgt.steps.Count + "]");
        }

        tgt.replayIndex = EditorGUILayout.IntSlider("Current Step: ", tgt.replayIndex, 0, tgt.steps.Count - 1);

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Step Replay"))
        {
            tgt.StepReplay();
            needsRepaint = true;
        }
        if(GUILayout.Button("Restart Replay"))
        {
            tgt.RestartReplay();        
            needsRepaint = true;
        }
        GUILayout.EndHorizontal();

        if(tgt.replayIndex == -1)
            return;

        var step = tgt.steps[tgt.replayIndex];
        for (int i = 0; i < step.InspectorCommandCount; i++)
        {
            var ic = step.inspectorCommands[i];
            GUILayout.Label(ic.text);
        }

    }

    void OnSceneGUI() 
    {
        var guiEvent = Event.current;
        if(guiEvent.type == EventType.Repaint)
        {
            Draw();
        }
        else
        {
            if (needsRepaint)
            {
                EditorUtility.SetDirty(target);
                HandleUtility.Repaint();
            }
            
        }
    }

    public void Draw()
    {
        if(tgt.replayIndex == -1)
            return;

        var step = tgt.steps[tgt.replayIndex];
        for (int i = 0; i < step.DrawCommandCount; i++)
        {
            var currentDrawCommand = step.drawCommands[i];
            Handles.color = currentDrawCommand.color;
            GUIStyle fontStyle = new GUIStyle();
            fontStyle.fontSize = 16;
            switch(currentDrawCommand.type)
            {
                case DrawCommand.DrawCommandType.shape:
                {
                    Handles.DrawAAConvexPolygon(currentDrawCommand.vertices);   
                    break;
                }
                case DrawCommand.DrawCommandType.label:
                {
                    Handles.Label(currentDrawCommand.position, currentDrawCommand.text, fontStyle);
                    break;
                }
            }
 
        }
    }

    void OnEnable() 
    {
        tgt = target as AlgoViz;
    }
}
