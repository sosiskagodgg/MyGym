using UnityEngine;

public class OpenStartTrening : MonoBehaviour
{
    [SerializeField] GameObject upper;
    [SerializeField] Transform content;

    [SerializeField] GameObject description;//для описания
    public static GameObject _description;

    private delegate void Void();
    private static event Void UpdateActiveDayCard;
    private void OnEnable()
    {
        _description = description;
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
        content.GetComponentInParent<OnEnableSetY>().enabled = false;
        gameObject.SetActive(false);   
        gameObject.SetActive(true);
        content.GetComponentInParent<OnEnableSetY>().enabled = true  ;
    }
    void CreateCards()
    {
        Debug.Log($"создаем {Day.ActiveDay.setsOfExercises.Count} сетов");
        for (int i = 0; i < Day.ActiveDay.setsOfExercises.Count; i++) 
        {
            GameObject obj = Instantiate(upper, content);
            obj.GetComponent<UpperCard>().setOfExercises = Day.ActiveDay.setsOfExercises[i];
            obj.GetComponent<UpperCard>().SetActive();
        }
    }
}
