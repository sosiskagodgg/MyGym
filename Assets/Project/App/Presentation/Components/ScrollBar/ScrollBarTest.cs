using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using static UnityEngine.Rendering.DebugUI;

public class ScrollBarUI : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] float max = 100;
    [SerializeField] float min = 0;
    [Header("Objects")]
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Scrollbar scrollbar;
    public float Value { get; private set; }


    void Awake()
    {
        scrollbar.onValueChanged.AddListener(ValueChanged);
        if(text.text =="")text.text = min.ToString();
    }
    public void SetScrollBarPos(float value)
    {
        scrollbar.value = Mathf.InverseLerp(min, max, value);
        ValueChanged(scrollbar.value);
    }
    void ValueChanged(float value)
    {
        if (text != null)
        {
            this.Value = Mathf.Lerp(min, max, value);
            text.text = Convert.ToInt32(Value).ToString();
        }
    }
}