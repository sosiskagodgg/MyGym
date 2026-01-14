using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
public class DoubleClickEvent : MonoBehaviour
{
    public bool isClick = false;
    public event EventHandler<string> DoubleClick;
    [SerializeField] bool deactiveOnDoubleClick;
    private void OnDisable()
    {
        isClick = false;
    }
    private void OnEnable()
    {
        isClick = false;
    }

    public void CheackDoubleClick()
    {
        if (isClick) { DoubleClick?.Invoke(this, gameObject.name);isClick = false;if (deactiveOnDoubleClick) gameObject.SetActive(false); } 
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
