using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpperCard : MonoBehaviour
{
    [SerializeField] GameObject DestroyWithMe;
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
        if (setOfExercises == null) throw new NotImplementedException($"setOfExercises == null");
        for (int i = 0; setOfExercises.exercises.Count > i; i++)
        {
            var inst = Instantiate(lowerCard, upperCard);
            inst.GetComponent<LowerCard>().exercise = setOfExercises.exercises[i];
            inst.GetComponentInChildren<TextMeshProUGUI>().text = inst.GetComponent<LowerCard>().exercise.specificParameters.ToString();
        }
        ForCanvas.UpdateCanvas();
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
        byte ind = (byte)ViewProgram.day.setsOfExercises.FindIndex(set=>set.id==setOfExercises.id);
        ViewProgram.day.setsOfExercises.Remove(ViewProgram.day.setsOfExercises[ind]);
        ViewProgram.day.Sort();
        Week.SaveDay(ViewProgram.day);
        ViewProgram.UpdateProgram();
        DestroyWithMe.SetActive(false);
    }
    public void Copy()
    {
        byte ind = (byte)ViewProgram.day.setsOfExercises.FindIndex(set => set.id == setOfExercises.id);
        ViewProgram.day.setsOfExercises.Insert(ind, setOfExercises.DeepClone(ViewProgram.day.setsOfExercises[ind]));
        ViewProgram.day.Sort();
        Week.SaveDay(ViewProgram.day);
        ViewProgram.UpdateProgram();
    }
    public void OpenDiscription()
    {
        ViewProgram.Description.SetActive(true);
        ViewProgram.Description.GetComponentInChildren<TextMeshProUGUI>().text = Description.GetDescriptionByName(setOfExercises.exercises[0].name);
    }
    #endregion
}
