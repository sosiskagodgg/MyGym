using UnityEngine;
using System;
public class SetStreetOrGym : MonoBehaviour
{
    Player player;
    public event EventHandler GymOrStreetChange;
    private void Awake()
    {
        player = Player.LoadPlayer();
        CheackDataValid();
        GymOrStreetChange += DebugLog;
    }
    void CheackDataValid() => player.treningParametrs ??= new TreningParametrs();

    public void GymClick()
    {
        player.treningParametrs.gymOrStreet = GymOrStreet.Gym;
        Save();
    }
    public void StreetClick()
    {
        player.treningParametrs.gymOrStreet = GymOrStreet.Street;
        Save();
    }
    void Save()
    {
        player.SavePlayer();
        GymOrStreetChange?.Invoke(this, EventArgs.Empty);
    }
    void DebugLog(object sender, EventArgs e)
    {
        Debug.Log("GymOrStreetChange");
    }
}
