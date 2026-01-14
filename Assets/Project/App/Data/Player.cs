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

    public static Player player 
    {
        get 
        {
            return LoadPlayer();
        }
        set
        {
            value.SavePlayer();
            _player = value;
        }
    }

    static Player _player;
    public static DateTime updateTime;
    public static string path => DataPath.Path() + "/PlayerData.json";
    public void SavePlayer()
    {
        File.WriteAllText(path, JsonUtility.ToJson(this, true));
        updateTime = File.GetLastWriteTime(path);

    }
    public static Player LoadPlayer()
    {
        if (!File.Exists(path))
        {
            _player = new Player();
            _player.SavePlayer();
            Debug.Log($"Файла {path} не существует ");
            return _player;
        }
        if (File.GetLastWriteTime(path) != updateTime)
        {
            _player = JsonUtility.FromJson<Player>(File.ReadAllText(path));
            Debug.Log("Игрок был загружен из файла");
        }

        return _player;
    }
    #endregion
}



public class DataPath : MonoBehaviour
{
    static public string Path() => $"{Application.persistentDataPath}";
}
