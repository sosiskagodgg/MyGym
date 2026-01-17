using UnityEngine;
using System;
using TMPro;
public class SetGoal : MonoBehaviour
{
    Player player;
    [SerializeField] GameObject description;
    static TextMeshProUGUI text;
    public void SetGoals(string goal)
    {
        player = Player.LoadPlayer();
        switch (goal)
        {
            case "WeightLoss": player.treningParametrs.goal = Goal.WeightLoss; break;
            case "GainingMuscleMass": player.treningParametrs.goal = Goal.GainingMuscleMass;break;
            case "IncreasedStrength": player.treningParametrs.goal= Goal.IncreasedStrength; break;
            case "IncreasedEndurance": player.treningParametrs.goal = Goal.IncreasedEndurance;break;
            case "Recomposition": player.treningParametrs.goal = Goal.Recomposition;break;
            case "Flexibility": player.treningParametrs.goal = Goal.Flexibility;break;
            case "Recovery": player.treningParametrs.goal = Goal.Recovery;break;
            default: Debug.Log($"Ошибка цель {goal} не существует");break;
        }
        Player.player = player;
    }

    public void OpenDiscription(string goal) 
    {
        description.SetActive(!description.activeInHierarchy);
        text ??= description.GetComponentInChildren<TextMeshProUGUI>();
        text.text = TreningParametrs.GetDescription(goal);
    }


}
