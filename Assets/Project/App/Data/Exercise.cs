
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Unity.VectorGraphics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

#region Основной класс
[System.Serializable]
public class Exercise
{
    #region Параметры и конструктор
    public string name;
    [SerializeReference]
    public SpecificParameters specificParameters;
    public List<Muscle> muscles;
    public int priority;
    public bool isStreet;
    public Exercise(string name, List<Muscle> muscles, SpecificParameters specificParameters, int priority = 4, bool isStreet=false)
    {
        this.name = name;
        this.specificParameters = specificParameters;
        this.muscles = muscles;
        this.priority = priority;
        this.isStreet = isStreet;
    }
    public Exercise() { }
    #endregion

    #region Работа с ID
    public short id;


    #endregion

}
#endregion

#region Доп классы
[System.Serializable]
public abstract class SpecificParameters
{
    public abstract override string ToString();
    public abstract string GetDescription(string name);
    /// <summary>
    /// Автоматически устанавливает параметры
    /// </summary>
    public abstract void SetParametrs(Player player,byte ApproachNumber = 0);
    public abstract SpecificParameters DeepClone(SpecificParameters specificParameters);

    /// <summary>
    /// Устанавливает новые параметры из списка.
    /// первый параметр в классе == newParametrs[0].
    /// </summary>
    public abstract void SetNewParametrs(List<float> newParametrs);

    /// <summary>
    /// Выдает параметры упражнения.
    /// List[0] == первый параметр в классе.
    /// </summary>
    public abstract List<float> GetParametrs();





    public string debugString;
    protected string GetHourWord(int hours)
    {
        int lastTwo = hours % 100;
        if (lastTwo >= 11 && lastTwo <= 14) return "часов";

        return (hours % 10) switch
        {
            1 => "час",
            2 or 3 or 4 => "часа",
            _ => "часов"
        };
    }
    protected string GetMinuteWord(int minutes)
    {
        int lastTwo = minutes % 100;
        if (lastTwo >= 11 && lastTwo <= 14) return "минут";

        return (minutes % 10) switch
        {
            1 => "минута",
            2 or 3 or 4 => "минуты",
            _ => "минут"
        };
    }
    protected string GetSecondsWord(int seconds)
    {
        int lastTwo = seconds % 100;
        if (lastTwo >= 11 && lastTwo <= 14) return "секунд";

        return (seconds % 10) switch
        {
            1 => "секунда",
            2 or 3 or 4 => "секунды",
            _ => "секунд"
        };
    }
}
[System.Serializable]




public class Cardio : SpecificParameters
{
    #region Класс для сериализации
    [System.Serializable]
    public class SerializableTimeSpan
    {
        public long ticks;

        public TimeSpan TimeSpan
        {
            get => new TimeSpan(ticks);
            set => ticks = value.Ticks;
        }

        public SerializableTimeSpan() => ticks = 0;
        public SerializableTimeSpan(TimeSpan timeSpan) => ticks = timeSpan.Ticks;

        public static implicit operator TimeSpan(SerializableTimeSpan sts) => sts.TimeSpan;
        public static implicit operator SerializableTimeSpan(TimeSpan ts) => new SerializableTimeSpan(ts);
    } 
    #endregion
    #region Переменные и конструкторы
    public SerializableTimeSpan time;
    public float MET;
    public Cardio(float MET, TimeSpan time)
    {
        this.MET = MET;
        this.time = time;
    }
    public Cardio() { }
    #endregion


    #region Методы для расчета параметров



    #endregion
    #region Основные публичные методы
    public override SpecificParameters DeepClone(SpecificParameters specificParameters)
    {
        if (specificParameters is Cardio сardioToClone)
        {
            return new Cardio(
                сardioToClone.MET,
                сardioToClone.time
            );
        }

        throw new ArgumentException("Параметр должен быть типа Cardio", nameof(specificParameters));
    }
    public override void SetParametrs(Player player, byte ApproachNumber = 0) {  }
    public override string ToString() { return $"{(int)time.TimeSpan.TotalMinutes} минут"; }
    public override string GetDescription(string name)
    {
        return null;
    }
    public override void SetNewParametrs(List<float> newParametrs)
    {
        time.TimeSpan = TimeSpan.FromMinutes(newParametrs[0]);
    }
    public override List<float> GetParametrs()
    {
        return new List<float> { (float)time.TimeSpan.TotalMinutes };
    }
    #endregion
    #region Для расчета каллорий
    public float GetCalories()
    {
        return (float)(MET*time.TimeSpan.TotalHours*Player.player.weight);
    }
    #endregion
}
[System.Serializable]
class Walk : Cardio
{
    #region Конструкторы и параметры
    public Distance distance;
    public Speed speed;
    public int angle;
    public Walk(Distance distance, Speed speed, int angle = 0)
    {
        this.distance = distance;
        this.speed = speed;
        this.angle = angle;
        time = distance / speed;
        MET = GetMetBySpeed(speed) * GetInclineCoefficient(angle);
    }
    #endregion

    #region Расчет МЕТ
    private static float GetMetBySpeed(Speed speedKmH)
    {
        return speedKmH.KilometersPerHour switch
        {
            // Ходьба
            <= 2.5f => 2.0f,      // Очень медленно
            <= 3.0f => 2.8f,
            <= 4.0f => 3.0f,      // Прогулка
            <= 5.0f => 3.5f,      // Обычная ходьба
            <= 6.0f => 4.3f,      // Быстрая ходьба
            <= 7.0f => 5.0f,      // Очень быстрая ходьба

            // Бег трусцой
            <= 8.0f => 8.0f,      // 8 км/ч
            <= 9.0f => 9.0f,      // 9 км/ч
            <= 10.0f => 10.0f,    // 10 км/ч

            // Бег
            <= 11.0f => 11.0f,    // 11 км/ч
            <= 12.0f => 12.3f,    // 12 км/ч
            <= 13.0f => 13.5f,    // 13 км/ч
            <= 14.0f => 14.5f,    // 14 км/ч
            <= 15.0f => 15.8f,    // 15 км/ч
            <= 16.0f => 17.0f,    // 16 км/ч

            // Спринт
            _ => 18.0f + (speedKmH.KilometersPerHour - 16) * 1.5f // +1.5 MET за каждый км/ч сверх 16
        };
    }
    private static float GetInclineCoefficient(float angleDegrees)
    {
        // Абсолютное значение угла (работает и для подъема, и для спуска)
        float absAngle = Mathf.Abs(angleDegrees);

        // Для подъёма (угол > 0) MET увеличивается
        // Для спуска (угол < 0) MET уменьшается, но не менее базового
        if (angleDegrees > 0)
        {
            // Коэффициент увеличения MET при подъёме
            return absAngle switch
            {
                <= 2 => 1.1f,    // +10%
                <= 5 => 1.25f,   // +25%
                <= 8 => 1.4f,    // +40%
                <= 12 => 1.6f,   // +60%
                <= 15 => 1.8f,   // +80%
                <= 20 => 2.1f,   // +110%
                <= 25 => 2.5f,   // +150%
                <= 30 => 3.0f,   // +200%
                _ => 3.5f        // +250% при очень крутых подъёмах
            };
        }
        else if (angleDegrees < 0)
        {
            // Коэффициент уменьшения MET при спуске
            return absAngle switch
            {
                <= 2 => 0.95f,   // -5%
                <= 5 => 0.9f,    // -10%
                <= 10 => 0.85f,  // -15%
                <= 15 => 0.8f,   // -20%
                _ => 0.7f        // -30% при крутых спусках
            };
        }

        return 1.0f; // Угол = 0
    }

    /// <summary>
    /// Возвращает скорость (км/ч) на основе значения MET для ровной поверхности (угол = 0)
    /// </summary>
    /// <param name="targetMET">Целевое значение MET (2.0 - 30.0)</param>
    /// <returns>Скорость, соответствующая целевому MET</returns>
    public static Speed GetSpeedByMET(float targetMET)
    {
        // Проверка входных данных
        if (targetMET < 2.0f)
            return Speed.FromKilometersPerHour(2.5f); // Минимальная скорость ходьбы

        if (targetMET > 30.0f)
            return Speed.FromKilometersPerHour(25.0f); // Максимальная реалистичная скорость

        // Обратное преобразование на основе твоей функции GetMetBySpeed
        return targetMET switch
        {
            // Ходьба
            <= 2.0f => Speed.FromKilometersPerHour(2.5f),      // Очень медленно
            <= 2.8f => Speed.FromKilometersPerHour(3.0f),
            <= 3.0f => Speed.FromKilometersPerHour(4.0f),      // Прогулка
            <= 3.5f => Speed.FromKilometersPerHour(5.0f),      // Обычная ходьба
            <= 4.3f => Speed.FromKilometersPerHour(6.0f),      // Быстрая ходьба
            <= 5.0f => Speed.FromKilometersPerHour(7.0f),      // Очень быстрая ходьба

            // Бег трусцой
            <= 8.0f => Speed.FromKilometersPerHour(8.0f),
            <= 9.0f => Speed.FromKilometersPerHour(9.0f),
            <= 10.0f => Speed.FromKilometersPerHour(10.0f),

            // Бег
            <= 11.0f => Speed.FromKilometersPerHour(11.0f),
            <= 12.3f => Speed.FromKilometersPerHour(12.0f),
            <= 13.5f => Speed.FromKilometersPerHour(13.0f),
            <= 14.5f => Speed.FromKilometersPerHour(14.0f),
            <= 15.8f => Speed.FromKilometersPerHour(15.0f),
            <= 17.0f => Speed.FromKilometersPerHour(16.0f),

            // Спринт (линейная интерполяция для значений выше 17.0)
            _ => Speed.FromKilometersPerHour(16.0f + (targetMET - 17.0f) / 1.5f)
        };
    }
    #endregion

    #region автоматическое создание упражнения
    public static Exercise AutoCreateWalk(TimeSpan time, int calories)
    {
        if(time<TimeSpan.Zero) return null;
        float targetMET = calories / (Player.player.weight * (float)time.TotalHours);
        Speed speed = GetSpeedByMET(targetMET);


        string getName()=>speed.KilometersPerHour switch
        {
            >= 1 and < 5 => "Ходьба",
            >= 5 and < 7 => "Быстрая ходьба",
            >= 7 =>"Бег",
            _=>"Ошибка скорость > 1"
        };

        return new Exercise(getName(), null,new Walk(speed*time,speed));
    }
    #endregion

