using System;
using UnityEngine;
using UnityEngine.UI;

public class ViewProgram : MonoBehaviour
{
    [SerializeField] GameObject programWindow;
    [SerializeField] Transform content;
    [SerializeField] GameObject upperCard;
    [SerializeField] Button[] buttons;
    public static Day day;
    private void Awake()
    {
        for (int i = 0; i < buttons.Length; i++) 
        {
            buttons[i].GetComponent<DoubleClickEvent>().DoubleClick+= OpenProgram;
        }
    }
    private void OnDestroy()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponent<DoubleClickEvent>().DoubleClick -= OpenProgram;
        }
    }
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
                Debug.LogError("Неверный формат дня");
                break;
        }
        ForCanvas.UpdateCanvas();
    }
    void CreateUpperCards(byte dayNum)
    {
        day = Week.week.Days[dayNum];
        for (int i = 0; i < day.setsOfExercises.Count; i++)
        {
            var inst = Instantiate(upperCard, content);
            inst.GetComponent<UpperCard>().setOfExercises = day.setsOfExercises[i];
            inst.GetComponent<UpperCard>().SetActive();
        }
    }
}
