using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class DisplaySegment : MonoBehaviour
{
    TMP_Text text;

    private void Awake() 
    {
        text = GetComponent<TMP_Text>();
    }

    public void SetText(float number)
    {
        //float rounded = Mathf.Round(number);
        text.text = number.ToString();
    }

    public void SetText(string text) 
    {
        this.text.text = text;
    }
}