    #region Публичные методы
    public override void SetNewParametrs(List<float> newParametrs)
    {
        distance = new Distance(newParametrs[0]);
        speed = new Speed(newParametrs[1], "KPH");
        angle = (int)newParametrs[2];
        MET = GetMetBySpeed(speed)* GetInclineCoefficient(angle);
    }
    public override List<float> GetParametrs()
    {
        return new List<float> { distance.Meters, speed.KilometersPerHour, angle };
    }
    public override void SetParametrs(Player player, byte ApproachNumber = 0)
    {
        
    }


    public override string GetDescription(string name)
    {
        var parts = new List<string>
        {
            $"Скорость: {speed} км/ч",
            $"Время: {time}",
            $"Дистанция: {distance}"
        };

        var angleStr = FormatAngleString(angle);
        if (!string.IsNullOrEmpty(angleStr))
            parts.Add(angleStr);

        return string.Join(Environment.NewLine, parts.Where(p => !string.IsNullOrEmpty(p)));
    }
    public override string ToString()
    {
        return $"{speed.ToString("kmh")} {time.TimeSpan.TotalMinutes} минут";
    }

    public override SpecificParameters DeepClone(SpecificParameters specificParameters)
    {
        Walk walk = specificParameters as Walk;
        return new Walk(walk.distance, walk.speed, walk.angle);

    }
    #endregion

    #region Методы для вывода информации
    private string FormatAngleString(float _angle) =>
        _angle == 0 ? string.Empty : $"Угол подъема: {_angle}°";
    #endregion


}





[System.Serializable]
public class StrengthTraining : SpecificParameters
{
    #region Переменные и конструкторы
    public float workWeight;
    public int repetitions;
    public int baseRep;
    public float onePm;
    public float twelvePm;
    public byte ApproachNumber;
    public string description;
    public StrengthTraining(int baseRepetitions, float onePm, float twelvePm)
    {
        this.baseRep = baseRepetitions;
        this.onePm = onePm;
        this.twelvePm = twelvePm;
    }

    #endregion
    #region Методы для расчета параметров

    private void SetWorkWeight(Player player)
    {
        StringBuilder debugString = new StringBuilder();
        if (repetitions != 0 && onePm != 0 && twelvePm != 0 && player.weight!=0)
        {
            debugString.AppendLine($"Параметры игрока\n" +
                $"Вес - {player.weight}\n" 



                );
            debugString.AppendLine($"Параметры упражнения\n" +
            $"1 пм  - {this.onePm}\n"+
            $"12 пм  - {this.twelvePm}\n"


            );
            float onePm = player.weight*((float)this.onePm/100);
            debugString.AppendLine($"Эталонный 1 пм - {(short)onePm}кг");
            float twelvePm = player.weight * ((float)this.twelvePm / 100);
            debugString.AppendLine($"Эталонный 12 пм - {(short)twelvePm}");
            workWeight = (byte)GetWorkWeightByRepetitions(onePm, twelvePm,(byte)repetitions);
            debugString.AppendLine($"Эталонный {repetitions} пм - {workWeight}");
            workWeight = (byte)((int)workWeight *ExerciseManager.Coefficient.StrengthCoefficient);
            debugString.AppendLine($"итоговый {workWeight} - коифицент силы - {ExerciseManager.Coefficient.StrengthCoefficient}");

            debugString.AppendLine();
            debugString.AppendLine();
            debugString.AppendLine();
            debugString.AppendLine();
            debugString.AppendLine(ExerciseManager.Coefficient.GetDebugReport());



        }
        this.debugString = debugString.ToString();
    }
    private float GetWorkWeightByRepetitions(float onePm, float twelvePm,byte repetitions)
    {
        float workWeight;
        float step;
        if (repetitions == 1) return onePm;
        else if(repetitions == 12) return twelvePm;
        step = (onePm - twelvePm)/11;
        workWeight = onePm - step * (repetitions - 1);
        return workWeight;
    }


    #endregion
    #region Публичные методы
    public override SpecificParameters DeepClone(SpecificParameters specificParameters)
    {
        if (specificParameters is StrengthTraining strengthToClone)
        {
            StrengthTraining clone = new StrengthTraining(
                strengthToClone.baseRep,
                strengthToClone.onePm,
                strengthToClone.twelvePm)
            {
                repetitions = strengthToClone.repetitions,
                workWeight = strengthToClone.workWeight,
                ApproachNumber = strengthToClone.ApproachNumber,
                description = strengthToClone.description,
                debugString = strengthToClone.debugString
            };
            return clone;
        }
        throw new ArgumentException("Параметр должен быть типа StrengthTraining", nameof(specificParameters));
    }
    public override void SetParametrs(Player player, byte ApproachNumber = 0) 
    {
        if (player.treningParametrs.goal == Goal.IncreasedStrength) repetitions = (int)(baseRep * 0.5f);
        else repetitions = baseRep;

        SetWorkWeight(player);
    }
    public override string ToString()
    {
        if (workWeight > 0 && repetitions > 0) { return $"{workWeight} кг на {repetitions} раз"; }
        else if (workWeight <= 0 && repetitions > 0) return $"{repetitions} раз";
        else 
        {
            SetParametrs(Player.player,ApproachNumber);
            if (workWeight > 0 && repetitions > 0) { return $"{workWeight} кг на {repetitions} раз"; }
            else if (workWeight <= 0 && repetitions > 0) return $"{repetitions} раз";
            return "Ошибка : повторений < 1"; 
        }
    }
    public override string GetDescription(string name)
    {
        return Description.GetDescriptionByName(name);
    }

    public override void SetNewParametrs(List<float> newParametrs)
    {
        if(newParametrs.Count == 2)
        {
            workWeight = (byte)newParametrs[0];
            repetitions = (byte)newParametrs[1];
        }
        else
        {
            throw new ArgumentException($"неверное число параметров должно быть 2, а щас - {newParametrs.Count}");
        }
    }

    public override List<float> GetParametrs()
    {
        return new List<float> { (float)workWeight,(float)repetitions};
    }
    #endregion
}
[System.Serializable]
public class Static : SpecificParameters
{
    #region Переменные и конструкторы 
    public byte minutes;
    public byte seconds;
    public Static( byte minutes, byte seconds)
    {
        this.minutes = minutes;
        this.seconds = seconds;
    }

    #endregion
    #region Публичные методы
    public override SpecificParameters DeepClone(SpecificParameters specificParameters)
    {
        if (specificParameters is Static staticToClone)
        {
            return new Static(staticToClone.minutes, staticToClone.seconds);
        }

        throw new ArgumentException("Параметр должен быть типа Static", nameof(specificParameters));
    }
    public override string ToString()
    {
        string result = "";
        if (minutes > 0)
        {
            result += minutes + " " + GetMinuteWord(minutes);
            if (seconds > 0) result += " ";
        }
        if (seconds > 0)
        {
            result += seconds + " " + GetSecondsWord(seconds);
        }
        return result;
    }
    public override string GetDescription(string name) => Description.GetDescriptionByName(name);
    public override void SetParametrs(Player player, byte ApproachNumber = 0)
    {
    }

    public override void SetNewParametrs(List<float> newParametrs)
    {
        //throw new NotImplementedException();
    }

    public override List<float> GetParametrs()
    {
        return null;
    }


    #endregion
}
[System.Serializable]
public class Stretching : SpecificParameters
{
    public float seconds;
    public Stretching(float seconds) 
    { 
        this.seconds = seconds;
    }

    #region публичные методы
    public override SpecificParameters DeepClone(SpecificParameters specificParameters)
    {
        return new Stretching((specificParameters as Stretching).seconds);
    }

    public override string GetDescription(string name)
    {
        return Description.GetDescriptionByName(name);
    }

    public override List<float> GetParametrs()
    {
        return new List<float> { seconds };
    }

    public override void SetNewParametrs(List<float> newParametrs)
    {
        seconds = newParametrs[0];
    }

    public override void SetParametrs(Player player, byte ApproachNumber = 0)
    {
        
    }

    public override string ToString()
    {
        return $"{seconds} {GetSecondsWord((int)seconds)}";
    } 
    #endregion
}
[System.Serializable]
public class Calisthenics : SpecificParameters
{
    public int replications;
    public int baseRep;
    public Calisthenics(int replications)
    {
        this.baseRep = replications;
    }
    #region публичные методы
    public override SpecificParameters DeepClone(SpecificParameters specificParameters)
    {
        return new Calisthenics((specificParameters as Calisthenics).baseRep)
        {
            replications = (specificParameters as Calisthenics).replications
        };
    }

    public override string GetDescription(string name)
    {
        return Description.GetDescriptionByName(name);
    }

    public override List<float> GetParametrs()
    {
        return new List<float> {replications };
    }

    public override void SetNewParametrs(List<float> newParametrs)
    {
        replications = (int)newParametrs[0];
    }

    public override void SetParametrs(Player player, byte ApproachNumber = 0)
    {
        replications = (int)(baseRep * ExerciseManager.Coefficient.EnduranceCoefficient*ExerciseManager.Coefficient.StrengthCoefficient );
        
debugString += $"replications: {replications} → " +
            $"{replications * ExerciseManager.Coefficient.EnduranceCoefficient * ExerciseManager.Coefficient.StrengthCoefficient:F0}" +
            $" (Endurance: {ExerciseManager.Coefficient.EnduranceCoefficient:F2}," +
            $" Strength: {ExerciseManager.Coefficient.StrengthCoefficient:F2})\n";
    }

    public override string ToString()
    {
        return $"{replications} раз";
    } 
    #endregion
}

#endregion

#region Менеджер
[System.Serializable]
public class ExerciseManager
{
    #region Загрузка - сохранение

    #region Конструкторы
    public ExerciseManager(List<Exercise> exercises)
    {
        this.exercises = exercises;
    }
    public ExerciseManager() { }
    #endregion

    #region Статичные поля
    #region Сохранение загрузка апдэйт
    private static DateTime _lastLoadTime;
    public static string path { get { return $"{DataPath.Path()}/ExerciseData.json"; } }

