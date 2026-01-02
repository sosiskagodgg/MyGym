using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBarUI : Scrollbar
{
    [Header("Parameters")]
    [SerializeField] public float max;
    [SerializeField] public float min;
    [Header("Objects")]
    [SerializeField] public TextMeshProUGUI text;

    protected override void Awake()
    {
        base.Awake();
        onValueChanged.AddListener(UpdateText);
    }

    private float LerpValue(float value) => Mathf.Lerp(min, max, value);

    private void UpdateText(float value)
    {
        if (text != null)
            text.text = LerpValue(value).ToString();
    }
}