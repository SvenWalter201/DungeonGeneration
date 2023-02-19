using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InspectorCommand
{
    InspectorCommand(){}

    public string text;

    public static InspectorCommand WriteLine(string text)
    {
        var ic = new InspectorCommand();
        ic.text = text;
        return ic;
    }
}