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
            if (!File.Exists(path))
            {
                Muscle.Save(GetBaseMuscles());
                _musclesCash = GetBaseMuscles();
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
    public static readonly string path = DataPath.Path() + "/MuscleData.json";
    private static DateTime cashUpdate;
    private static List<Muscle> _musclesCash;


    #endregion

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

    #region Загрузка
    private static List<Muscle> Load()
    {
        return JsonUtility.FromJson<MusclesWrapper>(File.ReadAllText(path)).muscles;
    } 
    #endregion

    #region Сохранение
    private static void Save(List<Muscle> muscles)
    {
        File.WriteAllText(path, JsonUtility.ToJson(new MusclesWrapper(muscles), true));
    }
    public void SaveMuscle()
    {
        // 1. Находим индекс мышцы в списке
        int index = Muscles.FindIndex(m => m.name == name);

        if (index != -1)
        {
            // 2. Заменяем объект по индексу
            Muscles[index] = this;

            // 3. Сохраняем весь список
            Save(Muscles);
        }
    }
    #endregion

    #region Базовые мышцы
    public static List<Muscle> GetBaseMuscles()
    {
        List<Muscle> muscles = new List<Muscle>();

        #region Грудь

        muscles.Add(new Muscle("Верх груди", MuscleGroup.chest, new Burden(4, 12, 35,72)));
        muscles.Add(new Muscle("Середина груди", MuscleGroup.chest, new Burden(4, 12, 35,65)));
        muscles.Add(new Muscle("Низ груди", MuscleGroup.chest, new Burden(3, 9, 20,55)));
        muscles.Add(new Muscle("Внутренняя часть груди", MuscleGroup.chest, new Burden(2, 6, 10,55)));

        #endregion
        #region Спина

        muscles.Add(new Muscle("Широчайшие", MuscleGroup.back, new Burden(8, 18, 60,72)));
        muscles.Add(new Muscle("Трапеции", MuscleGroup.back, new Burden(4, 9, 20,55)));
        muscles.Add(new Muscle("Ромбовидные", MuscleGroup.back, new Burden(3, 7, 10,48)));
        muscles.Add(new Muscle("Поясница", MuscleGroup.back, new Burden(3, 6, 10)));

        #endregion
        #region Плечи
        muscles.Add(new Muscle("Передние дельты", MuscleGroup.deltoid, new Burden(4, 9, 50,48)));
        muscles.Add(new Muscle("Средние дельты", MuscleGroup.deltoid, new Burden(5, 12, 25,48)));
        muscles.Add(new Muscle("Задние дельты", MuscleGroup.deltoid, new Burden(4, 12, 25, 48)));

        #endregion
        #region Руки
        muscles.Add(new Muscle("Бицепс", MuscleGroup.hands, new Burden(6, 12, 40, 48)));
        muscles.Add(new Muscle("Трицепс", MuscleGroup.hands, new Burden(6, 14, 50, 48)));
        muscles.Add(new Muscle("Предплечья", MuscleGroup.hands, new Burden(5, 10, 10, 30)));

        #endregion
        #region Ноги

        muscles.Add(new Muscle("Квадрицепс", MuscleGroup.legs, new Burden(8, 16, 35,96)));
        muscles.Add(new Muscle("Бицепс бедра", MuscleGroup.legs, new Burden(6, 12, 25, 72)));
        muscles.Add(new Muscle("Ягодичные", MuscleGroup.legs, new Burden(6, 12, 25, 72)));
        muscles.Add(new Muscle("Икры", MuscleGroup.legs, new Burden(8, 18, 15, 48)));

        #endregion
        #region Кор
        muscles.Add(new Muscle("Верх пресса", MuscleGroup.core, new Burden(5, 10, 45, 48)));
        muscles.Add(new Muscle("Низ пресса", MuscleGroup.core, new Burden(4, 8, 45, 48)));
        muscles.Add(new Muscle("Косые мышцы", MuscleGroup.core, new Burden(3, 6, 10, 48)));

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

    #endregion
}
[System.Serializable]
public class MuscleGroup
    {
    #region Конструктор и параметры
    public string name;
    public Burden burden;
    public static MuscleGroup chest = GetMuscleGroupByName("chest");
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
    public static List<SetOfExercises> GetExercisesByMuscleGroupWeekWA(MuscleGroup muscleGroup, int WeekWA, StringBuilder debugString = null)
    {
        List<Muscle> muscles = GetMusclesByGroupName(muscleGroup.name);
        List<SetOfExercises> result = new();

        debugString?.AppendLine($"=== Распределение {WeekWA} подходов для группы: {muscleGroup.name} ===");
        debugString?.AppendLine($"Найдено мышц в группе: {muscles.Count}");

        // Если нет мышц или подходов
        if (muscles.Count == 0 || WeekWA <= 0)
        {
            debugString?.AppendLine("⚠️ Нет мышц для распределения или подходов = 0");
            return result;
        }

        // 1. Распределяем подходы между мышцами пропорционально их максимальной нагрузке
        Dictionary<Muscle, int> muscleAllocation = new();
        int totalMaxWeekWA = muscles.Sum(m => m.burden.MaxWeekWA);
        int remainingWA = WeekWA;

        debugString?.AppendLine($"Суммарный недельный максимум группы: {totalMaxWeekWA}");

        // Первое распределение: пропорционально MaxWeekWA
        foreach (var muscle in muscles)
        {
            float proportion = (float)muscle.burden.MaxWeekWA / totalMaxWeekWA;
            int allocated = (int)Math.Round(WeekWA * proportion);

            // Минимум 1 подход, максимум по лимиту мышцы
            allocated = Math.Clamp(allocated, 1, Math.Min(4, muscle.burden.MaxWeekWA));

            muscleAllocation[muscle] = allocated;
            remainingWA -= allocated;

            debugString?.AppendLine($"  {muscle.name}: {allocated} подходов (пропорция: {proportion:P0})");
        }

        // 2. Распределяем остаток (если есть)
        if (remainingWA > 0)
        {
            debugString?.AppendLine($"Осталось распределить: {remainingWA} подходов");

            // Сортируем мышцы по приоритету (меньше подходов → больше приоритет для доп. нагрузки)
            var sortedMuscles = muscles
                .OrderBy(m => muscleAllocation[m])
                .ThenByDescending(m => m.burden.MaxWeekWA)
                .ToList();

            for (int i = 0; remainingWA > 0 && i < sortedMuscles.Count; i++)
            {
                var muscle = sortedMuscles[i];
                int currentAllocation = muscleAllocation[muscle];
                int maxForMuscle = Math.Min(4, muscle.burden.MaxWeekWA);

                if (currentAllocation < maxForMuscle)
                {
                    int toAdd = Math.Min(remainingWA, maxForMuscle - currentAllocation);
                    muscleAllocation[muscle] += toAdd;
                    remainingWA -= toAdd;

                    debugString?.AppendLine($"  Добавлено {toAdd} подходов к {muscle.name}");
                }
            }
        }
        else if (remainingWA < 0)
        {
            debugString?.AppendLine($"Перераспределение: слишком много подходов выделено");
            // Можно добавить логику уменьшения
        }

        // 3. Создаем сеты для каждой мышцы
        debugString?.AppendLine("=== Создание сетов ===");
        foreach (var kvp in muscleAllocation)
        {
            var muscle = kvp.Key;
            int setsForMuscle = kvp.Value;

            if (setsForMuscle > 0)
            {
                var muscleSets = SetOfExercises.GetExercisesByMuscleWeekWA(
                    muscle,
                    setsForMuscle,
                    debugString);

                result.AddRange(muscleSets);

                debugString?.AppendLine($"  Мышца '{muscle.name}': {setsForMuscle} подходов → {muscleSets.Count} сетов");
            }
        }

        debugString?.AppendLine($"=== Итого: {result.Count} сетов, {result.Sum(s => s.exercises.Count)} подходов ===");

        return result;
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
    public static readonly string path = DataPath.Path() + "/MuscleGroupData.json";
    public static List<MuscleGroup> muscleGroups
    {
        get
        {
            return Load();

        }
        set
        {
            _muscleGroups = value;
            Save(_muscleGroups);
        }
    }
    public static List<MuscleGroup> CreateMuscleGroups()
    {
        return new List<MuscleGroup>
        {
            // Грудь: 6-10 подходов в день, 12-18 в неделю
            new MuscleGroup("chest", new Burden(8, 16,18)),
        
            // Спина: 8-12 подходов в день, 16-22 в неделю
           new MuscleGroup("back", new Burden(10, 20,22)),
        
            // Плечи: 6-9 подходов в день, 12-18 в неделю
           new MuscleGroup("deltoid", new Burden(9, 18,17)),
        
           // Руки (бицепс+трицепс+предплечья): 8-13 подходов в день, 17-26 в неделю
            new MuscleGroup("hands", new Burden(13, 26,13)),
        
           // Ноги: 10-15 подходов в день, 16-25 в неделю
           new MuscleGroup("legs", new Burden(15, 25,20)),
        
           // Кор/Пресс: 4-8 подходов в день, 10-18 в неделю
           new MuscleGroup("core", new Burden(8, 16,10))
        };
    }

    private static DateTime updateTime; 
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
    #region Загрузка
    private static List<MuscleGroup> Load()
    {
        if (!File.Exists(path ?? DataPath.Path() + "/MuscleGroupData.json"))
        {
            Save(CreateMuscleGroups());
            return CreateMuscleGroups();
        }
        if (updateTime != File.GetLastWriteTime(path ?? DataPath.Path() + "/MuscleGroupData.json")) return JsonUtility.FromJson<MuscleGroupsWrapper>(File.ReadAllText(path ?? DataPath.Path() + "/MuscleGroupData.json")).muscleGroups;
        else return _muscleGroups;
    } 
    #endregion
    #region Сохранение
    private static void Save(List<MuscleGroup> muscleGroups)
    {
        File.WriteAllText(path ?? DataPath.Path() + "/MuscleGroupData.json", JsonUtility.ToJson(new MuscleGroupsWrapper(muscleGroups), true));
        updateTime = File.GetLastWriteTime(path ?? DataPath.Path() + "/MuscleGroupData.json");
    }
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

            // 4. СОХРАНЯЕМ ВСЕЙ СПИСОК
            muscleGroups = groups; // Вызовет сеттер, который сохранит в файл
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
