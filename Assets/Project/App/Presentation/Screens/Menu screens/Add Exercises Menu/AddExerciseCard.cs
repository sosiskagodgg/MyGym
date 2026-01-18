using System;
using TMPro;
using UnityEngine;

public class AddExerciseCard : MonoBehaviour
{
    public Exercise exercise;
    public void AddButtonClick()
    {
        AddExerciseLowerMenu.Invoke(this, exercise);
        Canvas.ForceUpdateCanvases();
    }
    public void OpenDiscription()
    {
        Description.OpenDescription(exercise);
    }
}
