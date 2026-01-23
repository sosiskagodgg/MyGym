using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class SwapUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] protected Transform parent;
    [SerializeField] protected List<RectTransform> swapFields;
    [SerializeField] protected SwapManager swapManager;
    [SerializeField] private ViewProgram viewProgram;
    Vector2 startPos;
    public RectTransform rectTransform;
    RectTransform nearestField;
    bool isDrag;
    bool isStartSets;
    bool hasSwapped = false; // Флаг чтобы свап происходил только один раз

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
        isDrag = false; // Начинаем с false чтобы магнит работал
        
    }

    #region Для перемещения
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta;
        isDrag = true;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        parent.SetAsLastSibling();
        isDrag = true;
        hasSwapped = false; // Сбрасываем флаг свапа
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
    }
    private void OnEnable()
    {
        StartCoroutine(SetPosition());
    }
    #endregion

    #region Для магнита
    protected static RectTransform FindNearest(RectTransform source, List<RectTransform> targets)
    {
        if (targets == null || targets.Count == 0) return null;

        RectTransform nearest = null;
        float minDistance = float.MaxValue;
        Vector2 sourcePos = source.position;

        foreach (var target in targets)
        {
            if (target == source || target == null) continue;

            float distance = Vector2.Distance(sourcePos, target.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = target;
            }
        }

        return nearest;
    }

    protected void Magnit()
    {
        nearestField = FindNearest(rectTransform, swapFields);
        if (nearestField == null) return;

        // Двигаемся к полю
        transform.position = Vector2.Lerp(transform.position, nearestField.position, 5f * Time.deltaTime);

        // Проверяем достижение цели (используем квадрат расстояния для производительности)
        if (Vector2.Distance(transform.position,nearestField.position) < 0.01f && !hasSwapped)
        {
            // Фиксируем позицию точно в центре поля
            transform.position = nearestField.position;

            // Выполняем логику свапа (только один раз!)
            Logic(gameObject, nearestField.transform.parent.gameObject);
            hasSwapped = true; // Устанавливаем флаг, чтобы не свапать повторно
        }
    }

    protected void Update()
    {
        if (!isDrag&&!isStartSets) Magnit();
    }

    protected virtual void Logic(GameObject obj, GameObject field)
    {
        Debug.Log("начало свапа");
        if (obj.name == field.name)
        {
            Debug.Log("одинаковые дни не свапаем");
            return;
        }

        Debug.Log($"Дни разные начинаем свап {obj.name} и {field.name}");

        // Находим дни
        Day day1 = Week.week.Days.FirstOrDefault(d => d.name == obj.name);
        Day day2 = Week.week.Days.FirstOrDefault(d => d.name == field.name);

        if (day1 != null && day2 != null)
        {
            // Меняем местами одним выражением
            (day1.setsOfExercises, day2.setsOfExercises) = (day2.setsOfExercises, day1.setsOfExercises);
            (day1.programName, day2.programName) = (day2.programName, day1.programName);

            // Сохраняем
            Week.SaveDay(day1);
            Week.SaveDay(day2);

            Debug.Log($"Свапнуты программы: {day1.name}={day1.programName}, {day2.name}={day2.programName}");

            // Обновляем UI
            viewProgram.UpdateProgramNames();
        }
        else
        {
            Debug.LogWarning($"Не найдены дни: obj={obj.name}, field={field.name}");
        }
        StartCoroutine(SetPosition());
    }
    #endregion
    protected virtual IEnumerator SetPosition()
    {
        isStartSets = true;
        while (Vector2.Distance(rectTransform.anchoredPosition, startPos) > 0.1&&!isDrag)
        {
            rectTransform.anchoredPosition =Vector2.Lerp(rectTransform.anchoredPosition,startPos,5f * Time.deltaTime);
            yield return null;
        }
        isStartSets = false;
    }
}