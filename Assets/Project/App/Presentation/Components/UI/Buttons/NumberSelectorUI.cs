
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
using System.Collections;

public class NumberSelectorUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    #region Настройка параметров
    [Header("Сcылки")]
    [SerializeField] GameObject content;
                     RectTransform contentRt;
    [SerializeField] GameObject textObject;
    [SerializeField] SizeFilterAndVerticalGroup sizeFilterAndVerticalGroup;
    

    [Header("Настройки")]
    [SerializeField] public int min;
    [SerializeField] public int max;
    [SerializeField] bool setCastomStart;
    [SerializeField] int startY;
    [SerializeField] public float fontSize;
    [SerializeField] bool isAutoCreate = true;
    [SerializeField] bool isAutoColor = true;
    [SerializeField] int stepSize = 1;
    [Header("Настройки Магнита")]
    [SerializeField] RectTransform magnit;
    [SerializeField] float durationAnimation;
    [SerializeField] float magnitRange;
    [Header("Вывод")]
    [SerializeField] public string value;
    public delegate void Int(GameObject obj,int value);
    public event Int valueChanged;
    [SerializeField] public bool isPress;
    [SerializeField] public bool isDontChange;
    [SerializeField] public bool isPlayingAnimation;
    [SerializeField] public int localId;
    [SerializeField] public bool isScrolling;
    [SerializeField] ScrollRect scrollRect;
    private bool IsScrolling
    {
        get
        {
            if (scrollRect == null) return false;

            isScrolling= Mathf.Abs(scrollRect.velocity.x) > 0.1f ||
                   Mathf.Abs(scrollRect.velocity.y) > 0.1f;

            return Mathf.Abs(scrollRect.velocity.x) > 0.1f ||
                   Mathf.Abs(scrollRect.velocity.y) > 0.1f;
        }
    }
    #endregion
    #region Проверка зажат ли скрол рект
    public void OnPointerUp(PointerEventData eventData)
    {
        isPress = false;
        isDontChange=false;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        isPress = true;
        isDontChange = false;
        StopAllCoroutines();
        isPlayingAnimation = false;
    }
    #endregion
    #region Монобихейвор методы(awake и тд)

    private void OnEnable()
    {
        if(isAutoCreate)CreateTextObjects();
       // StartCoroutine(SetContentPosition(GetTargetPos(FindNearestByY(content, magnit).transform as RectTransform, magnit, contentRt)));
    }
    #endregion
    #region Методы для создания текстовых обьектов
    private List<GameObject> instantiateObjects = new();
    public void CreateTextObjects()
    {
        int numberOfObjects = Mathf.CeilToInt((float)(max - min) / stepSize) + 1;

        for (int i = 0; i < numberOfObjects; i++)
        {
            int value = min + (i * stepSize);

            // Проверка, чтобы не превысить max
            if (value > max) break;

            GameObject instObj = Instantiate(textObject, content.transform);
            instObj.GetComponentInChildren<TextMeshProUGUI>().text = value.ToString();

            if (fontSize > 0)
                instObj.GetComponentInChildren<TextMeshProUGUI>().fontSizeMax = fontSize;

            instantiateObjects.Add(instObj);
            instObj.SetActive(true);
            instObj.AddComponent<DestroyOnDisable>();
        }

        sizeFilterAndVerticalGroup.SetTransform();
        if (setCastomStart) SetStart();
    }
    private void SetStart() 
    {
        contentRt??= content.GetComponent<RectTransform>();
        contentRt.anchoredPosition = new Vector2(contentRt.anchoredPosition.x, startY);
    }




    #endregion
    #region Методы для управления позицией контента
    public static GameObject FindNearestByY(GameObject content,RectTransform rect)
    {
        List<RectTransform> childTransforms =
                        content.transform
                        .Cast<Transform>()
                        .Select(t => t.GetComponent<RectTransform>())
                        .Where(rt => rt != null)
                        .ToList(); ;

        GameObject nearest = null;
        float nearestY = float.MaxValue;
        for (int i = 0; i < childTransforms.Count; i++) 
        {
            float y = Mathf.Abs(childTransforms[i].transform.position.y- rect.position.y)  ;
            if( y < nearestY) 
            {
                nearestY = y;
                nearest = childTransforms[i].gameObject;
            }
        }
        return nearest;
    }

    int lastValue;
    Image parentImage;
    Image image;
    private void Update() 
    {
        contentRt ??= content.GetComponent<RectTransform>();
        GameObject nearest = FindNearestByY(content,magnit);
        RectTransform nearestRt = nearest.GetComponent<RectTransform>();
        if (nearestRt == null)  Debug.LogError("Нет ближающего обьекта");

        if (!isPlayingAnimation && !IsScrolling&&!isPress) StartCoroutine(SetContentPosition(GetTargetPos(FindNearestByY(content, magnit).transform as RectTransform, magnit, contentRt)));
        #region Изменения цвета
        parentImage ??= transform.parent.GetComponentInParent<Image>();
        image ??= transform.GetComponent<Image>();
        if (isAutoColor)
            if (parentImage.color != image.color) image.color = parentImage.color;
        #endregion

        #region Событие изменения значения
        value = nearest.GetComponentInChildren<TextMeshProUGUI>()?.text ?? "0";
        if (lastValue != System.Convert.ToInt32(value))
        {
            valueChanged?.Invoke(gameObject, System.Convert.ToInt32(value));
            lastValue = System.Convert.ToInt32(value);
        } 
        #endregion
    }
    private Vector2 GetTargetPos(RectTransform Obj,RectTransform magnit,RectTransform content)
    {
        Vector3 targetWorldPos = Obj.position;

        // 2. Вычисляем разницу по Y между целевой точкой и объектом контента
        float yDifference = magnit.position.y - targetWorldPos.y;
        
        // 3. Сдвигаем весь контент на эту разницу
        Vector3 newContentPos = content.position;
        newContentPos.y += yDifference;
        return newContentPos;
    }
    private IEnumerator SetContentPosition(Vector2  targetPos)
    {
        isPlayingAnimation = true;
        float elapsed = 0f;
        Vector2 startPos = contentRt.position;
        while (elapsed < durationAnimation)
        {

            elapsed += Time.deltaTime;
            float t = elapsed / durationAnimation;
            t = Mathf.SmoothStep(0f, 1f, t);

            contentRt.position = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }
        isPlayingAnimation= false;
    }


    #endregion
}