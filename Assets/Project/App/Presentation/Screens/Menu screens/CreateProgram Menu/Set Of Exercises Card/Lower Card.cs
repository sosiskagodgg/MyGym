using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class LowerCard : MonoBehaviour
{
    public Exercise exercise;




    #region Создание и насткройка текст скрола

    [Header("для создания скрол текста")]
    [SerializeField] TextMeshProUGUI textMeshProUGUI;
    [SerializeField] GameObject scrollText;
    [SerializeField] GameObject button;
    [SerializeField] float modifierMax = 1.3f;
    [SerializeField] float modifierMin = 0.7f;
    [Header("дебаг")]
    [SerializeField] List<float> parametrs;
    private void FindMiddleOfNumberGroups(TextMeshProUGUI text,out int count,out List<Vector2> groupCenters,out List<int> groupValues)
    {
        groupCenters = new List<Vector2>();
        groupValues = new List<int>();

        text.ForceMeshUpdate();
        TMP_TextInfo textInfo = text.textInfo;

        List<List<Vector2>> digitGroups = new List<List<Vector2>>();
        List<StringBuilder> digitValues = new List<StringBuilder>();
        List<Vector2> currentGroup = null;
        StringBuilder currentValue = null;

        // Собираем цифры в группы (последовательности подряд идущих цифр)
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (charInfo.isVisible && char.IsDigit(charInfo.character))
            {
                // Начало новой группы цифр
                if (currentGroup == null)
                {
                    currentGroup = new List<Vector2>();
                    digitGroups.Add(currentGroup);

                    currentValue = new StringBuilder();
                    digitValues.Add(currentValue);
                }

                // Добавляем центр текущей цифры в группу
                Vector2 center = (charInfo.bottomLeft + charInfo.topRight) / 2f;
                currentGroup.Add(center);

                // Добавляем цифру к значению
                currentValue.Append(charInfo.character);
            }
            else
            {
                currentGroup = null; // Не цифра - сбрасываем группу
                currentValue = null;
            }
        }

        // Вычисляем центр каждой группы и преобразуем значения
        for (int i = 0; i < digitGroups.Count; i++)
        {
            var group = digitGroups[i];
            if (group.Count > 0)
            {
                // Среднее арифметическое всех центров в группе
                Vector2 groupCenter = Vector2.zero;
                foreach (var point in group)
                {
                    groupCenter += point;
                }
                groupCenter /= group.Count;

                groupCenters.Add(groupCenter);

                // Преобразуем собранные цифры в число
                if (digitValues[i] != null && digitValues[i].Length > 0)
                {
                    string valueStr = digitValues[i].ToString();
                    if (int.TryParse(valueStr, out int value))
                    {
                        groupValues.Add(value);
                    }
                    else
                    {
                        groupValues.Add(0); // или какое-то значение по умолчанию
                    }
                }
            }
        }

        count = groupCenters.Count;
    }
    private List<GameObject> Buttons = new();
    private void CreateButtons()
    {
        FindMiddleOfNumberGroups(textMeshProUGUI, out int count, out List<Vector2> groupCenters, out List<int> groupValues);
        parametrs = exercise.specificParameters.GetParametrs();
        for (int i = 0; i < count; i++)
        {

            Vector2 localCoords = groupCenters[i];
            int localValue = groupValues[i];
            int indexParametr = i;

            var obj = Instantiate(button, transform);
            Buttons.Add(obj);
            (obj.transform as RectTransform).anchoredPosition = localCoords;
            obj.GetComponent<Button>().onClick.AddListener(() => CreateScrollText(localCoords, localValue, indexParametr));
        }
    }
    private void CreateScrollText(Vector2 coordinates,int value,int indexParametr)
    {

        var newScrollText = Instantiate(scrollText,transform);

        (newScrollText.transform as RectTransform).anchoredPosition = coordinates;

        NumberSelectorUI scrollCompontent = newScrollText.GetComponent<NumberSelectorUI>();

        int max = (int)(value * modifierMax);
        int min = (int)(value * modifierMin);
        if (value < 10) { max = value * 2;min = value / 2; }
        scrollCompontent.max = max;
        scrollCompontent.min = min;
        scrollCompontent.localId = indexParametr;
        scrollCompontent.fontSize = textMeshProUGUI.fontSize;
        scrollCompontent.CreateTextObjects();
        scrollCompontent.valueChanged += StartCloseTime;
    }
    private void StartCloseTime(GameObject obj,int i)
    {
        this.StopAllCoroutines();
        StartCoroutine(Close(obj));
    }
    private IEnumerator Close(GameObject obj)
    {
        NumberSelectorUI numberSelectorUI=obj.GetComponent<NumberSelectorUI>();
        yield return new WaitForSeconds(3f);
        parametrs[numberSelectorUI.localId] = Convert.ToInt32(numberSelectorUI.value);
        UpdateExercise(parametrs);
        GameObject.Destroy(obj);
    }



    #endregion

    #region монобихейвор методы
    private void OnEnable()
    {
        buttonHoldEvent = GetComponent<ButtonHoldEvent>();
        buttonHoldEvent.Held += Held;
        StartCoroutine(Enumerator());
        IEnumerator Enumerator()
        {
            yield return new WaitForEndOfFrame();
            CreateButtons();
        }
    }
    private void OnDisable()
    {
        buttonHoldEvent = GetComponent<ButtonHoldEvent>();
        buttonHoldEvent.Held -= Held;
    }
    #endregion

    #region Дебаг
    public void DebugStringBilder()
    {
        Debug.Log(exercise.specificParameters.debugString.ToString());
    } 
    #endregion

    #region Обработка зажатия кнопки
    private ButtonHoldEvent buttonHoldEvent;
    [Header("меню взаимодействия")]
    [SerializeField] GameObject[] ToOpenOnPress;
    [SerializeField] GameObject[] ToCloseOnPress;
    bool openOnPress;
    void Held(object obj, float time)
    {
        for (int i = 0; i < ToOpenOnPress.Length; i++) { ToOpenOnPress[i].SetActive(openOnPress); }
        for (int i = 0; i < ToCloseOnPress.Length; i++) { ToCloseOnPress[i].SetActive(!openOnPress); }
        for(int i = 0;i < Buttons.Count; i++) {  Buttons[i].SetActive(openOnPress); }
        openOnPress = !openOnPress;
    }
    #endregion
 
    #region Копирование удаление обновление
    UpperCard upperCard;
    Day day;
    SetOfExercises setOfExercises;
    [SerializeField] bool isActiveDay;
    public void Delite()
    {
        upperCard ??= GetComponentInParent<UpperCard>();
        if (!isActiveDay) day = ViewProgram.day;
        else day = Day.ActiveDay;
        setOfExercises ??= upperCard.setOfExercises;
        int i = day.setsOfExercises.FindIndex(set => set.id == setOfExercises.id);
        int i2 = day.setsOfExercises[i].exercises.FindIndex(ex => ex.id == exercise.id);
        day.setsOfExercises[i].exercises.Remove(day.setsOfExercises[i].exercises[i2]);
        if (!isActiveDay) {Week.SaveDay(day);ViewProgram.UpdateProgram(); }
        else { Day.ActiveDay = Day.ActiveDay; OpenStartTrening.UpdateActiveDayCards(); }
        
    }
    public void Copy()
    {
        upperCard ??= GetComponentInParent<UpperCard>();
        if (!isActiveDay) day = ViewProgram.day;
        else day = Day.ActiveDay;
        setOfExercises ??= upperCard.setOfExercises;
        int i = day.setsOfExercises.FindIndex(set => set.id == setOfExercises.id);
        int i2 = day.setsOfExercises[i].exercises.FindIndex(ex => ex.id == exercise.id);
        day.setsOfExercises[i].exercises.Insert(i2,ExerciseManager.DeepClone(day.setsOfExercises[i].exercises[i2]));
        day.setsOfExercises[i].Sort();
        if (!isActiveDay) { Week.SaveDay(day); ViewProgram.UpdateProgram(); }
        else { Day.ActiveDay = Day.ActiveDay; OpenStartTrening.UpdateActiveDayCards(); }
    }
    public void UpdateExercise(List<float> newParametrs)
    {
        upperCard ??= GetComponentInParent<UpperCard>();
        if (!isActiveDay) day = ViewProgram.day;
        else day = Day.ActiveDay;
        setOfExercises ??= upperCard.setOfExercises;
        int i = day.setsOfExercises.FindIndex(set => set.id == setOfExercises.id);
        int i2 = day.setsOfExercises[i].exercises.FindIndex(ex => ex.id == exercise.id);
        day.setsOfExercises[i].exercises[i2].specificParameters.SetNewParametrs(newParametrs);
        if (!isActiveDay) { Week.SaveDay(day); ViewProgram.UpdateProgram(); }
        else { Day.ActiveDay = Day.ActiveDay; OpenStartTrening.UpdateActiveDayCards(); }
  
    }
    
    #endregion
}
