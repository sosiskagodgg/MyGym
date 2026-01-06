using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
public class DoubleClickEvent : MonoBehaviour
{
    bool isClick = false;
    public event EventHandler DoubleClick;
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(CheackDoubleClick);
    }
    void CheackDoubleClick()
    {
        if (isClick) DoubleClick?.Invoke(this,EventArgs.Empty);
        else
        {
            isClick = true;
            StartCoroutine(Time());
        }
    }
    IEnumerator Time()
    {
        yield return new WaitForSeconds(0.5f);
        isClick = false;
    }
}
