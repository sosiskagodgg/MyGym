using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddExerciseMenu : MonoBehaviour
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
        ExerciseChangedEvent += ExerciseChange;
    }
    void CreateWindows()
    {
        ClearObj();
        corectExercises = exercises.Where(ex => ex.name.Contains(imputText)).ToList();
        for (int i = 0; corectExercises.Count > i; i++)
        {
            GameObject obj = Instantiate(card, content);
            obj.GetComponent<LowerCard>().exercise = exercises[i];
            obj.GetComponentInChildren<TextMeshProUGUI>().text = exercises[i].name;
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

    [Header("exercise set settings")]
    [SerializeField] TextMeshProUGUI exerciseName;
    static public event EventHandler<Exercise> ExerciseChangedEvent;
    public Exercise exercise;
    public static void NotifyExerciseChanged(object sender, Exercise changedExercise)
    {
        ExerciseChangedEvent?.Invoke(sender, changedExercise);
    }
    void ExerciseChange(object obj ,Exercise exercise)
    {
        exerciseName.text = exercise.name;
        this.exercise = exercise;
    }
}
