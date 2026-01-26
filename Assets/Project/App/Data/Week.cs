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

    #region Загрузка - Сохранение
    public static Week week
    {
        get
        {
            if(_cachedWeek == null)
            {
                _cachedWeek = EmptyWeek;
                try
                {
                   DataManager.SEM.LoadWeeklyTrainingSchedule(DataManager.id, (weekly) =>
            {
               _cachedWeek.Days[0] = Day.CreateDayByTrainingSet(weekly.days[0].exercises, "Понедельник", weekly.days[0].notes);
               _cachedWeek.Days[1] = Day.CreateDayByTrainingSet(weekly.days[1].exercises, "Вторник", weekly.days[1].notes);
                _cachedWeek.Days[2] = Day.CreateDayByTrainingSet(weekly.days[2].exercises, "Среда", weekly.days[2].notes);
               _cachedWeek.Days[3] = Day.CreateDayByTrainingSet(weekly.days[3].exercises, "Четверг", weekly.days[3].notes);
                _cachedWeek.Days[4] = Day.CreateDayByTrainingSet(weekly.days[4].exercises, "Пятница", weekly.days[4].notes);
               _cachedWeek.Days[5] = Day.CreateDayByTrainingSet(weekly.days[5].exercises, "Суббота" , weekly.days[5].notes);
                _cachedWeek.Days[6] = Day.CreateDayByTrainingSet(weekly.days[6].exercises, "Воскресенье", weekly.days[6].notes);
                ViewProgram.UpdateProgramNames();
            });
                }
                catch 
                {

                
                }
            }
            return _cachedWeek;
        }
        set 
        {
            _cachedWeek = value;
            DataManager.SEM.SaveEntireWeek(DataManager.id, new WeeklyTrainingSchedule(DataManager.id,
                new List<TrainingDaySchedule>
                {
        new TrainingDaySchedule("Понедельник",
            value.Days.First(d=>d.name == "Понедельник").CreateTrainingSet(),
            value.Days.First(d=>d.name == "Понедельник").programName),
        new TrainingDaySchedule("Вторник",
            value.Days.First(d=>d.name == "Вторник").CreateTrainingSet(),
            value.Days.First(d=>d.name == "Вторник").programName),
        new TrainingDaySchedule("Среда",
            value.Days.First(d=>d.name == "Среда").CreateTrainingSet(),
            value.Days.First(d=>d.name == "Среда").programName),
        new TrainingDaySchedule("Четверг",
            value.Days.First(d=>d.name == "Четверг").CreateTrainingSet(),
            value.Days.First(d=>d.name == "Четверг").programName),
        new TrainingDaySchedule("Пятница",
            value.Days.First(d=>d.name == "Пятница").CreateTrainingSet(),
            value.Days.First(d=>d.name == "Пятница").programName),
        new TrainingDaySchedule("Суббота",
            value.Days.First(d=>d.name == "Суббота").CreateTrainingSet(),
            value.Days.First(d=>d.name == "Суббота").programName),
        new TrainingDaySchedule("Воскресенье",
            value.Days.First(d=>d.name == "Воскресенье").CreateTrainingSet(),
            value.Days.First(d=>d.name == "Воскресенье").programName)
                }));
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

    private static Week _cachedWeek;
    public static void SaveDay(Day day)
    {
        byte dayNum = (byte)week.Days.FindIndex(d => d.name == day.name);
        week.Days[dayNum] = Day.DeepClone(day);
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
    public SetOfExercises(Exercise exercise, int quantity = 1, bool isSetId = true)
    {
        if (exercise == null) { return; }
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
                newSetOfExercises.id = setOfExercises.id;
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



    #endregion
}