    private static List<Exercise> _cachedExercises;
    public static List<Exercise> Exercises
    {
        get
        {
            {
                var fileTime = File.GetLastWriteTime(path);
                if (_cachedExercises == null || _lastLoadTime < fileTime)
                {
                    _cachedExercises = Load();
                    _lastLoadTime = DateTime.Now;
                }
                return _cachedExercises;
            }
        }
    }
    public static void Save(List<Exercise> exercises)
    {
        File.WriteAllText(path, JsonUtility.ToJson(new ExerciseManager(exercises), true));
    }
    public static void UpdateExercise(Exercise exercise)
    {
        var exercises = Exercises.ToList();
        int index = exercises.FindIndex(e => e.name == exercise.name);

        if (index == -1)
            throw new KeyNotFoundException($"Упражнение '{exercise.name}' не найдено");

        exercises[index] = exercise;
        Save(exercises);
    }

    private static List<Exercise> Load()
    {
        if (File.Exists(path))
        {
            return JsonUtility.FromJson<ExerciseManager>(File.ReadAllText(path)).exercises;
        }
        else
        {
            return GetBaseExercises();
        }
    }
    #endregion
    #region Get методы
    public static Exercise GetExercisesByName(string name)
    {
        var exercise = Exercises.FirstOrDefault(e => e.name == name);
        return exercise == null ? throw new KeyNotFoundException($"Упражнение '{name}' не найдено") : exercise;
    }
    public static List<Exercise> GetExercisesByNames(List<string> names)
    {
        List<Exercise> exercises = new();
        for(int i = 0; i < names.Count; i++)
        {
            var exercise = Exercises.FirstOrDefault(e => e.name == names[i]);
            if (exercise == null) { Debug.Log($"Упражнение '{names[i]}' не найдено");continue; }
            exercises.Add(exercise);
        }
        return exercises;
    }
    public static List<Exercise> GetExercisesByMuscle(Muscle muscle, int minPercentageOfWork = 50)
    {
        return ExerciseManager.Exercises
            .Where(ex =>
            {
                // Находим мышцы с процентом > minPercentageOfWork
                var primaryMuscles = ex.muscles
                    .Where(m => m.percentageOfWork > minPercentageOfWork)
                    .ToList();

                // Если есть первичные мышцы, проверяем нашу мышцу
                if (primaryMuscles.Any())
                    return primaryMuscles.Any(m => m.name == muscle.name);

                // Если нет первичных (>50), берем 2 самые работающие мышцы
                var topMuscles = ex.muscles
                    .OrderByDescending(m => m.percentageOfWork)
                    .Take(2)
                    .ToList();

                return topMuscles.Any(m => m.name == muscle.name);
            })
            .ToList();
    }
    #endregion
    #endregion

    #region Нестатичные поля
    [SerializeField] private List<Exercise> exercises;

    #endregion

    #endregion

