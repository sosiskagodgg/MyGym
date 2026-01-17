using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ViewProgram : MonoBehaviour
{
    [SerializeField] GameObject programWindow; // для открытия меню
    
    [SerializeField] Transform content; 
    [SerializeField] GameObject upperCard;

    [SerializeField] Button[] buttons; // Для привязки событий на дабл клик
    [SerializeField] GameObject[] treningNames;
    
      

    [SerializeField] GameObject description;//для описания
    public static GameObject Description;
    
    public void UpdateProgramNames()
    {
        for (int i = 0; i < buttons.Length; i++) 
        {
            treningNames[i].GetComponent<TextMeshProUGUI>().text = Week.week.Days[i].programName ==""?"отдых": Week.week.Days[i].programName;
        }

    }
    private void OnEnable()
    {
        UpdateProgramNames();
    }
    private static void Set_Description(GameObject Description) => ViewProgram.Description = Description;
    #region для обновления
    static public List<GameObject> instList = new List<GameObject>();
    private static byte lastDayNum;
    private static ViewProgram CreateUpperCardsObj; 
    #endregion

    public static Day day; // для работы с текушим днем

    #region для привязки,отвязки событий
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
            case "Понедельник":
                CreateUpperCards(0);
                break;
            case "Вторник":
                CreateUpperCards(1);
                break;
            case "Среда":
                CreateUpperCards(2);
                break;
            case "Четверг":
                CreateUpperCards(3);
                break;
            case "Пятница":
                CreateUpperCards(4);
                break;
            case "Субота":
                CreateUpperCards(5);
                break;
            case "Воскресенье":
                CreateUpperCards(6);
                break;
            default:
                Debug.LogError("Неверный формат дня");
                break;
        }
        ForCanvas.UpdateCanvas();
    } // для 


    #region Создание,обновление карточек
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
        if(contentSizeFitter!=null) contentSizeFitter.enabled = false;
        if (contentSizeFitter != null) contentSizeFitter.enabled = true;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());

        // Если есть ContentSizeFitter
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
