using UnityEngine;

public class OpenStartTrening : MonoBehaviour
{
    [SerializeField] GameObject upper;
    [SerializeField] Transform content;
    private void OnEnable()
    {
        CreateCards();
    }
    void CreateCards()
    {
        Debug.Log($"создаем {Day.ActiveDay.setsOfExercises.Count} дней");
        for (int i = 0; i < Day.ActiveDay.setsOfExercises.Count; i++) 
        {
            GameObject obj = Instantiate(upper, content);
            obj.GetComponent<UpperCard>().setOfExercises = Day.ActiveDay.setsOfExercises[i];
            obj.GetComponent<UpperCard>().SetActive();
        }
    }
}
