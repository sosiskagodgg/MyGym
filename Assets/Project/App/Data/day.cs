using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class DayMono : MonoBehaviour
{
    public void RefreshDay()
    {
        Day.RefreshActiveDay();
        OpenStartTrening.UpdateActiveDayCards();
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

    public Day(string v)
    {
        this.name = v;
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
        if (setOfExercises.exercises.Count <= 0){ return; }
        setsOfExercises.Add(setOfExercises);
        Week.SaveDay(this);
    }

    #region Для загрузки текущего дня
    private static bool _isLoading = false;

    public static Day ActiveDay
    {
        get
        {
            // Если уже загружено - возвращаем
            if (_activeDay != null)
                return _activeDay;

            // Если идет загрузка - возвращаем временный объект
            if (_isLoading)
                return new Day("Загрузка...");

            // Начинаем загрузку
            StartLoadingActiveDay();
            return new Day("Загрузка...");
        }
        set
        {
            _activeDay = value;
            // Сохраняем на сервер
            if (value != null)
            {
                DataManager.SEM.SaveTrainingDayWithSets(DataManager.id, value.name, value.CreateTrainingSet());
            }
        }
    }

    private static void StartLoadingActiveDay()
    {
        _isLoading = true;

        DayOfWeek currentDayOfWeek = DateTime.Now.DayOfWeek;
        int dayIndex = (int)currentDayOfWeek;
        dayIndex = currentDayOfWeek == DayOfWeek.Sunday ? 6 : ((int)currentDayOfWeek - 1);
        string dayName = GetDayOfWeek(dayIndex);

        // Сначала проверяем есть ли на сервере
        DataManager.SEM.CheckDayExists(DataManager.id, dayName, (exists) =>
        {
            if (exists)
            {
                // Загружаем с сервера
                DataManager.SEM.LoadTrainingDayWithSets(DataManager.id, dayName, (sets) =>
                {
                    _activeDay = CreateDayByTrainingSet(sets, dayName);
                    _isLoading = false;
                    Debug.Log($"Активный день '{dayName}' загружен с сервера");
                });
            }
            else
            {
                // Берем из недели или создаем новый
                if (Week.week != null && Week.week.Days != null && dayIndex < Week.week.Days.Count)
                {
                    _activeDay = DeepClone(Week.week.Days[dayIndex]);
                }
                else
                {
                    _activeDay = new Day(dayName);
                }
                _isLoading = false;
                Debug.Log($"Создан новый активный день '{dayName}'");
            }
        });
    }
    public static void RefreshActiveDay()
    {
         int i = Week.week.Days.FindIndex(d => d.name == ActiveDay.name);
         Debug.Log($"новый активный день имеет {SetOfExercises.Count(Week.week.Days[i].setsOfExercises)}");
         ActiveDay = Week.week.Days[i];
    }
    // Добавляем приватные поля для отслеживания дня недели
    private static Day _activeDay;

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
    public List<TrainingSet> CreateTrainingSet()
    {
        List<TrainingSet> trainingExercises = new();

        for (int setNum = 0; setNum < setsOfExercises.Count; setNum++)
        {
            // ПРОЙДИСЬ ПО КАЖДОМУ ПОДХОДУ!
            for (int approachIndex = 0; approachIndex < setsOfExercises[setNum].exercises.Count; approachIndex++)
            {
                Exercise exercise = setsOfExercises[setNum].exercises[approachIndex];
                if (exercise == null) continue;

                float workWeight = 0;
                int repetitions = 0;

                if (exercise.specificParameters is StrengthTraining strength)
                {
                    workWeight = strength.workWeight;
                    repetitions = strength.repetitions;
                }
                else if (exercise.specificParameters is Calisthenics calisthenics)
                {
                    repetitions = calisthenics.replications;
                }

                // КАЖДЫЙ подход - отдельная запись!
                trainingExercises.Add(new TrainingSet(
                    approachIndex,  // exercise_id = номер подхода (0, 1, 2...)
                    exercise.name,
                    setNum,
                    workWeight,
                    repetitions
                ));
            }
        }
        return trainingExercises;
    }

    public static Day CreateDayByTrainingSet(List<TrainingSet> trainingSets, string name)
    {
        // Группируем по set_number
        var groupedBySet = trainingSets.GroupBy(t => t.set_number);

        List<SetOfExercises> setOfExercises = new List<SetOfExercises>();

        foreach (var setGroup in groupedBySet.OrderBy(g => g.Key))
        {
            int setNumber = setGroup.Key;

            // Убедимся что есть место в списке
            while (setOfExercises.Count <= setNumber)
            {
                setOfExercises.Add(new SetOfExercises()
                {
                    id = (byte)setNumber, // ID сета = его номер
                    exercises = new List<Exercise>()
                });
            }

            // ДЛЯ КАЖДОГО ПОДХОДА создаем упражнение
            foreach (var trainingSet in setGroup.OrderBy(t => t.exercise_id))
            {
                Exercise exercise = ExerciseManager.GetExercisesByName(trainingSet.exercise_name);
                if (exercise == null) continue;

                // Клонируем упражнение для каждого подхода!
                Exercise clonedExercise = ExerciseManager.DeepClone(exercise);

                // Присваиваем ID упражнению из базы
                clonedExercise.id = (byte)trainingSet.exercise_id; // Используем exercise_id из TrainingSet

                if (clonedExercise.specificParameters is StrengthTraining strength)
                {
                    strength.workWeight = trainingSet.working_weight_kg;
                    strength.repetitions = trainingSet.repetitions;
                }
                else if (clonedExercise.specificParameters is Calisthenics calisthenics)
                {
                    calisthenics.replications = trainingSet.repetitions;
                }

                setOfExercises[setNumber].exercises.Add(clonedExercise);
            }
        }

        return new Day() { name = name, setsOfExercises = setOfExercises };
    }

    #endregion
    #region публичные методы
    public static Day DeepClone(Day cloneDay)
    {
        Day day = new Day();
        day.setsOfExercises = new();
        for (int i = 0; i < cloneDay.setsOfExercises.Count; i++)
        {
            day.setsOfExercises.Add(cloneDay.setsOfExercises[i].DeepClone(cloneDay.setsOfExercises[i]));
        }
        day.name = cloneDay.name;
        day.num = cloneDay.num;
        day.programName = cloneDay.programName;
        return day;
    }
    #endregion
    public static string GetDayOfWeek(int dayNumber)
    {
        switch (dayNumber)
        {
            case 0: return "Понедельник";
            case 1: return "Вторник";
            case 2: return "Среда";
            case 3: return "Четверг";
            case 4: return "Пятница";
            case 5: return "Суббота";
            case 6: return "Воскресенье";
            default:
                throw new ArgumentOutOfRangeException(nameof(dayNumber),
                    "День недели должен быть от 0 до 6. Получено: " + dayNumber);
        }
    }
}
