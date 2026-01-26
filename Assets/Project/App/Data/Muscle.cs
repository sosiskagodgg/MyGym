using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
[System.Serializable]
public class Muscle
{
    #region Параметры для файла
    public string name;
    public byte percentageOfWork;
    public bool canWork = true;
    public Burden burden;
    public MuscleGroup muscleGroup;

    #endregion

    #region Статичные параметры


    public static List<Muscle> Muscles
    {
        get
        {
            if(_musclesCash == null)
            {
                _musclesCash = GetBaseMuscles();
            }
            return _musclesCash;
        }
        set
        {
            _musclesCash = value;
        }
    }
    #endregion

    #region констуркторы
    public Muscle(string name, MuscleGroup muscleGroup)
    {
        this.name = name;
        this.muscleGroup = muscleGroup;
    }
    public Muscle(string name, MuscleGroup muscleGroup, Burden burden)
    {
        this.name = name;
        this.muscleGroup = muscleGroup;
        this.burden = burden;
    }
    public Muscle(string name, byte percentageOfWork)
    {
        if (!isValidMuscle(name)) throw new ArgumentException($"Мышца '{name}' не найдена в базовом списке");
        this.name = name;
        this.percentageOfWork = percentageOfWork;
        this.burden = GetMuscleByName(name).burden;
        this.muscleGroup = GetMuscleByName(name).muscleGroup;
    }

    #endregion

    #region Публичные взаимодействия
    public static Muscle GetMuscleByName(string name) 
    {
        if (!Muscles.Any(m => m.name == name)) throw new ArgumentException($"мышцы {name } не существует");
        return Muscles.First(m => m.name == name);
    }
    public static bool isValidMuscle(string name) => Muscles.Any(m => m.name == name);
    public static Muscle DeepClone(Muscle muscle)
    {
        if (muscle == null)
            return null;
        if (muscle.burden == null) throw new Exception();
        Muscle clonedMuscle = new Muscle(muscle.name, MuscleGroup.DeepClone(muscle.muscleGroup))
        {
            percentageOfWork = muscle.percentageOfWork,
            canWork = muscle.canWork,
            burden = Burden.DeepClone(muscle.burden),

        };

        return clonedMuscle;
    }

    #endregion

    #region Создание - Сохранение - Загрузка
    #region Параметры
    private static List<Muscle> _musclesCash;


    #endregion

    public void SaveMuscle()
    {
        // 1. Находим индекс мышцы в списке
        int index = Muscles.FindIndex(m => m.name == name);

        if (index != -1)
        {
            // 2. Заменяем объект по индексу
            Muscles[index] = this;
        }
    }
    #endregion

