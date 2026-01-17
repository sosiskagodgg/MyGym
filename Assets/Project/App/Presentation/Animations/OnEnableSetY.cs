using System.Collections;
using UnityEngine;

public class OnEnableSetY : MonoBehaviour
{
    void OnEnable()
    {
        StartCoroutine(SetY());
    }
    IEnumerator SetY()
    {
        yield return new WaitForEndOfFrame();
        float y = ((transform as RectTransform).rect.height - (transform.parent.transform as RectTransform).rect.height) / 2;

                  (transform as RectTransform).anchoredPosition = new Vector2((transform as RectTransform).position.x, -y);
                  
    }
}
