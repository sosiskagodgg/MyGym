using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        Sort();
        File.WriteAllText(path, JsonUtility.ToJson(this, true));
    }
    public static void SaveDay(Day day)
    {
        byte dayNum = (byte)week.Days.FindIndex(d => d.num == day.num);
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
            return result == null ? EmptyWeek : result;
        }

    }
    #endregion
    #region Сортировка
    public void Sort()
    {
        for (int i = 0; Days.Count > i; i++)
        {
            Days[i].Sort();
        }
        #endregion

    }

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
[System.Serializable]
public class SetOfExercises
{
    #region Параметры и конструкторы

    public List<Exercise> exercises = new List<Exercise>();
    public SetOfExercises(Exercise exercise, byte quantity, bool isSetId = true)
    {
        Player player = Player.LoadPlayer();
        exercises = new List<Exercise>();
        for (int i = 0; i < quantity; i++)
        {
            Exercise newExercise = ExerciseManager.DeepClone(exercise);
            newExercise.specificParameters.SetParametrs(player, (byte)i);
            exercises.Add(newExercise);
        }
        if (isSetId) exercises = ExerciseManager.SetId(exercises);
    }
    public SetOfExercises() { }
    #endregion

    #region Загрузка - Сохранение - Обновление
    public void UpdateExercise(Day day, Exercise exercise)
    {
        byte i = (byte)exercises.FindIndex(ex => ex.id == exercise.id);
        exercises[i] = exercise;
        day.UpdateSetOfExercises(this);
    }
    #endregion

    #region Сортировка
    public byte id;
    public void Sort()
    {
        for (int i = 0; i < exercises.Count; i++) 
        {
            exercises[i].id = (byte)i;
        }
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

    #region Клонирование
    public SetOfExercises DeepClone(SetOfExercises setOfExercises)
    {
        SetOfExercises newSetOfExercises = new SetOfExercises();
        for(int i  = 0; i < exercises.Count; i++)
        {
            try
            {
                newSetOfExercises.exercises.Add(ExerciseManager.DeepClone(exercises[i]));
            }
            catch 
            {
                CreateFile.Test(exercises[i].name);
                CreateFile.Test(ExerciseManager.DeepClone(exercises[i]).name);
            }
        }
        newSetOfExercises.Sort();
        return newSetOfExercises;
    }
    #endregion

    #region Автоматическое создание сетов
    public static List<Exercise> GetExercisesByMuscleGroup(int exercisesCount, MuscleGroup targetMuscleGroup)
    {
        List<Exercise> fullExercises = ExerciseManager.Exercises.Where(exercise =>
        {
            // Находим информацию о целевой мышце в этом упражнении
            var targetMuscleInfo = exercise.muscles
                .FirstOrDefault(m => m.muscleGroup == targetMuscleGroup);

            // Если мышцы нет в упражнении - пропускаем
            if (targetMuscleInfo == null)
                return false;

            // Проверяем, что у целевой мышцы максимальный процент
            float maxPercentage = exercise.muscles.Max(m => m.percentageOfWork);

            // Сравниваем с погрешностью (на случай одинаковых процентов)
            return Mathf.Approximately(targetMuscleInfo.percentageOfWork, maxPercentage) ||
                   targetMuscleInfo.percentageOfWork > maxPercentage - 0.01f;
        })
        .ToList();
        List<Exercise> returnList = new();
        for (int i = 0; i < exercisesCount; i++) 
        {
            if (fullExercises.Count > i)
                returnList.Add(fullExercises[i]);
        }
        return returnList;
    }
    #endregion
}

