using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DrawCommand
{
    DrawCommand(){}

    public enum DrawCommandType { label, shape, ui }
    public DrawCommandType type;
    public Vector3[] vertices;
    public Vector3 position;
    public Vector2 size;
    public Color color;
    public string text;
    public Sprite sprite;
    public static DrawCommand DrawUITexture(Vector3 position, Sprite sprite, Vector2 size)
    {
        var dc = new DrawCommand();
        dc.sprite = sprite;
        dc.position = position;
        dc.type = DrawCommandType.ui;
        dc.size = size;
        return dc;
    }

    public static DrawCommand DrawSquare(Vector3 position, float sideLength, Color color)
    { 
        var dc = new DrawCommand();
        float halfSideLength = sideLength * 0.5f;
        Vector3 v1 = position + Vector3.forward * halfSideLength + Vector3.right * halfSideLength;
        Vector3 v2 = position - Vector3.forward * halfSideLength + Vector3.right * halfSideLength;
        Vector3 v3 = position - Vector3.forward * halfSideLength - Vector3.right * halfSideLength;
        Vector3 v4 = position + Vector3.forward * halfSideLength - Vector3.right * halfSideLength;

        dc.type = DrawCommandType.shape;
        dc.vertices = new Vector3[]{v1,v2,v3,v4};
        dc.color = color;
        dc.position = position;
        return dc;
    }

    public static DrawCommand DrawLabel(Vector3 position, string text, Color color)
    {
        var dc = new DrawCommand();
        dc.position = position;
        dc.text = text;
        dc.color = color;
        return dc;
    }

    public static DrawCommand DrawLine(Vector3 start, Vector3 end, Color color)
    {
        var dc = new DrawCommand();
        return dc;
    }

    public static DrawCommand DrawArrow(Vector3 start, Vector3 end, Color color)
    {
        var dc = new DrawCommand();
        return dc;
    }

}