    #region Создание упражнений
    private static List<Exercise> GetBaseExercises()
    {

        List<Exercise> exercises = new List<Exercise>();
        #region только зал
        #region Грудь
        // Верх груди
        exercises.Add(new Exercise(
            "Жим гантелей на наклонной скамье",
            new List<Muscle>
            {
        new Muscle("Верх груди", 70),
        new Muscle("Передние дельты", 20),
        new Muscle("Трицепс", 10)
            },
            new StrengthTraining(8, 90, 65),
            2 // Средний приоритет (не самая базовая вариация жима)
        ));
        exercises.Add(new Exercise(
            "Подъемы гантелей лежа на наклонной скамье",
            new List<Muscle>
            {
        new Muscle("Верх груди", 85),
        new Muscle("Передние дельты", 15)
            },
            new StrengthTraining(12, 35, 25),
            3 // Низкий приоритет (изолирующее)
        ));

        // Середина груди
        exercises.Add(new Exercise(
            "Сведения в кроссовере через верхние блоки",
            new List<Muscle>
            {
        new Muscle("Середина груди", 90),
        new Muscle("Передние дельты", 10)
            },
            new StrengthTraining(12, 40, 30),
            3 // Низкий приоритет (изолирующее)
        ));
        exercises.Add(new Exercise(
            "Жим лежа",
            new List<Muscle>
            {
        new Muscle("Середина груди", 65),
        new Muscle("Трицепс", 25),
        new Muscle("Передние дельты", 10)
            },
            new StrengthTraining(10, 117, 82),
            1 // Высокий приоритет (базовое упражнение №1)
        ));
        exercises.Add(new Exercise(
            "Пуловер с гантелью лежа поперек скамьи",
            new List<Muscle>
            {
        new Muscle("Середина груди", 70),
        new Muscle("Широчайшие", 25),
        new Muscle("Трицепс", 5)
            },
            new StrengthTraining(12, 45, 35),
            2 // Средний приоритет (вспомогательное)
        ));

        #endregion

        #region Спина
        // Широчайшие мышцы


        exercises.Add(new Exercise(
            "Тяга верхнего блока широким хватом к груди",
            new List<Muscle>
            {
        new Muscle("Широчайшие", 75),
        new Muscle("Ромбовидные", 15),
        new Muscle("Бицепс", 10)
            },
            new StrengthTraining(12, 143, 100),
            2 // Средний приоритет (аналог подтягиваний)
        ));

        exercises.Add(new Exercise(
            "Тяга штанги в наклоне (хват на ширине плеч)",
            new List<Muscle>
            {
        new Muscle("Широчайшие", 60),
        new Muscle("Ромбовидные", 25),
        new Muscle("Задние дельты", 10),
        new Muscle("Бицепс", 5)
            },
            new StrengthTraining(8, 106, 74),
            1 // Высокий приоритет (базовое упражнение)
        ));

        exercises.Add(new Exercise(
            "Тяга горизонтального блока узким хватом",
            new List<Muscle>
            {
        new Muscle("Широчайшие", 70),
        new Muscle("Ромбовидные", 20),
        new Muscle("Бицепс", 10)
            },
            new StrengthTraining(10, 94, 66),
            2 // Средний приоритет (вспомогательное)
        ));

        // Трапециевидные мышцы
        exercises.Add(new Exercise(
            "Шраги со штангой сзади",
            new List<Muscle>
            {
        new Muscle("Трапеции", 100),
            },
            new StrengthTraining(15, 42, 29),
            3 // Низкий приоритет (изолирующее)
        ));

        exercises.Add(new Exercise(
            "Шраги с гантелями",
            new List<Muscle>
            {
        new Muscle("Трапеции", 100),
            },
            new StrengthTraining(12, 84, 59),
            3 // Низкий приоритет (изолирующее)
        ));

        exercises.Add(new Exercise(
            "Тяга штанги к подбородку широким хватом",
            new List<Muscle>
            {
        new Muscle("Трапеции", 80),
        new Muscle("Передние дельты", 20),
            },
            new StrengthTraining(10, 47, 33),
            2 // Средний приоритет (вспомогательное для дельт и трапеций)
        ));

        // Ромбовидные мышцы
        exercises.Add(new Exercise(
            "Тяга Т-грифа с упором в грудь",
            new List<Muscle>
            {
        new Muscle("Ромбовидные", 65),
        new Muscle("Широчайшие", 25),
        new Muscle("Задние дельты", 10)
            },
            new StrengthTraining(10, 106, 74),
            2 // Средний приоритет (вспомогательное)
        ));

        exercises.Add(new Exercise(
            "Разведение гантелей в наклоне",
            new List<Muscle>
            {
        new Muscle("Задние дельты", 70),
        new Muscle("Ромбовидные", 20),
        new Muscle("Трапеции", 10)
            },
            new StrengthTraining(12, 24, 16),
            2 // Средний приоритет (вспомогательное для задних дельт)
        ));

        // Поясница
        exercises.Add(new Exercise(
            "Становая тяга",
            new List<Muscle>
            {
        new Muscle("Поясница", 40),
        new Muscle("Широчайшие", 25),
        new Muscle("Ягодичные", 20),
        new Muscle("Бицепс бедра", 15)
            },
            new StrengthTraining(5, 141, 99),
            1 // Высокий приоритет (базовое упражнение №1 для спины)
        ));

        exercises.Add(new Exercise(
            "Гиперэкстензия с дополнительным весом",
            new List<Muscle>
            {
        new Muscle("Поясница", 90),
        new Muscle("Ягодичные", 10)
            },
            new StrengthTraining(12, 59, 41),
            2 // Средний приоритет (вспомогательное)
        ));
        #endregion

        #region Плечи
        // Передняя дельта
        exercises.Add(new Exercise(
            "Армейский жим стоя",
            new List<Muscle>
            {
        new Muscle("Передние дельты", 60),
        new Muscle("Средние дельты", 25),
        new Muscle("Трицепс", 15)
            },
            new StrengthTraining(8, 56, 39),
            1 // Высокий приоритет (базовое упражнение для плеч)
        ));

        exercises.Add(new Exercise(
            "Жим гантелей сидя",
            new List<Muscle>
            {
        new Muscle("Передние дельты", 70),
        new Muscle("Средние дельты", 20),
        new Muscle("Трицепс", 10)
            },
            new StrengthTraining(10, 59, 41),
            1 // Высокий приоритет (базовое упражнение)
        ));

        exercises.Add(new Exercise(
            "Подъемы гантелей перед собой",
            new List<Muscle>
            {
        new Muscle("Передние дельты", 90),
        new Muscle("Средние дельты", 10)
            },
            new StrengthTraining(12, 31, 22),
            3 // Низкий приоритет (изолирующее)
        ));

        // Средняя дельта
        exercises.Add(new Exercise(
            "Махи гантелями в стороны стоя",
            new List<Muscle>
            {
        new Muscle("Средние дельты", 95),
        new Muscle("Передние дельты", 5)
            },
            new StrengthTraining(15, 37, 26),
            2 // Средний приоритет (ключевое для средней дельты)
        ));

        exercises.Add(new Exercise(
            "Тяга штанги к подбородку широким хватом",
            new List<Muscle>
            {
        new Muscle("Средние дельты", 70),
        new Muscle("Трапеции", 20),
        new Muscle("Передние дельты", 10)
            },
            new StrengthTraining(10, 47, 33),
            2 // Средний приоритет (вспомогательное)
        ));

        exercises.Add(new Exercise(
            "Махи в стороны в тренажере",
            new List<Muscle>
            {
        new Muscle("Средние дельты", 90),
        new Muscle("Передние дельты", 10)
            },
            new StrengthTraining(12, 40, 28),
            3 // Низкий приоритет (изолирующее в тренажере)
        ));

        // Задняя дельта
        exercises.Add(new Exercise(
            "Махи гантелями в наклоне",
            new List<Muscle>
            {
        new Muscle("Задние дельты", 85),
        new Muscle("Средние дельты", 15)
            },
            new StrengthTraining(12, 32, 22),
            2 // Средний приоритет (ключевое для задних дельт)
        ));

        exercises.Add(new Exercise(
            "Разведения в тренажере Peck-Deck",
            new List<Muscle>
            {
        new Muscle("Задние дельты", 80),
        new Muscle("Средние дельты", 20)
            },
            new StrengthTraining(12, 35, 25),
            3 // Низкий приоритет (изолирующее в тренажере)
        ));
        #endregion

        #region Руки
        // Бицепс
        exercises.Add(new Exercise(
            "Подъем штанги на бицепс стоя",
            new List<Muscle>
            {
        new Muscle("Бицепс", 95),
        new Muscle("Предплечья", 5)
            },
            new StrengthTraining(8, 76, 47),
            1 // Высокий приоритет (базовое упражнение для бицепса)
        ));

        exercises.Add(new Exercise(
            "Подъем гантелей на бицепс сидя",
            new List<Muscle>
            {
        new Muscle("Бицепс", 90),
        new Muscle("Предплечья", 10)
            },
            new StrengthTraining(10, 71, 44),
            2 // Средний приоритет (вспомогательное)
        ));

        exercises.Add(new Exercise(
            "Молотковые сгибания с гантелями",
            new List<Muscle>
            {
        new Muscle("Бицепс", 70),
        new Muscle("Предплечья", 30)
            },
            new StrengthTraining(10, 65, 40),
            2 // Средний приоритет (вспомогательное для брахиалиса)
        ));

        // Трицепс
        exercises.Add(new Exercise(
            "Французский жим лежа (EZ-гриф)",
            new List<Muscle>
            {
        new Muscle("Трицепс", 95),
        new Muscle("Передние дельты", 5)
            },
            new StrengthTraining(10, 55, 38),
            1 // Высокий приоритет (базовое упражнение для трицепса)
        ));


        exercises.Add(new Exercise(
            "Разгибания на трицепс в верхнем блоке с канатом",
            new List<Muscle>
            {
        new Muscle("Трицепс", 100)
            },
            new StrengthTraining(12, 40, 28),
            2 // Средний приоритет (вспомогательное/добивающее)
        ));

        // Предплечья
        exercises.Add(new Exercise(
            "Сгибание запястий со штангой сидя",
            new List<Muscle>
            {
        new Muscle("Предплечья", 100)
            },
            new StrengthTraining(15, 25, 18),
            3 // Низкий приоритет (изолирующее)
        ));

        exercises.Add(new Exercise(
            "Разгибание запястий со штангой сидя",
            new List<Muscle>
            {
        new Muscle("Предплечья", 100)
            },
            new StrengthTraining(15, 20, 14),
            3 // Низкий приоритет (изолирующее)
        ));
        #endregion

        #region Ноги
        // Квадрицепс
        exercises.Add(new Exercise(
            "Приседания со штангой на спине",
            new List<Muscle>
            {
        new Muscle("Квадрицепс", 60),
        new Muscle("Ягодичные", 25),
        new Muscle("Бицепс бедра", 10),
        new Muscle("Поясница", 5)
            },
            new StrengthTraining(10, 141, 94),
            1 // Высокий приоритет (базовое упражнение №1 для ног)
        ));

        exercises.Add(new Exercise(
            "Жим ногами в тренажере",
            new List<Muscle>
            {
        new Muscle("Квадрицепс", 80),
        new Muscle("Ягодичные", 15),
        new Muscle("Бицепс бедра", 5)
            },
            new StrengthTraining(10, 212, 141),
            2 // Средний приоритет (вспомогательное)
        ));

        exercises.Add(new Exercise(
            "Разгибания ног в тренажере",
            new List<Muscle>
            {
        new Muscle("Квадрицепс", 95),
        new Muscle("Ягодичные", 5)
            },
            new StrengthTraining(12, 94, 66),
            3 // Низкий приоритет (изолирующее)
        ));

        // Ягодичные
        exercises.Add(new Exercise(
            "Румынская тяга",
            new List<Muscle>
            {
        new Muscle("Ягодичные", 60),
        new Muscle("Бицепс бедра", 30),
        new Muscle("Поясница", 10)
            },
            new StrengthTraining(8, 118, 82),
            1 // Высокий приоритет (базовое для задней поверхности)
        ));

        exercises.Add(new Exercise(
            "Выпады со штангой",
            new List<Muscle>
            {
        new Muscle("Ягодичные", 70),
        new Muscle("Квадрицепс", 20),
        new Muscle("Бицепс бедра", 10)
            },
            new StrengthTraining(10, 88, 62),
            2 // Средний приоритет (вспомогательное)
        ));

        exercises.Add(new Exercise(
            "Ягодичный мост со штангой",
            new List<Muscle>
            {
        new Muscle("Ягодичные", 85),
        new Muscle("Бицепс бедра", 10),
        new Muscle("Поясница", 5)
            },
            new StrengthTraining(10, 176, 124),
            2 // Средний приоритет (вспомогательное, но эффективное для ягодиц)
        ));

        // Бицепс бедра
        exercises.Add(new Exercise(
            "Сгибания ног лежа в тренажере",
            new List<Muscle>
            {
        new Muscle("Бицепс бедра", 95),
        new Muscle("Ягодичные", 5)
            },
            new StrengthTraining(12, 59, 41),
            3 // Низкий приоритет (изолирующее)
        ));

        exercises.Add(new Exercise(
            "Становая тяга на прямых ногах",
            new List<Muscle>
            {
        new Muscle("Бицепс бедра", 70),
        new Muscle("Ягодичные", 20),
        new Muscle("Поясница", 10)
            },
            new StrengthTraining(8, 124, 87),
            1 // Высокий приоритет (базовое упражнение)
        ));

        // Икры
        exercises.Add(new Exercise(
            "Подъемы на носки стоя в тренажере",
            new List<Muscle>
            {
        new Muscle("Икры", 100)
            },
            new StrengthTraining(15, 176, 124),
            2 // Средний приоритет (основное для икр)
        ));

        exercises.Add(new Exercise(
            "Подъемы на носки сидя",
            new List<Muscle>
            {
        new Muscle("Икры", 100)
            },
            new StrengthTraining(15, 141, 99),
            3 // Низкий приоритет (дополнительное для икр)
        ));
        #endregion

        #region Пресс
        exercises.Add(new Exercise(
            "Скручивания на римском стуле",
            new List<Muscle>
            {
        new Muscle("Верх пресса", 80),
        new Muscle("Низ пресса", 15),
        new Muscle("Косые мышцы", 5)
            },
            new StrengthTraining(15, 35, 25),
            2 // Средний приоритет (вспомогательное для пресса)
        ));

        exercises.Add(new Exercise(
            "Подъемы ног в висе",
            new List<Muscle>
            {
        new Muscle("Низ пресса", 85),
        new Muscle("Верх пресса", 10),
        new Muscle("Косые мышцы", 5)
            },
            new Static(1, 0),
            1 // Высокий приоритет (базовое для пресса)
        ));

        exercises.Add(new Exercise(
            "Боковые скручивания на полу",
            new List<Muscle>
            {
        new Muscle("Косые мышцы", 90),
        new Muscle("Верх пресса", 10)
            },
            new Static(1, 0),
            3 // Низкий приоритет (изолирующее для косых)
        ));
        #endregion

        #endregion

        #region улица
        #region Силовые

        exercises.Add(new Exercise(
            "Отжимания на брусьях с акцентом на грудь",
            new List<Muscle>
            {
        new Muscle("Низ груди", 60),
        new Muscle("Трицепс", 30),
        new Muscle("Передние дельты", 10)
            },
            new Calisthenics(12),
            1, true // Высокий приоритет (базовое упражнение)
        ));

        exercises.Add(new Exercise(
            "Отжимания на брусьях (акцент на трицепс)",
            new List<Muscle>
            {
        new Muscle("Трицепс", 85),
        new Muscle("Низ груди", 10),
        new Muscle("Передние дельты", 5)
            },
            new Calisthenics(12),
            1,true // Высокий приоритет (базовое упражнение)
        ));

        exercises.Add(new Exercise(
            "Отжимания от пола (классические)",
            new List<Muscle>
            {
        new Muscle("Середина груди", 52),  // Было: "Грудь (средняя часть)"
        new Muscle("Трицепс", 30),
        new Muscle("Передние дельты", 15),
        new Muscle("Верх пресса", 3)       // Было: "Пресс"
                                           // Убрано: "Передняя зубчатая" (2%) - нет в базовом списке
            },
            new Calisthenics(35),
            1, true // Высокий приоритет (базовое упражнение)
        ));

        exercises.Add(new Exercise(
            "Отжимания с широкой постановкой рук",
            new List<Muscle>
            {
        new Muscle("Середина груди", 60),  // Было: "Грудь (внешняя часть)"
        new Muscle("Трицепс", 20),
        new Muscle("Передние дельты", 15),
        new Muscle("Широчайшие", 3),
        new Muscle("Верх пресса", 2)       // Было: "Пресс"
            },
            new Calisthenics(20),
            2, true // Средний приоритет (акцент на грудь)
        ));

        exercises.Add(new Exercise(
            "Алмазные отжимания (узкий хват)",
            new List<Muscle>
            {
        new Muscle("Трицепс", 65),
        new Muscle("Внутренняя часть груди", 25),  // Было: "Грудь (внутренняя часть)"
        new Muscle("Передние дельты", 8),
        new Muscle("Предплечья", 2)
            },
            new Calisthenics(20),
            2, true // Средний приоритет (акцент на трицепс)
        ));

        exercises.Add(new Exercise(
            "Отжимания с ногами на возвышении",
            new List<Muscle>
            {
        new Muscle("Верх груди", 55),
        new Muscle("Трицепс", 25),
        new Muscle("Передние дельты", 15),
        new Muscle("Верх пресса", 3),      // Было: "Пресс"
        new Muscle("Косые мышцы", 2)       // Было: "Косая мышца живота"
            },
            new Calisthenics(25),
            2, true // Средний приоритет (акцент на верх груди)
        ));

        exercises.Add(new Exercise(
            "Подтягивания широким хватом",
            new List<Muscle>
            {
        new Muscle("Широчайшие", 80),
        new Muscle("Бицепс", 15),
        new Muscle("Предплечья", 5)
            },
            new Calisthenics(6),
            1, true // Высокий приоритет (базовое упражнение №1)
        ));

        exercises.Add(new Exercise(
            "Подтягивания (стандартный хват)",
            new List<Muscle>
            {
        new Muscle("Широчайшие", 75),
        new Muscle("Бицепс", 15),
        new Muscle("Ромбовидные", 5),
        new Muscle("Задние дельты", 3),
        new Muscle("Предплечья", 2)
            },
            new Calisthenics(12),
            1, true // Высокий приоритет (базовое упражнение №1 для спины)
        ));

        exercises.Add(new Exercise(
            "Подтягивания обратным хватом",
            new List<Muscle>
            {
        new Muscle("Широчайшие", 70),
        new Muscle("Бицепс", 25),
        new Muscle("Предплечья", 3),
        new Muscle("Ромбовидные", 2)
            },
            new Calisthenics(15),
            1, true // Высокий приоритет (базовое)
        ));

        exercises.Add(new Exercise(
            "Приседания с собственным весом",
            new List<Muscle>
            {
        new Muscle("Квадрицепс", 65),
        new Muscle("Ягодичные", 25),
        new Muscle("Бицепс бедра", 5),
        new Muscle("Икры", 3),
        new Muscle("Верх пресса", 2)       // Было: "Пресс"
            },
            new Calisthenics(25),
            1, true // Высокий приоритет (фундаментальное упражнение)
        ));

        exercises.Add(new Exercise(
            "Подъемы ног в висе",
            new List<Muscle>
            {
        new Muscle("Низ пресса", 85),
        new Muscle("Верх пресса", 10),     // Было: "Верх пресса"
        new Muscle("Косые мышцы", 5)       // Было: "Косые мышцы"
            },
            new Static(1, 0),
            1, true // Высокий приоритет (базовое для пресса)
        ));

        exercises.Add(new Exercise(
            "Боковые скручивания на полу",
            new List<Muscle>
            {
        new Muscle("Косые мышцы", 90),
        new Muscle("Верх пресса", 10)      // Было: "Верх пресса"
            },
            new Static(1, 0),
            3, true // Низкий приоритет (изолирующее для косых)
        ));

        exercises.Add(new Exercise(
            "Планка на предплечьях",
            new List<Muscle>
            {
        new Muscle("Верх пресса", 40),
        new Muscle("Низ пресса", 40),
        new Muscle("Поясница", 10),
        new Muscle("Ягодичные", 5),
        new Muscle("Передние дельты", 3),
        new Muscle("Бицепс бедра", 2)
            },
            new Static(1, 0), // 30 секунд для начала
            1, // Высокий приоритет для пауэрлифтеров
            true
        ));
        exercises.Add(new Exercise(
    "Шраги в висе на турнике",
    new List<Muscle>
    {
        new Muscle("Трапеции", 85),
        new Muscle("Широчайшие", 10),
        new Muscle("Предплечья", 5)
    },
    new Calisthenics(15),
    2,
    true
));

        exercises.Add(new Exercise(
            "Удержание виса на турнике с подъемом плеч",
            new List<Muscle>
            {
        new Muscle("Трапеции", 90),
        new Muscle("Предплечья", 10)
            },
            new Static(0, 30), // 30 секунд
            3,
            true
        ));
        exercises.Add(new Exercise(
    "Супермен (гиперэкстензия на полу)",
    new List<Muscle>
    {
        new Muscle("Поясница", 85),
        new Muscle("Ягодичные", 10),
        new Muscle("Бицепс бедра", 5)
    },
    new Calisthenics(15),
    2,
    false // Можно делать дома
));

        exercises.Add(new Exercise(
            "Мостик на одной ноге",
            new List<Muscle>
            {
        new Muscle("Поясница", 60),
        new Muscle("Ягодичные", 30),
        new Muscle("Бицепс бедра", 10)
            },
            new Calisthenics(12),
            2,
            false
        ));
        exercises.Add(new Exercise(
    "Вис на турнике",
    new List<Muscle>
    {
        new Muscle("Предплечья", 95),
        new Muscle("Трапеции", 5)
    },
    new Static(0, 120), // 120 секунд
    3,
    true
));

        exercises.Add(new Exercise(
            "Прогулка фермера (с гантелями/бутылками)",
            new List<Muscle>
            {
        new Muscle("Предплечья", 85),
        new Muscle("Трапеции", 10),
        new Muscle("Квадрицепс", 5)
            },
            new Static(0, 60), // 60 секунд ходьбы
            3,
            true
        ));
        exercises.Add(new Exercise(
    "Отжимания в стойке у стены (плечи)",
    new List<Muscle>
    {
        new Muscle("Передние дельты", 80),
        new Muscle("Трицепс", 15),
        new Muscle("Верх пресса", 5)
    },
    new Calisthenics(10),
    2,
    true
));

        exercises.Add(new Exercise(
            "Подъемы рук перед собой с импровизированным весом",
            new List<Muscle>
            {
        new Muscle("Передние дельты", 95),
        new Muscle("Средние дельты", 5)
            },
            new Calisthenics(20),
            3,
            false
        ));

        #endregion

        #endregion

        #region Растяжка

        #region Грудь
        // Растяжка верхней части груди
        exercises.Add(new Exercise(
            "Растяжка верхней части груди у стены",
            new List<Muscle>
            {
        new Muscle("Верх груди", 85),
        new Muscle("Передние дельты", 15)
            },
            new Stretching(40), // 40 секунд
            2,
            false
        ));

        // Растяжка середины груди
        exercises.Add(new Exercise(
            "Растяжка середины груди в дверном проеме",
            new List<Muscle>
            {
        new Muscle("Середина груди", 90),
        new Muscle("Передние дельты", 10)
            },
            new Stretching(45), // 45 секунд
            2,
            false
        ));

        // Растяжка нижней части груди
        exercises.Add(new Exercise(
            "Растяжка нижней части груди на фитболе",
            new List<Muscle>
            {
        new Muscle("Низ груди", 85),
        new Muscle("Передние дельты", 10),
        new Muscle("Верх пресса", 5)
            },
            new Stretching(35), // 35 секунд
            3,
            false
        ));

        // Растяжка внутренней части груди
        exercises.Add(new Exercise(
            "Растяжка внутренней части груди (ладони вместе)",
            new List<Muscle>
            {
        new Muscle("Внутренняя часть груди", 95),
        new Muscle("Передние дельты", 5)
            },
            new Stretching(30), // 30 секунд
            3,
            false
        ));

        #endregion

        #region Спина
        // Растяжка широчайших
        exercises.Add(new Exercise(
            "Растяжка широчайших в висе на турнике",
            new List<Muscle>
            {
        new Muscle("Широчайшие", 90),
        new Muscle("Поясница", 10)
            },
            new Stretching(50), // 50 секунд
            1,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка широчайших сидя наклон вперед",
            new List<Muscle>
            {
        new Muscle("Широчайшие", 80),
        new Muscle("Поясница", 15),
        new Muscle("Бицепс бедра", 5)
            },
            new Stretching(60), // 60 секунд
            2,
            false
        ));

        // Растяжка трапеций
        exercises.Add(new Exercise(
            "Растяжка трапеций наклон головы вбок",
            new List<Muscle>
            {
        new Muscle("Трапеции", 100),
            },
            new Stretching(30), // 30 секунд
            2,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка трапеций с помощью руки",
            new List<Muscle>
            {
        new Muscle("Трапеции", 100),
            },
            new Stretching(35), // 35 секунд
            3,
            false
        ));

        // Растяжка ромбовидных
        exercises.Add(new Exercise(
            "Растяжка ромбовидных обхват себя руками",
            new List<Muscle>
            {
        new Muscle("Ромбовидные", 85),
        new Muscle("Задние дельты", 15)
            },
            new Stretching(40), // 40 секунд
            3,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка ромбовидных сидя наклонившись вперед",
            new List<Muscle>
            {
        new Muscle("Ромбовидные", 80),
        new Muscle("Широчайшие", 20)
            },
            new Stretching(45), // 45 секунд
            3,
            false
        ));

        // Растяжка поясницы
        exercises.Add(new Exercise(
            "Растяжка поясницы кошка-корова",
            new List<Muscle>
            {
        new Muscle("Поясница", 95),
        new Muscle("Широчайшие", 5)
            },
            new Stretching(60), // 60 секунд (30+30)
            1,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка поясницы лежа на спине",
            new List<Muscle>
            {
        new Muscle("Поясница", 90),
        new Muscle("Ягодичные", 10)
            },
            new Stretching(50), // 50 секунд
            2,
            false
        ));

        // Шея
         exercises.Add(new Exercise(
        "Изометрическая растяжка шеи в стороны",
        new List<Muscle>
        {
            new Muscle("Шея", 100)  // 100% фокус на шею
        },
        new Stretching(40), // 40 секунд (20 секунд на каждую сторону)
        1,
        false
    ));
        #endregion

        #region Плечи
        // Растяжка передних дельт
        exercises.Add(new Exercise(
            "Растяжка передних дельт за спиной",
            new List<Muscle>
            {
        new Muscle("Передние дельты", 100),
            },
            new Stretching(40), // 40 секунд
            2,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка передних дельт у стены",
            new List<Muscle>
            {
        new Muscle("Передние дельты", 100),
            },
            new Stretching(35), // 35 секунд
            3,
            false
        ));

        // Растяжка средних дельт
        exercises.Add(new Exercise(
            "Растяжка средних дельт через руку",
            new List<Muscle>
            {
        new Muscle("Средние дельты", 85),
        new Muscle("Трапеции", 15)
            },
            new Stretching(30), // 30 секунд
            3,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка средних дельт скрестив руки",
            new List<Muscle>
            {
        new Muscle("Средние дельты", 80),
        new Muscle("Задние дельты", 20)
            },
            new Stretching(25), // 25 секунд
            3,
            false
        ));

        // Растяжка задних дельт
        exercises.Add(new Exercise(
            "Растяжка задних дельт обхват плеча",
            new List<Muscle>
            {
        new Muscle("Задние дельты", 90),
        new Muscle("Ромбовидные", 10)
            },
            new Stretching(35), // 35 секунд
            2,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка задних дельт с полотенцем",
            new List<Muscle>
            {
        new Muscle("Задние дельты", 85),
        new Muscle("Средние дельты", 15)
            },
            new Stretching(40), // 40 секунд
            3,
            false
        ));

        #endregion

        #region Руки
        // Растяжка бицепса
        exercises.Add(new Exercise(
            "Растяжка бицепса у стены",
            new List<Muscle>
            {
        new Muscle("Бицепс", 95),
        new Muscle("Передние дельты", 5)
            },
            new Stretching(30), // 30 секунд
            2,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка бицепса с опорой",
            new List<Muscle>
            {
        new Muscle("Бицепс", 90),
        new Muscle("Предплечья", 10)
            },
            new Stretching(25), // 25 секунд
            3,
            false
        ));

        // Растяжка трицепса
        exercises.Add(new Exercise(
            "Растяжка трицепса за головой",
            new List<Muscle>
            {
        new Muscle("Трицепс", 95),
        new Muscle("Передние дельты", 5)
            },
            new Stretching(35), // 35 секунд
            2,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка трицепса через плечо",
            new List<Muscle>
            {
        new Muscle("Трицепс", 90),
        new Muscle("Широчайшие", 10)
            },
            new Stretching(30), // 30 секунд
            3,
            false
        ));

        // Растяжка предплечий
        exercises.Add(new Exercise(
            "Растяжка предплечий ладонью вниз",
            new List<Muscle>
            {
        new Muscle("Предплечья", 100)
            },
            new Stretching(30), // 30 секунд
            3,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка предплечий ладонью вверх",
            new List<Muscle>
            {
        new Muscle("Предплечья", 100)
            },
            new Stretching(30), // 30 секунд
            3,
            false
        ));

        #endregion

        #region Ноги
        // Растяжка квадрицепса
        exercises.Add(new Exercise(
            "Растяжка квадрицепса стоя",
            new List<Muscle>
            {
        new Muscle("Квадрицепс", 95),
        new Muscle("Бицепс бедра", 5)
            },
            new Stretching(45), // 45 секунд
            1,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка квадрицепса лежа на боку",
            new List<Muscle>
            {
        new Muscle("Квадрицепс", 90),
        new Muscle("Бицепс бедра", 10)
            },
            new Stretching(50), // 50 секунд
            2,
            false
        ));

        // Растяжка бицепса бедра
        exercises.Add(new Exercise(
            "Растяжка бицепса бедра сидя",
            new List<Muscle>
            {
        new Muscle("Бицепс бедра", 95),
        new Muscle("Ягодичные", 5)
            },
            new Stretching(60), // 60 секунд
            1,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка бицепса бедра стоя",
            new List<Muscle>
            {
        new Muscle("Бицепс бедра", 90),
        new Muscle("Икры", 10)
            },
            new Stretching(40), // 40 секунд
            2,
            false
        ));

        // Растяжка ягодичных
        exercises.Add(new Exercise(
            "Растяжка ягодичных сидя скрестив ноги",
            new List<Muscle>
            {
        new Muscle("Ягодичные", 95),
        new Muscle("Бицепс бедра", 5)
            },
            new Stretching(50), // 50 секунд
            1,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка ягодичных лежа на спине",
            new List<Muscle>
            {
        new Muscle("Ягодичные", 90),
        new Muscle("Поясница", 10)
            },
            new Stretching(45), // 45 секунд
            2,
            false
        ));

        // Растяжка икр
        exercises.Add(new Exercise(
            "Растяжка икр у стены",
            new List<Muscle>
            {
        new Muscle("Икры", 100)
            },
            new Stretching(40), // 40 секунд
            2,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка икр на ступеньке",
            new List<Muscle>
            {
        new Muscle("Икры", 100)
            },
            new Stretching(45), // 45 секунд
            2,
            false
        ));

        #endregion

        #region Кор
        // Растяжка верхнего пресса
        exercises.Add(new Exercise(
            "Растяжка верхнего пресса лежа на животе",
            new List<Muscle>
            {
        new Muscle("Верх пресса", 90),
        new Muscle("Поясница", 10)
            },
            new Stretching(40), // 40 секунд
            2,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка верхнего пресса мостик",
            new List<Muscle>
            {
        new Muscle("Верх пресса", 85),
        new Muscle("Поясница", 15)
            },
            new Stretching(35), // 35 секунд
            3,
            false
        ));

        // Растяжка нижнего пресса
        exercises.Add(new Exercise(
            "Растяжка нижнего пресса кобра",
            new List<Muscle>
            {
        new Muscle("Низ пресса", 95),
        new Muscle("Поясница", 5)
            },
            new Stretching(45), // 45 секунд
            2,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка нижнего пресса лежа на спине",
            new List<Muscle>
            {
        new Muscle("Низ пресса", 90),
        new Muscle("Бицепс бедра", 10)
            },
            new Stretching(50), // 50 секунд
            2,
            false
        ));

        // Растяжка косых мышц
        exercises.Add(new Exercise(
            "Растяжка косых мышц в боковом наклоне",
            new List<Muscle>
            {
        new Muscle("Косые мышцы", 95),
        new Muscle("Широчайшие", 5)
            },
            new Stretching(35), // 35 секунд
            2,
            false
        ));

        exercises.Add(new Exercise(
            "Растяжка косых мышц сидя скручивание",
            new List<Muscle>
            {
        new Muscle("Косые мышцы", 90),
        new Muscle("Ромбовидные", 10)
            },
            new Stretching(30), // 30 секунд
            3,
            false
        ));

        #endregion
        #endregion

        #region Кардио
        exercises.Add(new Exercise("Ходьба",
            new List<Muscle>(),
            new Walk(new Distance(1000),
            new Speed(5,"KPH")
            )));

        exercises.Add(new Exercise("Бег",
            new List<Muscle>(),
            new Walk(new Distance(1000),
            new Speed(10, "KPH")
            )));
        exercises.Add(new Exercise(
    "Берпи (Burpees)",
    new List<Muscle>
    {
        new Muscle("Квадрицепс", 30),
        new Muscle("Ягодичные", 25),
        new Muscle("Трицепс", 15),
        new Muscle("Передние дельты", 15),
        new Muscle("Верх пресса", 10),
        new Muscle("Низ пресса", 5)
    },
    new Cardio(8.0f, TimeSpan.FromMinutes(2)), // MET 8.0, время 2 минуты
    1,
    true
));

        // Скакалка
        exercises.Add(new Exercise(
            "Скакалка",
            new List<Muscle>
            {
        new Muscle("Икры", 70),
        new Muscle("Квадрицепс", 15),
        new Muscle("Ягодичные", 10),
        new Muscle("Предплечья", 5)
            },
            new Cardio(10.0f, TimeSpan.FromMinutes(3)), // MET 10.0, время 3 минуты
            1,
            true
        ));

        #endregion
        return exercises;
    }
    #endregion

