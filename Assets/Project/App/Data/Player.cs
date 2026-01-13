using System;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEngine.InputSystem.XR.TrackedPoseDriver;
[System.Serializable]
public class Player
{
    #region Параметры и конструкторы

    public string name;
    public byte weight, height, age, percentageOfFat, experience;
    public TreningParametrs treningParametrs;
    #endregion

    #region Загрузка - Сохранение
    static Player lastPlayer;
    public static DateTime updateTime;
    public static string path => DataPath.Path() + "/PlayerData.json";
    public void SavePlayer()
    {
        File.WriteAllText(path, JsonUtility.ToJson(this, true));
        updateTime = DateTime.Now;

    }
    public static Player LoadPlayer()
    {
        if (!File.Exists(path))
        {
            lastPlayer = new Player();
            lastPlayer.SavePlayer();
            Debug.Log($"Файла {path} не существует ");
            return lastPlayer;
        }
        string secondLine = File.ReadLines(path).ElementAt(1);
        if (File.GetLastWriteTime(path) != updateTime)
        {
            lastPlayer = JsonUtility.FromJson<Player>(File.ReadAllText(path));
            Debug.Log("Игрок был загружен из файла");
        }

        return lastPlayer;
    }
    #endregion
}



public class DataPath : MonoBehaviour
{
    static public string Path() => $"{Application.persistentDataPath}";
}
