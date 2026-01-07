using UnityEngine;

public class ViewProgram : MonoBehaviour
{
    [SerializeField] GameObject programWindow;
    [SerializeField] Transform content;
    [SerializeField] GameObject upperCard;
    [Header("Buttons")]
    [SerializeField] GameObject[] buttons;
    Day day;
    private void Awake()
    {
        for (int i = 0; i < buttons.Length; i++) { buttons[i].GetComponent<DoubleClickEvent>().DoubleClick += (s, e) => OpenProgram(e); }
    }
    public void OpenProgram(string name)
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
    }
    void CreateUpperCards(byte dayNum)
    {
        Debug.Log($"День номер №{dayNum} открыт");
        day = Week.week.Days[dayNum];
        Debug.Log($"Количество сетов - {day.setsOfExercises.Count}");
        for (int i = 0; i < day.setsOfExercises.Count; i++)
        {
            var inst = Instantiate(upperCard, content);
            Debug.Log("Upper создан");
            inst.GetComponent<UpperCard>().setOfExercises = day.setsOfExercises[i];
            inst.GetComponent<UpperCard>().SetActive();
        }
    }
}
