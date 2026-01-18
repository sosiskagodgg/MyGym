using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DayMono : MonoBehaviour
{
    [SerializeField] GameObject refreshWindow;
    public void RefreshDay()
    {
        Day.RefreshActiveDay();
        refreshWindow.SetActive(false);
        refreshWindow.SetActive(true);
    }
}

[System.Serializable]
public class Day
{
    #region Параметры и конструкторы
    public byte num;
    public string name;
    public string programName;
    public List<SetOfExercises> setsOfExercises;
    public Day() { }
    public Day(byte num, string name, List<SetOfExercises> setsOfExercises)
    {
        this.num = num;
        this.name = name;
        this.setsOfExercises = setsOfExercises;
    }


    #endregion
    #region Загрузка - Сохранение - Обновление
    public void UpdateSetOfExercises(SetOfExercises setOfExercises)
    {
        byte i = (byte)setsOfExercises.FindIndex(set => set.id == setOfExercises.id);
        setsOfExercises[i] = setOfExercises;
        Week.SaveDay(this);
    }
    public void AddSetOfExercises(SetOfExercises setOfExercises)
    {
        setsOfExercises.Add(setOfExercises);
        Week.SaveDay(this);
    }

    #region Для загрузки текущего дня
    public static string path { get; private set; }
    public static Day ActiveDay
    {
        get
        {
            path ??= DataPath.Path() + "/DayData.json";
            // Проверяем, изменился ли день недели
            DayOfWeek currentDayOfWeek = DateTime.Now.DayOfWeek;
            int dayIndex = (int)currentDayOfWeek;
            dayIndex = currentDayOfWeek == DayOfWeek.Sunday ? 6 : ((int)currentDayOfWeek - 1);
            if (_lastCheckedDay != currentDayOfWeek || _activeDay == null)
            {
                _lastCheckedDay = currentDayOfWeek;

                if (Week.week != null && Week.week.Days != null && dayIndex < Week.week.Days.Count)
                {
                    _activeDay = Week.week.Days[dayIndex];
                    File.WriteAllText(path, JsonUtility.ToJson(_activeDay, true));
                    Debug.Log("WriteAllText");
                }
            }

            // Проверка обновления файла (если всё ещё нужно)
            if (File.Exists(path))
            {
                if (File.GetLastWriteTime(path) != updateTime)
                {
                    updateTime = File.GetLastWriteTime(path);
                    _activeDay = JsonUtility.FromJson<Day>(File.ReadAllText(path));
                    Debug.Log("ReadAllText");
                    // Обновляем день недели при загрузке из файла
                    _lastCheckedDay = DateTime.Now.DayOfWeek;
                }
                return _activeDay;
            }
            else
            {
                // Обработка случая, когда файл не существует
                return _activeDay ?? new Day(); // или вернуть null/default
            }
        }
        set
        {
            path ??= DataPath.Path() + "/DayData.json";
            _activeDay = value;
            
            // Сохраняем в файл, если нужно
            if (path != null)
            {
                File.WriteAllText(path, JsonUtility.ToJson(value,true));
                Debug.Log("WriteAllText");
            }
            updateTime = File.GetLastWriteTime(path);
        }
    }
    public static void RefreshActiveDay()
    {
        ActiveDay = Week.week.Days.Find(d => d.num == ActiveDay.num);
    }
    // Добавляем приватные поля для отслеживания дня недели
    private static DayOfWeek? _lastCheckedDay = null;
    private static Day _activeDay;
    private static DateTime updateTime;

    #endregion



    #endregion
    #region Соритровка
    public void Sort()
    {
        for (int i = 0; i < setsOfExercises.Count; i++)
        {
            setsOfExercises[i].id = (byte)i;
        }
    }
    #endregion


}