    #region Взаимодействие с упражнением

    public static Exercise DeepClone(Exercise exercise)
    {
        if(exercise.specificParameters == null) return new Exercise();
        Exercise cloneExercise = new Exercise()
        {
            name = exercise.name,
            specificParameters = exercise.specificParameters.DeepClone(exercise.specificParameters),
            muscles = exercise.muscles?.Select(m => Muscle.DeepClone(m)).ToList(),
            priority = exercise.priority,
            id = exercise.id
        };

        return cloneExercise;
    }
    public static List<Exercise> SetId(List<Exercise> exercises)
    {
        for (byte i = 0; i < exercises.Count; i++)
        {
            exercises[i].id = i;
        }
        return exercises;
    }
    #endregion

    #region Класс с коефицентами
    public static class Coefficient
    {
        static Player player { get { return Player.player; } set { } }
        public static float StrengthCoefficient { get { return GetStrengthCoefficient(); } private set { } }
        public static float EnduranceCoefficient { get { return GetEnduranceCoefficient(); } private set { } }
        public static float VolumeTolerance { get { return GetVolumeTolerance(); } private set { } }
        public static float WeightLossVolumeCoefficient { get { return GetWeightLossVolumeCoefficient(); } private set { } }

        // Вложенный класс для хранения отладочной информации
        private static class DebugInfo
        {
            // Исходные данные игрока
            public static int PlayerWeight;
            public static int PlayerAge;
            public static int PlayerExperience;
            public static int PlayerBodyFat;

