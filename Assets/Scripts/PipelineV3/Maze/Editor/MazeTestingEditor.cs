using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeTesting))]
public class MazeTestingEditor : Editor
{
    MazeTesting tgt;
    GUIStyle style = new GUIStyle();

    void OnSceneGUI() 
    {
        Tools.current = Tool.None;

        var grid = tgt.grid;
        if(grid == null)
            return;

        
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;

        for (int y = 0, idx = 0; y < tgt.height; y++)
        {
            for (int x = 0; x < tgt.width; x++, idx++)
            {
                var cell = grid[idx];
                var pos = new Vector3(x,0,y) - new Vector3(tgt.width * 0.5f, 0f, tgt.height * 0.5f) + new Vector3(0.3f, 0f, 0.3f);
                GUI.color = Color.gray;
                style.fontSize = 18;
                style.alignment = TextAnchor.MiddleCenter;
                Handles.Label(pos, cell.distanceToStart.ToString(), style);

                if(cell.isCulDeSac)
                {
                    GUI.color = Color.black;
                    style.fontSize = 12;
                    style.alignment = TextAnchor.MiddleCenter;
                    Handles.Label(pos + new Vector3(0.4f,0f,0.4f), cell.culDeSacLength.ToString(), style);
                }
            }
        }
    }
//
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.BeginHorizontal();
        int originalWidth = tgt.width;
        int originalHeight = tgt.height;

        tgt.width = EditorGUILayout.IntField(tgt.width);
        tgt.height = EditorGUILayout.IntField(tgt.height);
        GUILayout.EndHorizontal();       

        if(tgt.width != originalWidth || tgt.height != originalHeight || tgt.grid == null)
        {
            tgt.grid = new Cell[tgt.width * tgt.height];
            if(tgt.boxCollider == null)
                tgt.GetComps();
            tgt.boxCollider.size = new Vector3(tgt.width, 0.1f, tgt.height);
            tgt.InitializeGrid();
            tgt.CalculateGridMetrics();
        }

        var gridChanged = false;

        for (int y = tgt.height - 1; y >= 0; y--)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < tgt.width; x++)
            {
                var idx = tgt.GetIndex(x,y);
                var originalValue = tgt.grid[idx].occupied;
                tgt.grid[idx].occupied = EditorGUILayout.Toggle(tgt.grid[idx].occupied);

                if(tgt.grid[idx].occupied != originalValue)
                    gridChanged = true;
            }
            GUILayout.EndHorizontal();       
        }

        if(gridChanged)
        {
            tgt.CalculateGridMetrics();
        }
    }

    void OnEnable() 
    {
        tgt = target as MazeTesting;
    }
}
