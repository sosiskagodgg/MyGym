using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
[System.Serializable]
public class Week
{
    public List<Day> Days = new List<Day>();
    public static string path = DataPath.Path() + "/WeekData.Json";

    #region Загрузка - Сохранение
    public static Week week
    {
        get
        {
            var fileTime = File.GetLastWriteTime(path);
            if (_cachedWeek == null || _lastLoadTime < fileTime)
            {
                _cachedWeek = Load();
                Debug.Log("кэш обновлен");
                _lastLoadTime = DateTime.Now;
            }
            return _cachedWeek;
        }
    }
    public static Week EmptyWeek = new Week()
    {
        Days = new List<Day>()
    {
        new Day(0, "Понедельник", new List<SetOfExercises>()),
        new Day(1, "Вторник", new List<SetOfExercises>()),
        new Day(2, "Среда", new List<SetOfExercises>()),
        new Day(3, "Четверг", new List<SetOfExercises>()),
        new Day(4, "Пятница", new List<SetOfExercises>()),
        new Day(5, "Суббота", new List<SetOfExercises>()),
        new Day(6, "Воскресенье", new List<SetOfExercises>())
    }
    };
    private static bool isFileFind;
    private static Week _cachedWeek;
    private static DateTime _lastLoadTime;
    public void SaveWeek()
    {
        Days ??= new List<Day>();
        File.WriteAllText(path, JsonUtility.ToJson(this,true));
    }
    public static void SaveDay(Day day)
    {
        byte dayNum=(byte)week.Days.FindIndex(d => d.num == day.num);
        week.Days[dayNum] = day;
        week.SaveWeek();
    }

    private static Week Load()
    {

        if (!File.Exists(path))
        {
             isFileFind = true; EmptyWeek.SaveWeek(); return EmptyWeek; 
        }
        else
        {
        Week result;
        result = JsonUtility.FromJson<Week>(File.ReadAllText(path));
        return result == null? EmptyWeek : result;
        }

    }
    #endregion


}
[System.Serializable]
public class Day
{
    #region Параметры и конструкторы
    public byte num;
    public string name;
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
    #endregion



}
[System.Serializable]
public class SetOfExercises
{
    #region Параметры и конструкторы
    private static Player player = Player.LoadPlayer();

    public List<Exercise> exercises;
    public SetOfExercises(Exercise exercise, byte quantity,bool isSetId = true)
    {
        exercises = new List<Exercise>();
        for (int i = 0; i < quantity; i++)
        {
            Exercise newExercise = ExerciseManager.DeepClone(exercise);
            newExercise.specificParameters.SetParametrs(player, (byte)i);
            exercises.Add(newExercise);
        }
        if (isSetId) exercises = ExerciseManager.SetId(exercises);
    }
    #endregion
    #region Загрузка - Сохранение - Обновление
    public void UpdateExercise(Day day, Exercise exercise)
    {
        byte i = (byte)exercises.FindIndex(ex => ex.id == exercise.id);
        exercises[i] = exercise;
        day.UpdateSetOfExercises(this);
    }
    #endregion
    #region Работа с id
    public byte id;
    public List<SetOfExercises> SetId(List<SetOfExercises> setOfExercises)
    {
        for (int i = 0; i < setOfExercises.Count; i++)
        {
            setOfExercises[i].id = (byte)i;
        }
        return setOfExercises;
    }
    #endregion
    #region Визуальные методы
    public override string ToString()
    {
        try
        {
            return exercises[0].name;
        }
        catch
        {
            return "Пустой сет";
        }
    }
    #endregion

}

