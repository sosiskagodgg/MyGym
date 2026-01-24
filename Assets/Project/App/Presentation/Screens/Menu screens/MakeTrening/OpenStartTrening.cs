using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;

public class OpenStartTrening : MonoBehaviour
{
    [SerializeField] GameObject upper;
    [SerializeField] Transform content;
    [SerializeField] TextMeshProUGUI dayName;

    [SerializeField] GameObject description;//для описания
    public static GameObject _description;

    public delegate void Void();
    public static event Void UpdateActiveDayCard;
    private void OnEnable()
    {
        _description = description;
        CreateCards();
        UpdateActiveDayCard += CreateCards;
    }
    private void OnDisable()
    {
        UpdateActiveDayCard -= CreateCards;
    }
    public static void UpdateActiveDayCards()
    {
        UpdateActiveDayCard?.Invoke();
    }

    List<GameObject> instObjs = new();
    void CreateCards()
    {
        dayName.text = Day.ActiveDay.name;
        if (Day.ActiveDay.setsOfExercises == null)
        {
            StartCoroutine(WaitingDay());
            return;
        }
        for(int i = 0; i<instObjs.Count;i++)
        {
            if(instObjs[i]!=null) instObjs[i].SetActive(false);     
        }
        instObjs.Clear();
        for (int i = 0; i < Day.ActiveDay.setsOfExercises.Count; i++) 
        {
            GameObject obj = Instantiate(upper, content);
            instObjs.Add(obj);
            obj.GetComponent<UpperCard>().setOfExercises = Day.ActiveDay.setsOfExercises[i];
            obj.GetComponent<UpperCard>().SetActive();
        }
    }
    IEnumerator WaitingDay()
    {
        while(Day.ActiveDay.setsOfExercises == null)
        {
            yield return new WaitForSeconds(0.3f);
        }
        CreateCards();
    }
}
