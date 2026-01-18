using System;
using TMPro;
using UnityEngine;

public class AddExerciseLowerMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] ScrollBarUI scrollBarUI;
    [SerializeField] bool isActiveDay;
    public static event EventHandler<Exercise> exerciseChange;
    public static void Invoke(object obj, Exercise exercise)
    {
        exerciseChange?.Invoke(obj, exercise);
    }
    Exercise exercise;
    private void Awake()
    {
        exerciseChange += ExerciseChange;
    }
    private void ExerciseChange(object obj,Exercise exercise)
    {
        text.text = exercise.name;
        Canvas.ForceUpdateCanvases();
        this.exercise = exercise;
    }
    public void AddExercise()
    {
        try
        {
            SetOfExercises newSet = new SetOfExercises(exercise, (byte)scrollBarUI.Value);
            Day day = isActiveDay ? Day.ActiveDay : ViewProgram.day;
            day.AddSetOfExercises(newSet);
            text.text = "Успех!";
            if (isActiveDay) Day.ActiveDay = day;
            else
            {
                Week.SaveDay(day);
            }
        }
        catch
        {
            text.text = "Выберите упражнение!";
        }
    }
}
