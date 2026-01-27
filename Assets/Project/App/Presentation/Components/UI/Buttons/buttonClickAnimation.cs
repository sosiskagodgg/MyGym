using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class buttonClickAnimation : MonoBehaviour
{
    [SerializeField] buttonClickAnimation[] UnPress;
    [SerializeField] float timeAnimation = 1f;
    [SerializeField] bool isPress;
    [SerializeField] GameObject[] setActiveObj;
    [SerializeField] GameObject[] setDeactiveObj;
    static bool isAnimation;
    RectTransform rectTransform;
    Vector2 startPos;
    Shadow shadow;
    Vector2 startChadow;

    Vector2 targetPos;
    Vector2 targetShadow;
    VerticalLayoutGroup verticalLayoutGroup;
    float distance = 0f;
    public void OnClick()
    {
        distance = shadow.effectDistance.y;
        targetPos = new Vector2(rectTransform.anchoredPosition.x-distance, rectTransform.anchoredPosition.y+distance);
        targetShadow = new Vector2(0, 0);
        if (!isPress && !isAnimation)
        {
            SetUnPress();
            StartCoroutine(StartPressAnimation(true));
        }
    }
    public void SetUnPress()
    {
        for(int i = 0; UnPress.Length > i; i++)
        {
            UnPress[i].StartUnPress();
        }
    }
    public void StartUnPress()
    {
        targetShadow = startChadow;
        targetPos = new Vector2(rectTransform.anchoredPosition.x + distance, rectTransform.anchoredPosition.y - distance);
        if (isPress && !isAnimation)StartCoroutine(StartPressAnimation(false));
    }
    
    IEnumerator StartPressAnimation(bool isPressed)
    {
        if(verticalLayoutGroup!=null) verticalLayoutGroup.enabled = false;
        float elapsed = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 startShadowPos = shadow.effectDistance;
        while (elapsed < timeAnimation )
        {
            isAnimation=true;
            elapsed += Time.deltaTime;
            float t = elapsed / timeAnimation;
            t = Mathf.SmoothStep(0,1,t);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            shadow.effectDistance = Vector2.Lerp(startShadowPos, targetShadow, t);
            yield return null;
        }
        isAnimation=false;
        isPress= isPressed;
        foreach(GameObject obj in setActiveObj) { obj.SetActive(!isPress); }
        foreach (GameObject obj in setDeactiveObj) { obj.SetActive(isPress); }
        if (!isPress) SetStartPos();
    }
    void SetStartPos()
    {
        rectTransform.anchoredPosition = startPos;
    }
    private void OnEnable()
    {
        if (verticalLayoutGroup != null) verticalLayoutGroup.enabled = true;
    }
    private void Awake()
    {
        shadow = GetComponent<Shadow>();
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
        startChadow = shadow.effectDistance;
        verticalLayoutGroup= transform.GetComponentInParent<VerticalLayoutGroup>();
    }
}
