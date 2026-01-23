using System;
using UnityEngine;
using UnityEngine.Analytics;
using System.IO;
[System.Serializable]
public class Player
{
    #region Параметры и конструкторы
    public string name;
    public int weight, height, age, percentageOfFat, experience;
    public TreningParametrs treningParametrs = new TreningParametrs();
    public Gender gender;
    #endregion

    #region Загрузка - Сохранение

    private static Player _cachedPlayer;
    private static DateTime _lastFileUpdate;
    private static readonly object _lock = new object();

    public static Player player
    {
        get
        {
            lock (_lock) // Защита от многопоточности
            {
                return GetPlayer();
            }
        }
        set
        {
            lock (_lock)
            {
                SavePlayer(value);
            }
        }
    }

    public static string path => DataPath.Path() + "/PlayerData.json";

    private static Player GetPlayer()
    {
        // Если файл не существует
        if (!File.Exists(path))
        {
            _cachedPlayer = new Player();
            SavePlayer(_cachedPlayer); // Создаем и сохраняем
            Debug.Log($"Создан новый игрок, файл: {path}");
            return _cachedPlayer;
        }

        // Проверяем, изменился ли файл
        var fileTime = File.GetLastWriteTime(path);
        if (_cachedPlayer == null || _lastFileUpdate < fileTime)
        {
            try
            {
                string json = File.ReadAllText(path);
                _cachedPlayer = JsonUtility.FromJson<Player>(json);
                _lastFileUpdate = fileTime;
                Debug.Log("Игрок загружен из файла");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Ошибка загрузки игрока: {ex.Message}");
                _cachedPlayer = new Player(); // Fallback
            }
        }

        return _cachedPlayer;
    }

    private static void SavePlayer(Player playerToSave)
    {
        try
        {
            string json = JsonUtility.ToJson(playerToSave, true);
            File.WriteAllText(path, json);

            // Обновляем кэш
            _cachedPlayer = playerToSave;
            _lastFileUpdate = File.GetLastWriteTime(path);

            Debug.Log("Игрок сохранен в файл");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка сохранения игрока: {ex.Message}");
        }
    }

    #endregion

    #region Данные по упражнениям

    #endregion
}
public class DataPath : MonoBehaviour
{
    static public string Path() => $"{Application.persistentDataPath}";
}