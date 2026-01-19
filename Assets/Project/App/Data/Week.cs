using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
        set 
        {
            _cachedWeek = value;
            value.SaveWeek();
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
        new Day(5, "Субота", new List<SetOfExercises>()),
        new Day(6, "Воскресенье", new List<SetOfExercises>())
    }
    };

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
            EmptyWeek.SaveWeek(); return EmptyWeek;
        }
        else
        {
            Week result;
            result = JsonUtility.FromJson<Week>(File.ReadAllText(path));
            Debug.Log("неделя загруженна из файла");
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
    public void SetParametrs()
    {
        Week setWeek = Week.week;
        for (int i1 = 0; setWeek.Days.Count > i1; i1++)
        {
            for (int i2 = 0; setWeek.Days[i1].setsOfExercises.Count > i2; i2++)
            {
                for (int i3 = 0; setWeek.Days[i1].setsOfExercises[i2].exercises.Count > i3; i3++)
                {
                    setWeek.Days[i1].setsOfExercises[i2].exercises[i3].specificParameters.SetParametrs(Player.player);
                }
            }
        }
        Week.week = setWeek;
    }
}
[System.Serializable]
public class SetOfExercises
{
    #region Параметры и конструкторы

    public List<Exercise> exercises = new List<Exercise>();
    public SetOfExercises(Exercise exercise, byte quantity, bool isSetId = true)
    {
        Player player = Player.player;
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

    public static int Count(List<SetOfExercises> setsOfExercises)
    {
        int count = 0;
        for (int i = 0; i < setsOfExercises.Count; i++)
        {
            count += setsOfExercises[i].exercises.Count;
        }
        return count;
    }
    #endregion

    #region Клонирование
    public SetOfExercises DeepClone(SetOfExercises setOfExercises)
    {
        SetOfExercises newSetOfExercises = new SetOfExercises();
        for (int i = 0; i < exercises.Count; i++)
        {
            try
            {
                newSetOfExercises.exercises.Add(ExerciseManager.DeepClone(exercises[i]));
            }
            catch
            {

            }
        }
        newSetOfExercises.Sort();
        return newSetOfExercises;
    }
    #endregion

    #region Автоматическое создание сетов
    public static List<Exercise> GetExercisesByMuscleGroup( MuscleGroup targetMuscleGroup,int exercisesCount = 0)
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
        if (exercisesCount == 0) exercisesCount = fullExercises.Count;
        for (int i = 0; i < exercisesCount; i++)
        {
            if (fullExercises.Count > i)
                returnList.Add(fullExercises[i]);
        }
        return returnList;
    }

    public static List<SetOfExercises> GetExercisesByMuscleWeekWA(Muscle muscle,int weekWA,StringBuilder debugString = null)
    {
        // 1. Получаем упражнения, отсортированные по приоритету (1 - самый высокий)
        List<Exercise> listExercises = new();
        if (Player.player.treningParametrs.goal==Goal.GainingMuscleMass)
        {
            listExercises = ExerciseManager
            .GetExercisesByMuscle(muscle)
            .OrderBy(ex => ex.priority)  // 1 → 2 → 3
            .ToList();
        }
        else if (Player.player.treningParametrs.goal == Goal.IncreasedStrength)
        {
                listExercises = ExerciseManager
                .GetExercisesByMuscle(muscle)
                .Where(ex => ExerciseManager.powerliftingExercises.Contains(ex.name))
                .OrderBy(ex => ex.priority)
                .ToList();
            for (int i = 0; i < listExercises.Count; i++) 
            {
                listExercises[i].priority = ExerciseManager.powerliftingExercises.FindIndex(ex => ex == listExercises[i].name);
            }
        }

            List<SetOfExercises> setOfExercises = new();

        // 2. Если нет упражнений - возвращаем пустой список
        if (listExercises.Count == 0)
        {
            debugString?.AppendLine($"      Нет упражнений для мышцы: {muscle.name}");
            return setOfExercises;
        }

        debugString?.AppendLine($"      Для мышцы '{muscle.name}' найдено {listExercises.Count} упражнений");
        debugString?.AppendLine($"      Необходимо распределить {weekWA} подходов за неделю");

        // 3. Простой алгоритм распределения
        int remainingWA = weekWA;
        int exerciseIndex = 0;

        while (remainingWA > 0)
        {
            // Определяем сколько подходов дать на этом шаге (макс 4)
            int setsForThisStep = Math.Min(4, remainingWA);

            // Берем упражнение (циклически по кругу, начиная с приоритетных)
            Exercise currentExercise = listExercises[exerciseIndex % listExercises.Count];

            // Создаем сет упражнений
            setOfExercises.Add(new SetOfExercises(
                currentExercise,
                (byte)setsForThisStep
            ));

            debugString?.AppendLine(
                $"            Добавлен сет: '{currentExercise.name}' " +
                $"(приоритет {currentExercise.priority}) - {setsForThisStep} подходов");

            // Уменьшаем оставшиеся подходы
            remainingWA -= setsForThisStep;

            // Переходим к следующему упражнению
            exerciseIndex++;

            // Если прошли все доступные упражнения и еще есть подходы,
            // начинаем с начала (дублируем упражнения)
            if (exerciseIndex >= listExercises.Count && remainingWA > 0)
            {
                debugString?.AppendLine("  Упражнения закончились, начинаем дублирование...");
            }
        }

        debugString?.AppendLine(
            $"      Итого создано {setOfExercises.Count} сетов, " +
            $"всего {setOfExercises.Sum(s => s.exercises.Count)} упражнений");

        return setOfExercises.OrderBy(ex => ex.exercises[0].priority).ToList();
    }

    #endregion
}

