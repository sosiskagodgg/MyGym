using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;


public class CreateProgram : MonoBehaviour
{
    #region Методы для юнити
    #region Данные из вне
    [SerializeField] GameObject daySelectorUI;
    [SerializeField] GameObject difficlitySelectorUI;
    [SerializeField] GameObject caloriesSelectorUI;
    [SerializeField] GameObject timeCardioSelectorUI;
    NumberSelectorUI daySelector;
    NumberSelectorUI difficlitySelector;
    NumberSelectorUI caloriesSelector;
    NumberSelectorUI timeCardioSelector;
    static int dayCount = 0;
    static float difficlity = 0;
    static int caloriesCount = 0;
    static int timeTreningCount = 0; 

    [SerializeField] ViewProgram ViewProgram;
    #endregion
    #region Константы
    private const float TimeOnApproach = 3f;
    private const float OneMinutOutHour = 0.016f;
    private const float METOnApproach = 2.5f;
    #endregion
    public void CreateTrening()
    {
        InstantiateTreningType();

        SetDifficlity(ref difficlity);

        StringBuilder stringBuilder = new StringBuilder();
        CreateStrengthTraining(difficlity, dayCount, stringBuilder);
        Debug.Log(stringBuilder.ToString());



        ViewProgram.UpdateProgramNames();

    }

    private void InstantiateTreningType()
    {
        daySelector = this.daySelectorUI.GetComponentInChildren<NumberSelectorUI>();
        difficlitySelector = this.difficlitySelectorUI.GetComponentInChildren<NumberSelectorUI>();
        caloriesSelector = this.caloriesSelectorUI.GetComponentInChildren<NumberSelectorUI>();
        timeCardioSelector= this.timeCardioSelectorUI.GetComponentInChildren<NumberSelectorUI>();
        dayCount = Convert.ToInt32(daySelector.value);
        difficlity = Convert.ToInt32(difficlitySelector.value)/100;
        if(Player.player.treningParametrs.goal==Goal.WeightLoss|| Player.player.treningParametrs.goal == Goal.Recomposition) caloriesCount = Convert.ToInt32(caloriesSelector.value);
        timeTreningCount = Convert.ToInt32(timeCardioSelector.value);
    }
    private void OnEnable()
    {
        if(Player.player.treningParametrs.goal == Goal.WeightLoss|| Player.player.treningParametrs.goal == Goal.Recomposition)
        {
            caloriesSelectorUI.SetActive(true);
        }
        else
        {
            caloriesSelectorUI.SetActive(false);
        }
    }
    private void SetDifficlity(ref float difficlity)
    {
        if (Player.player.treningParametrs.goal == Goal.WeightLoss) difficlity *= ExerciseManager.Coefficient.WeightLossVolumeCoefficient;
    }
    #endregion

    #region Распределение нагрузки на неделю

    #endregion

    #region Распределения
    StringBuilder DebugStringBilder;
    private static List<MuscleGroup>  DistributeMuscleGroup(List<MuscleGroup> muscleGroups,int weekWA,StringBuilder DB=null)
	{
        DB?.AppendLine($"Распределяем {weekWA} weekWA");
		float summa = 0;
		for (int i = 0; i < muscleGroups.Count; i++)
		{
			summa += muscleGroups[i].burden.importancePercentage; 
        }
        // узнали сумму процентов
		

		for(int i = 0;i < muscleGroups.Count;i++) 
		{
			muscleGroups[i].burden.importancePercentage = (muscleGroups[i].burden.importancePercentage / summa) * 100;
			//Новый процент = (Текущий процент / Общая сумма процентов) × 100

			muscleGroups[i].burden.workingApproaches = weekWA * (muscleGroups[i].burden.importancePercentage / 100);
			//новое количество рабочих подходов = количество подходов * (процент работы / 100)
            DB?.AppendLine($" {muscleGroups[i].name} - процент {muscleGroups[i].burden.importancePercentage},WeekWA - {muscleGroups[i].burden.workingApproaches}");
        }
        return muscleGroups;
    }
    public static List<SetOfExercises> DistributeExercises(List<Muscle> muscles, List<MuscleGroup> muscleGroups, int weekWA, StringBuilder DB = null)
    {
        // 1. Распределили подходы между группами
        muscleGroups = DistributeMuscleGroup(muscleGroups, weekWA,DB);

        DB?.AppendLine();
        DB?.AppendLine("Распределение подходов внутри групп мышц:");

        List<SetOfExercises> setsOfExercises = new();

        // 2. Для каждой группы отдельно
        foreach (var group in muscleGroups)
        {
            DB?.AppendLine($"--- Группа: {group.name} (имеет {group.burden.workingApproaches} подходов) ---");

            // Найди мышцы этой группы
            var musclesInThisGroup = new List<Muscle>();
            foreach (var muscle in muscles)
            {
                if (muscle.muscleGroup != null && muscle.muscleGroup.name == group.name)
                {
                    musclesInThisGroup.Add(muscle);
                }
            }

            // Распредели подходы ЭТОЙ группы между мышцами ЭТОЙ группы
            float groupApproaches = group.burden.workingApproaches;

            // Временно сохрани проценты мышц
            Dictionary<Muscle, float> savedPercentages = new Dictionary<Muscle, float>();
            foreach (var muscle in musclesInThisGroup)
            {
                savedPercentages[muscle] = muscle.burden.importancePercentage;
            }

            // Распредели подходы группы между ее мышцами
            DistributeMuscleForGroup(musclesInThisGroup, groupApproaches, DB);

            // Создай упражнения
            foreach (var muscle in musclesInThisGroup)
            {
                int approaches = (int)Math.Round(muscle.burden.workingApproaches);
                if (approaches <= 0) continue;

                var newSet = SetOfExercises.GetExercisesByMuscleWeekWA(muscle, approaches, DB);
                setsOfExercises.AddRange(newSet);
                DB?.AppendLine($"  {muscle.name} - {newSet.Count} упражнений, {SetOfExercises.Count(newSet)} подходов");

                // Верни исходный процент
                if (savedPercentages.ContainsKey(muscle))
                {
                    muscle.burden.importancePercentage = savedPercentages[muscle];
                }
            }
        }
        return setsOfExercises;
    }
    private static List<Muscle> DistributeMuscleForGroup(List<Muscle> muscles, float groupWA, StringBuilder DB = null)
    {
        // Точно такой же код как в DistributeMuscle, но работает только с подходов группы

        if (muscles == null || muscles.Count == 0)
            return muscles;

        // Сумма процентов мышц
        float summa = 0;
        foreach (var muscle in muscles)
        {
            summa += muscle.burden.importancePercentage;
        }

        // Если сумма 0 - все получают поровну
        if (summa == 0)
        {
            float equalWA = groupWA / muscles.Count;
            foreach (var muscle in muscles)
            {
                muscle.burden.workingApproaches = equalWA;
            }
            return muscles;
        }

        // Распределяем groupWA (подходы группы) между мышцами
        foreach (var muscle in muscles)
        {
            float percentage = (muscle.burden.importancePercentage / summa) * 100f;
            muscle.burden.workingApproaches = groupWA * (percentage / 100f);

            DB?.AppendLine($"  {muscle.name} - {percentage:F2}%, WA - {muscle.burden.workingApproaches:F2}");
        }
        return muscles;
    }

