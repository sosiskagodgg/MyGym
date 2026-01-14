
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class NumberSelectorUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    #region Настройка параметров
    [Header("Сcылки")]
    [SerializeField] GameObject content;
                     RectTransform contentRt;
    [SerializeField] GameObject textObject;
    [SerializeField] SizeFilterAndVerticalGroup sizeFilterAndVerticalGroup;

    [Header("Настройки")]
    [SerializeField] int min;
    [SerializeField] int max;
    [SerializeField] bool setCastomStart;
    [SerializeField] int startY;
    [SerializeField] int fontSize;

    [Header("Настройки Магнита")]
    [SerializeField] RectTransform magnit;
    [SerializeField] float speed;
    [SerializeField] float magnitRange;
    [Header("Вывод")]
    [SerializeField] public string value;
    [SerializeField] public bool isPress;
    [SerializeField] public bool isDontChange = true;
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
    }
    #endregion
    #region Монобихейвор методы(awake и тд)

    private void OnEnable()
    {
        CreateTextObjects();
    }
    #endregion
    #region Методы для создания текстовых обьектов
    private List<GameObject> instantiateObjects = new();
    private void CreateTextObjects()
    {
        for(int i = 0;max - min+1 > i; i++)
        {
            GameObject instObj = Instantiate(textObject,content.transform);
            instObj.GetComponentInChildren<TextMeshProUGUI>().text = (min + i).ToString();
            if (fontSize > 0) instObj.GetComponentInChildren<TextMeshProUGUI>().fontSizeMax = fontSize;
            instantiateObjects.Add(instObj);
            instObj.SetActive(true);
            instObj.AddComponent<DestroyOnDisable>();
        }
        sizeFilterAndVerticalGroup.SetTransform();
        if(setCastomStart)SetStart();
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


    private void Update() 
    {
        contentRt ??= content.GetComponent<RectTransform>();
        GameObject nearest = FindNearestByY(content,magnit);
        RectTransform nearestRt = nearest.GetComponent<RectTransform>();

        if (nearestRt == null)  Debug.LogError("Нет ближающего обьекта");

        if (nearestRt.transform.position.y - magnit.position.y >= magnitRange && !isPress &&!isDontChange) 
        {
            contentRt.transform.position = new Vector2(contentRt.transform.position.x, contentRt.transform.position.y-speed);
        }
        else if(nearestRt.transform.position.y - magnit.position.y <= -magnitRange&& !isPress&& !isDontChange)
        {
            contentRt.transform.position = new Vector2(contentRt.transform.position.x, contentRt.transform.position.y + speed);
        }
        if (nearestRt.transform.position.y - magnit.position.y <= -magnitRange && nearestRt.transform.position.y - magnit.position.y >= magnitRange) isDontChange = false;
        value = nearest.GetComponentInChildren<TextMeshProUGUI>()?.text ?? "0";
    }
    #endregion
}