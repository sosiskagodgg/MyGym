using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
[System.Serializable]
public class Muscle
{
    #region Параметры для файла
    public string name;
    public byte percentageOfWork;
    public int weekWork;
    public bool canWork = true;
    public MuscleGroup muscleGroup;

    #endregion
    #region Статичные параметры


    public static List<Muscle> Muscles 
    {
        get
        {
            if (!File.Exists(path))
            {
                Muscle.Save(GetBaseMuscles());
            }
            if (Math.Abs((File.GetLastWriteTime(path) - cashUpdate).TotalSeconds) > 0.1)
            {
                _musclesCash = Load();
            }
            return _musclesCash;
        }
        set 
        {
            _musclesCash = value;
            
            Muscle.Save(_musclesCash);
            cashUpdate = File.GetLastWriteTime(path);
        }
    } 
    #endregion

    #region констуркторы
    public Muscle(string name, MuscleGroup muscleGroup)
    {
        this.name = name;
        this.muscleGroup = muscleGroup;
    }
    public Muscle(string name, MuscleGroup muscleGroup, int weekWork)
    {
        this.name = name;
        this.muscleGroup = muscleGroup;
        this.weekWork = weekWork;
    }
    public Muscle(string name, byte percentageOfWork)
    {
        if (!isValidMuscle(name)) throw new ArgumentException($"Мышца '{name}' не найдена в базовом списке");
        this.name = name;
        this.percentageOfWork = percentageOfWork;
        this.muscleGroup = GetMuscleByName(name).muscleGroup;
    }

    #endregion

    #region Публичные взаимодействия
    public static Muscle GetMuscleByName(string name) => Muscles.First(m => m.name == name);
    public static bool isValidMuscle(string name) => Muscles.Any(m => m.name == name);
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

    #endregion

    #region Создание - Сохранение - Загрузка
    private static readonly string path = DataPath.Path() + "MuscleData.json";
    private static DateTime cashUpdate;
    private static List<Muscle> _musclesCash;
    #region Класс обертка
    [System.Serializable]
    public class MusclesWrapper
    {
        public List<Muscle> muscles;
        public MusclesWrapper(List<Muscle> muscles)
        {
            this.muscles = muscles;
        }
    }
    #endregion
    private static List<Muscle> Load()
    {
        return JsonUtility.FromJson<MusclesWrapper>(File.ReadAllText(path)).muscles;
    }
    private static void Save(List<Muscle> muscles)
    {
        File.WriteAllText(path,JsonUtility.ToJson(new MusclesWrapper(muscles), true));
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


    #endregion
    
}
[System.Serializable]
public enum MuscleGroup
{
    chest,
    back,
    deltoid,
    hands,
    legs,
    core,
}
