using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
[System.Serializable]
public class Muscle
{
    #region Параметры и конструтор
    public string name;
    public byte percentageOfWork;
    public bool canWork = true;
    public MuscleGroup muscleGroup { get; private set; }
    public static List<Muscle> Muscles { get; } = GetBaseMuscles();

    public Muscle(string name, MuscleGroup muscleGroup)
    {
        this.name = name;
        this.muscleGroup = muscleGroup;
    }
    public Muscle(string name, byte percentageOfWork)
    {
        if (!isValidMuscle(name)) throw new ArgumentException($"Мышца '{name}' не найдена в базовом списке");
        this.name = name;
        this.percentageOfWork = percentageOfWork;
        this.muscleGroup = GetMuscleByName(name).muscleGroup;
    }
    #endregion
    public static Muscle GetMuscleByName(string name) => Muscles.First(m=>m.name == name);
    public static bool isValidMuscle(string name)=>Muscles.Any(m=>m.name == name);
    public static Muscle DeepClone(Muscle muscle)
    {
        if (muscle == null)
            return null;

        Muscle clonedMuscle = new Muscle(muscle.name, muscle.muscleGroup)
        {
            percentageOfWork = muscle.percentageOfWork,
            canWork = muscle.canWork
        };

        return clonedMuscle;
    }

    public static List<Muscle> GetBaseMuscles()
    {
        List<Muscle> muscles = new List<Muscle>();

        #region Грудь

        muscles.Add(new Muscle("Верх груди", MuscleGroup.chest));
        muscles.Add(new Muscle("Середина груди", MuscleGroup.chest));
        muscles.Add(new Muscle("Низ груди", MuscleGroup.chest));
        muscles.Add(new Muscle("Внутренняя часть груди", MuscleGroup.chest));

        #endregion
        #region Спина

        muscles.Add(new Muscle("Широчайшие", MuscleGroup.back));
        muscles.Add(new Muscle("Трапеции", MuscleGroup.back));
        muscles.Add(new Muscle("Ромбовидные", MuscleGroup.back));
        muscles.Add(new Muscle("Поясница", MuscleGroup.back));

        #endregion
        #region Плечи
        muscles.Add(new Muscle("Передние дельты", MuscleGroup.deltoid));
        muscles.Add(new Muscle("Средние дельты", MuscleGroup.deltoid));
        muscles.Add(new Muscle("Задние дельты", MuscleGroup.deltoid));

        #endregion
        #region Руки
        muscles.Add(new Muscle("Бицепс", MuscleGroup.hands));
        muscles.Add(new Muscle("Трицепс", MuscleGroup.hands));
        muscles.Add(new Muscle("Предплечья", MuscleGroup.hands));

        #endregion
        #region Ноги

        muscles.Add(new Muscle("Квадрицепс", MuscleGroup.legs));
        muscles.Add(new Muscle("Бицепс бедра", MuscleGroup.legs));
        muscles.Add(new Muscle("Ягодичные", MuscleGroup.legs));
        muscles.Add(new Muscle("Икры", MuscleGroup.legs));

        #endregion
        #region Кор
        muscles.Add(new Muscle("Верх пресса", MuscleGroup.core));
        muscles.Add(new Muscle("Низ пресса", MuscleGroup.core));
        muscles.Add(new Muscle("Косые мышцы", MuscleGroup.core));

        #endregion
        return muscles;
    }
}
public enum MuscleGroup
{
    chest,
    back,
    deltoid,
    hands,
    legs,
    core,
}
