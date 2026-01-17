using UnityEngine;

public class OpenStartTrening : MonoBehaviour
{
    [SerializeField] GameObject upper;
    [SerializeField] Transform content;
    private delegate void Void();
    private static event Void UpdateActiveDayCard;
    private void OnEnable()
    {
        CreateCards();
        UpdateActiveDayCard += UpdateCards;
    }
    private void OnDisable()
    {
        UpdateActiveDayCard -= UpdateCards;
    }
    public static void UpdateActiveDayCards()
    {
        UpdateActiveDayCard?.Invoke();
    }
    private void UpdateCards()
    {
        gameObject.SetActive(false);   
        gameObject.SetActive(true);
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
