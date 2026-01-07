using System;
using System.IO;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class Player
{
    #region Параметры и конструкторы
    public string updateTime;
    public string name;
    public byte weight, height, age, percentageOfFat, experience;
    public TreningParametrs treningParametrs;
    #endregion

    #region Загрузка - Сохранение
    static Player lastPlayer;
    static string lastUpdateTime;
    public static string path => DataPath.Path() + "/PlayerData.json";
    public void SavePlayer()
    {
        lastUpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        updateTime = lastUpdateTime;

        File.WriteAllText(path, JsonUtility.ToJson(this, true));

    }
    public static Player LoadPlayer()
    {
        if (!File.Exists(path))
        {
            lastPlayer = new Player();
            Debug.Log($"Файла {path} не существует ");
            return lastPlayer;
        }
        string secondLine = File.ReadLines(path).ElementAt(1);
        string fileUpdateTime = ExtractValue(secondLine, "updateTime");
        if (lastUpdateTime != fileUpdateTime)
        {
            lastPlayer = JsonUtility.FromJson<Player>(File.ReadAllText(path));
            Debug.Log("Игрок был загружен из файла");
        }
        else { Debug.Log(lastUpdateTime + " == " + fileUpdateTime); }

        return lastPlayer;
    }
    private static string ExtractValue(string line, string fieldName)
    {
        string pattern = $"\"{fieldName}\": \"";
        int start = line.IndexOf(pattern);

        if (start == -1)
        {
            pattern = $"\"{fieldName}\":\"";
            start = line.IndexOf(pattern);
        }

        if (start == -1)
        {
            Debug.LogError($"Не найден паттерн: {pattern} в строке: {line}");
            return null;
        }

        start += pattern.Length;
        int end = line.IndexOf("\"", start);

        if (end == -1)
        {
            Debug.LogError($"Не найдена закрывающая кавычка в: {line}");
            return null;
        }

        return line.Substring(start, end - start);
    }

    #endregion
}



public class DataPath : MonoBehaviour
{
    static public string Path() => $"{Application.persistentDataPath}";
}
