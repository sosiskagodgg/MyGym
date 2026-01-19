using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ChangePlayerData : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] ScrollBarUI weightScrollBar;
    [SerializeField] ScrollBarUI heightScrollBar;
    [SerializeField] ScrollBarUI ageScrollBar;
    [SerializeField] ScrollBarUI percentageOfFatScrollBar;
    [SerializeField] ScrollBarUI experienceScrollBar;

    void Save()
    {
        Player player = new Player() 
        {
            name = inputField.text,
            weight = (byte)weightScrollBar.Value,
            height = (byte)heightScrollBar.Value,
            age = (byte)ageScrollBar.Value,
            percentageOfFat = (byte)percentageOfFatScrollBar.Value,
            experience = (byte)experienceScrollBar.Value,
            treningParametrs = Player.player.treningParametrs ?? new TreningParametrs() { gymOrStreet =GymOrStreet.Gym,goal = Goal.GainingMuscleMass}
        };
        Player.player = player;
    }
    void SetScrollBarPos()
    {
        Player player = Player.player;
        inputField.text = player.name;
        weightScrollBar.SetScrollBarPos (player.weight);
        heightScrollBar.SetScrollBarPos (player.height);
        ageScrollBar.SetScrollBarPos (player.age);
        percentageOfFatScrollBar.SetScrollBarPos (player.percentageOfFat);
        experienceScrollBar.SetScrollBarPos (player.experience);
    }
    private void OnDisable()
    {
        Save();
    }
    private void OnEnable()
    {
        SetScrollBarPos();
    }
}
