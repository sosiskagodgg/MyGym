using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ViewProgram : MonoBehaviour
{
    [SerializeField] GameObject programWindow; // дл€ открыти€ меню
    
    [SerializeField] Transform content; 
    [SerializeField] GameObject upperCard;

    [SerializeField] Button[] buttons; // ƒл€ прив€зки событий на дабл клик
    [SerializeField] GameObject description;//дл€ описани€
    public static GameObject Description;
    private static void Set_Description(GameObject Description) => ViewProgram.Description = Description;
    #region дл€ обновлени€
    static public List<GameObject> instList = new List<GameObject>();
    private static byte lastDayNum;
    private static ViewProgram CreateUpperCardsObj; 
    #endregion

    public static Day day; // дл€ работы с текушим днем

    #region дл€ прив€зки,отв€зки событий
    private void Awake()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<DoubleClickEvent>().DoubleClick += OpenProgram;
        }
        Set_Description(description);
    }
    private void OnDestroy()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<DoubleClickEvent>().DoubleClick -= OpenProgram;
        }
    }

    #endregion

    public void OpenProgram(object obj,string name)
    {
        programWindow.SetActive(true);
        switch (name)
        {
            case "Monday":
                CreateUpperCards(0);
                break;
            case "Tuesday":
                CreateUpperCards(1);
                break;
            case "Wednesday":
                CreateUpperCards(2);
                break;
            case "Thursday":
                CreateUpperCards(3);
                break;
            case "Friday":
                CreateUpperCards(4);
                break;
            case "Saturday":
                CreateUpperCards(5);
                break;
            case "Sunday":
                CreateUpperCards(6);
                break;
            default:
                Debug.LogError("Ќеверный формат дн€");
                break;
        }
        ForCanvas.UpdateCanvas();
    } // дл€ 


    #region —оздание,обновление карточек
    void CreateUpperCards(byte dayNum)
    {
        day = Week.week.Days[dayNum];
        for (int i = 0; i < day.setsOfExercises.Count; i++)
        {
            var inst = Instantiate(upperCard, content);
            inst.GetComponentInChildren<UpperCard>().setOfExercises = day.setsOfExercises[i];
            inst.GetComponentInChildren<UpperCard>().SetActive();
            instList.Add(inst);
        }
        lastDayNum = dayNum;
        ForceLayoutRebuild();
    }
    public static void UpdateProgram()
    {
        CreateUpperCardsObj ??= GameObject.FindGameObjectWithTag("CreateUpperCards").GetComponent<ViewProgram>();
        for (int i = 0;i <instList.Count;i++)
        {
            try
            {
                instList[i].SetActive(false);
            }
            catch { }
            
           
        }
        CreateUpperCardsObj.CreateUpperCards(lastDayNum);
    }
    [SerializeField] ContentSizeFitter contentSizeFitter;
     void ForceLayoutRebuild()
    {
        contentSizeFitter.enabled = false;
        contentSizeFitter.enabled = true;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());

        // ≈сли есть ContentSizeFitter
        var fitter = contentSizeFitter;
        if (fitter != null)
        {
            fitter.SetLayoutHorizontal();
            fitter.SetLayoutVertical();
        }

        ForCanvas.UpdateCanvas();
    }
    #endregion
}
