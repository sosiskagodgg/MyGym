using System;
using TMPro;
using UnityEngine;

public class UpperCard : MonoBehaviour
{
    public SetOfExercises setOfExercises;
    [SerializeField] Transform upperCard;
    [SerializeField] GameObject lowerCard;
    
    void CreateLowerCard()
    {
        if(setOfExercises==null) throw new NotImplementedException($"setOfExercises == null");
        for(int i = 0;setOfExercises.exercises.Count > i; i++)
        {
            var inst = Instantiate(lowerCard, upperCard);
            inst.GetComponent<LowerCard>().exercise = setOfExercises.exercises[i];
            inst.GetComponentInChildren<TextMeshProUGUI>().text = inst.GetComponent<LowerCard>().exercise.specificParameters.ToString();
        }
    }
    public void SetActive()
    {
        CreateLowerCard();
        GetComponentInChildren<TextMeshProUGUI>().text = setOfExercises.ToString();
    }
    private void OnDisable()
    {
        Destroy(gameObject);
    }
}
