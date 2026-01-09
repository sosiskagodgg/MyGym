using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHoldEvent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] float timeHeld = 0.5f;
    public event EventHandler<float> Held;
    #region Работа с event датой на кнопке
    private bool isButtonHeld = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        isButtonHeld = true;
        OnHoldStarted();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (isButtonHeld)
        {
            isButtonHeld = false;
            OnHoldEnded();
        }
    }
    #endregion

    protected virtual void OnHoldStarted()
    {
        StartCoroutine(CheackTimeHold());
    }

    protected virtual void OnHoldEnded()
    {

    }
    IEnumerator CheackTimeHold()
    {
        yield return new WaitForSeconds(timeHeld);
        if (isButtonHeld)
        {
            Held?.Invoke(this, timeHeld);
        }
    }
}
