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
            experience = (byte)experienceScrollBar.Value
        };
        player.SavePlayer();
    }
    void SetScrollBarPos()
    {
        Player player = new Player().LoadPlayer();
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
