using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpperCard : MonoBehaviour
{
    [SerializeField] GameObject DestroyWithMe;
    [SerializeField] bool isActiveDay;
    #region Awake
    private void Awake()
    {
        buttonHoldEvent = GetComponent<ButtonHoldEvent>();
        buttonHoldEvent.Held += Held;
    }
    #endregion
    #region Создание - Удаление подкарточек
    public SetOfExercises setOfExercises;
    [SerializeField] Transform upperCard;
    [SerializeField] GameObject lowerCard;

    void CreateLowerCard()
    {
        if (setOfExercises == null&& setOfExercises.exercises.Count==0) throw new NotImplementedException($"setOfExercises == null");
        for (int i = 0; setOfExercises.exercises.Count > i; i++)
        {
            var inst = Instantiate(lowerCard, upperCard);
            inst.GetComponentInChildren<LowerCard>().exercise = setOfExercises.exercises[i];
            try
            {
                inst.GetComponentInChildren<TextMeshProUGUI>().text = inst.GetComponentInChildren<LowerCard>().exercise.specificParameters.ToString();
            }
            catch 
            { 
                inst.transform.Find("Lower Visual").GetComponentInChildren<TextMeshProUGUI>().text = inst.GetComponentInChildren<LowerCard>().exercise.specificParameters.ToString();
            }

            
        }
        ForCanvas.UpdateCanvas();
        if (setOfExercises.exercises.Count == 0) gameObject.SetActive(false);

    }
    public void SetActive()
    {
        CreateLowerCard();
        GetComponentInChildren<TextMeshProUGUI>().text = setOfExercises.ToString();
    }
    private void OnDisable()
    {
        Destroy(gameObject);
        ViewProgram.instList.Remove(gameObject);
    }
    #endregion
    #region Открытие меню взаимодействия
    private ButtonHoldEvent buttonHoldEvent;
    [Header("меню взаимодействия")]
    [SerializeField] GameObject[] ToOpen;
    [SerializeField] GameObject[] ToClose;
    bool open;
    void Held(object obj,float time)
    {
        for (int i = 0; i < ToOpen.Length; i++) { ToOpen[i].SetActive(open);}
        for (int i = 0; i < ToClose.Length; i++) { ToClose[i].SetActive(!open);}
        open = !open;
    }
    #endregion
    #region Удаление - Копирование
    public void Delite()
    {
        
        if(!isActiveDay)
        {
            byte ind = (byte)ViewProgram.day.setsOfExercises.FindIndex(set=>set.id==setOfExercises.id);
            ViewProgram.day.setsOfExercises.Remove(ViewProgram.day.setsOfExercises[ind]);
            ViewProgram.day.Sort();
            Week.SaveDay(ViewProgram.day);
            ViewProgram.UpdateProgram();
        }
        else
        {
            byte ind = (byte)Day.ActiveDay.setsOfExercises.FindIndex(set => set.id == setOfExercises.id);
            Day.ActiveDay.setsOfExercises.Remove(Day.ActiveDay.setsOfExercises[ind]);
            Day.ActiveDay.Sort();
            Day.ActiveDay = Day.ActiveDay;
            OpenStartTrening.UpdateActiveDayCards();
        }
            DestroyWithMe?.SetActive(false);
    }
    public void Copy()
    {
        Day day = isActiveDay ? Day.ActiveDay : ViewProgram.day;
        byte ind = (byte)day.setsOfExercises.FindIndex(set => set.id == setOfExercises.id);
        day.setsOfExercises.Insert(ind, setOfExercises.DeepClone(day.setsOfExercises[ind]));
        day.Sort();
        if(!isActiveDay)
        {   
            Week.SaveDay(ViewProgram.day);
            ViewProgram.UpdateProgram();
        }
        else
        {
            Day.ActiveDay = day;
            OpenStartTrening.UpdateActiveDayCards();
        }
    }
    public void OpenDiscription()
    {
        if(!isActiveDay)
        {
            ViewProgram.Description.SetActive(true);
            ViewProgram.Description.GetComponentInChildren<TextMeshProUGUI>().text = Description.GetDescriptionByName(setOfExercises.exercises[0].name);
        }
        else 
        {
            OpenStartTrening._description.SetActive(true);
            OpenStartTrening._description.GetComponentInChildren<TextMeshProUGUI>().text = Description.GetDescriptionByName(setOfExercises.exercises[0].name);
        }
    }
    #endregion

}
