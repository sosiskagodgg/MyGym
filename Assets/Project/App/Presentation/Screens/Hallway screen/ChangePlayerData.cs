using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ChangePlayerData : MonoBehaviour
{
    [SerializeField] SupabaseExerciseManager supabaseExerciseManager;
    [SerializeField] NumberSelectorUI weightNumberSelectorUI;
    [SerializeField] NumberSelectorUI ageNumberSelectorUI;
    [SerializeField] NumberSelectorUI percentageOfFatNumberSelectorUI;
    [SerializeField] NumberSelectorUI experienceNumberSelectorUI;

    public void Save()
    {
        
        Player player = new Player() 
        {
            weight = Convert.ToInt16(weightNumberSelectorUI.value),
            age = Convert.ToInt16(ageNumberSelectorUI.value),
            percentageOfFat = Convert.ToInt16(percentageOfFatNumberSelectorUI.value),
            experience = Convert.ToInt16(experienceNumberSelectorUI.value),
            treningParametrs = Player.player.treningParametrs ?? new TreningParametrs() { gymOrStreet =GymOrStreet.Gym,goal = Goal.GainingMuscleMass}
        };
        if(player != Player.player)
        {
            Player.player = player;
            supabaseExerciseManager.SaveUserMetrics(DataManager.id, player.weight, player.percentageOfFat, player.age, player.experience);
        }
   }
}
