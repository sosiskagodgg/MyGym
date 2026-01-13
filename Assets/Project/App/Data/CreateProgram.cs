using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class CreateProgram 
{
	#region Распределение нагрузки на неделю

	#endregion
	#region Создание силовой тренировки
	StringBuilder DebugStringBilder;
    private static void DistributeMuscleGroup(List<MuscleGroup> muscleGroups,int weekWA,StringBuilder DB=null)
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
			muscleGroups[i].Save();
            DB?.AppendLine($" {muscleGroups[i].name} - процент {muscleGroups[i].burden.importancePercentage},WeekWA - {muscleGroups[i].burden.workingApproaches}");
            //начинаем распределять по мышцам 
            DistributeMuscle(MuscleGroup.GetMusclesByGroupName(muscleGroups[i].name), (int)muscleGroups[i].burden.workingApproaches, DB);
        }

		 //сохранение в файл
    }
    private static void DistributeMuscle(List<Muscle> muscles, int weekWA, StringBuilder DB = null)
	{
		
        DB?.AppendLine($"Распределяем {weekWA} weekWA");
        float summa = 0;
        for (int i = 0; i < muscles.Count; i++)
        {
			summa += muscles[i].burden.importancePercentage;
        }
        // узнали сумму процентов


        for (int i = 0; i < muscles.Count; i++)
        {
            muscles[i].burden.importancePercentage = (muscles[i].burden.importancePercentage / summa) * 100;
            //Новый процент = (Текущий процент / Общая сумма процентов) × 100
            muscles[i].burden.workingApproaches = weekWA * (muscles[i].burden.importancePercentage / 100);
			//новое количество рабочих подходов = количество подходов * (процент работы / 100)
			muscles[i].SaveMuscle();

            DB?.AppendLine($" {muscles[i].name} - процент {muscles[i].burden.importancePercentage},WeekWA - {muscles[i].burden.workingApproaches}");
        }
    }
	public static List<SetOfExercises> DistributeExercises(List<Exercise> exercises,List<Muscle> muscles,List<MuscleGroup> muscleGroups, int weekWA, StringBuilder DB)
	{
		
		DistributeMuscleGroup(muscleGroups, weekWA, DB);
		// настроили проценты мышцам и группам мыщц

		DB.AppendLine(); DB.AppendLine();

        DB.AppendLine("Распределения упражнений по мышцам");
        List<SetOfExercises> setsOfExercises = new();

        for (int i = 0;i < muscles.Count;i++)
		{
			List<SetOfExercises> newSet = SetOfExercises.GetExercisesByMuscleWeekWA(muscles[i], (int)muscles[i].burden.workingApproaches, DB);
			// создали новые сеты основываясь на количество рабочих подходов на мышцу
            setsOfExercises.AddRange(newSet);
			DB.AppendLine($"	{muscles[i].name} - {newSet.Count} упражнений,{SetOfExercises.Count(newSet)} подходов");
        }
        return setsOfExercises;

    }
    #endregion

    #region Основной метод создания тренеровки


    public static Week CreateTrening(float intensity,int DaysCount)
	{
		Week week = new();
		




		return week;
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
	private static int GetMax(int treningsDayCount) 
	{
		switch (treningsDayCount)
		{
            case 1:  // 1 день в неделю
                return 55;  // Абсолютный максимум, физиологический предел

            case 2:  // 2 дня в неделю
                return 45;  // Оба Full Body или Upper/Lower

            case 3:  // 3 дня в неделю
                return 40;  // Full Body x3 или PPL

            case 4:  // 4 дня в неделю
                return 35;  // Upper/Lower x2 или 4-дневный сплит

            case 5:  // 5 дней в неделю
                return 30;  // Bro Split или PPL+Upper/Lower

            case 6:  // 6 дней в неделю
                return 25;  // PPL x2 или 6-дневный сплит

            case 7:  // 7 дней в неделю
                return 20;  // Только для профи, большинству не нужно
			default : { return 0; }
        }
	}

	#endregion
}