            // Модификаторы для силы
            public static float StrWeightMod;
            public static float StrAgeMod;
            public static float StrExpMod;
            public static float StrBodyfatMod;
            public static float StrFinal;

            // Модификаторы для выносливости
            public static float EndWeightMod;
            public static float EndAgeMod;
            public static float EndExpMod;
            public static float EndBodyfatMod;
            public static float EndFinal;

            // Модификаторы для переносимости объема
            public static float VolAgeMod;
            public static float VolExpMod;
            public static float VolBodyfatMod;
            public static float VolDailyMod;
            public static float VolFinal;

            // Метод для обновления данных игрока
            public static void UpdatePlayerData()
            {
                PlayerWeight = player.weight;
                PlayerAge = player.age;
                PlayerExperience = player.experience;
                PlayerBodyFat = player.percentageOfFat;
            }
        }

        // Публичный метод для получения отладочного отчета
        public static string GetDebugReport()
        {
            // Вызываем расчет всех коэффициентов, чтобы обновить DebugInfo
            float strength = StrengthCoefficient;
            float endurance = EnduranceCoefficient;
            float volume = VolumeTolerance;

            StringBuilder report = new StringBuilder();

            // Заголовок с данными игрока
            report.AppendLine("=== ОТЧЕТ О РАСЧЕТЕ КОЭФФИЦИЕНТОВ ===");
            report.AppendLine($"Данные игрока: Вес={DebugInfo.PlayerWeight}кг, Возраст={DebugInfo.PlayerAge}лет, ");
            report.AppendLine($"Опыт={DebugInfo.PlayerExperience}мес, %жира={DebugInfo.PlayerBodyFat}%");
            report.AppendLine();

            // 1. Коэффициент силы
            report.AppendLine("1. КОЭФФИЦИЕНТ СИЛЫ:");
            report.AppendLine($"   • Весовой модификатор: {DebugInfo.StrWeightMod:F3} ({(DebugInfo.StrWeightMod >= 1 ? "+" : "")}{(DebugInfo.StrWeightMod - 1) * 100:F1}%)");
            report.AppendLine($"   • Возрастной модификатор: {DebugInfo.StrAgeMod:F3} ({(DebugInfo.StrAgeMod >= 1 ? "+" : "")}{(DebugInfo.StrAgeMod - 1) * 100:F1}%)");
            report.AppendLine($"   • Модификатор опыта: {DebugInfo.StrExpMod:F3} ({(DebugInfo.StrExpMod >= 1 ? "+" : "")}{(DebugInfo.StrExpMod - 1) * 100:F1}%)");
            report.AppendLine($"   • Модификатор %жира: {DebugInfo.StrBodyfatMod:F3} ({(DebugInfo.StrBodyfatMod >= 1 ? "+" : "")}{(DebugInfo.StrBodyfatMod - 1) * 100:F1}%)");
            report.AppendLine($"   = ИТОГО: {DebugInfo.StrFinal:F3} (сила {(DebugInfo.StrFinal >= 1 ? "выше" : "ниже")} на {Mathf.Abs(DebugInfo.StrFinal - 1) * 100:F1}% от эталона)");
            report.AppendLine();

            // 2. Коэффициент выносливости
            report.AppendLine("2. КОЭФФИЦИЕНТ ВЫНОСЛИВОСТИ:");
            report.AppendLine($"   • Весовой модификатор: {DebugInfo.EndWeightMod:F3} ({(DebugInfo.EndWeightMod >= 1 ? "+" : "")}{(DebugInfo.EndWeightMod - 1) * 100:F1}%)");
            report.AppendLine($"   • Возрастной модификатор: {DebugInfo.EndAgeMod:F3} ({(DebugInfo.EndAgeMod >= 1 ? "+" : "")}{(DebugInfo.EndAgeMod - 1) * 100:F1}%)");
            report.AppendLine($"   • Модификатор опыта: {DebugInfo.EndExpMod:F3} ({(DebugInfo.EndExpMod >= 1 ? "+" : "")}{(DebugInfo.EndExpMod - 1) * 100:F1}%)");
            report.AppendLine($"   • Модификатор %жира: {DebugInfo.EndBodyfatMod:F3} ({(DebugInfo.EndBodyfatMod >= 1 ? "+" : "")}{(DebugInfo.EndBodyfatMod - 1) * 100:F1}%)");
            report.AppendLine($"   = ИТОГО: {DebugInfo.EndFinal:F3} (выносливость {(DebugInfo.EndFinal >= 1 ? "выше" : "ниже")} на {Mathf.Abs(DebugInfo.EndFinal - 1) * 100:F1}% от эталона)");
            report.AppendLine();

            // 3. Коэффициент переносимости объема
            report.AppendLine("3. КОЭФФИЦИЕНТ ПЕРЕНОСИМОСТИ ОБЪЕМА:");
            report.AppendLine($"   • Возрастной модификатор: {DebugInfo.VolAgeMod:F3} ({(DebugInfo.VolAgeMod >= 1 ? "+" : "")}{(DebugInfo.VolAgeMod - 1) * 100:F1}%)");
            report.AppendLine($"   • Модификатор опыта: {DebugInfo.VolExpMod:F3} ({(DebugInfo.VolExpMod >= 1 ? "+" : "")}{(DebugInfo.VolExpMod - 1) * 100:F1}%)");
            report.AppendLine($"   • Модификатор %жира: {DebugInfo.VolBodyfatMod:F3} ({(DebugInfo.VolBodyfatMod >= 1 ? "+" : "")}{(DebugInfo.VolBodyfatMod - 1) * 100:F1}%)");
            report.AppendLine($"   • Суточный модификатор: {DebugInfo.VolDailyMod:F3} (частота тренировок)");
            report.AppendLine($"   = ИТОГО: {DebugInfo.VolFinal:F3} (объем {(DebugInfo.VolFinal >= 1 ? "выше" : "ниже")} на {Mathf.Abs(DebugInfo.VolFinal - 1) * 100:F1}% от эталона)");
            report.AppendLine();

            // Итоговые рекомендации
            report.AppendLine("=== РЕКОМЕНДАЦИИ ===");
            report.AppendLine($"1. Рабочий вес: используйте {strength:F2} × ваш эталонный вес");
            report.AppendLine($"2. Количество повторений: используйте {endurance:F2} × ваши эталонные повторения");
            report.AppendLine($"3. Количество подходов в неделю: используйте {volume:F2} × ваш эталонный недельный объем");
            report.AppendLine();
            report.AppendLine($"Пример: если ваш эталонный жим = 100кг на 8 раз, 20 подходов/неделю:");
            report.AppendLine($"• Фактический вес: {100 * strength:F1}кг");
            report.AppendLine($"• Фактические повторения: {8 * endurance:F1} раз");
            report.AppendLine($"• Фактические подходы/неделю: {20 * volume:F1}");

            return report.ToString();
        }

