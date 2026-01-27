using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] RectTransform playerRt;
    public Player player;

    [SerializeField] Scrollbar scrollbar;
    [SerializeField] TextMeshProUGUI text;

    [SerializeField] float heightBase;
    [SerializeField] float heightMax;
    [SerializeField] float heightMin;

    [SerializeField] float weightBase;
    [SerializeField] float weightMax;
    [SerializeField] float weightMin;

    [SerializeField] float percentageOfFatBase;
    [SerializeField] float percentageOfFatMax;
    [SerializeField] float percentageOfFatMin;
    [SerializeField] Image percentageOfFatImage;
    [SerializeField] Sprite[] percentageOfFatSprites;

    [SerializeField] float ageBase;
    [SerializeField] float ageMax;
    [SerializeField] float ageMin;
    [SerializeField] Image ageImage;
    [SerializeField] Sprite[] ageSprites;

    [SerializeField] float experienceBase;
    [SerializeField] float experienceMax;
    [SerializeField] float experienceMin;
    [SerializeField] Image experienceImage;
    [SerializeField] Sprite[] experienceSprites;

    [Header("Вывод")]
    [SerializeField] string setingsName;

    public void Initialization()
    {
        player = Player.player;
        if (player == null) { Debug.Log("Игрок не найден");return; }

        player.height = (int)Mathf.Clamp(player.height, heightMin, heightMax);
        player.weight = (int)Mathf.Clamp(player.weight, weightMin, weightMax);
        player.percentageOfFat = (int)Mathf.Clamp(player.percentageOfFat, percentageOfFatMin, percentageOfFatMax); 
        setingsName = "weight";
        SetScrollBarPos();
        OnValueChanged();
        SetScaleY();
        SetScaleX();
        SetPercentageOfFat();
        SetExperience();
    }
    public void SetSetingsName(string name)
    {
        setingsName = name;
        SetScrollBarPos();
        GetDataByScrollBar();

    }

    void SetScaleY()
    {
        float scaleY = 1;
        if (player.height > heightBase)
        {
            scaleY = player.height / heightBase;
        }
        else
        {
            scaleY = player.height / heightBase;
        }
        playerRt.localScale = new Vector2(playerRt.localScale.x, scaleY);
    }
    void SetScaleX()
    {
        float scaleX = 1;
        if (player.weight > weightBase)
        {
            scaleX =  player.weight / weightBase;
        }
        else
        {
            scaleX =  player.weight / weightBase;
        }
        playerRt.localScale = new Vector2(scaleX, playerRt.localScale.y);
    }
    void SetPercentageOfFat()
    {
        percentageOfFatImage.sprite = (int)player.percentageOfFat switch
        {
            <= 15 => percentageOfFatSprites[0],
            <= 20 => percentageOfFatSprites[1],
            <= 25 => percentageOfFatSprites[2],
            <= 30 => percentageOfFatSprites[3],
            > 30 => percentageOfFatSprites[4]
        };
    }
    void SetExperience()
    {
        experienceImage.sprite = (int)player.experience switch
        {
            <= 6 => experienceSprites[0],
            <= 12 => experienceSprites[1],
            <= 18 => experienceSprites[2],
            <= 24 => experienceSprites[3],
            > 24 => experienceSprites[4]
        };
    }
    void SetAge()
    {
        ageImage.sprite = player.age switch
        {
            <= 20 => ageSprites[0],
            >= 20 and <= 25=> ageSprites[1],
            <= 35 => ageSprites[2],
            <= 40 => ageSprites[3],
            <= 45 => ageSprites[4],
            <= 50 => ageSprites[5],
            > 50 => ageSprites[6]
        };
    }



    void SetScrollBarPos()
    {
        float scrollPos=0;
        if (setingsName == "height") scrollPos = Mathf.InverseLerp(heightMin, heightMax, player.height);
        else if (setingsName == "weight") scrollPos = Mathf.InverseLerp(weightMin, weightMax, player.weight);
        else if (setingsName == "percentageOfFat") scrollPos = Mathf.InverseLerp(percentageOfFatMin, percentageOfFatMax,player.percentageOfFat);
        else if (setingsName == "experience") scrollPos = Mathf.InverseLerp(experienceMin, experienceMax, player.experience);
        else if (setingsName == "age") scrollPos = Mathf.InverseLerp(ageMin, ageMax, player.age);
        scrollbar.value = scrollPos;
    }
    void GetDataByScrollBar()
    {
        if (setingsName == "height") { player.height = (int)Mathf.Lerp(heightMin, heightMax, scrollbar.value); text.text = $"Рост {player.height.ToString("F1")} см"; }
        else if (setingsName == "weight") { player.weight = Mathf.Lerp(weightMin, weightMax, scrollbar.value); text.text = $"Вес {player.weight.ToString("F1")} кг"; }
        else if (setingsName == "percentageOfFat"){player.percentageOfFat = Mathf.Lerp(percentageOfFatMin, percentageOfFatMax, scrollbar.value); text.text = $"Процент жира {player.percentageOfFat.ToString("F1")}%"; }
        else if (setingsName == "experience") { player.experience = (int)Mathf.Lerp(experienceMin, experienceMax, scrollbar.value); text.text = $"Опыт тренировок {player.experience} месяцев"; }
        else if (setingsName == "age") { player.age = (int)Mathf.Lerp(ageMin, ageMax, scrollbar.value);text.text = $"Возраст {player.age} лет"; }
}
    public void OnValueChanged()
    {
        GetDataByScrollBar();

        if (setingsName == "height") SetScaleY();
        else if (setingsName == "weight") SetScaleX();
        else if (setingsName == "percentageOfFat") SetPercentageOfFat();
        else if (setingsName == "experience") SetExperience();
        else if (setingsName =="age")SetAge();
    }



}
