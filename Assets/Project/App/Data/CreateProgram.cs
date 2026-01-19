using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class CreateProgram : MonoBehaviour
{
    #region Методы для юнити
    [SerializeField] NumberSelectorUI daySelectorUI = null;
    [SerializeField] NumberSelectorUI difficlitySelectorUI = null;
    [SerializeField] ViewProgram ViewProgram;
    
    public void CreateTrening()
    {
        int value = System.Convert.ToInt32(daySelectorUI.value);
        float difficlity = (float)Convert.ToInt32(difficlitySelectorUI.value) / 100;
        StringBuilder stringBuilder = new StringBuilder();
        CreateProgram.CreateStrengthTraining(1 * difficlity, value, stringBuilder);
        ViewProgram.UpdateProgramNames();


        Debug.Log(stringBuilder.ToString());
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
        DB?.AppendLine(Player.player.treningParametrs.goal.ToString()); 



        int treningNum = 0;
        for (int i = 0; Week.week.Days.Count > i; i++)
        {

            if (GetDaysList(daysCount).Any(i1 => i1 == i))
            {
                List<Muscle> muscles = CreateSplitForDay(daysCount)[treningNum];
                float maxWA = GetMax(daysCount,intensity);

                List<SetOfExercises> setsOfExercises = DistributeExercises(muscles, MuscleGroup.GetPrimaryMyscleGroups(muscles), (int)maxWA,DB);
                Week.week.Days[i].setsOfExercises = setsOfExercises;

                Week.SaveDay(Week.week.Days[i]);
                DB?.AppendLine($"===День{i+1}/7 - добавленно {setsOfExercises.Count} упражнений===");

                treningNum++;
            }
                //Week.week.SetParametrs();
        }



	}
    public static void CreateFlexibilityTraining()
    {

    }
    private static List<List<Muscle>> CreateSplitForDay(int nums)
    {
        switch (nums)
        {
            case 1:
                {
                    Week.week.Days[2].programName = "Фулл бади";
                    return new List<List<Muscle>> { new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Квадрицепс"),
                        Muscle.GetMuscleByName("Широчайшие"),
                        Muscle.GetMuscleByName("Середина груди"),
                        Muscle.GetMuscleByName("Бицепс бедра"),
                        Muscle.GetMuscleByName("Ягодичные"),
                        Muscle.GetMuscleByName("Трицепс"),
                        Muscle.GetMuscleByName("Средние дельты"),
                    }};

                }
            case 2:
                {
                    Week.week.Days[1].programName = "Вверх";
                    Week.week.Days[3].programName = "Низ";
                    return new List<List<Muscle>> { new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Широчайшие"),
                        Muscle.GetMuscleByName("Середина груди"),
                        Muscle.GetMuscleByName("Средние дельты"),
                        Muscle.GetMuscleByName("Трицепс"),
                        Muscle.GetMuscleByName("Бицепс"),
                        Muscle.GetMuscleByName("Задние дельты"),
                    },
                        new List<Muscle>
                        {
                        Muscle.GetMuscleByName("Квадрицепс"),
                        Muscle.GetMuscleByName("Бицепс бедра"),
                        Muscle.GetMuscleByName("Ягодичные"),
                        Muscle.GetMuscleByName("Поясница"),
                        Muscle.GetMuscleByName("Икры"),
                        }
                    };
                }
            case 3:
                {
                    Week.week.Days[0].programName = "Вверх";
                    Week.week.Days[2].programName = "Низ";
                    Week.week.Days[4].programName = "Плечи,Руки";
                    return new List<List<Muscle>> { new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Широчайшие"),
                        Muscle.GetMuscleByName("Середина груди"),
                        Muscle.GetMuscleByName("Верх груди"),
                        Muscle.GetMuscleByName("Трапеции"),
                        Muscle.GetMuscleByName("Средние дельты"),
                    },
                        new List<Muscle>
                        {
                        Muscle.GetMuscleByName("Квадрицепс"),
                        Muscle.GetMuscleByName("Бицепс бедра"),
                        Muscle.GetMuscleByName("Ягодичные"),
                        Muscle.GetMuscleByName("Поясница"),
                        Muscle.GetMuscleByName("Икры"),
                    },

                         new List<Muscle>
                    {
                         Muscle.GetMuscleByName("Средние дельты"),
                         Muscle.GetMuscleByName("Трицепс"),
                         Muscle.GetMuscleByName("Бицепс"),
                         Muscle.GetMuscleByName("Задние дельты"),
                         Muscle.GetMuscleByName("Передние дельты"),
                    }
                };
            }
            case 4:
                {
                    Week.week.Days[0].programName = "Грудь,Трицепс";
                    Week.week.Days[1].programName = "Спина,Бицепс";
                    Week.week.Days[3].programName = "Ноги";
                    Week.week.Days[4].programName = "Плечи,Пресс";
                    return new List<List<Muscle>> { new List<Muscle>
                    {
                        Muscle.GetMuscleByName("Середина груди"),
                        Muscle.GetMuscleByName("Верх груди"),
                        Muscle.GetMuscleByName("Трицепс"),
                        Muscle.GetMuscleByName("Передние дельты")
                    },
                        new List<Muscle>
                        {
                        Muscle.GetMuscleByName("Широчайшие"),
                        Muscle.GetMuscleByName("Трапеции"),
                        Muscle.GetMuscleByName("Бицепс"),
                        Muscle.GetMuscleByName("Задние дельты"),
                        Muscle.GetMuscleByName("Ромбовидные"),
                    },

                         new List<Muscle>
                    {
                         Muscle.GetMuscleByName("Квадрицепс"),
                         Muscle.GetMuscleByName("Бицепс бедра"),
                         Muscle.GetMuscleByName("Ягодичные"),
                         Muscle.GetMuscleByName("Икры")
                    },
                         new List<Muscle>
                    {
                         Muscle.GetMuscleByName("Средние дельты"),
                         Muscle.GetMuscleByName("Задние дельты"),
                         Muscle.GetMuscleByName("Верх пресса"),
                         Muscle.GetMuscleByName("Низ пресса"),
                         Muscle.GetMuscleByName("Косые мышцы"),
                    }

                };
            }
            case 5:
                {
                    Week.week.Days[0].programName = "Грудь";
                    Week.week.Days[1].programName = "Спина";
                    Week.week.Days[2].programName = "Ноги";
                    Week.week.Days[3].programName = "Плечи";
                    Week.week.Days[4].programName = "Кор";
                    return new List<List<Muscle>>
                    {
                        new List<Muscle>
                        {
                            Muscle.GetMuscleByName("Середина груди"),
                            Muscle.GetMuscleByName("Верх груди"),
                            Muscle.GetMuscleByName("Трицепс"),
                            Muscle.GetMuscleByName("Передние дельты")
                        },
                        new List<Muscle>
                        {
                            Muscle.GetMuscleByName("Широчайшие"),
                            Muscle.GetMuscleByName("Трапеции"),
                            Muscle.GetMuscleByName("Бицепс"),
                            Muscle.GetMuscleByName("Задние дельты"),
                            Muscle.GetMuscleByName("Ромбовидные"),
                        },
                        new List<Muscle>
                        {
                            Muscle.GetMuscleByName("Квадрицепс"),
                            Muscle.GetMuscleByName("Бицепс бедра"),
                            Muscle.GetMuscleByName("Ягодичные"),
                            Muscle.GetMuscleByName("Икры")
                        },
                        new List<Muscle>
                        {
                            Muscle.GetMuscleByName("Средние дельты"),
                            Muscle.GetMuscleByName("Задние дельты"),
                            Muscle.GetMuscleByName("Передние дельты"),
                            Muscle.GetMuscleByName("Трапеции"),
                        },
                        new List<Muscle>
                        {
                            Muscle.GetMuscleByName("Верх пресса"),
                            Muscle.GetMuscleByName("Низ пресса"),
                            Muscle.GetMuscleByName("Косые мышцы"),
                            Muscle.GetMuscleByName("Поясница"),
                            Muscle.GetMuscleByName("Предплечья")
                        }
                    };
                }
            case 6:
                {
                    return new List<List<Muscle>>
                    {
                        new List<Muscle> // Пн: Грудь
                        {
                            Muscle.GetMuscleByName("Верх груди"),
                            Muscle.GetMuscleByName("Середина груди"),
                            Muscle.GetMuscleByName("Низ груди"),
                            Muscle.GetMuscleByName("Трицепс"),
                        },
                        new List<Muscle> // Вт: Спина
                        {
                            Muscle.GetMuscleByName("Широчайшие"),
                            Muscle.GetMuscleByName("Трапеции"),
                            Muscle.GetMuscleByName("Ромбовидные"),
                            Muscle.GetMuscleByName("Бицепс"),
                            Muscle.GetMuscleByName("Поясница"),
                        },
                        new List<Muscle> // Ср: Ноги
                        {
                            Muscle.GetMuscleByName("Квадрицепс"),
                            Muscle.GetMuscleByName("Бицепс бедра"),
                            Muscle.GetMuscleByName("Ягодичные"),
                            Muscle.GetMuscleByName("Икры")
                        },
                        new List<Muscle> // Чт: Плечи
                        {
                            Muscle.GetMuscleByName("Передние дельты"),
                            Muscle.GetMuscleByName("Средние дельты"),
                            Muscle.GetMuscleByName("Задние дельты"),
                            Muscle.GetMuscleByName("Трапеции"),
                        },
                        new List<Muscle> // Пт: Руки
                        {
                            Muscle.GetMuscleByName("Бицепс"),
                            Muscle.GetMuscleByName("Трицепс"),
                            Muscle.GetMuscleByName("Предплечья"),
                        },
                        new List<Muscle> // Сб: Кор + легкая работа
                        {
                            Muscle.GetMuscleByName("Верх пресса"),
                            Muscle.GetMuscleByName("Низ пресса"),
                            Muscle.GetMuscleByName("Косые мышцы"),
                            Muscle.GetMuscleByName("Поясница"),
                            Muscle.GetMuscleByName("Икры"),
                        }
                    };
                }
            case 7:
                {
                    return new List<List<Muscle>>
                    {
                        new List<Muscle> // Пн: Грудь + Трицепс
                        {
                            Muscle.GetMuscleByName("Верх груди"),
                            Muscle.GetMuscleByName("Середина груди"),
                            Muscle.GetMuscleByName("Низ груди"),
                            Muscle.GetMuscleByName("Трицепс"),
                            Muscle.GetMuscleByName("Передние дельты")
                        },
                        new List<Muscle> // Вт: Спина + Бицепс
                        {
                            Muscle.GetMuscleByName("Широчайшие"),
                            Muscle.GetMuscleByName("Трапеции"),
                            Muscle.GetMuscleByName("Ромбовидные"),
                            Muscle.GetMuscleByName("Бицепс"),
                            Muscle.GetMuscleByName("Задние дельты"),
                        },
                        new List<Muscle> // Ср: Ноги
                        {
                            Muscle.GetMuscleByName("Квадрицепс"),
                            Muscle.GetMuscleByName("Бицепс бедра"),
                            Muscle.GetMuscleByName("Ягодичные"),
                            Muscle.GetMuscleByName("Икры"),
                            Muscle.GetMuscleByName("Поясница")
                        },
                        new List<Muscle> // Чт: Плечи + Трапеции
                        {
                            Muscle.GetMuscleByName("Передние дельты"),
                            Muscle.GetMuscleByName("Средние дельты"),
                            Muscle.GetMuscleByName("Задние дельты"),
                            Muscle.GetMuscleByName("Трапеции"),
                        },
                        new List<Muscle> // Пт: Пресс + Косые
                        {
                            Muscle.GetMuscleByName("Верх пресса"),
                            Muscle.GetMuscleByName("Низ пресса"),
                            Muscle.GetMuscleByName("Косые мышцы"),
                        },
                        new List<Muscle> // Сб: Руки + Предплечья
                        {
                            Muscle.GetMuscleByName("Бицепс"),
                            Muscle.GetMuscleByName("Трицепс"),
                            Muscle.GetMuscleByName("Предплечья"),
                        },
                        new List<Muscle> // Вс: Отдых или кардио/легкая работа
                        {
                            Muscle.GetMuscleByName("Икры"),
                            Muscle.GetMuscleByName("Поясница"),
                            Muscle.GetMuscleByName("Ромбовидные"),
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
    private static float GetMax(int trainingsDayCount,float cof)
    {
        switch (trainingsDayCount)
        {
            case 1:  // 1 день в неделю - Full Body
                return 35* cof;  // Максимум для одной полной тренировки

            case 2:  // 2 дня в неделю - Full Body x2 или Upper/Lower
                return 30 * cof;  // Каждая тренировка немного короче, так как их две

            case 3:  // 3 дня в неделю - Full Body x3 или PPL
                return 25 * cof;  // Ещё меньше на тренировку, но больше за неделю

            case 4:  // 4 дня в неделю - Upper/Lower x2 или 4-дневный сплит
                return 22 * cof;  // Более специализированные тренировки

            case 5:  // 5 дней в неделю - Bro Split или PPL+Upper
                return 18 * cof;  // Высокая частота, низкий объем за раз

            case 6:  // 6 дней в неделю - PPL x2
                return 15 * cof;  // Очень специализированные, частые тренировки

            case 7:  // 7 дней в неделю - только для продвинутых
                return 12 * cof;  // Минимум на тренировку, максимум частоты

            default:
                return 20 * cof;  // Значение по умолчанию для безопасности
        }
    }

    #endregion
}
