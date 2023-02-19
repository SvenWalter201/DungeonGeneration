using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PlotPointConnection : MonoBehaviour
{
    RectTransform rT;
    Image image;

    void Awake() 
    {
        image = GetComponent<Image>();
        rT = GetComponent<RectTransform>();
    }

    public void PlaceConnection(Vector2 pointAPosition, Vector2 pointBPosition, Color color, float width)
    {
        Vector2 dir = (pointBPosition - pointAPosition).normalized;
        float distance = Vector2.Distance(pointAPosition, pointBPosition);
        rT.anchorMin = Vector2.zero;
        rT.anchorMax = Vector2.zero;
        rT.sizeDelta = new Vector2(distance, width);
        rT.anchoredPosition = pointAPosition + dir * distance * 0.5f;
        rT.localEulerAngles = new Vector3(0,0, MathHelper.GetAngleFromFloatVector(dir));

        image.color = color;
    }
}