    #endregion

    #region Основной метод создания тренеровки


    public static void CreateStrengthTraining(float intensity, int daysCount, StringBuilder DB = null)
    {
        intensity *= ExerciseManager.Coefficient.VolumeTolerance;
        Week.week = Week.EmptyWeek;

        if (Player.player.treningParametrs.goal == Goal.IncreasedStrength) intensity *= 0.6f;
        if (Player.player.treningParametrs.goal == Goal.Recomposition) intensity *= 0.75f;

        DB?.AppendLine(Player.player.treningParametrs.goal.ToString());

        // СОХРАНИМ ОРИГИНАЛЬНЫЕ ЗНАЧЕНИЯ (это КАЖДАЯ тренировка должна сжечь столько)
        int targetCaloriesPerWorkout = caloriesCount; // калорий за ОДНУ тренировку
        int targetTimePerWorkout = timeTreningCount;  // минут на ОДНУ тренировку

        DB?.AppendLine($"=== Цель КАЖДОЙ тренировки: {targetCaloriesPerWorkout} ккал, {targetTimePerWorkout} минут ===");

        int treningNum = 0;
        for (int i = 0; i < Week.week.Days.Count; i++)
        {
            if (GetDaysList(daysCount).Contains(i))
            {
                // КАЖДАЯ тренировка имеет одинаковую цель
                int workoutTimeLeft = targetTimePerWorkout;
                int workoutCaloriesLeft = targetCaloriesPerWorkout; // Всегда начинаем с полной цели

                DB?.AppendLine($"=== День {i + 1}/7 ===");
                DB?.AppendLine($"Цель тренировки: {workoutCaloriesLeft} ккал, {workoutTimeLeft} мин");

                List<Muscle> muscles = CreateSplitForDay(daysCount)[treningNum];
                float maxWA = GetMax(daysCount, intensity, DB);

                List<SetOfExercises> setsOfExercises = DistributeExercises(muscles,
                    MuscleGroup.GetPrimaryMyscleGroups(muscles),
                    (int)maxWA, DB);

                Week.week.Days[i].setsOfExercises = setsOfExercises;
                Week.SaveDay(Week.week.Days[i]);

                // РАСЧЕТ КАЛОРИЙ ДЛЯ ЭТОЙ ТРЕНИРОВКИ
                int strengthCalories = (int)((METOnApproach * Player.player.weight *
                    OneMinutOutHour * TimeOnApproach) * SetOfExercises.Count(setsOfExercises));

                // Уменьшаем доступное время для кардио
                workoutTimeLeft -= (int)(SetOfExercises.Count(setsOfExercises) * TimeOnApproach);

                // Сколько осталось сжечь кардио для этой тренировки
                workoutCaloriesLeft -= strengthCalories;

                DB?.AppendLine($"Силовая часть: {strengthCalories} ккал");
                DB?.AppendLine($"Осталось сжечь кардио: {workoutCaloriesLeft} ккал");
                DB?.AppendLine($"Осталось времени: {workoutTimeLeft} мин");

                // Добавляем кардио, если нужно (ТОЛЬКО для этой тренировки)
                if (Player.player.treningParametrs.goal == Goal.WeightLoss && workoutCaloriesLeft > 0 && workoutTimeLeft > 0)
                {
                    AddCardioInProgram(i, workoutCaloriesLeft, workoutTimeLeft);
                    DB?.AppendLine($"Добавлено кардио: {workoutCaloriesLeft} ккал, {workoutTimeLeft} мин");
                }

                DB?.AppendLine($"Итого на день {i + 1}: {setsOfExercises.Count} упражнений");

                treningNum++;
            }
        }
    }
    public static void CreateFlexibilityTraining()
    {

    }
    private static List<List<Muscle>> CreateSplitForDay(int nums)
    {
        Goal goal = Player.player.treningParametrs.goal;


        if (goal == Goal.IncreasedStrength) switch (nums)
                {
                    // === 1 ДЕНЬ В НЕДЕЛЮ (Минимум времени, максимум нейромышечной стимуляции) ===
                    // Принцип: Тяжелая базовая тренировка, затрагивающая все тело.
                    // Акцент на технике и максимальных весах в основных движениях.
                    case 1:
                        {
                            Week.week.Days[2].programName = "БАЗА (Сила)";
                            return new List<List<Muscle>> {
                    new List<Muscle>
                    {
                        // ОСНОВНЫЕ ДВИЖЕНИЯ (в порядке выполнения)
                        Muscle.GetMuscleByName("Поясница"),            // Становая тяга - ЦЕНТР ТРЕНИРОВКИ
                        Muscle.GetMuscleByName("Квадрицепс"),          // Фронтальные приседания (легче для спины)
                        Muscle.GetMuscleByName("Середина груди"),      // Жим лежа
                        Muscle.GetMuscleByName("Широчайшие"),          // Подтягивания с весом/тяга верхнего блока
                        
                        // ВСПОМОГАТЕЛЬНЫЕ (по необходимости)
                        Muscle.GetMuscleByName("Бицепс бедра"),        // Румынская тяга (легкая, на технику)
                        Muscle.GetMuscleByName("Трицепс"),             // Жим узким хватом (сила жима)
                        Muscle.GetMuscleByName("Трапеции"),            // Шраги (для становой)
                        Muscle.GetMuscleByName("Верх пресса"),         // Пресс как стабилизатор
                    }
                };
                        }

                    // === 2 ДНЯ В НЕДЕЛЮ (Классическое разделение на жим/тягу) ===
                    // Принцип: Один день - жимовые движения, второй - тяговые.
                    case 2:
                        {
                            Week.week.Days[1].programName = "ЖИМЫ (Присед + Жим лежа)";
                            Week.week.Days[3].programName = "ТЯГИ (Становая + Подтягивания)";
                            return new List<List<Muscle>> {
                    // День 1: ЖИМЫ
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Квадрицепс"),          // Приседания со штангой (5x5)
                        Muscle.GetMuscleByName("Середина груди"),      // Жим лежа (5x5)
                        Muscle.GetMuscleByName("Трицепс"),             // Жим узким хватом (3x5)
                        Muscle.GetMuscleByName("Передние дельты"),     // Жим гантелей стоя (3x5)
                        Muscle.GetMuscleByName("Бицепс бедра"),        // Румынская тяга (3x8, легкая)
                    },
                    
                    // День 2: ТЯГИ
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Поясница"),            // Становая тяга (5x3)
                        Muscle.GetMuscleByName("Широчайшие"),          // Подтягивания с весом (5x5)
                        Muscle.GetMuscleByName("Трапеции"),            // Тяга штанги к подбородку (3x5)
                        Muscle.GetMuscleByName("Бицепс"),              // Подъем штанги на бицепс (3x5)
                        Muscle.GetMuscleByName("Задние дельты"),       // Махи в наклоне (3x8)
                        Muscle.GetMuscleByName("Верх пресса"),         // Скручивания с весом
                    }
                };
                        }

