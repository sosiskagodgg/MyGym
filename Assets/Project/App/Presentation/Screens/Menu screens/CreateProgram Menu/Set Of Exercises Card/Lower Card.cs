using System;
using UnityEngine;
using UnityEngine.UI;

public class LowerCard : MonoBehaviour
{
    public Exercise exercise;
    private void Awake()
    {
        buttonHoldEvent = GetComponent<ButtonHoldEvent>();
        buttonHoldEvent.Held += Held;
    }
    public void DebugStringBilder() 
    {
        if(exercise == null)  throw new Exception("Упражнение пустое!"); 
        if(exercise.specificParameters == null) throw new Exception("Специальные параметры пустые!");
        if(exercise.specificParameters.debugString == null ) throw new Exception("Дебаг стринг пустой!");
        if (exercise.specificParameters.debugString == null) Debug.Log("Дебаг стринг пустой");
        Debug.Log(exercise.specificParameters.debugString.ToString());
    }
    #region Обработка зажатия кнопки
    private ButtonHoldEvent buttonHoldEvent;
    [Header("меню взаимодействия")]
    [SerializeField] GameObject[] ToOpen;
    [SerializeField] GameObject[] ToClose;
    bool open;
    void Held(object obj, float time)
    {
        for (int i = 0; i < ToOpen.Length; i++) { ToOpen[i].SetActive(open); }
        for (int i = 0; i < ToClose.Length; i++) { ToClose[i].SetActive(!open); }
        open = !open;
    }
    #endregion
    #region Копирование удаление
    UpperCard upperCard;
    Day day;
    SetOfExercises setOfExercises;
    public void Delite()
    {
        upperCard ??= GetComponentInParent<UpperCard>();
        day ??= ViewProgram.day;
        setOfExercises ??= upperCard.setOfExercises;
        int i = day.setsOfExercises.FindIndex(set => set.id == setOfExercises.id);
        int i2 = day.setsOfExercises[i].exercises.FindIndex(ex => ex.id == exercise.id);
        day.setsOfExercises[i].exercises.Remove(day.setsOfExercises[i].exercises[i2]);
        Week.SaveDay(day);
        ViewProgram.UpdateProgram();
    }
    public void Copy()
    {
        upperCard ??= GetComponentInParent<UpperCard>();
        day ??= ViewProgram.day;
        setOfExercises ??= upperCard.setOfExercises;
        int i = day.setsOfExercises.FindIndex(set => set.id == setOfExercises.id);
        int i2 = day.setsOfExercises[i].exercises.FindIndex(ex => ex.id == exercise.id);
        day.setsOfExercises[i].exercises.Insert(i2,ExerciseManager.DeepClone(day.setsOfExercises[i].exercises[i2]));
        day.setsOfExercises[i].Sort();
        Week.SaveDay(day);
        ViewProgram.UpdateProgram();
    }
    #endregion
}
