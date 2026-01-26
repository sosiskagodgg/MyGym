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


    #region Распределения
    StringBuilder DebugStringBilder;
    public static List<SetOfExercises> DistributeExercises(List<Exercise> exercises, int weekWA, StringBuilder DB = null)
    {
        DB?.AppendLine();
        DB?.AppendLine("Распределение подходов:");

        List<SetOfExercises> setsOfExercises = new();

        // Определяем базовый размер сета
        int minSetSize = weekWA < 12 ? 3 : 4;
        int maxSetSize = 4; // Максимальный размер сета

        // Для каждого упражнения распределяем подходы в зависимости от приоритета (индекса)
        for (int i = 0; i < exercises.Count; i++)
        {
            Exercise exercise = exercises[i];

            // Определяем количество подходов для упражнения
            int quantity = GetSetQuantity(i, exercises.Count, minSetSize, maxSetSize);

            DB?.AppendLine($"Упражнение {i + 1}: '{exercise.name}', приоритет: {i + 1}, подходы: {quantity}");

            // Создаем сет с нужным количеством подходов
            SetOfExercises set = new SetOfExercises(exercise, quantity, true);
            setsOfExercises.Add(set);
        }

        DB?.AppendLine($"Всего сетов: {setsOfExercises.Count}");

        return setsOfExercises;
    }

    private static int GetSetQuantity(int index, int totalExercises, int minSetSize, int maxSetSize)
    {
        // Первые упражнения имеют высший приоритет и получают больше подходов

        if (totalExercises <= 3)
        {
            // Если упражнений мало, все получают максимальное количество
            return maxSetSize;
        }

        if (minSetSize == maxSetSize)
        {
            // Если мин и макс одинаковы, все получают одинаково
            return maxSetSize;
        }

        // Определяем количество подходов на основе индекса
        // Распределяем так, чтобы первые 2 упражнения получали maxSetSize,
        // следующие 2 - среднее значение,
        // остальные - minSetSize

        if (index < 2)
        {
            // Высший приоритет - первые 2 упражнения
            return maxSetSize;
        }
        else if (index < 4 && totalExercises > 4)
        {
            // Средний приоритет - следующие 2 упражнения
            return (maxSetSize + minSetSize) / 2; // Среднее значение
        }
        else
        {
            // Низкий приоритет - остальные упражнения
            return minSetSize;
        }
    }

    #endregion

    #region Основной метод создания тренеровки


    public static void CreateStrengthTraining(float intensity, int daysCount, StringBuilder DB = null)
    {
        intensity *= ExerciseManager.Coefficient.VolumeTolerance;

        // УБРАТЬ: Week.week = Week.EmptyWeek; // Не нужно ставить пустую неделю

        if (Player.player.treningParametrs.goal == Goal.IncreasedStrength) intensity *= 0.6f;
        if (Player.player.treningParametrs.goal == Goal.Recomposition) intensity *= 0.75f;

        DB?.AppendLine(Player.player.treningParametrs.goal.ToString());

        // СОХРАНИМ ОРИГИНАЛЬНЫЕ ЗНАЧЕНИЯ
        int targetCaloriesPerWorkout = caloriesCount;
        int targetTimePerWorkout = timeTreningCount;

        DB?.AppendLine($"=== Цель КАЖДОЙ тренировки: {targetCaloriesPerWorkout} ккал, {targetTimePerWorkout} минут ===");

        int treningNum = 0;

        // СОЗДАЕМ НОВУЮ НЕДЕЛЮ (не сохраняем ее пока)
        Week newWeek = new Week()
        {
            Days = Week.EmptyWeek.Days.Select(d =>
                new Day(d.num, d.name, new List<SetOfExercises>())).ToList()
        };
        Dictionary<int, string> getWorkoutNames = GetWorkoutNames(daysCount, Player.player.treningParametrs.goal);
        for (int i = 0; i < 7; i++) // Проходим все 7 дней недели
        {
            if (GetDaysList(daysCount).Contains(i))
            {
                
                // КАЖДАЯ тренировка имеет одинаковую цель
                int workoutTimeLeft = targetTimePerWorkout;
                int workoutCaloriesLeft = targetCaloriesPerWorkout;

                DB?.AppendLine($"=== День {i + 1}/7 ===");
                DB?.AppendLine($"Цель тренировки: {workoutCaloriesLeft} ккал, {workoutTimeLeft} мин");

                List<Exercise> exercises = CreateSplitForDay(daysCount)[treningNum];
                float maxWA = GetMax(daysCount, intensity, DB);

                List<SetOfExercises> setsOfExercises = DistributeExercises(exercises,(int)maxWA, DB);

                // Заполняем день в НОВОЙ неделе
                newWeek.Days[i].setsOfExercises = setsOfExercises;
                newWeek.Days[i].Sort();
                newWeek.Days[i].programName = getWorkoutNames[i];
                // УБРАТЬ: Week.SaveDay(Week.week.Days[i]); // УБИРАЕМ СОХРАНЕНИЕ В ЦИКЛЕ!

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

                // Добавляем кардио, если нужно
                if (Player.player.treningParametrs.goal == Goal.WeightLoss && workoutCaloriesLeft > 0 && workoutTimeLeft > 0)
                {
                    newWeek.Days[i].AddSetOfExercises(
                        new SetOfExercises(Walk.AutoCreateWalk(
                            TimeSpan.FromMinutes(workoutTimeLeft),
                            workoutCaloriesLeft))
                    );
                    DB?.AppendLine($"Добавлено кардио: {workoutCaloriesLeft} ккал, {workoutTimeLeft} мин");
                }

                DB?.AppendLine($"Итого на день {i + 1}: {setsOfExercises.Count} упражнений");

                treningNum++;
            }
        }
        
        // ТОЛЬКО ОДИН РАЗ: сохраняем готовую неделю
        Week.week = newWeek;
        ViewProgram.UpdateProgramNames();
        Debug.Log("=== НЕДЕЛЯ СОЗДАНА И СОХРАНЕНА ===");
    }
    public static void CreateFlexibilityTraining()
    {

    }
    private static List<List<Exercise>> CreateSplitForDay(int nums)
    {
        Goal goal = Player.player.treningParametrs.goal;
        var allExercises = ExerciseManager.Exercises;

        // Функция для поиска упражнения по названию
        Exercise FindExercise(string name) =>
            ExerciseManager.GetExercisesByName(name) ??
            throw new ArgumentException($"Упражнение '{name}' не найдено из похожих {allExercises.Where(e => e.name.Contains(name.Split()[0]))}");

        if (goal == Goal.IncreasedStrength) switch (nums)
            {
                // === 1 ДЕНЬ В НЕДЕЛЮ ===
                case 1:
                    {
                        Week.week.Days[2].programName = "БАЗА (Сила)";
                        return new List<List<Exercise>> {
                new List<Exercise>
                {
                    FindExercise("Становая тяга"),                    // 1 - самый высокий приоритет
                    FindExercise("Приседания со штангой на спине"),   // 2
                    FindExercise("Жим лежа"),                         // 3
                    FindExercise("Подтягивания широким хватом"),      // 4
                    FindExercise("Румынская тяга"),                   // 5
                    FindExercise("Жим узким хватом"),                 // 6
                    FindExercise("Шраги со штангой сзади"),           // 7
                    FindExercise("Скручивания на римском стуле")      // 8 - самый низкий приоритет
                }
            };
                    }

                // === 2 ДНЯ В НЕДЕЛЮ ===
                case 2:
                    {
                        Week.week.Days[1].programName = "ЖИМЫ (Присед + Жим лежа)";
                        Week.week.Days[3].programName = "ТЯГИ (Становая + Подтягивания)";
                        return new List<List<Exercise>> {
                // День 1: ЖИМЫ
                new List<Exercise>
                {
                    FindExercise("Приседания со штангой на спине"),   // 1
                    FindExercise("Жим лежа"),                         // 2
                    FindExercise("Жим узким хватом"),                 // 3
                    FindExercise("Армейский жим стоя"),               // 4
                    FindExercise("Румынская тяга")                    // 5
                },
                
                // День 2: ТЯГИ
                new List<Exercise>
                {
                    FindExercise("Становая тяга"),                    // 1
                    FindExercise("Подтягивания широким хватом"),      // 2
                    FindExercise("Тяга штанги к подбородку широким хватом"),  // 3
                    FindExercise("Подъем штанги на бицепс стоя"),     // 4
                    FindExercise("Махи гантелями в наклоне"),         // 5
                    FindExercise("Скручивания на римском стуле")      // 6
                }
            };
                    }

                // === 3 ДНЯ В НЕДЕЛЮ ===
                case 3:
                    {
                        Week.week.Days[0].programName = "ПРИСЕД + ЖИМ + ТЯГА";
                        Week.week.Days[2].programName = "ПРИСЕД + ЖИМ ЛЕЖА + СТАНОВАЯ";
                        Week.week.Days[4].programName = "ПРИСЕД + ЖИМ + ТЯГА ГРУДНАЯ";
                        return new List<List<Exercise>> {
                // День 1 (Понедельник)
                new List<Exercise>
                {
                    FindExercise("Приседания со штангой на спине"),   // 1
                    FindExercise("Жим лежа"),                         // 2
                    FindExercise("Тяга штанги в наклоне"),            // 3
                    FindExercise("Скручивания на римском стуле")      // 4
                },
                
                // День 2 (Среда)
                new List<Exercise>
                {
                    FindExercise("Приседания со штангой на спине"),   // 1
                    FindExercise("Жим гантелей на наклонной скамье"), // 2
                    FindExercise("Становая тяга"),                    // 3
                    FindExercise("Отжимания на брусьях с акцентом на грудь") // 4
                },
                
                // День 3 (Пятница)
                new List<Exercise>
                {
                    FindExercise("Приседания со штангой на спине"),   // 1
                    FindExercise("Жим лежа"),                         // 2
                    FindExercise("Тяга Т-грифа с упором в грудь"),    // 3
                    FindExercise("Подъем штанги на бицепс стоя"),     // 4
                    FindExercise("Скручивания на римском стуле")      // 5
                }
            };
                    }

                // === 4 ДНЯ В НЕДЕЛЮ ===
                case 4:
                    {
                        Week.week.Days[0].programName = "ЖИМ ЛЕЖА (5/3/1)";
                        Week.week.Days[1].programName = "СТАНОВАЯ (5/3/1)";
                        Week.week.Days[3].programName = "ЖИМ СТОЯ (5/3/1)";
                        Week.week.Days[4].programName = "ПРИСЕДАНИЯ (5/3/1)";
                        return new List<List<Exercise>> {
                // День 1: ЖИМ ЛЕЖА
                new List<Exercise>
                {
                    FindExercise("Жим лежа"),                         // 1
                    FindExercise("Жим узким хватом"),                 // 2
                    FindExercise("Тяга верхнего блока широким хватом к груди"), // 3
                    FindExercise("Молотковые сгибания с гантелями")   // 4
                },
                
                // День 2: СТАНОВАЯ ТЯГА
                new List<Exercise>
                {
                    FindExercise("Становая тяга"),                    // 1
                    FindExercise("Румынская тяга"),                   // 2
                    FindExercise("Шраги со штангой сзади"),           // 3
                    FindExercise("Скручивания на римском стуле")      // 4
                },
                
                // День 3: ЖИМ СТОЯ
                new List<Exercise>
                {
                    FindExercise("Армейский жим стоя"),               // 1
                    FindExercise("Махи гантелями в наклоне"),         // 2
                    FindExercise("Подтягивания широким хватом"),      // 3
                    FindExercise("Подъемы ног в висе")                // 4
                },
                
                // День 4: ПРИСЕДАНИЯ
                new List<Exercise>
                {
                    FindExercise("Приседания со штангой на спине"),   // 1
                    FindExercise("Сгибания ног лежа в тренажере"),    // 2
                    FindExercise("Подъемы на носки стоя в тренажере"),// 3
                    FindExercise("Гиперэкстензия с дополнительным весом"), // 4
                    FindExercise("Планка на предплечьях")             // 5
                }
            };
                    }

                // === 5 ДНЕЙ В НЕДЕЛЮ ===
                case 5:
                    {
                        Week.week.Days[0].programName = "ПРИСЕД (Тяжелый)";
                        Week.week.Days[1].programName = "ЖИМ ЛЕЖА (Тяжелый)";
                        Week.week.Days[2].programName = "СТАНОВАЯ (Тяжелая)";
                        Week.week.Days[3].programName = "ВСПОМОГАТЕЛЬНЫЙ";
                        Week.week.Days[4].programName = "СЛАБЫЕ ЗВЕНЬЯ";
                        return new List<List<Exercise>> {
                // День 1: ПРИСЕД
                new List<Exercise>
                {
                    FindExercise("Приседания со штангой на спине"),   // 1
                    FindExercise("Приседания со штангой на спине"),   // 2 (фронтальные - заменяем на обычные)
                    FindExercise("Румынская тяга"),                   // 3
                    FindExercise("Подъемы на носки стоя в тренажере") // 4
                },
                
                // День 2: ЖИМ ЛЕЖА
                new List<Exercise>
                {
                    FindExercise("Жим лежа"),                         // 1
                    FindExercise("Жим гантелей на наклонной скамье"), // 2
                    FindExercise("Отжимания на брусьях с акцентом на грудь"), // 3
                    FindExercise("Французский жим лежа (EZ-гриф)")    // 4
                },
                
                // День 3: СТАНОВАЯ
                new List<Exercise>
                {
                    FindExercise("Становая тяга"),                    // 1
                    FindExercise("Тяга штанги в наклоне"),            // 2
                    FindExercise("Шраги со штангой сзади"),           // 3
                    FindExercise("Подъем штанги на бицепс стоя")      // 4
                },
                
                // День 4: ВСПОМОГАТЕЛЬНЫЙ (Объем)
                new List<Exercise>
                {
                    FindExercise("Жим ногами в тренажере"),           // 1
                    FindExercise("Жим гантелей на наклонной скамье"), // 2
                    FindExercise("Тяга верхнего блока широким хватом к груди"), // 3
                    FindExercise("Махи гантелями в стороны стоя"),    // 4
                    FindExercise("Скручивания на римском стуле")      // 5
                },
                
                // День 5: СЛАБЫЕ ЗВЕНЬЯ
                new List<Exercise>
                {
                    FindExercise("Махи гантелями в наклоне"),         // 1
                    FindExercise("Сгибания ног лежа в тренажере"),    // 2
                    FindExercise("Разведение гантелей в наклоне"),    // 3
                    FindExercise("Сгибание запястий со штангой сидя"),// 4
                    FindExercise("Подъемы ног в висе"),               // 5
                    FindExercise("Гиперэкстензия с дополнительным весом") // 6
                }
            };
                    }

                default:
                    throw new ArgumentException($"Неподдерживаемое количество дней: {nums}");
            }
        else if (goal == Goal.Flexibility) switch (nums)
            {
                // === 1 ДЕНЬ В НЕДЕЛЮ ===
                case 1:
                    {
                        Week.week.Days[2].programName = "ПОЛНАЯ РАСТЯЖКА ВСЕГО ТЕЛА";
                        return new List<List<Exercise>> {
                new List<Exercise>
                {
                    // Растяжка ног
                    FindExercise("Растяжка квадрицепса стоя"),
                    FindExercise("Растяжка бицепса бедра сидя"),
                    FindExercise("Растяжка ягодичных сидя скрестив ноги"),
                    FindExercise("Растяжка икр у стены"),
                    
                    // Растяжка спины
                    FindExercise("Растяжка широчайших в висе на турнике"),
                    FindExercise("Растяжка трапеций наклон головы вбок"),
                    FindExercise("Растяжка ромбовидных обхват себя руками"),
                    FindExercise("Растяжка поясницы кошка-корова"),
                    
                    // Растяжка груди
                    FindExercise("Растяжка верхней части груди у стены"),
                    FindExercise("Растяжка середины груди в дверном проеме"),
                    FindExercise("Растяжка нижней части груди на фитболе"),
                    FindExercise("Растяжка внутренней части груди (ладони вместе)"),
                    
                    // Растяжка плеч
                    FindExercise("Растяжка передних дельт за спиной"),
                    FindExercise("Растяжка средних дельт через руку"),
                    FindExercise("Растяжка задних дельт обхват плеча"),
                    
                    // Растяжка рук
                    FindExercise("Растяжка бицепса у стены"),
                    FindExercise("Растяжка трицепса за головой"),
                    FindExercise("Растяжка предплечий ладонью вниз"),
                    
                    // Растяжка кора
                    FindExercise("Растяжка верхнего пресса лежа на животе"),
                    FindExercise("Растяжка нижнего пресса кобра"),
                    FindExercise("Растяжка косых мышц в боковом наклоне"),
                    
                    // Растяжка шеи
                    FindExercise("Изометрическая растяжка шеи в стороны")
                }
            };
                    }

                // === 2 ДНЯ В НЕДЕЛЮ ===
                case 2:
                    {
                        Week.week.Days[1].programName = "РАСТЯЖКА НОГ И КОРА";
                        Week.week.Days[3].programName = "РАСТЯЖКА ВЕРХНЕЙ ЧАСТИ ТЕЛА";
                        return new List<List<Exercise>> {
                // День 1: НОГИ И КОР
                new List<Exercise>
                {
                    FindExercise("Растяжка квадрицепса стоя"),
                    FindExercise("Растяжка бицепса бедра сидя"),
                    FindExercise("Растяжка ягодичных сидя скрестив ноги"),
                    FindExercise("Растяжка икр у стены"),
                    FindExercise("Растяжка верхнего пресса лежа на животе"),
                    FindExercise("Растяжка нижнего пресса кобра"),
                    FindExercise("Растяжка косых мышц в боковом наклоне"),
                    FindExercise("Растяжка поясницы кошка-корова")
                },
                
                // День 2: ВЕРХНЯЯ ЧАСТЬ ТЕЛА
                new List<Exercise>
                {
                    FindExercise("Растяжка широчайших в висе на турнике"),
                    FindExercise("Растяжка трапеций наклон головы вбок"),
                    FindExercise("Растяжка ромбовидных обхват себя руками"),
                    FindExercise("Растяжка верхней части груди у стены"),
                    FindExercise("Растяжка середины груди в дверном проеме"),
                    FindExercise("Растяжка нижней части груди на фитболе"),
                    FindExercise("Растяжка передних дельт за спиной"),
                    FindExercise("Растяжка средних дельт через руку"),
                    FindExercise("Растяжка задних дельт обхват плеча"),
                    FindExercise("Растяжка бицепса у стены"),
                    FindExercise("Растяжка трицепса за головой"),
                    FindExercise("Растяжка предплечий ладонью вниз"),
                    FindExercise("Изометрическая растяжка шеи в стороны")
                }
            };
                    }

                // === 3 ДНЯ В НЕДЕЛЮ ===
                case 3:
                    {
                        Week.week.Days[0].programName = "РАСТЯЖКА НОГ И ТАЗА";
                        Week.week.Days[2].programName = "ГИБКОСТЬ СПИНЫ И ГРУДИ";
                        Week.week.Days[4].programName = "МОБИЛЬНОСТЬ ПЛЕЧ И РУК";
                        return new List<List<Exercise>> {
                // День 1: НОГИ И ТАЗ
                new List<Exercise>
                {
                    FindExercise("Растяжка квадрицепса стоя"),
                    FindExercise("Растяжка бицепса бедра сидя"),
                    FindExercise("Растяжка ягодичных сидя скрестив ноги"),
                    FindExercise("Растяжка икр у стены"),
                    FindExercise("Растяжка верхнего пресса лежа на животе"),
                    FindExercise("Растяжка нижнего пресса кобра"),
                    FindExercise("Растяжка косых мышц в боковом наклоне")
                },
                
                // День 2: СПИНА И ГРУДЬ
                new List<Exercise>
                {
                    FindExercise("Растяжка широчайших в висе на турнике"),
                    FindExercise("Растяжка трапеций наклон головы вбок"),
                    FindExercise("Растяжка ромбовидных обхват себя руками"),
                    FindExercise("Растяжка поясницы кошка-корова"),
                    FindExercise("Растяжка верхней части груди у стены"),
                    FindExercise("Растяжка середины груди в дверном проеме"),
                    FindExercise("Растяжка нижней части груди на фитболе"),
                    FindExercise("Растяжка внутренней части груди (ладони вместе)")
                },
                
                // День 3: ПЛЕЧИ И РУКИ
                new List<Exercise>
                {
                    FindExercise("Растяжка передних дельт за спиной"),
                    FindExercise("Растяжка средних дельт через руку"),
                    FindExercise("Растяжка задних дельт обхват плеча"),
                    FindExercise("Растяжка бицепса у стены"),
                    FindExercise("Растяжка трицепса за головой"),
                    FindExercise("Растяжка предплечий ладонью вниз"),
                    FindExercise("Изометрическая растяжка шеи в стороны")
                }
            };
                    }

                // === 4 ДНЯ В НЕДЕЛЮ ===
                case 4:
                    {
                        Week.week.Days[0].programName = "ГИБКОСТЬ НОГ";
                        Week.week.Days[1].programName = "РАСТЯЖКА СПИНЫ И КОРА";
                        Week.week.Days[3].programName = "РАСКРЫТИЕ ГРУДИ И ПЛЕЧ";
                        Week.week.Days[4].programName = "МОБИЛЬНОСТЬ РУК И ШЕИ";
                        return new List<List<Exercise>> {
                // День 1: НОГИ
                new List<Exercise>
                {
                    FindExercise("Растяжка квадрицепса стоя"),
                    FindExercise("Растяжка бицепса бедра сидя"),
                    FindExercise("Растяжка ягодичных сидя скрестив ноги"),
                    FindExercise("Растяжка икр у стены")
                },
                
                // День 2: СПИНА И КОР
                new List<Exercise>
                {
                    FindExercise("Растяжка широчайших в висе на турнике"),
                    FindExercise("Растяжка трапеций наклон головы вбок"),
                    FindExercise("Растяжка ромбовидных обхват себя руками"),
                    FindExercise("Растяжка поясницы кошка-корова"),
                    FindExercise("Растяжка верхнего пресса лежа на животе"),
                    FindExercise("Растяжка нижнего пресса кобра"),
                    FindExercise("Растяжка косых мышц в боковом наклоне")
                },
                
                // День 3: ГРУДЬ И ПЛЕЧИ
                new List<Exercise>
                {
                    FindExercise("Растяжка верхней части груди у стены"),
                    FindExercise("Растяжка середины груди в дверном проеме"),
                    FindExercise("Растяжка нижней части груди на фитболе"),
                    FindExercise("Растяжка внутренней части груди (ладони вместе)"),
                    FindExercise("Растяжка передних дельт за спиной"),
                    FindExercise("Растяжка средних дельт через руку"),
                    FindExercise("Растяжка задних дельт обхват плеча")
                },
                
                // День 4: РУКИ И ШЕЯ
                new List<Exercise>
                {
                    FindExercise("Растяжка бицепса у стены"),
                    FindExercise("Растяжка трицепса за головой"),
                    FindExercise("Растяжка предплечий ладонью вниз"),
                    FindExercise("Изометрическая растяжка шеи в стороны")
                }
            };
                    }

                // === 5 ДНЕЙ В НЕДЕЛЮ ===
                case 5:
                    {
                        Week.week.Days[0].programName = "РАСТЯЖКА ПЕРЕДНИХ МЫШЦ БЕДРА";
                        Week.week.Days[1].programName = "РАСТЯЖКА ЗАДНИХ МЫШЦ НОГ";
                        Week.week.Days[2].programName = "ГИБКОСТЬ СПИНЫ И ПОЯСНИЦЫ";
                        Week.week.Days[3].programName = "РАСКРЫТИЕ ГРУДНОГО ОТДЕЛА";
                        Week.week.Days[4].programName = "МОБИЛЬНОСТЬ ПЛЕЧЕВОГО ПОЯСА";
                        return new List<List<Exercise>> {
                // День 1: ПЕРЕДНИЕ МЫШЦЫ БЕДРА
                new List<Exercise>
                {
                    FindExercise("Растяжка квадрицепса стоя"),
                    FindExercise("Растяжка верхнего пресса лежа на животе"),
                    FindExercise("Растяжка нижнего пресса кобра")
                },
                
                // День 2: ЗАДНИЕ МЫШЦЫ НОГ
                new List<Exercise>
                {
                    FindExercise("Растяжка бицепса бедра сидя"),
                    FindExercise("Растяжка ягодичных сидя скрестив ноги"),
                    FindExercise("Растяжка икр у стены"),
                    FindExercise("Растяжка поясницы кошка-корова")
                },
                
                // День 3: СПИНА И ПОЯСНИЦА
                new List<Exercise>
                {
                    FindExercise("Растяжка широчайших в висе на турнике"),
                    FindExercise("Растяжка трапеций наклон головы вбок"),
                    FindExercise("Растяжка ромбовидных обхват себя руками"),
                    FindExercise("Растяжка косых мышц в боковом наклоне")
                },
                
                // День 4: ГРУДНОЙ ОТДЕЛ
                new List<Exercise>
                {
                    FindExercise("Растяжка верхней части груди у стены"),
                    FindExercise("Растяжка середины груди в дверном проеме"),
                    FindExercise("Растяжка нижней части груди на фитболе"),
                    FindExercise("Растяжка внутренней части груди (ладони вместе)"),
                    FindExercise("Растяжка передних дельт за спиной")
                },
                
                // День 5: ПЛЕЧЕВОЙ ПОЯС
                new List<Exercise>
                {
                    FindExercise("Растяжка средних дельт через руку"),
                    FindExercise("Растяжка задних дельт обхват плеча"),
                    FindExercise("Растяжка бицепса у стены"),
                    FindExercise("Растяжка трицепса за головой"),
                    FindExercise("Растяжка предплечий ладонью вниз"),
                    FindExercise("Изометрическая растяжка шеи в стороны")
                }
            };
                    }

                default:
                    throw new ArgumentException($"Неподдерживаемое количество дней: {nums}");
            }
        else if (goal == Goal.IncreasedEndurance) switch (nums)
            {
                // === 1 ДЕНЬ В НЕДЕЛЮ ===
                case 1:
                    {
                        Week.week.Days[2].programName = "КРУГОВАЯ ТРЕНИРОВКА НА ВЫНОСЛИВОСТЬ";
                        return new List<List<Exercise>> {
                new List<Exercise>
                {
                    FindExercise("Приседания с собственным весом"),
                    FindExercise("Отжимания от пола (классические)"),
                    FindExercise("Подтягивания широким хватом"),
                    FindExercise("Выпады со штангой"), // Для ягодичных
                    FindExercise("Планка на предплечьях"),
                    FindExercise("Скручивания на римском стуле")
                }
            };
                    }

                // === 2 ДНЯ В НЕДЕЛЮ ===
                case 2:
                    {
                        Week.week.Days[1].programName = "ВЫНОСЛИВОСТЬ ВЕРХНЕЙ ЧАСТИ ТЕЛА";
                        Week.week.Days[3].programName = "ВЫНОСЛИВОСТЬ НИЖНЕЙ ЧАСТИ ТЕЛА";
                        return new List<List<Exercise>> {
                // День 1: ВЕРХ
                new List<Exercise>
                {
                    FindExercise("Отжимания от пола (классические)"),
                    FindExercise("Подтягивания широким хватом"),
                    FindExercise("Отжимания на брусьях с акцентом на трицепс"),
                    FindExercise("Подтягивания обратным хватом"),
                    FindExercise("Отжимания в стойке у стены (плечи)")
                },
                
                // День 2: НИЗ
                new List<Exercise>
                {
                    FindExercise("Приседания с собственным весом"),
                    FindExercise("Румынская тяга"),
                    FindExercise("Выпады со штангой"),
                    FindExercise("Скакалка"),
                    FindExercise("Планка на предплечьях")
                }
            };
                    }

                // === 3 ДНЯ В НЕДЕЛЮ ===
                case 3:
                    {
                        Week.week.Days[0].programName = "ТОЛКАЮЩИЕ ДВИЖЕНИЯ";
                        Week.week.Days[2].programName = "ТЯНУЩИЕ ДВИЖЕНИЯ";
                        Week.week.Days[4].programName = "НОГИ И КОР";
                        return new List<List<Exercise>> {
                // День 1: ТОЛКАЙ (Push)
                new List<Exercise>
                {
                    FindExercise("Отжимания от пола (классические)"),
                    FindExercise("Алмазные отжимания (узкий хват)"),
                    FindExercise("Отжимания в стойке у стены (плечи)"),
                    FindExercise("Отжимания от пола (классические)") // Альтернатива
                },
                
                // День 2: ТЯНИ (Pull)
                new List<Exercise>
                {
                    FindExercise("Подтягивания широким хватом"),
                    FindExercise("Подтягивания (стандартный хват)"),
                    FindExercise("Подтягивания обратным хватом"),
                    FindExercise("Подтягивания широким хватом") // Альтернатива
                },
                
                // День 3: НОГИ И КОР (Legs & Core)
                new List<Exercise>
                {
                    FindExercise("Приседания с собственным весом"),
                    FindExercise("Выпады со штангой"),
                    FindExercise("Ягодичный мост со штангой"),
                    FindExercise("Берпи (Burpees)"),
                    FindExercise("Планка на предплечьях")
                }
            };
                    }

                // === 4 ДНЯ В НЕДЕЛЮ ===
                case 4:
                    {
                        Week.week.Days[0].programName = "ГРУДЬ+ТРИЦЕПС (Толкай)";
                        Week.week.Days[1].programName = "СПИНА+БИЦЕПС (Тяни)";
                        Week.week.Days[3].programName = "НОГИ+ПЛЕЧИ";
                        Week.week.Days[4].programName = "ФУНКЦИОНАЛЬНАЯ ВЫНОСЛИВОСТЬ";
                        return new List<List<Exercise>> {
                // День 1: Грудь + Трицепс
                new List<Exercise>
                {
                    FindExercise("Отжимания от пола (классические)"),
                    FindExercise("Отжимания на брусьях с акцентом на трицепс"),
                    FindExercise("Отжимания в стойке у стены (плечи)")
                },
                
                // День 2: Спина + Бицепс
                new List<Exercise>
                {
                    FindExercise("Подтягивания широким хватом"),
                    FindExercise("Подтягивания обратным хватом"),
                    FindExercise("Шраги со штангой сзади")
                },
                
                // День 3: Ноги + Плечи
                new List<Exercise>
                {
                    FindExercise("Приседания с собственным весом"),
                    FindExercise("Ягодичный мост со штангой"),
                    FindExercise("Махи гантелями в стороны стоя"),
                    FindExercise("Скакалка")
                },
                
                // День 4: Функциональная выносливость
                new List<Exercise>
                {
                    FindExercise("Берпи (Burpees)"),
                    FindExercise("Планка на предплечьях"),
                    FindExercise("Приседания с собственным весом"),  // Для берпи
                    FindExercise("Отжимания от пола (классические)") // Для берпи/отжиманий
                }
            };
                    }

                // === 5 ДНЕЙ В НЕДЕЛЮ ===
                case 5:
                    {
                        Week.week.Days[0].programName = "ТОЛКАЮЩИЕ УПРАЖНЕНИЯ";
                        Week.week.Days[1].programName = "ТЯНУЩИЕ УПРАЖНЕНИЯ";
                        Week.week.Days[2].programName = "НОГИ И КОР (Силовая выносливость)";
                        Week.week.Days[3].programName = "ВЕРХ ТЕЛА (Круговая)";
                        Week.week.Days[4].programName = "ФУНКЦИОНАЛЬНАЯ ТРЕНИРОВКА";
                        return new List<List<Exercise>> {
                // День 1: Толкающие
                new List<Exercise>
                {
                    FindExercise("Отжимания от пола (классические)"),
                    FindExercise("Отжимания на брусьях с акцентом на трицепс")
                },
                
                // День 2: Тянущие
                new List<Exercise>
                {
                    FindExercise("Подтягивания широким хватом"),
                    FindExercise("Подтягивания обратным хватом")
                },
                
                // День 3: Ноги и кор
                new List<Exercise>
                {
                    FindExercise("Приседания с собственным весом"),
                    FindExercise("Берпи (Burpees)")
                },
                
                // День 4: Верх тела круговая
                new List<Exercise>
                {
                    FindExercise("Отжимания в стойке у стены (плечи)"),
                    FindExercise("Шраги со штангой сзади"),
                    FindExercise("Шраги в висе на турнике")
                },
                
                // День 5: Функциональная (берпи, скакалка и т.д.)
                new List<Exercise>
                {
                    FindExercise("Берпи (Burpees)"),  // Берпи
                    FindExercise("Скакалка"),         // Скакалка
                    FindExercise("Планка на предплечьях") // Стабилизация
                }
            };
                    }

                default:
                    throw new ArgumentException($"Неподдерживаемое количество дней: {nums}");
            }
        else switch (nums)
            {
                // === 1 ДЕНЬ В НЕДЕЛЮ ===
                case 1:
                    {
                        Week.week.Days[2].programName = "Фулл-бади. Интенсивность!";
                        return new List<List<Exercise>> {
                new List<Exercise>
                {
                    FindExercise("Приседания со штангой на спине"),   // 1
                    FindExercise("Подтягивания широким хватом"),      // 2
                    FindExercise("Жим лежа"),                         // 3
                    FindExercise("Румынская тяга"),                   // 4
                    FindExercise("Махи гантелями в стороны стоя"),    // 5
                    FindExercise("Жим узким хватом"),                 // 6
                    FindExercise("Подъем штанги на бицепс стоя"),     // 7
                    FindExercise("Скручивания на римском стуле")      // 8
                }
            };
                    }

                // === 2 ДНЯ В НЕДЕЛЮ ===
                case 2:
                    {
                        Week.week.Days[1].programName = "ВЕРХ (Тяги + Жимы)";
                        Week.week.Days[3].programName = "НИЗ + Пресс";
                        return new List<List<Exercise>> {
                // День 1: ВЕРХ
                new List<Exercise>
                {
                    FindExercise("Подтягивания широким хватом"),      // 1
                    FindExercise("Шраги со штангой сзади"),           // 2
                    FindExercise("Махи гантелями в наклоне"),         // 3
                    FindExercise("Подъем штанги на бицепс стоя"),     // 4
                    FindExercise("Жим лежа"),                         // 5
                    FindExercise("Жим гантелей на наклонной скамье"), // 6
                    FindExercise("Жим гантелей сидя"),                // 7
                    FindExercise("Французский жим лежа (EZ-гриф)")    // 8
                },
                
                // День 2: НИЗ
                new List<Exercise>
                {
                    FindExercise("Приседания со штангой на спине"),   // 1
                    FindExercise("Сгибания ног лежа в тренажере"),    // 2
                    FindExercise("Ягодичный мост со штангой"),        // 3
                    FindExercise("Подъемы на носки стоя в тренажере"),// 4
                    FindExercise("Гиперэкстензия с дополнительным весом"), // 5
                    FindExercise("Скручивания на римском стуле")      // 6
                }
            };
                    }

                // === 3 ДНЯ В НЕДЕЛЮ ===
                case 3:
                    {
                        Week.week.Days[0].programName = "ТЯНИ (Спина, Бицепс, Задние дельты)";
                        Week.week.Days[2].programName = "ЖМИ (Грудь, Трицепс, Плечи)";
                        Week.week.Days[4].programName = "НОГИ + Пресс";
                        return new List<List<Exercise>> {
                // День 1: ТЯНИ (PULL)
                new List<Exercise>
                {
                    FindExercise("Тяга верхнего блока широким хватом к груди"), // 1
                    FindExercise("Шраги со штангой сзади"),           // 2
                    FindExercise("Тяга штанги в наклоне"),            // 3
                    FindExercise("Махи гантелями в наклоне"),         // 4
                    FindExercise("Подъем штанги на бицепс стоя"),     // 5
                    FindExercise("Молотковые сгибания с гантелями"),  // 6
                    FindExercise("Сгибание запястий со штангой сидя") // 7
                },
                
                // День 2: ЖМИ (PUSH)
                new List<Exercise>
                {
                    FindExercise("Жим гантелей на наклонной скамье"), // 1
                    FindExercise("Жим лежа"),                         // 2
                    FindExercise("Махи гантелями в стороны стоя"),    // 3
                    FindExercise("Подъемы гантелей перед собой"),     // 4
                    FindExercise("Жим узким хватом"),                 // 5
                    FindExercise("Французский жим лежа (EZ-гриф)")    // 6
                },
                
                // День 3: НОГИ (LEGS)
                new List<Exercise>
                {
                    FindExercise("Приседания со штангой на спине"),   // 1
                    FindExercise("Сгибания ног лежа в тренажере"),    // 2
                    FindExercise("Ягодичный мост со штангой"),        // 3
                    FindExercise("Подъемы на носки стоя в тренажере"),// 4
                    FindExercise("Гиперэкстензия с дополнительным весом"), // 5
                    FindExercise("Скручивания на римском стуле"),     // 6
                    FindExercise("Подъемы ног в висе")                // 7
                }
            };
                    }

                // === 4 ДНЯ В НЕДЕЛЮ ===
                case 4:
                    {
                        Week.week.Days[0].programName = "ВЕРХ (А) - Грудь/Спина акцент";
                        Week.week.Days[1].programName = "НИЗ (А) - Квадрицепсы акцент";
                        Week.week.Days[3].programName = "ВЕРХ (Б) - Плечи/Руки акцент";
                        Week.week.Days[4].programName = "НИЗ (Б) - Бицепс бедра/Ягодицы акцент";
                        return new List<List<Exercise>> {
                // День 1: ВЕРХ (А) - Горизонтальные тяги и жимы
                new List<Exercise>
                {
                    FindExercise("Жим лежа"),                         // 1
                    FindExercise("Тяга штанги в наклоне"),            // 2
                    FindExercise("Жим гантелей на наклонной скамье"), // 3
                    FindExercise("Тяга Т-грифа с упором в грудь"),    // 4
                    FindExercise("Отжимания на брусьях с акцентом на грудь") // 5
                },
                
                // День 2: НИЗ (А) - Передняя цепь (Квадрицепс-доминантные)
                new List<Exercise>
                {
                    FindExercise("Приседания со штангой на спине"),   // 1
                    FindExercise("Разгибания ног в тренажере"),       // 2
                    FindExercise("Выпады со штангой"),                // 3
                    FindExercise("Подъемы на носки стоя в тренажере"),// 4
                    FindExercise("Скручивания на римском стуле")      // 5
                },
                
                // День 3: ВЕРХ (Б) - Вертикальные тяги и жимы + изоляция
                new List<Exercise>
                {
                    FindExercise("Подтягивания широким хватом"),      // 1
                    FindExercise("Жим гантелей сидя"),                // 2
                    FindExercise("Махи гантелями в наклоне"),         // 3
                    FindExercise("Подъем штанги на бицепс стоя"),     // 4
                    FindExercise("Молотковые сгибания с гантелями"),  // 5
                    FindExercise("Разгибания на трицепс в верхнем блоке с канатом") // 6
                },
                
                // День 4: НИЗ (Б) - Задняя цепь (Бицепс бедра-доминантные)
                new List<Exercise>
                {
                    FindExercise("Румынская тяга"),                   // 1
                    FindExercise("Ягодичный мост со штангой"),        // 2
                    FindExercise("Сгибания ног лежа в тренажере"),    // 3
                    FindExercise("Подъемы на носки сидя"),            // 4
                    FindExercise("Гиперэкстензия с дополнительным весом"), // 5
                    FindExercise("Подъемы ног в висе")                // 6
                }
            };
                    }

                // === 5 ДНЕЙ В НЕДЕЛЮ ===
                case 5:
                    {
                        Week.week.Days[0].programName = "ТЯНИ (А) - Спина ширина";
                        Week.week.Days[1].programName = "ЖМИ (А) - Грудь объем";
                        Week.week.Days[2].programName = "НОГИ (А) - Квадрицепсы";
                        Week.week.Days[3].programName = "ТЯНИ (Б) - Спина толщина + Бицепс";
                        Week.week.Days[4].programName = "ЖМИ (Б) - Плечи + Трицепс";
                        return new List<List<Exercise>> {
                // День 1: ТЯНИ (А) - Вертикальные тяги (ширина)
                new List<Exercise>
                {
                    FindExercise("Тяга верхнего блока широким хватом к груди"), // 1
                    FindExercise("Махи гантелями в наклоне"),         // 2
                    FindExercise("Шраги со штангой сзади")            // 3
                },
                
                // День 2: ЖМИ (А) - Грудь + Трицепс
                new List<Exercise>
                {
                    FindExercise("Жим гантелей на наклонной скамье"), // 1
                    FindExercise("Жим лежа"),                         // 2
                    FindExercise("Жим узким хватом")                  // 3
                },
                
                // День 3: НОГИ (А) - Квадрицепсы акцент
                new List<Exercise>
                {
                    FindExercise("Приседания со штангой на спине"),   // 1
                    FindExercise("Подъемы на носки стоя в тренажере"),// 2
                    FindExercise("Скручивания на римском стуле")      // 3
                },
                
                // День 4: ТЯНИ (Б) - Горизонтальные тяги + Бицепс
                new List<Exercise>
                {
                    FindExercise("Тяга штанги в наклоне"),            // 1
                    FindExercise("Подъем штанги на бицепс стоя"),     // 3
                    FindExercise("Молотковые сгибания с гантелями"),  // 4
                    FindExercise("Сгибание запястий со штангой сидя") // 5
                },
                
                // День 5: ЖМИ (Б) - Плечи + Задние дельты
                new List<Exercise>
                {
                    FindExercise("Жим гантелей сидя"),                // 1
                    FindExercise("Разведения в тренажере Peck-Deck"), // 2
                    FindExercise("Подъемы гантелей перед собой"),     // 3
                    FindExercise("Французский жим лежа (EZ-гриф)"),   // 4
                    FindExercise("Подъемы ног в висе")                // 5
                }
            };
                    }

                default:
                    throw new ArgumentException($"Неподдерживаемое количество дней: {nums}");
            }
    }
    private static Dictionary<int, string> GetWorkoutNames(int numberOfWorkouts, Goal workoutGoal)
    {
        var WorkoutNames = new Dictionary<int, string>();

        // Список дней для тренировок
        var TrainingDays = GetDaysList(numberOfWorkouts);

        if (workoutGoal == Goal.IncreasedStrength) switch (numberOfWorkouts)
            {
                case 1:
                    WorkoutNames[TrainingDays[0]] = "База (Сила)";
                    break;

                case 2:
                    WorkoutNames[TrainingDays[0]] = "Жимы (Присед + Жим Лежа)";
                    WorkoutNames[TrainingDays[1]] = "Тяги (Становая + Подтягивания)";
                    break;

                case 3:
                    WorkoutNames[TrainingDays[0]] = "Присед + Жим + Тяга";
                    WorkoutNames[TrainingDays[1]] = "Присед + Жим Лежа + Становая";
                    WorkoutNames[TrainingDays[2]] = "Присед + Жим + Тяга Грудная";
                    break;

                case 4:
                    WorkoutNames[TrainingDays[0]] = "Жим Лежа (5/3/1)";
                    WorkoutNames[TrainingDays[1]] = "Становая (5/3/1)";
                    WorkoutNames[TrainingDays[2]] = "Жим Стоя (5/3/1)";
                    WorkoutNames[TrainingDays[3]] = "Приседания (5/3/1)";
                    break;

                case 5:
                    WorkoutNames[TrainingDays[0]] = "Присед (Тяжелый)";
                    WorkoutNames[TrainingDays[1]] = "Жим Лежа (Тяжелый)";
                    WorkoutNames[TrainingDays[2]] = "Становая (Тяжелая)";
                    WorkoutNames[TrainingDays[3]] = "Вспомогательный";
                    WorkoutNames[TrainingDays[4]] = "Слабые Звенья";
                    break;
            }
        else if (workoutGoal == Goal.Flexibility) switch (numberOfWorkouts)
            {
                case 1:
                    WorkoutNames[TrainingDays[0]] = "Полная Растяжка Всего Тела";
                    break;

                case 2:
                    WorkoutNames[TrainingDays[0]] = "Растяжка Ног И Кора";
                    WorkoutNames[TrainingDays[1]] = "Растяжка Верхней Части Тела";
                    break;

                case 3:
                    WorkoutNames[TrainingDays[0]] = "Растяжка Ног И Таза";
                    WorkoutNames[TrainingDays[1]] = "Гибкость Спины И Груди";
                    WorkoutNames[TrainingDays[2]] = "Мобильность Плеч И Рук";
                    break;

                case 4:
                    WorkoutNames[TrainingDays[0]] = "Гибкость Ног";
                    WorkoutNames[TrainingDays[1]] = "Растяжка Спины И Кора";
                    WorkoutNames[TrainingDays[2]] = "Раскрытие Груди И Плеч";
                    WorkoutNames[TrainingDays[3]] = "Мобильность Рук И Шеи";
                    break;

                case 5:
                    WorkoutNames[TrainingDays[0]] = "Растяжка Передних Мышц Бедра";
                    WorkoutNames[TrainingDays[1]] = "Растяжка Задних Мышц Ног";
                    WorkoutNames[TrainingDays[2]] = "Гибкость Спины И Поясницы";
                    WorkoutNames[TrainingDays[3]] = "Раскрытие Грудного Отдела";
                    WorkoutNames[TrainingDays[4]] = "Мобильность Плечевого Пояса";
                    break;
            }
        else if (workoutGoal == Goal.IncreasedEndurance) switch (numberOfWorkouts)
            {
                case 1:
                    WorkoutNames[TrainingDays[0]] = "Круговая Тренировка На Выносливость";
                    break;

                case 2:
                    WorkoutNames[TrainingDays[0]] = "Выносливость Верхней Части Тела";
                    WorkoutNames[TrainingDays[1]] = "Выносливость Нижней Части Тела";
                    break;

                case 3:
                    WorkoutNames[TrainingDays[0]] = "Толкающие Движения";
                    WorkoutNames[TrainingDays[1]] = "Тянущие Движения";
                    WorkoutNames[TrainingDays[2]] = "Ноги И Кор";
                    break;

                case 4:
                    WorkoutNames[TrainingDays[0]] = "Грудь+Трицепс (Толкай)";
                    WorkoutNames[TrainingDays[1]] = "Спина+Бицепс (Тяни)";
                    WorkoutNames[TrainingDays[2]] = "Ноги+Плечи";
                    WorkoutNames[TrainingDays[3]] = "Функциональная Выносливость";
                    break;

                case 5:
                    WorkoutNames[TrainingDays[0]] = "Толкающие Упражнения";
                    WorkoutNames[TrainingDays[1]] = "Тянущие Упражнения";
                    WorkoutNames[TrainingDays[2]] = "Ноги И Кор (Силовая Выносливость)";
                    WorkoutNames[TrainingDays[3]] = "Верх Тела (Круговая)";
                    WorkoutNames[TrainingDays[4]] = "Функциональная Тренировка";
                    break;
            }
        else switch (numberOfWorkouts) // Для цели по умолчанию
            {
                case 1:
                    WorkoutNames[TrainingDays[0]] = "Фулл-Бади. Интенсивность!";
                    break;

                case 2:
                    WorkoutNames[TrainingDays[0]] = "Верх (Тяги + Жимы)";
                    WorkoutNames[TrainingDays[1]] = "Низ + Пресс";
                    break;

                case 3:
                    WorkoutNames[TrainingDays[0]] = "Тяни (Спина, Бицепс, Задние Дельты)";
                    WorkoutNames[TrainingDays[1]] = "Жми (Грудь, Трицепс, Плечи)";
                    WorkoutNames[TrainingDays[2]] = "Ноги + Пресс";
                    break;

                case 4:
                    WorkoutNames[TrainingDays[0]] = "Верх (А) - Грудь/Спина Акцент";
                    WorkoutNames[TrainingDays[1]] = "Низ (А) - Квадрицепсы Акцент";
                    WorkoutNames[TrainingDays[2]] = "Верх (Б) - Плечи/Руки Акцент";
                    WorkoutNames[TrainingDays[3]] = "Низ (Б) - Бицепс Бедра/Ягодицы Акцент";
                    break;

                case 5:
                    WorkoutNames[TrainingDays[0]] = "Тяни (А) - Спина Ширина";
                    WorkoutNames[TrainingDays[1]] = "Жми (А) - Грудь Объем";
                    WorkoutNames[TrainingDays[2]] = "Ноги (А) - Квадрицепсы";
                    WorkoutNames[TrainingDays[3]] = "Тяни (Б) - Спина Толщина + Бицепс";
                    WorkoutNames[TrainingDays[4]] = "Жми (Б) - Плечи + Трицепс";
                    break;
            }

        return WorkoutNames;
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