    #region Базовые мышцы
    public static List<Muscle> GetBaseMuscles()
    {
        Player currentPlayer = Player.player;
        List<Muscle> muscles = new List<Muscle>();

        // Если нет игрока, используем базовые значения
        Goal goal = currentPlayer?.treningParametrs?.goal ?? Goal.GainingMuscleMass;

        #region Грудь (сумма всегда 100)
        if (goal == Goal.WeightLoss)
        {
            // Похудение: равномерно для сжигания жира
            muscles.Add(new Muscle("Верх груди", MuscleGroup.chest, new Burden(4, 12, 25, 60)));
            muscles.Add(new Muscle("Середина груди", MuscleGroup.chest, new Burden(4, 12, 35, 55)));
            muscles.Add(new Muscle("Низ груди", MuscleGroup.chest, new Burden(3, 9, 30, 50)));
            muscles.Add(new Muscle("Внутренняя часть груди", MuscleGroup.chest, new Burden(2, 6, 10, 48)));
        }
        else if (goal == Goal.GainingMuscleMass)
        {
            // Набор массы: акцент на верх и середину для объема
            muscles.Add(new Muscle("Верх груди", MuscleGroup.chest, new Burden(4, 12, 35, 72)));
            muscles.Add(new Muscle("Середина груди", MuscleGroup.chest, new Burden(4, 12, 35, 65)));
            muscles.Add(new Muscle("Низ груди", MuscleGroup.chest, new Burden(3, 9, 20, 55)));
            muscles.Add(new Muscle("Внутренняя часть груди", MuscleGroup.chest, new Burden(2, 6, 10, 48)));
        }
        else if (goal == Goal.IncreasedStrength)
        {
            // Увеличение силы: акцент на середину для жима лежа
            muscles.Add(new Muscle("Верх груди", MuscleGroup.chest, new Burden(4, 12, 25, 72)));
            muscles.Add(new Muscle("Середина груди", MuscleGroup.chest, new Burden(5, 14, 45, 72)));
            muscles.Add(new Muscle("Низ груди", MuscleGroup.chest, new Burden(3, 9, 20, 60)));
            muscles.Add(new Muscle("Внутренняя часть груди", MuscleGroup.chest, new Burden(2, 6, 10, 48)));
        }
        else if (goal == Goal.IncreasedEndurance)
        {
            // Выносливость: равномерное распределение
            muscles.Add(new Muscle("Верх груди", MuscleGroup.chest, new Burden(3, 9, 25, 48)));
            muscles.Add(new Muscle("Середина груди", MuscleGroup.chest, new Burden(3, 9, 30, 48)));
            muscles.Add(new Muscle("Низ груди", MuscleGroup.chest, new Burden(2, 6, 25, 42)));
            muscles.Add(new Muscle("Внутренняя часть груди", MuscleGroup.chest, new Burden(2, 6, 20, 36)));
        }
        else if (goal == Goal.Recomposition)
        {
            // Рекомпозиция: баланс между силой и массой
            muscles.Add(new Muscle("Верх груди", MuscleGroup.chest, new Burden(4, 12, 30, 60)));
            muscles.Add(new Muscle("Середина груди", MuscleGroup.chest, new Burden(4, 12, 35, 60)));
            muscles.Add(new Muscle("Низ груди", MuscleGroup.chest, new Burden(3, 9, 25, 52)));
            muscles.Add(new Muscle("Внутренняя часть груди", MuscleGroup.chest, new Burden(2, 6, 10, 44)));
        }
        else if (goal == Goal.Flexibility)
        {
            // Гибкость: минимальная нагрузка
            muscles.Add(new Muscle("Верх груди", MuscleGroup.chest, new Burden(2, 6, 25, 48)));
            muscles.Add(new Muscle("Середина груди", MuscleGroup.chest, new Burden(2, 6, 30, 42)));
            muscles.Add(new Muscle("Низ груди", MuscleGroup.chest, new Burden(1, 3, 25, 36)));
            muscles.Add(new Muscle("Внутренняя часть груди", MuscleGroup.chest, new Burden(1, 3, 20, 30)));
        }
        else if (goal == Goal.Recovery)
        {
            // Восстановление: минимальные значения
            muscles.Add(new Muscle("Верх груди", MuscleGroup.chest, new Burden(2, 4, 25, 72)));
            muscles.Add(new Muscle("Середина груди", MuscleGroup.chest, new Burden(2, 4, 30, 72)));
            muscles.Add(new Muscle("Низ груди", MuscleGroup.chest, new Burden(1, 3, 25, 60)));
            muscles.Add(new Muscle("Внутренняя часть груди", MuscleGroup.chest, new Burden(1, 2, 20, 48)));
        }
        #endregion

        #region Спина (сумма всегда 100)
        if (goal == Goal.WeightLoss)
        {
            // Похудение: широчайшие и поясница для сжигания калорий
            muscles.Add(new Muscle("Широчайшие", MuscleGroup.back, new Burden(6, 14, 45, 60)));
            muscles.Add(new Muscle("Трапеции", MuscleGroup.back, new Burden(3, 7, 15, 48)));
            muscles.Add(new Muscle("Ромбовидные", MuscleGroup.back, new Burden(2, 5, 10, 42)));
            muscles.Add(new Muscle("Шея", MuscleGroup.neck, new Burden(2, 5, 100, 36)));
            muscles.Add(new Muscle("Поясница", MuscleGroup.back, new Burden(3, 6, 30, 48)));
        }
        else if (goal == Goal.GainingMuscleMass)
        {
            // Набор массы: акцент на широчайшие для V-образности
            muscles.Add(new Muscle("Широчайшие", MuscleGroup.back, new Burden(8, 18, 60, 72)));
            muscles.Add(new Muscle("Трапеции", MuscleGroup.back, new Burden(4, 9, 20, 55)));
            muscles.Add(new Muscle("Ромбовидные", MuscleGroup.back, new Burden(3, 7, 10, 48)));
            muscles.Add(new Muscle("Шея", MuscleGroup.neck, new Burden(2, 5, 100, 36)));
            muscles.Add(new Muscle("Поясница", MuscleGroup.back, new Burden(3, 6, 10, 48)));
        }
        else if (goal == Goal.IncreasedStrength)
        {
            // Увеличение силы: для становой тяги - поясница и трапеции
            muscles.Add(new Muscle("Широчайшие", MuscleGroup.back, new Burden(6, 14, 35, 72)));
            muscles.Add(new Muscle("Трапеции", MuscleGroup.back, new Burden(5, 11, 25, 60)));
            muscles.Add(new Muscle("Ромбовидные", MuscleGroup.back, new Burden(2, 5, 10, 48)));
            muscles.Add(new Muscle("Шея", MuscleGroup.neck, new Burden(2, 5, 100, 36)));
            muscles.Add(new Muscle("Поясница", MuscleGroup.back, new Burden(4, 8, 30, 60)));
        }
        else if (goal == Goal.IncreasedEndurance)
        {
            // Выносливость: равномерно
            muscles.Add(new Muscle("Широчайшие", MuscleGroup.back, new Burden(5, 12, 40, 48)));
            muscles.Add(new Muscle("Трапеции", MuscleGroup.back, new Burden(3, 8, 20, 42)));
            muscles.Add(new Muscle("Ромбовидные", MuscleGroup.back, new Burden(2, 6, 15, 36)));
            muscles.Add(new Muscle("Шея", MuscleGroup.neck, new Burden(2, 5, 100, 36)));
            muscles.Add(new Muscle("Поясница", MuscleGroup.back, new Burden(3, 7, 25, 42)));
        }
        else if (goal == Goal.Recomposition)
        {
            // Рекомпозиция: баланс
            muscles.Add(new Muscle("Широчайшие", MuscleGroup.back, new Burden(7, 16, 50, 60)));
            muscles.Add(new Muscle("Трапеции", MuscleGroup.back, new Burden(4, 8, 20, 48)));
            muscles.Add(new Muscle("Ромбовидные", MuscleGroup.back, new Burden(3, 6, 10, 42)));
            muscles.Add(new Muscle("Шея", MuscleGroup.neck, new Burden(2, 5, 100, 36)));
            muscles.Add(new Muscle("Поясница", MuscleGroup.back, new Burden(3, 6, 20, 48)));
        }
        else if (goal == Goal.Flexibility)
        {
            // Гибкость: для растяжки позвоночника
            muscles.Add(new Muscle("Широчайшие", MuscleGroup.back, new Burden(2, 5, 35, 36)));
            muscles.Add(new Muscle("Шея", MuscleGroup.neck, new Burden(2, 5, 100, 36)));
            muscles.Add(new Muscle("Трапеции", MuscleGroup.back, new Burden(2, 4, 25, 30)));
            muscles.Add(new Muscle("Ромбовидные", MuscleGroup.back, new Burden(1, 3, 15, 24)));
            muscles.Add(new Muscle("Поясница", MuscleGroup.back, new Burden(2, 4, 25, 30)));
        }
        else if (goal == Goal.Recovery)
        {
            // Восстановление: легкая нагрузка
            muscles.Add(new Muscle("Широчайшие", MuscleGroup.back, new Burden(3, 7, 30, 72)));
            muscles.Add(new Muscle("Шея", MuscleGroup.neck, new Burden(2, 5, 100, 36)));
            muscles.Add(new Muscle("Трапеции", MuscleGroup.back, new Burden(2, 4, 20, 60)));
            muscles.Add(new Muscle("Ромбовидные", MuscleGroup.back, new Burden(1, 2, 10, 48)));
            muscles.Add(new Muscle("Поясница", MuscleGroup.back, new Burden(2, 4, 40, 60)));
        }
        #endregion

        #region Плечи (сумма всегда 100)
        if (goal == Goal.WeightLoss)
        {
            // Похудение: все дельты равномерно
            muscles.Add(new Muscle("Передние дельты", MuscleGroup.deltoid, new Burden(3, 7, 35, 48)));
            muscles.Add(new Muscle("Средние дельты", MuscleGroup.deltoid, new Burden(3, 8, 35, 42)));
            muscles.Add(new Muscle("Задние дельты", MuscleGroup.deltoid, new Burden(2, 6, 30, 36)));
        }
        else if (goal == Goal.GainingMuscleMass)
        {
            // Набор массы: акцент на передние и средние
            muscles.Add(new Muscle("Передние дельты", MuscleGroup.deltoid, new Burden(4, 9, 50, 48)));
            muscles.Add(new Muscle("Средние дельты", MuscleGroup.deltoid, new Burden(5, 12, 25, 48)));
            muscles.Add(new Muscle("Задние дельты", MuscleGroup.deltoid, new Burden(4, 12, 25, 48)));
        }
        else if (goal == Goal.IncreasedStrength)
        {
            // Увеличение силы: передние для жима стоя
            muscles.Add(new Muscle("Передние дельты", MuscleGroup.deltoid, new Burden(5, 11, 60, 60)));
            muscles.Add(new Muscle("Средние дельты", MuscleGroup.deltoid, new Burden(4, 9, 20, 48)));
            muscles.Add(new Muscle("Задние дельты", MuscleGroup.deltoid, new Burden(3, 7, 20, 48)));
        }
        else if (goal == Goal.IncreasedEndurance)
        {
            // Выносливость: равномерно
            muscles.Add(new Muscle("Передние дельты", MuscleGroup.deltoid, new Burden(3, 8, 35, 36)));
            muscles.Add(new Muscle("Средние дельты", MuscleGroup.deltoid, new Burden(4, 10, 35, 36)));
            muscles.Add(new Muscle("Задние дельты", MuscleGroup.deltoid, new Burden(3, 8, 30, 30)));
        }
        else if (goal == Goal.Recomposition)
        {
            // Рекомпозиция: баланс
            muscles.Add(new Muscle("Передние дельты", MuscleGroup.deltoid, new Burden(4, 9, 45, 42)));
            muscles.Add(new Muscle("Средние дельты", MuscleGroup.deltoid, new Burden(4, 10, 30, 42)));
            muscles.Add(new Muscle("Задние дельты", MuscleGroup.deltoid, new Burden(3, 8, 25, 36)));
        }
        else if (goal == Goal.Flexibility)
        {
            // Гибкость: для подвижности плеч
            muscles.Add(new Muscle("Передние дельты", MuscleGroup.deltoid, new Burden(2, 4, 35, 30)));
            muscles.Add(new Muscle("Средние дельты", MuscleGroup.deltoid, new Burden(2, 4, 35, 30)));
            muscles.Add(new Muscle("Задние дельты", MuscleGroup.deltoid, new Burden(1, 3, 30, 24)));
        }
        else if (goal == Goal.Recovery)
        {
            // Восстановление: минимально
            muscles.Add(new Muscle("Передние дельты", MuscleGroup.deltoid, new Burden(2, 4, 40, 60)));
            muscles.Add(new Muscle("Средние дельты", MuscleGroup.deltoid, new Burden(2, 4, 35, 60)));
            muscles.Add(new Muscle("Задние дельты", MuscleGroup.deltoid, new Burden(1, 3, 25, 48)));
        }
        #endregion

        #region Руки (сумма всегда 100)
        if (goal == Goal.WeightLoss)
        {
            // Похудение: трицепс важнее (большая мышца)
            muscles.Add(new Muscle("Бицепс", MuscleGroup.hands, new Burden(4, 9, 30, 42)));
            muscles.Add(new Muscle("Трицепс", MuscleGroup.hands, new Burden(5, 11, 50, 42)));
            muscles.Add(new Muscle("Предплечья", MuscleGroup.hands, new Burden(3, 7, 20, 30)));
        }
        else if (goal == Goal.GainingMuscleMass)
        {
            // Набор массы: равномерно для симметрии
            muscles.Add(new Muscle("Бицепс", MuscleGroup.hands, new Burden(6, 12, 40, 48)));
            muscles.Add(new Muscle("Трицепс", MuscleGroup.hands, new Burden(6, 14, 50, 48)));
            muscles.Add(new Muscle("Предплечья", MuscleGroup.hands, new Burden(5, 10, 10, 30)));
        }
        else if (goal == Goal.IncreasedStrength)
        {
            // Увеличение силы: трицепс для жима
            muscles.Add(new Muscle("Бицепс", MuscleGroup.hands, new Burden(5, 10, 30, 48)));
            muscles.Add(new Muscle("Трицепс", MuscleGroup.hands, new Burden(7, 15, 60, 60)));
            muscles.Add(new Muscle("Предплечья", MuscleGroup.hands, new Burden(4, 8, 10, 36)));
        }
        else if (goal == Goal.IncreasedEndurance)
        {
            // Выносливость: все для выносливости
            muscles.Add(new Muscle("Бицепс", MuscleGroup.hands, new Burden(4, 10, 35, 30)));
            muscles.Add(new Muscle("Трицепс", MuscleGroup.hands, new Burden(5, 12, 45, 36)));
            muscles.Add(new Muscle("Предплечья", MuscleGroup.hands, new Burden(4, 9, 20, 24)));
        }
        else if (goal == Goal.Recomposition)
        {
            // Рекомпозиция: баланс
            muscles.Add(new Muscle("Бицепс", MuscleGroup.hands, new Burden(5, 11, 35, 42)));
            muscles.Add(new Muscle("Трицепс", MuscleGroup.hands, new Burden(6, 13, 50, 48)));
            muscles.Add(new Muscle("Предплечья", MuscleGroup.hands, new Burden(4, 8, 15, 30)));
        }
        else if (goal == Goal.Flexibility)
        {
            // Гибкость: для подвижности рук
            muscles.Add(new Muscle("Бицепс", MuscleGroup.hands, new Burden(2, 4, 35, 24)));
            muscles.Add(new Muscle("Трицепс", MuscleGroup.hands, new Burden(2, 5, 45, 30)));
            muscles.Add(new Muscle("Предплечья", MuscleGroup.hands, new Burden(2, 4, 20, 18)));
        }
        else if (goal == Goal.Recovery)
        {
            // Восстановление: минимально
            muscles.Add(new Muscle("Бицепс", MuscleGroup.hands, new Burden(3, 6, 30, 48)));
            muscles.Add(new Muscle("Трицепс", MuscleGroup.hands, new Burden(3, 7, 50, 48)));
            muscles.Add(new Muscle("Предплечья", MuscleGroup.hands, new Burden(2, 4, 20, 30)));
        }
        #endregion

        #region Ноги (сумма всегда 100)
        if (goal == Goal.WeightLoss)
        {
            // Похудение: все ноги для сжигания калорий
            muscles.Add(new Muscle("Квадрицепс", MuscleGroup.legs, new Burden(6, 13, 30, 72)));
            muscles.Add(new Muscle("Бицепс бедра", MuscleGroup.legs, new Burden(5, 10, 25, 60)));
            muscles.Add(new Muscle("Ягодичные", MuscleGroup.legs, new Burden(5, 10, 25, 60)));
            muscles.Add(new Muscle("Икры", MuscleGroup.legs, new Burden(6, 12, 20, 48)));
        }
        else if (goal == Goal.GainingMuscleMass)
        {
            // Набор массы: акцент на квадрицепс
            muscles.Add(new Muscle("Квадрицепс", MuscleGroup.legs, new Burden(8, 16, 35, 96)));
            muscles.Add(new Muscle("Бицепс бедра", MuscleGroup.legs, new Burden(6, 12, 25, 72)));
            muscles.Add(new Muscle("Ягодичные", MuscleGroup.legs, new Burden(6, 12, 25, 72)));
            muscles.Add(new Muscle("Икры", MuscleGroup.legs, new Burden(8, 18, 15, 48)));
        }
        else if (goal == Goal.IncreasedStrength)
        {
            // Увеличение силы: квадрицепс и ягодичные для приседа
            muscles.Add(new Muscle("Квадрицепс", MuscleGroup.legs, new Burden(9, 18, 40, 96)));
            muscles.Add(new Muscle("Бицепс бедра", MuscleGroup.legs, new Burden(5, 10, 20, 72)));
            muscles.Add(new Muscle("Ягодичные", MuscleGroup.legs, new Burden(6, 12, 30, 72)));
            muscles.Add(new Muscle("Икры", MuscleGroup.legs, new Burden(5, 10, 10, 48)));
        }
        else if (goal == Goal.IncreasedEndurance)
        {
            // Выносливость: равномерно для бега
            muscles.Add(new Muscle("Квадрицепс", MuscleGroup.legs, new Burden(6, 14, 30, 60)));
            muscles.Add(new Muscle("Бицепс бедра", MuscleGroup.legs, new Burden(5, 11, 25, 48)));
            muscles.Add(new Muscle("Ягодичные", MuscleGroup.legs, new Burden(5, 11, 25, 48)));
            muscles.Add(new Muscle("Икры", MuscleGroup.legs, new Burden(6, 13, 20, 36)));
        }
        else if (goal == Goal.Recomposition)
        {
            // Рекомпозиция: баланс
            muscles.Add(new Muscle("Квадрицепс", MuscleGroup.legs, new Burden(7, 14, 35, 72)));
            muscles.Add(new Muscle("Бицепс бедра", MuscleGroup.legs, new Burden(5, 11, 25, 60)));
            muscles.Add(new Muscle("Ягодичные", MuscleGroup.legs, new Burden(5, 11, 25, 60)));
            muscles.Add(new Muscle("Икры", MuscleGroup.legs, new Burden(6, 12, 15, 42)));
        }
        else if (goal == Goal.Flexibility)
        {
            // Гибкость: для растяжки ног
            muscles.Add(new Muscle("Квадрицепс", MuscleGroup.legs, new Burden(3, 6, 30, 36)));
            muscles.Add(new Muscle("Бицепс бедра", MuscleGroup.legs, new Burden(3, 6, 30, 30)));
            muscles.Add(new Muscle("Ягодичные", MuscleGroup.legs, new Burden(2, 5, 25, 30)));
            muscles.Add(new Muscle("Икры", MuscleGroup.legs, new Burden(3, 6, 15, 24)));
        }
        else if (goal == Goal.Recovery)
        {
            // Восстановление: легкая нагрузка
            muscles.Add(new Muscle("Квадрицепс", MuscleGroup.legs, new Burden(4, 8, 35, 72)));
            muscles.Add(new Muscle("Бицепс бедра", MuscleGroup.legs, new Burden(3, 6, 25, 60)));
            muscles.Add(new Muscle("Ягодичные", MuscleGroup.legs, new Burden(3, 6, 25, 60)));
            muscles.Add(new Muscle("Икры", MuscleGroup.legs, new Burden(4, 8, 15, 48)));
        }
        #endregion

        #region Кор (сумма всегда 100)
        if (goal == Goal.WeightLoss)
        {
            // Похудение: пресс для сжигания жира
            muscles.Add(new Muscle("Верх пресса", MuscleGroup.core, new Burden(4, 8, 40, 42)));
            muscles.Add(new Muscle("Низ пресса", MuscleGroup.core, new Burden(4, 8, 40, 42)));
            muscles.Add(new Muscle("Косые мышцы", MuscleGroup.core, new Burden(3, 6, 20, 36)));
        }
        else if (goal == Goal.GainingMuscleMass)
        {
            // Набор массы: пресс для стабилизации
            muscles.Add(new Muscle("Верх пресса", MuscleGroup.core, new Burden(5, 10, 45, 48)));
            muscles.Add(new Muscle("Низ пресса", MuscleGroup.core, new Burden(4, 8, 45, 48)));
            muscles.Add(new Muscle("Косые мышцы", MuscleGroup.core, new Burden(3, 6, 10, 48)));
        }
        else if (goal == Goal.IncreasedStrength)
        {
            // Увеличение силы: пресс для стабильности в тяжелых упражнениях
            muscles.Add(new Muscle("Верх пресса", MuscleGroup.core, new Burden(6, 12, 50, 60)));
            muscles.Add(new Muscle("Низ пресса", MuscleGroup.core, new Burden(5, 10, 40, 60)));
            muscles.Add(new Muscle("Косые мышцы", MuscleGroup.core, new Burden(3, 6, 10, 48)));
        }
        else if (goal == Goal.IncreasedEndurance)
        {
            // Выносливость: пресс для бега и кардио
            muscles.Add(new Muscle("Верх пресса", MuscleGroup.core, new Burden(4, 9, 40, 36)));
            muscles.Add(new Muscle("Низ пресса", MuscleGroup.core, new Burden(4, 8, 40, 36)));
            muscles.Add(new Muscle("Косые мышцы", MuscleGroup.core, new Burden(3, 6, 20, 30)));
        }
        else if (goal == Goal.Recomposition)
        {
            // Рекомпозиция: баланс
            muscles.Add(new Muscle("Верх пресса", MuscleGroup.core, new Burden(5, 10, 45, 42)));
            muscles.Add(new Muscle("Низ пресса", MuscleGroup.core, new Burden(4, 8, 45, 42)));
            muscles.Add(new Muscle("Косые мышцы", MuscleGroup.core, new Burden(3, 6, 10, 36)));
        }
        else if (goal == Goal.Flexibility)
        {
            // Гибкость: пресс для растяжки
            muscles.Add(new Muscle("Верх пресса", MuscleGroup.core, new Burden(3, 5, 40, 30)));
            muscles.Add(new Muscle("Низ пресса", MuscleGroup.core, new Burden(3, 5, 40, 30)));
            muscles.Add(new Muscle("Косые мышцы", MuscleGroup.core, new Burden(2, 4, 20, 24)));
        }
        else if (goal == Goal.Recovery)
        {
            // Восстановление: легкий пресс
            muscles.Add(new Muscle("Верх пресса", MuscleGroup.core, new Burden(3, 6, 40, 48)));
            muscles.Add(new Muscle("Низ пресса", MuscleGroup.core, new Burden(3, 6, 40, 48)));
            muscles.Add(new Muscle("Косые мышцы", MuscleGroup.core, new Burden(2, 4, 20, 42)));
        }
        #endregion

        return muscles;
    }
    #endregion

