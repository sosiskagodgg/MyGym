using System;
using TMPro;
using UnityEngine;

public class AddExerciseLowerMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] ScrollBarUI scrollBarUI;
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
        SetOfExercises newSet = new SetOfExercises(exercise, (byte)scrollBarUI.Value);
        ViewProgram.day.AddSetOfExercises(newSet);
    }
}