                    // === 3 ДНЯ В НЕДЕЛЮ (Силовая программа типа Starting Strength) ===
                    // Принцип: Линейная прогрессия, чередование упражнений.
                    case 3:
                        {
                            Week.week.Days[0].programName = "ПРИСЕД + ЖИМ + ТЯГА";
                            Week.week.Days[2].programName = "ПРИСЕД + ЖИМ ЛЕЖА + СТАНОВАЯ";
                            Week.week.Days[4].programName = "ПРИСЕД + ЖИМ + ТЯГА ГРУДНАЯ";
                            return new List<List<Muscle>> {
                    // День 1 (Понедельник)
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Квадрицепс"),          // Приседания (тяжелые)
                        Muscle.GetMuscleByName("Середина груди"),      // Жим лежа
                        Muscle.GetMuscleByName("Широчайшие"),          // Тяга штанги в наклоне
                        Muscle.GetMuscleByName("Верх пресса"),         // Пресс
                    },
                    
                    // День 2 (Среда)
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Квадрицепс"),          // Приседания (средние)
                        Muscle.GetMuscleByName("Верх груди"),          // Жим на наклонной
                        Muscle.GetMuscleByName("Поясница"),            // Становая тяга
                        Muscle.GetMuscleByName("Трицепс"),             // Отжимания на брусьях с весом
                    },
                    