        private static float GetStrengthCoefficient()
        {
            DebugInfo.UpdatePlayerData();

            // 1. ВЕСОВОЙ КОЭФФИЦИЕНТ
            float weightMod;
            switch (player.weight)
            {
                case int w when w <= 60:
                    weightMod = 1.12f;
                    break;
                case int w when w <= 70:
                    weightMod = 1.06f;
                    break;
                case int w when w <= 80:
                    weightMod = 1.02f;
                    break;
                case 85:
                    weightMod = 1.00f;
                    break;
                case int w when w <= 95:
                    weightMod = 0.98f;
                    break;
                case int w when w <= 110:
                    weightMod = 0.94f;
                    break;
                default:
                    weightMod = 0.90f;
                    break;
            }
            DebugInfo.StrWeightMod = weightMod;

            // 2. ВОЗРАСТНОЙ КОЭФФИЦИЕНТ
            float ageMod;
            switch (player.age)
            {
                case <= 15:
                    ageMod = 0.80f;
                    break;
                case 16:
                    ageMod = 0.90f;
                    break;
                case 17:
                    ageMod = 1.00f;
                    break;
                case 18:
                    ageMod = 1.05f;
                    break;
                case >= 19 and <= 20:
                    ageMod = 1.08f;
                    break;
                case >= 21 and <= 25:
                    ageMod = 1.12f;
                    break;
                case >= 26 and <= 35:
                    ageMod = 1.10f;
                    break;
                case >= 36 and <= 50:
                    ageMod = 1.00f;
                    break;
                case >= 51 and <= 60:
                    ageMod = 0.90f;
                    break;
                default:
                    ageMod = 0.80f;
                    break;
            }
            DebugInfo.StrAgeMod = ageMod;

            // 3. КОЭФФИЦИЕНТ ОПЫТА
            float expMod;
            switch (player.experience)
            {
                case 0:
                    expMod = 0.60f;
                    break;
                case <= 3:
                    expMod = 0.70f;
                    break;
                case <= 6:
                    expMod = 0.80f;
                    break;
                case <= 9:
                    expMod = 0.90f;
                    break;
                case <= 12:
                    expMod = 0.96f;
                    break;
                case 13:
                    expMod = 0.98f;
                    break;
                case 14:
                    expMod = 1.00f;
                    break;
                case <= 18:
                    expMod = 1.03f;
                    break;
                case <= 24:
                    expMod = 1.06f;
                    break;
                case <= 36:
                    expMod = 1.10f;
                    break;
                case <= 60:
                    expMod = 1.15f;
                    break;
                default:
                    expMod = 1.20f;
                    break;
            }
            DebugInfo.StrExpMod = expMod;

            // 4. КОЭФФИЦИЕНТ ПРОЦЕНТА ЖИРА
            float bodyCompMod;
            switch (player.percentageOfFat)
            {
                case <= 10:
                    bodyCompMod = 1.08f;
                    break;
                case <= 12:
                    bodyCompMod = 1.04f;
                    break;
                case <= 14:
                    bodyCompMod = 1.02f;
                    break;
                case 15:
                    bodyCompMod = 1.00f;
                    break;
                case <= 17:
                    bodyCompMod = 0.98f;
                    break;
                case <= 20:
                    bodyCompMod = 0.95f;
                    break;
                case <= 25:
                    bodyCompMod = 0.90f;
                    break;
                default:
                    bodyCompMod = 0.85f;
                    break;
            }
            DebugInfo.StrBodyfatMod = bodyCompMod;

            // Итоговый коэффициент
            float finalCoefficient = weightMod * ageMod * expMod * bodyCompMod;
            finalCoefficient = Mathf.Clamp(finalCoefficient, 0.2f, 2f);
            DebugInfo.StrFinal = finalCoefficient;
            if (Player.player.treningParametrs.goal == Goal.Recovery) finalCoefficient *= 0.5f;
            return finalCoefficient;
        }

        private static float GetEnduranceCoefficient()
        {
            DebugInfo.UpdatePlayerData();

            // 1. ВЕСОВОЙ КОЭФФИЦИЕНТ
            float weightMod;
            switch (player.weight)
            {
                case int w when w <= 60:
                    weightMod = 1.24f;
                    break;
                case int w when w <= 70:
                    weightMod = 1.12f;
                    break;
                case int w when w <= 80:
                    weightMod = 1.05f;
                    break;
                case 85:
                    weightMod = 1.00f;
                    break;
                case int w when w <= 95:
                    weightMod = 0.95f;
                    break;
                case int w when w <= 110:
                    weightMod = 0.88f;
                    break;
                default:
                    weightMod = 0.82f;
                    break;
            }
            DebugInfo.EndWeightMod = weightMod;

            // 2. ВОЗРАСТНОЙ КОЭФФИЦИЕНТ
            float ageMod;
            switch (player.age)
            {
                case <= 15:
                    ageMod = 0.85f;
                    break;
                case 16:
                    ageMod = 0.92f;
                    break;
                case 17:
                    ageMod = 1.00f;
                    break;
                case 18:
                    ageMod = 1.05f;
                    break;
                case >= 19 and <= 22:
                    ageMod = 1.08f;
                    break;
                case >= 23 and <= 28:
                    ageMod = 1.12f;
                    break;
                case >= 29 and <= 35:
                    ageMod = 1.10f;
                    break;
                case >= 36 and <= 45:
                    ageMod = 1.00f;
                    break;
                case >= 46 and <= 55:
                    ageMod = 0.90f;
                    break;
                default:
                    ageMod = 0.80f;
                    break;
            }
            DebugInfo.EndAgeMod = ageMod;

            // 3. КОЭФФИЦИЕНТ ОПЫТА
            float expMod;
            switch (player.experience)
            {
                case 0:
                    expMod = 0.65f;
                    break;
                case <= 2:
                    expMod = 0.75f;
                    break;
                case <= 5:
                    expMod = 0.85f;
                    break;
                case <= 8:
                    expMod = 0.92f;
                    break;
                case <= 11:
                    expMod = 0.97f;
                    break;
                case 12:
                    expMod = 0.99f;
                    break;
                case 13:
                    expMod = 0.995f;
                    break;
                case 14:
                    expMod = 1.000f;
                    break;
                case <= 18:
                    expMod = 1.03f;
                    break;
                case <= 24:
                    expMod = 1.06f;
                    break;
                case <= 36:
                    expMod = 1.10f;
                    break;
                case <= 48:
                    expMod = 1.12f;
                    break;
                default:
                    expMod = 1.15f;
                    break;
            }
            DebugInfo.EndExpMod = expMod;

            // 4. КОЭФФИЦИЕНТ ПРОЦЕНТА ЖИРА
            float bodyfatMod;
            switch (player.percentageOfFat)
            {
                case <= 10:
                    bodyfatMod = 1.06f;
                    break;
                case <= 12:
                    bodyfatMod = 1.03f;
                    break;
                case <= 14:
                    bodyfatMod = 1.01f;
                    break;
                case 15:
                    bodyfatMod = 1.00f;
                    break;
                case <= 18:
                    bodyfatMod = 0.97f;
                    break;
                case <= 22:
                    bodyfatMod = 0.92f;
                    break;
                case <= 27:
                    bodyfatMod = 0.86f;
                    break;
                default:
                    bodyfatMod = 0.80f;
                    break;
            }
            DebugInfo.EndBodyfatMod = bodyfatMod;

            // Итоговый коэффициент
            float finalCoefficient = weightMod * ageMod * expMod * bodyfatMod;
            finalCoefficient = Mathf.Clamp(finalCoefficient, 0.5f, 1.5f);
            DebugInfo.EndFinal = finalCoefficient;
            if (Player.player.treningParametrs.goal == Goal.Recovery) finalCoefficient *= 0.5f;
            return finalCoefficient;
        }

