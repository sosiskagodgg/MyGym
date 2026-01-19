using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddExerciseUpperMenu : MonoBehaviour
{
    [SerializeField] Button baseEx;
    [SerializeField] Button cardioEx;
    [SerializeField] Button stretchingEx;
    private Button lastClick;

    #region Вывод упражнений
    List<Exercise> exercises;
    List<Exercise> corectExercises;
    List<GameObject> spawnObj = new List<GameObject>();
    [SerializeField] GameObject card;
    [SerializeField] Transform content;
    [SerializeField] TMP_InputField InputField;

    string imputText;
    private void Awake()
    {
        exercises = ExerciseManager.Exercises;
        ChangeValue();
    }
    void CreateWindows()
    {

        if (lastClick != null) 
        {
            if (lastClick == baseEx)
            {
                corectExercises = exercises.Where(ex => ex.name.ToLower().Contains(imputText.ToLower())&& ((ex.specificParameters is StrengthTraining)|| (ex.specificParameters is Calisthenics))).ToList();
            }
            else if(lastClick == cardioEx)
            {
                corectExercises = exercises.Where(ex => ex.name.ToLower().Contains(imputText.ToLower()) && (ex.specificParameters is Walk) ).ToList();
            }
            else if (lastClick == stretchingEx)
            {
                corectExercises = exercises.Where(ex => ex.name.ToLower().Contains(imputText.ToLower()) && (ex.specificParameters is Stretching)).ToList();
            }
        }
        else
        {
            corectExercises = exercises.Where(ex => ex.name.ToLower().Contains(imputText.ToLower())).ToList();
        }

            ClearObj();

        for (int i = 0; corectExercises.Count > i; i++)
        {
            GameObject obj = Instantiate(card, content);
            obj.GetComponent<AddExerciseCard>().exercise = corectExercises[i];
            obj.GetComponentInChildren<TextMeshProUGUI>().text = corectExercises[i].name;
            spawnObj.Add(obj);
        }
        Canvas.ForceUpdateCanvases();
    }
    public void ChangeValue()
    {
        imputText = InputField.text;
        CreateWindows();
    }
    void ClearObj()
    {
        foreach (GameObject obj in spawnObj) { Destroy(obj); }
        spawnObj.Clear();
    }

    #endregion

    #region Установка диапазона упражнений
    public void BaseExClick() { lastClick = baseEx; CreateWindows(); }
    public void CardioExClick() { lastClick = cardioEx; CreateWindows(); }
    public void StretchingExClick() { lastClick = stretchingEx; CreateWindows(); }
    #endregion
}