                    // День 3 (Пятница)
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Квадрицепс"),          // Приседания (тяжелые)
                        Muscle.GetMuscleByName("Середина груди"),      // Жим лежа (прогрессия)
                        Muscle.GetMuscleByName("Широчайшие"),          // Тяга Т-грифа
                        Muscle.GetMuscleByName("Бицепс"),              // Подъем на бицепс
                        Muscle.GetMuscleByName("Верх пресса"),         // Пресс
                    }
                };
                        }

                    // === 4 ДНЯ В НЕДЕЛЮ (Программа 5/3/1 адаптированная) ===
                    // Принцип: Каждый день - одно основное движение + вспомогательные.
                    case 4:
                        {
                            Week.week.Days[0].programName = "ЖИМ ЛЕЖА (5/3/1)";
                            Week.week.Days[1].programName = "СТАНОВАЯ (5/3/1)";
                            Week.week.Days[3].programName = "ЖИМ СТОЯ (5/3/1)";
                            Week.week.Days[4].programName = "ПРИСЕДАНИЯ (5/3/1)";
                            return new List<List<Muscle>> {
                    // День 1: ЖИМ ЛЕЖА
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Середина груди"),      // Жим лежа по программе 5/3/1
                        Muscle.GetMuscleByName("Трицепс"),             // Жим узким хватом (5x10)
                        Muscle.GetMuscleByName("Широчайшие"),          // Тяга верхнего блока (5x10)
                        Muscle.GetMuscleByName("Бицепс"),              // Молотки (3x10)
                    },
                    
                    // День 2: СТАНОВАЯ ТЯГА
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Поясница"),            // Становая тяга по программе 5/3/1
                        Muscle.GetMuscleByName("Бицепс бедра"),        // Румынская тяга (5x10)
                        Muscle.GetMuscleByName("Трапеции"),            // Шраги (5x10)
                        Muscle.GetMuscleByName("Верх пресса"),         // Скручивания с весом (5x15)
                    },
                    
                    // День 3: ЖИМ СТОЯ
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Средние дельты"),      // Жим стоя/армейский жим 5/3/1
                        Muscle.GetMuscleByName("Задние дельты"),       // Махи в наклоне (5x10)
                        Muscle.GetMuscleByName("Широчайшие"),          // Подтягивания (5x макс)
                        Muscle.GetMuscleByName("Верх пресса"),         // Подъем ног в висе (5x15)
                    },
                    
                    // День 4: ПРИСЕДАНИЯ
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Квадрицепс"),          // Приседания по программе 5/3/1
                        Muscle.GetMuscleByName("Бицепс бедра"),        // Сгибания ног (5x10)
                        Muscle.GetMuscleByName("Икры"),                // Подъем на носки (5x15)
                        Muscle.GetMuscleByName("Поясница"),            // Гиперэкстензии (5x10)
                        Muscle.GetMuscleByName("Низ пресса"),          // Планка (3x60 сек)
                    }
                };
                        }

                    // === 5 ДНЕЙ В НЕДЕЛЮ (Продвинутая силовая программа) ===
                    // Принцип: Специализация на основных движениях + работа над слабыми местами.
                    case 5:
                        {
                            Week.week.Days[0].programName = "ПРИСЕД (Тяжелый)";
                            Week.week.Days[1].programName = "ЖИМ ЛЕЖА (Тяжелый)";
                            Week.week.Days[2].programName = "СТАНОВАЯ (Тяжелая)";
                            Week.week.Days[3].programName = "ВСПОМОГАТЕЛЬНЫЙ";
                            Week.week.Days[4].programName = "СЛАБЫЕ ЗВЕНЬЯ";
                            return new List<List<Muscle>> {
                    // День 1: ПРИСЕД
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Квадрицепс"),          // Приседания со штангой (3-5x3-5)
                        Muscle.GetMuscleByName("Квадрицепс"),          // Фронтальные приседания (3x5)
                        Muscle.GetMuscleByName("Бицепс бедра"),        // Румынская тяга (3x8)
                        Muscle.GetMuscleByName("Икры"),                // Подъем на носки в тренажере (5x10)
                    },
                    
                    // День 2: ЖИМ ЛЕЖА
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Середина груди"),      // Жим лежа (3-5x3-5)
                        Muscle.GetMuscleByName("Верх груди"),          // Жим на наклонной (3x5)
                        Muscle.GetMuscleByName("Трицепс"),             // Отжимания на брусьях с весом (3x8)
                        Muscle.GetMuscleByName("Трицепс"),             // Французский жим (3x8)
                    },
                    
                    // День 3: СТАНОВАЯ
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Поясница"),            // Становая тяга (3-5x1-3)
                        Muscle.GetMuscleByName("Широчайшие"),          // Тяга штанги в наклоне (3x5)
                        Muscle.GetMuscleByName("Трапеции"),            // Шраги со штангой (3x8)
                        Muscle.GetMuscleByName("Бицепс"),              // Подъем штанги на бицепс (3x8)
                    },
                    
                    // День 4: ВСПОМОГАТЕЛЬНЫЙ (Объем)
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Квадрицепс"),          // Жим ногами (3x10)
                        Muscle.GetMuscleByName("Середина груди"),      // Жим гантелей лежа (3x10)
                        Muscle.GetMuscleByName("Широчайшие"),          // Тяга верхнего блока широким хватом (3x10)
                        Muscle.GetMuscleByName("Средние дельты"),      // Махи гантелями в стороны (3x12)
                        Muscle.GetMuscleByName("Верх пресса"),         // Скручивания с весом (3x15)
                    },
                    
                    // День 5: СЛАБЫЕ ЗВЕНЬЯ (Коррекция дисбалансов)
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Задние дельты"),       // Махи в наклоне (4x12)
                        Muscle.GetMuscleByName("Бицепс бедра"),        // Сгибания ног лежа (4x12)
                        Muscle.GetMuscleByName("Ромбовидные"),         // Тяга лицо (4x12)
                        Muscle.GetMuscleByName("Предплечья"),          // Сгибания запястий (3x15)
                        Muscle.GetMuscleByName("Низ пресса"),          // Подъем ног в висе (3x15)
                        Muscle.GetMuscleByName("Поясница"),            // Гиперэкстензии (3x15, без веса)
                    }
                };
                        }

                    default:
                        throw new ArgumentException($"Неподдерживаемое количество дней: {nums}");
                }
        else if (goal == Goal.Flexibility) switch (nums)
            {
                // === 1 ДЕНЬ В НЕДЕЛЮ (Полная растяжка всего тела) ===
                case 1:
                    {
                        Week.week.Days[2].programName = "ПОЛНАЯ РАСТЯЖКА ВСЕГО ТЕЛА";
                        return new List<List<Muscle>> {
                new List<Muscle>
                {
                    // НОГИ
                    Muscle.GetMuscleByName("Квадрицепс"),
                    Muscle.GetMuscleByName("Бицепс бедра"),
                    Muscle.GetMuscleByName("Ягодичные"),
                    Muscle.GetMuscleByName("Икры"),
                    
                    // СПИНА
                    Muscle.GetMuscleByName("Широчайшие"),
                    Muscle.GetMuscleByName("Трапеции"),
                    Muscle.GetMuscleByName("Ромбовидные"),
                    Muscle.GetMuscleByName("Поясница"),
                    
                    // ГРУДЬ
                    Muscle.GetMuscleByName("Верх груди"),
                    Muscle.GetMuscleByName("Середина груди"),
                    Muscle.GetMuscleByName("Низ груди"),
                    Muscle.GetMuscleByName("Внутренняя часть груди"),
                    
                    // ПЛЕЧИ
                    Muscle.GetMuscleByName("Передние дельты"),
                    Muscle.GetMuscleByName("Средние дельты"),
                    Muscle.GetMuscleByName("Задние дельты"),
                    
                    // РУКИ
                    Muscle.GetMuscleByName("Бицепс"),
                    Muscle.GetMuscleByName("Трицепс"),
                    Muscle.GetMuscleByName("Предплечья"),
                    
                    // КОР
                    Muscle.GetMuscleByName("Верх пресса"),
                    Muscle.GetMuscleByName("Низ пресса"),
                    Muscle.GetMuscleByName("Косые мышцы"),
                    
                    // ШЕЯ
                    Muscle.GetMuscleByName("Шея")
                }
            };
                    }

                // === 2 ДНЯ В НЕДЕЛЮ (Разделение: нижняя/верхняя часть тела) ===
                case 2:
                    {
                        Week.week.Days[1].programName = "РАСТЯЖКА НОГ И КОРА";
                        Week.week.Days[3].programName = "РАСТЯЖКА ВЕРХНЕЙ ЧАСТИ ТЕЛА";
                        return new List<List<Muscle>> {
                // День 1: НОГИ И КОР
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Квадрицепс"),
                    Muscle.GetMuscleByName("Бицепс бедра"),
                    Muscle.GetMuscleByName("Ягодичные"),
                    Muscle.GetMuscleByName("Икры"),
                    Muscle.GetMuscleByName("Верх пресса"),
                    Muscle.GetMuscleByName("Низ пресса"),
                    Muscle.GetMuscleByName("Косые мышцы"),
                    Muscle.GetMuscleByName("Поясница")
                },
                
                // День 2: ВЕРХНЯЯ ЧАСТЬ ТЕЛА
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Широчайшие"),
                    Muscle.GetMuscleByName("Трапеции"),
                    Muscle.GetMuscleByName("Ромбовидные"),
                    Muscle.GetMuscleByName("Верх груди"),
                    Muscle.GetMuscleByName("Середина груди"),
                    Muscle.GetMuscleByName("Низ груди"),
                    Muscle.GetMuscleByName("Передние дельты"),
                    Muscle.GetMuscleByName("Средние дельты"),
                    Muscle.GetMuscleByName("Задние дельты"),
                    Muscle.GetMuscleByName("Бицепс"),
                    Muscle.GetMuscleByName("Трицепс"),
                    Muscle.GetMuscleByName("Предплечья"),
                    Muscle.GetMuscleByName("Шея")
                }
            };
                    }

                // === 3 ДНЯ В НЕДЕЛЮ (Ноги/Спина+Грудь/Плечи+Руки) ===
                case 3:
                    {
                        Week.week.Days[0].programName = "РАСТЯЖКА НОГ И ТАЗА";
                        Week.week.Days[2].programName = "ГИБКОСТЬ СПИНЫ И ГРУДИ";
                        Week.week.Days[4].programName = "МОБИЛЬНОСТЬ ПЛЕЧ И РУК";
                        return new List<List<Muscle>> {
                // День 1: НОГИ И ТАЗ
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Квадрицепс"),
                    Muscle.GetMuscleByName("Бицепс бедра"),
                    Muscle.GetMuscleByName("Ягодичные"),
                    Muscle.GetMuscleByName("Икры"),
                    Muscle.GetMuscleByName("Верх пресса"),
                    Muscle.GetMuscleByName("Низ пресса"),
                    Muscle.GetMuscleByName("Косые мышцы")
                },
                
                // День 2: СПИНА И ГРУДЬ
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Широчайшие"),
                    Muscle.GetMuscleByName("Трапеции"),
                    Muscle.GetMuscleByName("Ромбовидные"),
                    Muscle.GetMuscleByName("Поясница"),
                    Muscle.GetMuscleByName("Верх груди"),
                    Muscle.GetMuscleByName("Середина груди"),
                    Muscle.GetMuscleByName("Низ груди"),
                    Muscle.GetMuscleByName("Внутренняя часть груди")
                },
                
                // День 3: ПЛЕЧИ И РУКИ
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Передние дельты"),
                    Muscle.GetMuscleByName("Средние дельты"),
                    Muscle.GetMuscleByName("Задние дельты"),
                    Muscle.GetMuscleByName("Бицепс"),
                    Muscle.GetMuscleByName("Трицепс"),
                    Muscle.GetMuscleByName("Предплечья"),
                    Muscle.GetMuscleByName("Шея")
                }
            };
                    }

                // === 4 ДНЯ В НЕДЕЛЮ (Специализация по группам) ===
                case 4:
                    {
                        Week.week.Days[0].programName = "ГИБКОСТЬ НОГ";
                        Week.week.Days[1].programName = "РАСТЯЖКА СПИНЫ И КОРА";
                        Week.week.Days[3].programName = "РАСКРЫТИЕ ГРУДИ И ПЛЕЧ";
                        Week.week.Days[4].programName = "МОБИЛЬНОСТЬ РУК И ШЕИ";
                        return new List<List<Muscle>> {
                // День 1: НОГИ
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Квадрицепс"),
                    Muscle.GetMuscleByName("Бицепс бедра"),
                    Muscle.GetMuscleByName("Ягодичные"),
                    Muscle.GetMuscleByName("Икры")
                },
                
                // День 2: СПИНА И КОР
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Широчайшие"),
                    Muscle.GetMuscleByName("Трапеции"),
                    Muscle.GetMuscleByName("Ромбовидные"),
                    Muscle.GetMuscleByName("Поясница"),
                    Muscle.GetMuscleByName("Верх пресса"),
                    Muscle.GetMuscleByName("Низ пресса"),
                    Muscle.GetMuscleByName("Косые мышцы")
                },
                
                // День 3: ГРУДЬ И ПЛЕЧИ
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Верх груди"),
                    Muscle.GetMuscleByName("Середина груди"),
                    Muscle.GetMuscleByName("Низ груди"),
                    Muscle.GetMuscleByName("Внутренняя часть груди"),
                    Muscle.GetMuscleByName("Передние дельты"),
                    Muscle.GetMuscleByName("Средние дельты"),
                    Muscle.GetMuscleByName("Задние дельты")
                },
                
                // День 4: РУКИ И ШЕЯ
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Бицепс"),
                    Muscle.GetMuscleByName("Трицепс"),
                    Muscle.GetMuscleByName("Предплечья"),
                    Muscle.GetMuscleByName("Шея")
                }
            };
                    }

                // === 5 ДНЕЙ В НЕДЕЛЮ (Высокая частота + специализация) ===
                case 5:
                    {
                        Week.week.Days[0].programName = "РАСТЯЖКА ПЕРЕДНИХ МЫШЦ БЕДРА";
                        Week.week.Days[1].programName = "РАСТЯЖКА ЗАДНИХ МЫШЦ НОГ";
                        Week.week.Days[2].programName = "ГИБКОСТЬ СПИНЫ И ПОЯСНИЦЫ";
                        Week.week.Days[3].programName = "РАСКРЫТИЕ ГРУДНОГО ОТДЕЛА";
                        Week.week.Days[4].programName = "МОБИЛЬНОСТЬ ПЛЕЧЕВОГО ПОЯСА";
                        return new List<List<Muscle>> {
                // День 1: ПЕРЕДНИЕ МЫШЦЫ БЕДРА
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Квадрицепс"),
                    Muscle.GetMuscleByName("Верх пресса"),
                    Muscle.GetMuscleByName("Низ пресса")
                },
                
                // День 2: ЗАДНИЕ МЫШЦЫ НОГ
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Бицепс бедра"),
                    Muscle.GetMuscleByName("Ягодичные"),
                    Muscle.GetMuscleByName("Икры"),
                    Muscle.GetMuscleByName("Поясница")
                },
                
                // День 3: СПИНА И ПОЯСНИЦА
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Широчайшие"),
                    Muscle.GetMuscleByName("Трапеции"),
                    Muscle.GetMuscleByName("Ромбовидные"),
                    Muscle.GetMuscleByName("Косые мышцы")
                },
                
                // День 4: ГРУДНОЙ ОТДЕЛ
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Верх груди"),
                    Muscle.GetMuscleByName("Середина груди"),
                    Muscle.GetMuscleByName("Низ груди"),
                    Muscle.GetMuscleByName("Внутренняя часть груди"),
                    Muscle.GetMuscleByName("Передние дельты")
                },
                
                // День 5: ПЛЕЧЕВОЙ ПОЯС
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Средние дельты"),
                    Muscle.GetMuscleByName("Задние дельты"),
                    Muscle.GetMuscleByName("Бицепс"),
                    Muscle.GetMuscleByName("Трицепс"),
                    Muscle.GetMuscleByName("Предплечья"),
                    Muscle.GetMuscleByName("Шея")
                }
            };
                    }

                default:
                    throw new ArgumentException($"Неподдерживаемое количество дней: {nums}");
            }
        else if (goal == Goal.IncreasedEndurance) switch (nums)
            {
                // === 1 ДЕНЬ В НЕДЕЛЮ (Круговая тренировка всего тела) ===
                case 1:
                    {
                        Week.week.Days[2].programName = "КРУГОВАЯ ТРЕНИРОВКА НА ВЫНОСЛИВОСТЬ";
                        return new List<List<Muscle>> {
                new List<Muscle>
                {
                    // Основные большие группы для метаболического эффекта
                    Muscle.GetMuscleByName("Квадрицепс"),      // Приседания
                    Muscle.GetMuscleByName("Середина груди"),  // Отжимания
                    Muscle.GetMuscleByName("Широчайшие"),      // Подтягивания/тяги
                    Muscle.GetMuscleByName("Ягодичные"),       // Выпады/мостик
                    Muscle.GetMuscleByName("Поясница"),        // Стабилизация
                    Muscle.GetMuscleByName("Верх пресса")      // Кор
                }
            };
                    }

                // === 2 ДНЯ В НЕДЕЛЮ (Верх/Низ) ===
                case 2:
                    {
                        Week.week.Days[1].programName = "ВЫНОСЛИВОСТЬ ВЕРХНЕЙ ЧАСТИ ТЕЛА";
                        Week.week.Days[3].programName = "ВЫНОСЛИВОСТЬ НИЖНЕЙ ЧАСТИ ТЕЛА";
                        return new List<List<Muscle>> {
                // День 1: ВЕРХ
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Середина груди"),  // Отжимания
                    Muscle.GetMuscleByName("Широчайшие"),      // Подтягивания
                    Muscle.GetMuscleByName("Трицепс"),         // Разгибания/отжимания
                    Muscle.GetMuscleByName("Бицепс"),          // Подтягивания обратным хватом
                    Muscle.GetMuscleByName("Передние дельты")  // Отжимания на брусьях
                },
                
                // День 2: НИЗ
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Квадрицепс"),      // Приседания
                    Muscle.GetMuscleByName("Бицепс бедра"),    // Румынская тяга/мостик
                    Muscle.GetMuscleByName("Ягодичные"),       // Выпады/приседания
                    Muscle.GetMuscleByName("Икры"),            // Прыжки/скакалка
                    Muscle.GetMuscleByName("Верх пресса")      // Планка с движениями ног
                }
            };
                    }

                // === 3 ДНЯ В НЕДЕЛЮ (Толкай/Тяни/Ноги) ===
                case 3:
                    {
                        Week.week.Days[0].programName = "ТОЛКАЮЩИЕ ДВИЖЕНИЯ";
                        Week.week.Days[2].programName = "ТЯНУЩИЕ ДВИЖЕНИЯ";
                        Week.week.Days[4].programName = "НОГИ И КОР";
                        return new List<List<Muscle>> {
                // День 1: ТОЛКАЙ (Push)
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Середина груди"),  // Отжимания
                    Muscle.GetMuscleByName("Трицепс"),         // Алмазные отжимания
                    Muscle.GetMuscleByName("Передние дельты"), // Отжимания в стойке
                    Muscle.GetMuscleByName("Средние дельты")   // Отжимания с руками узко
                },
                
                // День 2: ТЯНИ (Pull)
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Широчайшие"),      // Подтягивания
                    Muscle.GetMuscleByName("Ромбовидные"),     // Австралийские подтягивания
                    Muscle.GetMuscleByName("Бицепс"),          // Подтягивания обратным хватом
                    Muscle.GetMuscleByName("Задние дельты")    // Подтягивания лицом к перекладине
                },
                
                // День 3: НОГИ И КОР (Legs & Core)
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Квадрицепс"),      // Приседания
                    Muscle.GetMuscleByName("Ягодичные"),       // Выпады
                    Muscle.GetMuscleByName("Бицепс бедра"),    // Мостик
                    Muscle.GetMuscleByName("Верх пресса"),     // Берпи
                    Muscle.GetMuscleByName("Поясница")         // Супермен/гиперэкстензии
                }
            };
                    }

                // === 4 ДНЯ В НЕДЕЛЮ (Сплит с акцентом на выносливость) ===
                case 4:
                    {
                        Week.week.Days[0].programName = "ГРУДЬ+ТРИЦЕПС (Толкай)";
                        Week.week.Days[1].programName = "СПИНА+БИЦЕПС (Тяни)";
                        Week.week.Days[3].programName = "НОГИ+ПЛЕЧИ";
                        Week.week.Days[4].programName = "ФУНКЦИОНАЛЬНАЯ ВЫНОСЛИВОСТЬ";
                        return new List<List<Muscle>> {
                // День 1: Грудь + Трицепс
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Середина груди"),
                    Muscle.GetMuscleByName("Трицепс"),
                    Muscle.GetMuscleByName("Передние дельты")
                },
                
                // День 2: Спина + Бицепс
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Широчайшие"),
                    Muscle.GetMuscleByName("Бицепс"),
                    Muscle.GetMuscleByName("Трапеции")
                },
                
                // День 3: Ноги + Плечи
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Квадрицепс"),
                    Muscle.GetMuscleByName("Ягодичные"),
                    Muscle.GetMuscleByName("Средние дельты"),
                    Muscle.GetMuscleByName("Икры")
                },
                
                // День 4: Функциональная выносливость
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Верх пресса"),
                    Muscle.GetMuscleByName("Поясница"),
                    Muscle.GetMuscleByName("Квадрицепс"),  // Для берпи
                    Muscle.GetMuscleByName("Трицепс")      // Для берпи/отжиманий
                }
            };
                    }

                // === 5 ДНЕЙ В НЕДЕЛЮ (Высокая частота + специализация) ===
                case 5:
                    {
                        Week.week.Days[0].programName = "ТОЛКАЮЩИЕ УПРАЖНЕНИЯ";
                        Week.week.Days[1].programName = "ТЯНУЩИЕ УПРАЖНЕНИЯ";
                        Week.week.Days[2].programName = "НОГИ И КОР (Силовая выносливость)";
                        Week.week.Days[3].programName = "ВЕРХ ТЕЛА (Круговая)";
                        Week.week.Days[4].programName = "ФУНКЦИОНАЛЬНАЯ ТРЕНИРОВКА";
                        return new List<List<Muscle>> {
                // День 1: Толкающие
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Середина груди"),
                    Muscle.GetMuscleByName("Трицепс")
                },
                
                // День 2: Тянущие
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Широчайшие"),
                    Muscle.GetMuscleByName("Бицепс")
                },
                
                // День 3: Ноги и кор
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Квадрицепс"),
                    Muscle.GetMuscleByName("Верх пресса")
                },
                
                // День 4: Верх тела круговая
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Передние дельты"),
                    Muscle.GetMuscleByName("Трапеции"),
                    Muscle.GetMuscleByName("Предплечья")
                },
                
                // День 5: Функциональная (берпи, скакалка и т.д.)
                new List<Muscle>
                {
                    Muscle.GetMuscleByName("Квадрицепс"),  // Берпи
                    Muscle.GetMuscleByName("Икры"),        // Скакалка
                    Muscle.GetMuscleByName("Поясница")     // Стабилизация
                }
            };
                    }

                default:
                    throw new ArgumentException($"Неподдерживаемое количество дней: {nums}");
            }
        else switch (nums)
            {
                // === 1 ДЕНЬ В НЕДЕЛЮ (Минимум времени, максимум отдачи) ===
                // Принцип: ФУЛЛ-БАДИ с упором на самые важные упражнения.
                // Акцент на большие мышечные группы, которые дают гормональный отклик.
                // За одну тренировку нужно "потрогать" всё, чтобы минимизировать потери.
                case 1:
                    {
                        Week.week.Days[2].programName = "Фулл-бади. Интенсивность!";
                        return new List<List<Muscle>> {
                    new List<Muscle>
                    {
                        // БАЗА (основные движениня, ставить в начало)
                        Muscle.GetMuscleByName("Квадрицепс"),          // Приседания/жим ногами
                        Muscle.GetMuscleByName("Широчайшие"),          // Тяга верхнего блока/подтягивания
                        Muscle.GetMuscleByName("Середина груди"),      // Жим штанги/гантелей лежа
                        Muscle.GetMuscleByName("Бицепс бедра"),        // Румынская тяга
                        
                        // ВСПОМОГАТЕЛЬНЫЕ (добиваем, но без фанатизма)
                        Muscle.GetMuscleByName("Средние дельты"),      // Махи гантелями/тяга к подбородку
                        Muscle.GetMuscleByName("Трицепс"),             // Жим узким хватом/отжимания на брусьях
                        Muscle.GetMuscleByName("Бицепс"),              // Подъем штанги/гантелей на бицепс
                        // Пресс - можно в конце, если есть силы
                    }
                };
                    }

                // === 2 ДНЯ В НЕДЕЛЮ (Классический Upper/Lower) ===
                // Принцип: Разделение на верх и низ. Идеально для баланса и прогрессии.
                // Частота: Каждая группа 1 раз в неделю (можно чередовать акценты по неделям).
                case 2:
                    {
                        Week.week.Days[1].programName = "ВЕРХ (Тяги + Жимы)";
                        Week.week.Days[3].programName = "НИЗ + Пресс";
                        return new List<List<Muscle>> {
                    // День 1: ВЕРХ
                    new List<Muscle>
                    {
                        // ТЯГОВЫЕ движения (спина)
                        Muscle.GetMuscleByName("Широчайшие"),          // Подтягивания/тяга блока
                        Muscle.GetMuscleByName("Трапеции"),            // Шраги/тяга штанги к подбородку
                        Muscle.GetMuscleByName("Задние дельты"),       // Махи в наклоне
                        Muscle.GetMuscleByName("Бицепс"),              // Сгибания рук
                        
                        // ЖИМОВЫЕ движения (грудь, плечи)
                        Muscle.GetMuscleByName("Середина груди"),      // Жим лежа
                        Muscle.GetMuscleByName("Верх груди"),          // Жим на наклонной
                        Muscle.GetMuscleByName("Средние дельты"),      // Жим гантелей сидя/армейский жим
                        Muscle.GetMuscleByName("Трицепс"),             // Разгибания на блоке/французский жим
                    },
                    
                    // День 2: НИЗ
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Квадрицепс"),          // Приседания/разгибания ног
                        Muscle.GetMuscleByName("Бицепс бедра"),        // Сгибания ног/румынская тяга
                        Muscle.GetMuscleByName("Ягодичные"),           // Ягодичный мостик/выпады
                        Muscle.GetMuscleByName("Икры"),                // Подъем на носки стоя/сидя
                        Muscle.GetMuscleByName("Поясница"),            // Гиперэкстензии (для здоровья спины)
                        Muscle.GetMuscleByName("Верх пресса"),         // Скручивания/подъем ног
                    }
                };
                    }

                // === 3 ДНЯ В НЕДЕЛЮ (Оптимальная схема Push/Pull/Legs) ===
                // Принцип: Разделение по функциям. Лучшее для частоты (каждую группу 2 раза в 8-9 дней).
                // Наиболее эффективный сплит для роста у продолжающих.
                case 3:
                    {
                        Week.week.Days[0].programName = "ТЯНИ (Спина, Бицепс, Задние дельты)";
                        Week.week.Days[2].programName = "ЖМИ (Грудь, Трицепс, Плечи)";
                        Week.week.Days[4].programName = "НОГИ + Пресс";
                        return new List<List<Muscle>> {
                    // День 1: ТЯНИ (PULL)
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Широчайшие"),          // Тяга вертикальная/горизонтальная
                        Muscle.GetMuscleByName("Трапеции"),            // Шраги
                        Muscle.GetMuscleByName("Ромбовидные"),         // Тяга в наклоне
                        Muscle.GetMuscleByName("Задние дельты"),       // ОБЯЗАТЕЛЬНО! Часто отстают
                        Muscle.GetMuscleByName("Бицепс"),              // 2 упражнения
                        Muscle.GetMuscleByName("Предплечья"),          // Если нужно, в конце
                    },
                    
                    // День 2: ЖМИ (PUSH)
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Верх груди"),          // Жим на наклонной (в начале!)
                        Muscle.GetMuscleByName("Середина груди"),      // Жим лежа/разводки
                        Muscle.GetMuscleByName("Средние дельты"),      // Жим гантелей/махи
                        Muscle.GetMuscleByName("Передние дельты"),     // Подъемы перед собой
                        Muscle.GetMuscleByName("Трицепс"),             // 2-3 упражнения
                    },
                    
                    // День 3: НОГИ (LEGS)
                    new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Квадрицепс"),          // Приседания/жим ногами (тяжело)
                        Muscle.GetMuscleByName("Бицепс бедра"),        // Мертвая тяга/сгибания
                        Muscle.GetMuscleByName("Ягодичные"),           // Выпады/ягодичный мостик
                        Muscle.GetMuscleByName("Икры"),                // 2 упражнения
                        Muscle.GetMuscleByName("Поясница"),            // Гиперэкстензии
                        Muscle.GetMuscleByName("Верх пресса"),         // Пресс
                        Muscle.GetMuscleByName("Низ пресса"),          // Нижний пресс
                    }
                };
                    }
                // === 4 ДНЯ В НЕДЕЛЮ (Частота 2x в неделю) ===
                // Принцип: Верх/Низ, разделенный на два разных варианта.
                // Понедельник: Верх А, Вторник: Низ А, Четверг: Верх Б, Пятница: Низ Б
                // Каждая группа получает нагрузку 2 раза в неделю с разными акцентами/упражнениями.
                case 4:
                    {
                        Week.week.Days[0].programName = "ВЕРХ (А) - Грудь/Спина акцент";
                        Week.week.Days[1].programName = "НИЗ (А) - Квадрицепсы акцент";
                        Week.week.Days[3].programName = "ВЕРХ (Б) - Плечи/Руки акцент";
                        Week.week.Days[4].programName = "НИЗ (Б) - Бицепс бедра/Ягодицы акцент";
                        return new List<List<Muscle>> {
            // День 1: ВЕРХ (А) - Горизонтальные тяги и жимы
            new List<Muscle>
            {
                Muscle.GetMuscleByName("Середина груди"),      // Жим лежа (горизонтальный жим)
                Muscle.GetMuscleByName("Широчайшие"),          // Тяга штанги в наклоне (горизонтальная тяга)
                Muscle.GetMuscleByName("Верх груди"),          // Жим гантелей на наклонной
                Muscle.GetMuscleByName("Ромбовидные"),         // Тяга Т-грифа
                Muscle.GetMuscleByName("Трицепс"),             // Одно базовое (отжимания на брусьях)
            },
            
            // День 2: НИЗ (А) - Передняя цепь (Квадрицепс-доминантные)
            new List<Muscle>
            {
                Muscle.GetMuscleByName("Квадрицепс"),          // Приседания со штангой (основное)
                Muscle.GetMuscleByName("Квадрицепс"),          // Разгибания ног (добивка)
                Muscle.GetMuscleByName("Ягодичные"),           // Выпады
                Muscle.GetMuscleByName("Икры"),                // Подъем на носки стоя
                Muscle.GetMuscleByName("Верх пресса"),         // Пресс
            },
            
            // День 3: ВЕРХ (Б) - Вертикальные тяги и жимы + изоляция
            new List<Muscle>
            {
                Muscle.GetMuscleByName("Широчайшие"),          // Подтягивания/тяга верхнего блока (вертикальная тяга)
                Muscle.GetMuscleByName("Средние дельты"),      // Жим гантелей сидя/стоя (вертикальный жим)
                Muscle.GetMuscleByName("Задние дельты"),       // Махи в наклоне
                Muscle.GetMuscleByName("Бицепс"),              // 2 упражнения на бицепс
                Muscle.GetMuscleByName("Трицепс"),             // Разгибания на блоке (изоляция)
            },
            
            // День 4: НИЗ (Б) - Задняя цепь (Бицепс бедра-доминантные)
            new List<Muscle>
            {
                Muscle.GetMuscleByName("Бицепс бедра"),        // Мертвая тяга на прямых ногах (основное)
                Muscle.GetMuscleByName("Ягодичные"),           // Ягодичный мостик/гиперэкстензия
                Muscle.GetMuscleByName("Бицепс бедра"),        // Сгибания ног лежа (добивка)
                Muscle.GetMuscleByName("Икры"),                // Подъем на носки сидя (акцент на камбаловидную)
                Muscle.GetMuscleByName("Поясница"),            // Гиперэкстензия (легкий вес)
                Muscle.GetMuscleByName("Низ пресса"),          // Подъем ног в висе
            }
        };
                    }

                // === 5 ДНЕЙ В НЕДЕЛЮ (Частота 2x в неделю с фокусом) ===
                // Принцип: PPL + 2 дополнительных дня для слабых мест/кардио/восстановления.
                // ИЛИ: Тяни-Жми-Ноги + Верх-Низ (разные акценты).
                // Вариант ниже - PPL + специализация.
                case 5:
                    {
                        Week.week.Days[0].programName = "ТЯНИ (А) - Спина ширина";
                        Week.week.Days[1].programName = "ЖМИ (А) - Грудь объем";
                        Week.week.Days[2].programName = "НОГИ (А) - Квадрицепсы";
                        Week.week.Days[3].programName = "ТЯНИ (Б) - Спина толщина + Бицепс";
                        Week.week.Days[4].programName = "ЖМИ (Б) - Плечи + Трицепс";
                        return new List<List<Muscle>> {
            // День 1: ТЯНИ (А) - Вертикальные тяги (ширина)
            new List<Muscle>
            {
                Muscle.GetMuscleByName("Широчайшие"),          // Тяга верхнего блока за голову
                Muscle.GetMuscleByName("Задние дельты"),       // Махи в наклоне
                Muscle.GetMuscleByName("Трапеции"),            // Шраги
                // Бицепс только косвенно
            },
            
            // День 2: ЖМИ (А) - Грудь + Трицепс
            new List<Muscle>
            {
                Muscle.GetMuscleByName("Верх груди"),          // Жим на наклонной (30°)
                Muscle.GetMuscleByName("Середина груди"),      // Жим гантелей лежа
                Muscle.GetMuscleByName("Трицепс"),             // Жим узким хватом
                // Плечи только косвенно
            },
            
            // День 3: НОГИ (А) - Квадрицепсы акцент
            new List<Muscle>
            {
                Muscle.GetMuscleByName("Квадрицепс"),          // Фронтальные приседания/жим ногами
                Muscle.GetMuscleByName("Икры"),                // Подъем на носки стоя
                Muscle.GetMuscleByName("Верх пресса"),         // Скручивания с весом
            },
            
            // День 4: ТЯНИ (Б) - Горизонтальные тяги + Бицепс
            new List<Muscle>
            {
                Muscle.GetMuscleByName("Широчайшие"),          // Тяга штанги в наклоне
                Muscle.GetMuscleByName("Ромбовидные"),         // Тяга гантели одной рукой
                Muscle.GetMuscleByName("Бицепс"),              // Подъем штанги на бицепс
                Muscle.GetMuscleByName("Бицепс"),              // "Молотки" с гантелями
                Muscle.GetMuscleByName("Предплечья"),          // Сгибания запястий
            },
            
            // День 5: ЖМИ (Б) - Плечи + Задние дельты
            new List<Muscle>
            {
                Muscle.GetMuscleByName("Средние дельты"),      // Армейский жим/жим гантелей
                Muscle.GetMuscleByName("Задние дельты"),       // Обратные разведения в тренажере
                Muscle.GetMuscleByName("Передние дельты"),     // Подъем гантелей перед собой
                Muscle.GetMuscleByName("Трицепс"),             // Французский жим лежа (длинная головка)
                Muscle.GetMuscleByName("Низ пресса"),          // Подъем ног в висе
            }
        };
                    }
                default:
                    throw new ArgumentException($"Неподдерживаемое количество дней: {nums}");
            }                
            

    }
    

    #endregion

    #region Методы для разбивки тренеровки на разные дни
    private static List<int> GetDaysList(int treningsDayCount)
	{
		switch (treningsDayCount)
		{
			case 1:
				{return new List<int>() {2};}
				
			case 2:
				{ return new List<int>() { 1, 3 }; }
			case 3:
				{ return new List<int>() { 0,2, 4}; }
			case 4:
				{ return new List<int>() { 0, 1, 3, 4 }; }
			case 5:
				{ return new List<int>() {0, 1, 2, 3,4}; }
            case 6:
                { return new List<int>() { 0, 1, 2, 3, 4,5 }; }
            case 7:
                { return new List<int>() { 0, 1, 2, 3, 4, 5,6 }; }
			default: { return new List<int>(); }
        }
	}
    private static float GetMax(int trainingsDayCount, float cof,StringBuilder DB = null)
    {
        float baseValue = trainingsDayCount switch
        {
            1 => 35f,
            2 => 30f,
            3 => 25f,
            4 => 22f,
            5 => 18f,
            6 => 15f,
            7 => 12f,
            _ => 20f
        };
        DB?.AppendLine($"Всего дней {trainingsDayCount},базовое MAX на сегодня - {baseValue} * {cof} ={baseValue * cof}, а успеем - {timeTreningCount / TimeOnApproach}");
        return Mathf.Clamp(baseValue * cof,10,timeTreningCount/TimeOnApproach);
    }

    #endregion

        #region Для кардио
    private static void  AddCardioInProgram(int dayNum,int calories,int time)
    {
         Week.week.Days[dayNum].AddSetOfExercises(new SetOfExercises(Walk.AutoCreateWalk(TimeSpan.FromMinutes(time), calories)));             
    }
    #endregion
}