        private static float GetVolumeTolerance()
        {
            DebugInfo.UpdatePlayerData();

            // 1. ВОЗРАСТНОЙ КОЭФФИЦИЕНТ
            float ageMod;
            switch (player.age)
            {
                case <= 16:
                    ageMod = 1.05f;
                    break;
                case 17:
                    ageMod = 1.00f;
                    break;
                case >= 18 and <= 22:
                    ageMod = 1.10f;
                    break;
                case >= 23 and <= 30:
                    ageMod = 1.05f;
                    break;
                case >= 31 and <= 40:
                    ageMod = 0.95f;
                    break;
                case >= 41 and <= 50:
                    ageMod = 0.85f;
                    break;
                default:
                    ageMod = 0.75f;
                    break;
            }
            DebugInfo.VolAgeMod = ageMod;

            // 2. КОЭФФИЦИЕНТ ОПЫТА
            float expMod;
            switch (player.experience)
            {
                case 0:
                    expMod = 0.60f;
                    break;
                case <= 3:
                    expMod = 0.70f;
                    break;
                case <= 6:
                    expMod = 0.80f;
                    break;
                case <= 12:
                    expMod = 0.90f;
                    break;
                case 13:
                    expMod = 0.95f;
                    break;
                case 14:
                    expMod = 1.00f;
                    break;
                case <= 18:
                    expMod = 1.05f;
                    break;
                case <= 24:
                    expMod = 1.10f;
                    break;
                case <= 36:
                    expMod = 1.15f;
                    break;
                case <= 60:
                    expMod = 1.20f;
                    break;
                default:
                    expMod = 1.25f;
                    break;
            }
            DebugInfo.VolExpMod = expMod;

            // 3. КОЭФФИЦИЕНТ ПРОЦЕНТА ЖИРА
            float bodyfatMod;
            switch (player.percentageOfFat)
            {
                case <= 12:
                    bodyfatMod = 0.95f;
                    break;
                case <= 14:
                    bodyfatMod = 1.02f;
                    break;
                case 15:
                    bodyfatMod = 1.00f;
                    break;
                case <= 18:
                    bodyfatMod = 0.98f;
                    break;
                case <= 22:
                    bodyfatMod = 0.92f;
                    break;
                case <= 27:
                    bodyfatMod = 0.85f;
                    break;
                default:
                    bodyfatMod = 0.75f;
                    break;
            }
            DebugInfo.VolBodyfatMod = bodyfatMod;

            // 4. СУТОЧНЫЙ КОЭФФИЦИЕНТ
            float dailyMod = 1.0f;
            DebugInfo.VolDailyMod = dailyMod;

            // Итоговый коэффициент
            float finalCoefficient = ageMod * expMod * bodyfatMod * dailyMod;
            finalCoefficient = Mathf.Clamp(finalCoefficient, 0.4f, 1.6f);
            DebugInfo.VolFinal = finalCoefficient;
            if (Player.player.treningParametrs.goal == Goal.Recovery) finalCoefficient *= 0.5f ;
            return finalCoefficient;
        }
        private static float GetWeightLossVolumeCoefficient()
        {
            float bodyFatPercentage = player.percentageOfFat;
            float experienceMonths = player.experience;
            // Базовый коэффициент
            float baseCoefficient = 0.7f;

            // Корректировка по % жира
            if (bodyFatPercentage > 30) baseCoefficient *= 0.85f;   // -15%
            else if (bodyFatPercentage > 25) baseCoefficient *= 0.9f; // -10%
            else if (bodyFatPercentage < 18) baseCoefficient *= 1.1f; // +10%

            // Корректировка по опыту
            if (experienceMonths < 3) baseCoefficient *= 0.9f;       // -10%
            else if (experienceMonths > 12) baseCoefficient *= 1.15f; // +15%

            return Mathf.Clamp(baseCoefficient, 0.5f, 0.9f);
        }
    }
    #endregion

    #region Данные для разных целей
    public static List<string> powerliftingExercises = new List<string>
{
    // БАЗОВЫЕ (The Big 3 + основные)
    "Становая тяга",
    "Жим гантелей на наклонной скамье",
    "Приседания со штангой на спине",
    "Жим лежа",
    "Тяга штанги в наклоне (хват на ширине плеч)",
    "Румынская тяга",
    "Армейский жим стоя",

    "Подтягивания широким хватом",
    "Жим ногами в тренажере",
    
    // ВСПОМОГАТЕЛЬНЫЕ (для жима)
    "Жим гантелей сидя",
    "Французский жим лежа (EZ-гриф)",
    "Отжимания на брусьях (акцент на трицепс)",
    "Подъем штанги на бицепс стоя",
    
    // ВСПОМОГАТЕЛЬНЫЕ (для тяги и приседа)
    "Шраги со штангой сзади",
    "Гиперэкстензия с дополнительным весом",
    "Сгибания ног лежа в тренажере",
    "Выпады со штангой",
    "Тяга штанги к подбородку широким хватом",
    "Разведение гантелей в наклоне",
    "Ягодичный мост со штангой",
    "Подъемы на носки стоя в тренажере",
    
    // ДОПОЛНИТЕЛЬНЫЕ (коре/общая сила)
    "Подъемы ног в висе",
    "Планка на предплечьях",
    "Отжимания от пола (классические)"
};

    public static List<string> stretchingExercises = new List<string>
{
    // Грудь
    "Растяжка верхней части груди у стены",
    "Растяжка середины груди в дверном проеме",
    "Растяжка нижней части груди на фитболе",
    "Растяжка внутренней части груди (ладони вместе)",
    
    // Спина
    "Растяжка широчайших в висе на турнике",
    "Растяжка широчайших сидя наклон вперед",
    "Растяжка трапеций наклон головы вбок",
    "Растяжка трапеций с помощью руки",
    "Растяжка ромбовидных обхват себя руками",
    "Растяжка ромбовидных сидя наклонившись вперед",
    "Растяжка поясницы кошка-корова",
    "Растяжка поясницы лежа на спине",
    "Изометрическая растяжка шеи в стороны",
    
    // Плечи
    "Растяжка передних дельт за спиной",
    "Растяжка передних дельт у стены",
    "Растяжка средних дельт через руку",
    "Растяжка средних дельт скрестив руки",
    "Растяжка задних дельт обхват плеча",
    "Растяжка задних дельт с полотенцем",
    
    // Руки
    "Растяжка бицепса у стены",
    "Растяжка бицепса с опорой",
    "Растяжка трицепса за головой",
    "Растяжка трицепса через плечо",
    "Растяжка предплечий ладонью вниз",
    "Растяжка предплечий ладонью вверх",
    
    // Ноги
    "Растяжка квадрицепса стоя",
    "Растяжка квадрицепса лежа на боку",
    "Растяжка бицепса бедра сидя",
    "Растяжка бицепса бедра стоя",
    "Растяжка ягодичных сидя скрестив ноги",
    "Растяжка ягодичных лежа на спине",
    "Растяжка икр у стены",
    "Растяжка икр на ступеньке",
    
    // Кор
    "Растяжка верхнего пресса лежа на животе",
    "Растяжка верхнего пресса мостик",
    "Растяжка нижнего пресса кобра",
    "Растяжка нижнего пресса лежа на спине",
    "Растяжка косых мышц в боковом наклоне",
    "Растяжка косых мышц сидя скручивание"
};
    
    public static List<string> calisthenicsAndCardioExercises = new List<string>
{
    // КАЛИСТЕНИКА
    "Отжимания на брусьях с акцентом на грудь",
    "Отжимания на брусьях (акцент на трицепс)",
    "Отжимания от пола (классические)",
    "Отжимания с широкой постановкой рук",
    "Алмазные отжимания (узкий хват)",
    "Отжимания с ногами на возвышении",
    "Подтягивания широким хватом",
    "Подтягивания (стандартный хват)",
    "Подтягивания обратным хватом",
    "Приседания с собственным весом",
    "Подъемы ног в висе",
    "Боковые скручивания на полу",
    "Планка на предплечьях",
        // Трапеции (для back)
    "Шраги в висе на турнике",
    "Удержание виса на турнике с подъемом плеч",
    
    // Поясница (для back)
    "Супермен (гиперэкстензия на полу)",
    "Мостик на одной ноге",
    
    // Предплечья (для hands)
    "Вис на турнике",
    "Прогулка фермера (с гантелями/бутылками)",
    
    // Передние дельты (для deltoid)
    "Отжимания в стойке у стены (плечи)",
    "Подъемы рук перед собой с импровизированным весом",
    // КАРДИО (только берпи и скакалка, без бега)
    "Берпи (Burpees)",
    "Скакалка"
};
    #endregion
} 
#endregion