    #region Методы для Update
    public static void UpdateCash(List<Muscle> muscles)
    {
        for (int i = 0; i < Muscles.Count; i++)
        {
            var m1 = Muscles[i];
            var m2 = muscles.FirstOrDefault(m => m.name == m1.name);

            if (m2 != null)
            {
                Muscles[i] = m2;
            }
        }

    }
    public static void UpdateCash(Muscle muscle) { var mus = Muscles.FirstOrDefault(m => m.name == muscle.name); mus = muscle; } 
    #endregion

}
[System.Serializable]
public class MuscleGroup
    {
    #region Конструктор и параметры
    public string name;
    public Burden burden;
    public static MuscleGroup chest = GetMuscleGroupByName("chest");
    public static MuscleGroup neck = GetMuscleGroupByName("neck");
    public static MuscleGroup back = GetMuscleGroupByName("back");
    public static MuscleGroup deltoid = GetMuscleGroupByName("deltoid");
    public static MuscleGroup hands = GetMuscleGroupByName("hands");
    public static MuscleGroup legs = GetMuscleGroupByName("legs");
    public static MuscleGroup core = GetMuscleGroupByName("core");


    public MuscleGroup(string name, Burden burden)
    {
        this.name = name;
        this.burden = burden;
        if (burden == null) throw new Exception();
    }
    #endregion
    #region Гетеры
    public static List<Muscle> GetMusclesByGroupName(string groupName)
    {
        // Приводим к нижнему регистру для унификации сравнения
        string normalizedName = groupName.ToLowerInvariant();

        try
        {
            switch (normalizedName)
            {
                case "chest":
                case "грудь":
                    return new List<Muscle>
                {
                    Muscle.GetMuscleByName("Верх груди"),
                    Muscle.GetMuscleByName("Середина груди"),
                    Muscle.GetMuscleByName("Низ груди"),
                    Muscle.GetMuscleByName("Внутренняя часть груди")
                };

                case "back":
                case "спина":
                    return new List<Muscle>
                {
                    Muscle.GetMuscleByName("Широчайшие"),
                    Muscle.GetMuscleByName("Трапеции"),
                    Muscle.GetMuscleByName("Ромбовидные"),
                    Muscle.GetMuscleByName("Поясница")
                };

                case "deltoid":
                case "плечи":
                case "shoulders":
                    return new List<Muscle>
                {
                    Muscle.GetMuscleByName("Передние дельты"),
                    Muscle.GetMuscleByName("Средние дельты"),
                    Muscle.GetMuscleByName("Задние дельты")
                };

                case "hands":
                case "руки":
                case "arms":
                    return new List<Muscle>
                {
                    Muscle.GetMuscleByName("Бицепс"),
                    Muscle.GetMuscleByName("Трицепс"),
                    Muscle.GetMuscleByName("Предплечья")
                };

                case "legs":
                case "ноги":
                    return new List<Muscle>
                {
                    Muscle.GetMuscleByName("Квадрицепс"),
                    Muscle.GetMuscleByName("Бицепс бедра"),
                    Muscle.GetMuscleByName("Ягодичные"),
                    Muscle.GetMuscleByName("Икры")
                };

                case "core":
                case "кор":
                case "пресс":
                case "abs":
                    return new List<Muscle>
                {
                    Muscle.GetMuscleByName("Верх пресса"),
                    Muscle.GetMuscleByName("Низ пресса"),
                    Muscle.GetMuscleByName("Косые мышцы")
                };

                default:
                    Debug.LogWarning($"Неизвестная группа мышц: {groupName}");
                    return new List<Muscle>();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Ошибка при получении мышц для группы '{groupName}': {ex.Message}");
            return new List<Muscle>();
        }
    }
    public static MuscleGroup GetMuscleGroupByName(string name)
    {
        try
        {
            return muscleGroups.FirstOrDefault(mG => mG.name == name);
        }
        catch
        {
            return CreateMuscleGroups().FirstOrDefault(mG => mG.name == name);
        }



    
    }
    public static MuscleGroup DeepClone(MuscleGroup muscleGroup) 
    {

        return new MuscleGroup(muscleGroup.name, Burden.DeepClone(muscleGroup.burden));
    }
    public static List<MuscleGroup> GetPrimaryMyscleGroups(List<Muscle> muscles)
    {
        List<MuscleGroup> primaryMuscleGroups = new();
        for(int i = 0; muscles.Count > i; i++)
        {
            if (!primaryMuscleGroups.Any(mg=>mg.name == muscles[i].muscleGroup.name))
            {
                primaryMuscleGroups.Add(muscles[i].muscleGroup);
            }
        }
        return primaryMuscleGroups;
    }
    #endregion
    #region Сохранение - загрузка
    #region Параметры для сохранения и загрузки
    private static List<MuscleGroup> _muscleGroups;
    public static List<MuscleGroup> muscleGroups
    {
        get
        {
            if (_muscleGroups == null)
            {
                _muscleGroups = CreateMuscleGroups();
            }
            return _muscleGroups;

        }
        set
        {
            _muscleGroups = value;
        }
    }

    #endregion
 
    #region Создание групп
    public static List<MuscleGroup> CreateMuscleGroups()
    {
        Player currentPlayer = Player.player;

        // Если нет игрока, возвращаем базовые значения
        if (currentPlayer == null || currentPlayer.treningParametrs == null)
        {
            return GetBaseMuscleGroups();
        }

        Goal goal = currentPlayer.treningParametrs.goal;

        if (goal == Goal.WeightLoss)
        {
            // Цель: Похудение - акцент на большие мышечные группы для сжигания калорий
            return new List<MuscleGroup>
        {
            new MuscleGroup("chest", new Burden(7, 14, 16, 60)),    // 16% важности
            new MuscleGroup("back", new Burden(9, 18, 18, 60)),     // 18% важности
            new MuscleGroup("deltoid", new Burden(7, 14, 12, 60)),  // 12% важности
            new MuscleGroup("hands", new Burden(6, 12, 8, 48)),     // 8% важности
            new MuscleGroup("legs", new Burden(13, 26, 28, 72)),    // 28% важности
            new MuscleGroup("core", new Burden(9, 18, 18, 48))  ,    // 18% важности
            new MuscleGroup("neck", new Burden(5, 10, 0, 36))
        };
        }
        else if (goal == Goal.GainingMuscleMass)
        {
            // Цель: Набор мышечной массы - равномерное развитие всех групп
            return new List<MuscleGroup>
        {
            new MuscleGroup("chest", new Burden(9, 18, 18, 72)),    // 18%
            new MuscleGroup("back", new Burden(11, 22, 20, 72)),    // 20%
            new MuscleGroup("deltoid", new Burden(9, 18, 16, 72)),  // 16%
            new MuscleGroup("hands", new Burden(11, 22, 14, 48)),   // 14%
            new MuscleGroup("legs", new Burden(13, 26, 22, 96)),    // 22%
            new MuscleGroup("core", new Burden(7, 14, 10, 48))   ,   // 10%
            new MuscleGroup("neck", new Burden(5, 10, 0, 36))
        };
        }
        else if (goal == Goal.IncreasedStrength)
        {
            // Цель: Увеличение силы - фокус на базовые упражнения
            return new List<MuscleGroup>
        {
            new MuscleGroup("chest", new Burden(8, 16, 22, 72)),    // 20% (жим лежа)
            new MuscleGroup("back", new Burden(8, 18, 26, 96)),     // 25% (становая тяга)
            new MuscleGroup("deltoid", new Burden(8, 16, 10, 72)),  // 15% (жим стоя)
            new MuscleGroup("hands", new Burden(8, 16, 10, 48)),    // 10% (вспомогательные)
            new MuscleGroup("legs", new Burden(12, 20, 26, 96)),    // 25% (приседания)
            new MuscleGroup("core", new Burden(6, 12, 6, 48))   ,    // 5% (стабильность)
            new MuscleGroup("neck", new Burden(5, 10, 0, 36))
        };
        }
        else if (goal == Goal.IncreasedEndurance)
        {
            // Цель: Увеличение выносливости - больше подходов, меньше отдыха
            return new List<MuscleGroup>
        {
            new MuscleGroup("chest", new Burden(6, 18, 15, 48)),    // 15%
            new MuscleGroup("back", new Burden(8, 20, 18, 48)),     // 18%
            new MuscleGroup("deltoid", new Burden(8, 20, 16, 48)),  // 16%
            new MuscleGroup("hands", new Burden(10, 24, 14, 36)),   // 14%
            new MuscleGroup("legs", new Burden(12, 28, 22, 60)),    // 22%
            new MuscleGroup("core", new Burden(8, 20, 15, 36))   ,   // 15%
            new MuscleGroup("neck", new Burden(5, 10, 0, 36))
        };
        }
        else if (goal == Goal.Recomposition)
        {
            // Цель: Рекомпозиция - сбалансированный подход
            return new List<MuscleGroup>
        {
            new MuscleGroup("chest", new Burden(8, 16, 17, 72)),    // 17%
            new MuscleGroup("back", new Burden(10, 20, 20, 72)),    // 20%
            new MuscleGroup("deltoid", new Burden(9, 18, 15, 72)),  // 15%
            new MuscleGroup("hands", new Burden(10, 20, 13, 48)),   // 13%
            new MuscleGroup("legs", new Burden(12, 24, 20, 72)),    // 20%
            new MuscleGroup("core", new Burden(8, 16, 15, 48)) ,     // 15%
            new MuscleGroup("neck", new Burden(5, 10, 0, 36))
        };
        }
        else if (goal == Goal.Flexibility)
        {
            // Цель: Гибкость - минимальная силовая нагрузка
            return new List<MuscleGroup>
        {
            // Распределение для гибкости (всего 100%)
            new MuscleGroup("legs", new Burden(4, 8, 25, 48)),        // 25% - Ноги требуют больше внимания для гибкости
            new MuscleGroup("back", new Burden(6, 12, 20, 48)),       // 20% - Гибкость спины критически важна
            new MuscleGroup("chest", new Burden(4, 8, 15, 48)),       // 15% - Раскрытие грудного отдела
            new MuscleGroup("deltoid", new Burden(4, 8, 12, 48)),     // 12% - Плечевой пояс
            new MuscleGroup("core", new Burden(5, 10, 10, 36)),       // 10% - Кор, пресс, поясница
            new MuscleGroup("hands", new Burden(4, 8, 10, 36)),       // 10% - Руки (бицепс/трицепс/предплечья)
            new MuscleGroup("neck", new Burden(5, 10, 8, 36))         // 8% - Шея (для общей мобильности)
        };
        }
        else if (goal == Goal.Recovery)
        {
            // Цель: Восстановление - очень легкая нагрузка
            return new List<MuscleGroup>
        {
            new MuscleGroup("chest", new Burden(4, 8, 10, 96)),     // 10%
            new MuscleGroup("back", new Burden(5, 10, 15, 96)),     // 15%
            new MuscleGroup("deltoid", new Burden(4, 8, 10, 96)),   // 10%
            new MuscleGroup("hands", new Burden(4, 8, 10, 72)),     // 10%
            new MuscleGroup("legs", new Burden(5, 10, 25, 96)),     // 25%
            new MuscleGroup("core", new Burden(5, 10, 30, 72)) ,     // 30%
            new MuscleGroup("neck", new Burden(5, 10, 0, 36))
        };
        }
        else
        {
            // Цель по умолчанию (если цель не распознана)
            return GetBaseMuscleGroups();
        }
    }
    private static List<MuscleGroup> GetBaseMuscleGroups()
    {
        // Возвращаем базовые значения (оригинальные)
        return new List<MuscleGroup>
    {
        new MuscleGroup("chest", new Burden(8, 16, 18, 72)),
        new MuscleGroup("back", new Burden(10, 20, 22, 72)),
        new MuscleGroup("deltoid", new Burden(9, 18, 17, 72)),
        new MuscleGroup("hands", new Burden(13, 26, 13, 48)),
        new MuscleGroup("legs", new Burden(15, 25, 20, 96)),
        new MuscleGroup("core", new Burden(8, 16, 10, 48))
    };
    } 
    #endregion


    #region Класс обертка
    [System.Serializable]
    public class MuscleGroupsWrapper
    {
        public List<MuscleGroup> muscleGroups;
        public MuscleGroupsWrapper(List<MuscleGroup> muscleGroups)
        {
            this.muscleGroups = muscleGroups;
        }
    }
    #endregion
    #region Сохранение
    public void Save()
    {
        // 1. Загружаем текущий список
        var groups = muscleGroups; // Получаем список

        // 2. Находим индекс
        int index = groups.FindIndex(m => m.name == name);

        if (index != -1)
        {
            // 3. Заменяем объект
            groups[index] = this;
        }
    } 
    #endregion
    #endregion
}

[System.Serializable]
public class Burden
{
    #region Параметры и конструкторы
    public float workingApproaches; //параметр отслеживующий нагрузку на мышцу
    public float importancePercentage;
    public TimeSpan timeRegenerate;
    public int MaxDayWA { get { return (int)(_MaxDayWA * Coefficcient); } private set { } }// сколько можно делать подходов на мышцу в день
    int _MaxDayWA;
    public int MaxWeekWA { get { return (int)(_MaxWeekWA * Coefficcient); } private set { } }// сколько можно делать подходов на мышцу в неделю
    int _MaxWeekWA;
    public float Coefficcient { get; set; } = 1;
    public Burden(int maxDayBurden, int maxWeekBurden, float importancePercentage = 0, int timeRegenerate = 72)
    {
        this._MaxDayWA = maxDayBurden;
        this._MaxWeekWA = maxWeekBurden;
        this.importancePercentage = importancePercentage;
        this.timeRegenerate = TimeSpan.FromHours(timeRegenerate);
    }
    public Burden() { }
    #endregion
    public static Burden DeepClone(Burden burden)
    {
        burden ??= new();
        return new Burden
        {
            workingApproaches = burden.workingApproaches,
            importancePercentage = burden.importancePercentage,
            MaxDayWA = burden.MaxDayWA,
            MaxWeekWA = burden.MaxWeekWA,
            Coefficcient = burden.Coefficcient
        };
    }
}
