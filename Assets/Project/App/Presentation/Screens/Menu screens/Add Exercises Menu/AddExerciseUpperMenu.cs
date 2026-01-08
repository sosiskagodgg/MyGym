using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddExerciseUpperMenu : MonoBehaviour
{
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
        ClearObj();
        corectExercises = exercises.Where(ex => ex.name.ToLower().Contains(imputText.ToLower())).ToList();
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

}
