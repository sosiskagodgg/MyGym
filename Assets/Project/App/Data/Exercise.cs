
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Text;

#region Основной класс
[System.Serializable]
public class Exercise
{
    #region Параметры и конструктор
    public string name;
    [SerializeReference]
    public SpecificParameters specificParameters;
    public List<Muscle> muscles;

    public Exercise(string name, List<Muscle> muscles, SpecificParameters specificParameters)
    {
        this.name = name;
        this.specificParameters = specificParameters;
        this.muscles = muscles;
    }
    public Exercise() { }
    #endregion

    #region Работа с ID
    public short id;


    #endregion

} 
#endregion

#region Доп классы
public abstract class SpecificParameters
{
    public abstract override string ToString();
    public abstract string GetDescription(string name);
    public abstract void SetParametrs(Player player,byte ApproachNumber = 0);
    public abstract SpecificParameters DeepClone(SpecificParameters specificParameters);
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
public class Walk : SpecificParameters
{
    #region Переменные и конструкторы
    private string _description;
    private byte _kmPerHour;
    private byte _kilometers;
    private short _meters;
    private byte _angle;
    public Walk(byte kmPerHour, byte kilometers, short meters,  byte angle = 0)
    {
            if (kmPerHour == 0)
            throw new ArgumentException("Скорость не может быть нулевой");
        if (meters >= 1000)
            throw new ArgumentException("Метры должны быть меньше 1000");

        _kmPerHour = kmPerHour;
        _kilometers = kilometers;
        _meters = meters;
        _angle = angle;
    }
    public Walk() { }
    #endregion
    private (byte Hours, byte Minutes) CalculateTime()
    {
        float totalHours = (_kilometers + _meters / 1000f) / _kmPerHour;

            if (totalHours > 255f)
            totalHours = 255f;

        byte hours = (byte)MathF.Floor(totalHours);
        byte minutes = (byte)((totalHours - hours) * 60f);

            if (minutes >= 60)
        {
            hours++;
            minutes = 0;
        }

        return (hours, minutes);
    }// Основной метод расчета времени
    #region Методы для вывода информации
    private string FormatTimeString()
    {
        var (hours, minutes) = CalculateTime();

        if (hours == 0)
            return $"{minutes} {GetMinuteWord(minutes)}";

        return $"{hours} {GetHourWord(hours)}, {minutes} {GetMinuteWord(minutes)}";
    }
    private string FormatDistanceString()
    {
        if (_kilometers == 0 && _meters == 0)
            return "0 метров";

        if (_kilometers == 0)
            return $"{_meters} {GetMeterWord(_meters)}";

        if (_meters == 0)
            return $"{_kilometers} {GetKilometerWord(_kilometers)}";

        return $"{_kilometers} {GetKilometerWord(_kilometers)} " +
               $"и {_meters} {GetMeterWord(_meters)}";
    }
    private string GetKilometerWord(int km) =>
        GetNounForm(km, "километр", "километра", "километров");
    private string GetMeterWord(int meters) =>
        GetNounForm(meters, "метр", "метра", "метров");
    private string GetNounForm(int number, string form1, string form2, string form5)
    {
        int n = Math.Abs(number % 100);
        if (n >= 11 && n <= 14) return form5;

        return (number % 10) switch
        {
            1 => form1,
            2 or 3 or 4 => form2,
            _ => form5
        };
    }
    private string FormatAngleString() =>
        _angle == 0 ? string.Empty : $"Угол подъема: {_angle}°";
    #endregion
    #region Методы для расчета параметров



    #endregion
    #region Основные публичные методы
    public override SpecificParameters DeepClone(SpecificParameters specificParameters)
    {
        if (specificParameters is Walk walkToClone)
        {
            return new Walk(
                walkToClone._kmPerHour,
                walkToClone._kilometers,
                walkToClone._meters,
                walkToClone._angle
            );
        }

        throw new ArgumentException("Параметр должен быть типа Walk", nameof(specificParameters));
    }
    public override void SetParametrs(Player player, byte ApproachNumber = 0) { throw new NotImplementedException(); }
    public override string ToString() => $"{_kilometers}.{_meters:D3}";
    public override string GetDescription(string name)
    {
        var parts = new List<string>
        {
            _description,
            $"Скорость: {_kmPerHour} км/ч",
            $"Время: {FormatTimeString()}",
            $"Дистанция: {FormatDistanceString()}"
        };

        var angleStr = FormatAngleString();
        if (!string.IsNullOrEmpty(angleStr))
            parts.Add(angleStr);

        return string.Join(Environment.NewLine, parts.Where(p => !string.IsNullOrEmpty(p)));
    }
    #endregion
}
public class StrengthTraining : SpecificParameters
{
    #region Переменные и конструкторы
    public byte workWeight;
    public byte repetitions;
    public byte onePm;
    public short twelvePm;
    public byte ApproachNumber;
    public string description;
    public StrengthTraining(byte repetitions, byte onePm, short twelvePm)
    {
        this.repetitions = repetitions;
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
            workWeight = (byte)GetWorkWeightByRepetitions(onePm, twelvePm,repetitions);
            debugString.AppendLine($"Эталонный {repetitions} пм - {workWeight}");
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
                strengthToClone.repetitions,
                strengthToClone.onePm,
                strengthToClone.twelvePm)
            {
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
        SetWorkWeight(player);
    }
    public override string ToString()
    {
        if (workWeight > 0 && repetitions > 0) { return $"{workWeight}кг на {repetitions} раз"; }
        else if (workWeight <= 0 && repetitions > 0) return $"{repetitions} раз";
        else return "Ошибка : повторений < 1";
    }
    public override string GetDescription(string name)
    {
        return Description.GetDescriptionByName(name);
    }
    #endregion
}
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
        throw new NotImplementedException();
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
    public static Exercise GetExercisesByName(string name)
    {
        var exercise = Exercises.FirstOrDefault(e => e.name == name);
        return exercise == null? throw new KeyNotFoundException($"Упражнение '{name}' не найдено"): exercise; 
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

    #region Нестатичные поля
    [SerializeField] private List<Exercise> exercises;

    #endregion

    #endregion

    #region Создание упражнений
    private static List<Exercise> GetBaseExercises()
    {

        List<Exercise> exercises = new List<Exercise>();
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
            new StrengthTraining(8, 90, 65)
        ));
        exercises.Add(new Exercise(
            "Подъемы гантелей лежа на наклонной скамье",
            new List<Muscle>
            {
                new Muscle("Верх груди", 85),
                new Muscle("Передние дельты", 15)
            },
            new StrengthTraining(12, 35, 25)
        ));

        // Середина груди
        exercises.Add(new Exercise(
            "Сведения в кроссовере через верхние блоки",
            new List<Muscle>
            {
                new Muscle("Середина груди", 90),
                new Muscle("Передние дельты", 10)
            },
            new StrengthTraining(12, 40, 30)
        ));
        exercises.Add(new Exercise
            ("Жим лежа",
            new List<Muscle>
            {
                new Muscle("Середина груди", 65),
                new Muscle("Трицепс", 25),
                new Muscle("Передние дельты", 10)
            },
            new StrengthTraining(10, 117, 82
            )));
        exercises.Add(new Exercise(
            "Пуловер с гантелью лежа поперек скамьи",
            new List<Muscle>
            {
                new Muscle("Середина груди", 70),
                new Muscle("Широчайшие", 25),
        new Muscle("Трицепс", 5)
            },
            new StrengthTraining(12, 45, 35)
        ));

        // Низ груди
        exercises.Add(new Exercise(
            "Отжимания на брусьях с акцентом на грудь",
            new List<Muscle>
            {
                new Muscle("Низ груди", 60),
                new Muscle("Трицепс", 30),
                new Muscle("Передние дельты", 10)
            },
            new StrengthTraining(8, 25, 0)
        ));
        #endregion
        #region Спина
        // Широчайшие мышцы (4 упражнения)
        exercises.Add(new Exercise(
            "Подтягивания широким хватом",
            new List<Muscle>
            {
        new Muscle("Широчайшие", 80),
        new Muscle("Бицепс", 15),
        new Muscle("Предплечья", 5)
            },
            new StrengthTraining(8, 25, -10)
        ));

        exercises.Add(new Exercise(
            "Тяга верхнего блока широким хватом к груди",
            new List<Muscle>
            {
        new Muscle("Широчайшие", 75),
        new Muscle("Ромбовидные", 15),
        new Muscle("Бицепс", 10)
            },
            new StrengthTraining(12, 143, 100)
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
            new StrengthTraining(8, 106, 74)
        ));

        exercises.Add(new Exercise(
            "Тяга горизонтального блока узким хватом",
            new List<Muscle>
            {
        new Muscle("Широчайшие", 70),
        new Muscle("Ромбовидные", 20),
        new Muscle("Бицепс", 10)
            },
            new StrengthTraining(10, 94, 66)
        ));

        // Трапециевидные мышцы (3 упражнения)
        exercises.Add(new Exercise(
            "Шраги со штангой сзади",
            new List<Muscle>
            {
        new Muscle("Трапеции", 100),
            },
            new StrengthTraining(15, 42, 29)
        ));

        exercises.Add(new Exercise(
            "Шраги с гантелями",
            new List<Muscle>
            {
        new Muscle("Трапеции", 100),
            },
            new StrengthTraining(12, 84, 59)
        ));

        exercises.Add(new Exercise(
            "Тяга штанги к подбородку широким хватом",
            new List<Muscle>
            {
        new Muscle("Трапеции", 80),
        new Muscle("Передние дельты", 20),
            },
            new StrengthTraining(10, 47, 33)
        ));

        // Ромбовидные мышцы (2 упражнения)
        exercises.Add(new Exercise(
            "Тяга Т-грифа с упором в грудь",
            new List<Muscle>
            {
        new Muscle("Ромбовидные", 65),
        new Muscle("Широчайшие", 25),
        new Muscle("Задние дельты", 10)
            },
            new StrengthTraining(10, 106, 74)
        ));

        exercises.Add(new Exercise(
            "Разведение гантелей в наклоне",
            new List<Muscle>
            {
        new Muscle("Задние дельты", 70),
        new Muscle("Ромбовидные", 20),
        new Muscle("Трапеции", 10)
            },
            new StrengthTraining(12, 24, 16)
        ));

        // Поясница (2 упражнения)
        exercises.Add(new Exercise(
            "Становая тяга",
            new List<Muscle>
            {
        new Muscle("Поясница", 40),
        new Muscle("Широчайшие", 25),
        new Muscle("Ягодичные", 20),
        new Muscle("Бицепс бедра", 15)
            },
            new StrengthTraining(5, 141, 99)
        ));

        exercises.Add(new Exercise(
            "Гиперэкстензия с дополнительным весом",
            new List<Muscle>
            {
        new Muscle("Поясница", 90),
        new Muscle("Ягодичные", 10)
            },
            new StrengthTraining(12, 59, 41)
        ));
        #endregion
        #region Плечи
        // Передняя дельта (3 упражнения)
        exercises.Add(new Exercise(
            "Армейский жим стоя",
            new List<Muscle>
            {
        new Muscle("Передние дельты", 60),
        new Muscle("Средние дельты", 25),
        new Muscle("Трицепс", 15)
            },
            new StrengthTraining(8, 56, 39)
        ));

        exercises.Add(new Exercise(
            "Жим гантелей сидя",
            new List<Muscle>
            {
        new Muscle("Передние дельты", 70),
        new Muscle("Средние дельты", 20),
        new Muscle("Трицепс", 10)
            },
            new StrengthTraining(10, 59, 41)
        ));

        exercises.Add(new Exercise(
            "Подъемы гантелей перед собой",
            new List<Muscle>
            {
        new Muscle("Передние дельты", 90),
        new Muscle("Средние дельты", 10)
            },
            new StrengthTraining(12, 31, 22)
        ));

        // Средняя дельта (3 упражнения)
        exercises.Add(new Exercise(
            "Махи гантелями в стороны стоя",
            new List<Muscle>
            {
        new Muscle("Средние дельты", 95),
        new Muscle("Передние дельты", 5)
            },
            new StrengthTraining(15, 37, 26)
        ));

        exercises.Add(new Exercise(
            "Тяга штанги к подбородку широким хватом",
            new List<Muscle>
            {
        new Muscle("Средние дельты", 70),
        new Muscle("Трапеции", 20),
        new Muscle("Передние дельты", 10)
            },
            new StrengthTraining(10, 47, 33)
        ));

        exercises.Add(new Exercise(
            "Махи в стороны в тренажере",
            new List<Muscle>
            {
        new Muscle("Средние дельты", 90),
        new Muscle("Передние дельты", 10)
            },
            new StrengthTraining(12, 40, 28)
        ));

        // Задняя дельта (2 упражнения)
        exercises.Add(new Exercise(
            "Махи гантелями в наклоне",
            new List<Muscle>
            {
        new Muscle("Задние дельты", 85),
        new Muscle("Средние дельты", 15)
            },
            new StrengthTraining(12, 32, 22)
        ));

        exercises.Add(new Exercise(
            "Разведения в тренажере Peck-Deck",
            new List<Muscle>
            {
        new Muscle("Задние дельты", 80),
        new Muscle("Средние дельты", 20)
            },
            new StrengthTraining(12, 35, 25)
        ));
        #endregion
        #region Руки
        // Бицепс (3 упражнения)
        exercises.Add(new Exercise(
            "Подъем штанги на бицепс стоя",
            new List<Muscle>
            {
        new Muscle("Бицепс", 95),
        new Muscle("Предплечья", 5)
            },
            new StrengthTraining(8, 76, 47)
        ));

        exercises.Add(new Exercise(
            "Подъем гантелей на бицепс сидя",
            new List<Muscle>
            {
        new Muscle("Бицепс", 90),
        new Muscle("Предплечья", 10)
            },
            new StrengthTraining(10, 71, 44)
        ));

        exercises.Add(new Exercise(
            "Молотковые сгибания с гантелями",
            new List<Muscle>
            {
        new Muscle("Бицепс", 70),
        new Muscle("Предплечья", 30)
            },
            new StrengthTraining(10, 65, 40)
        ));

        // Трицепс (3 упражнения)
        exercises.Add(new Exercise(
            "Французский жим лежа (EZ-гриф)",
            new List<Muscle>
            {
        new Muscle("Трицепс", 95),
        new Muscle("Передние дельты", 5)
            },
            new StrengthTraining(10, 55, 38)
        ));

        exercises.Add(new Exercise(
            "Отжимания на брусьях (акцент на трицепс)",
            new List<Muscle>
            {
        new Muscle("Трицепс", 85),
        new Muscle("Низ груди", 10),
        new Muscle("Передние дельты", 5)
            },
            new StrengthTraining(12, 18, 0)
        ));

        exercises.Add(new Exercise(
            "Разгибания на трицепс в верхнем блоке с канатом",
            new List<Muscle>
            {
        new Muscle("Трицепс", 100)
            },
            new StrengthTraining(12, 40, 28)
        ));

        // Предплечья (2 упражнения)
        exercises.Add(new Exercise(
            "Сгибание запястий со штангой сидя",
            new List<Muscle>
            {
        new Muscle("Предплечья", 100)
            },
            new StrengthTraining(15, 25, 18)
        ));

        exercises.Add(new Exercise(
            "Разгибание запястий со штангой сидя",
            new List<Muscle>
            {
        new Muscle("Предплечья", 100)
            },
            new StrengthTraining(15, 20, 14)
        ));
        #endregion
        #region Ноги
        // Квадрицепс (3 упражнения)
        exercises.Add(new Exercise(
            "Приседания со штангой на спине",
            new List<Muscle>
            {
        new Muscle("Квадрицепс", 60),
        new Muscle("Ягодичные", 25),
        new Muscle("Бицепс бедра", 10),
        new Muscle("Поясница", 5)
            },
            new StrengthTraining(6, 141, 94)
        ));

        exercises.Add(new Exercise(
            "Жим ногами в тренажере",
            new List<Muscle>
            {
        new Muscle("Квадрицепс", 80),
        new Muscle("Ягодичные", 15),
        new Muscle("Бицепс бедра", 5)
            },
            new StrengthTraining(10, 212, 141)
        ));

        exercises.Add(new Exercise(
            "Разгибания ног в тренажере",
            new List<Muscle>
            {
        new Muscle("Квадрицепс", 95),
        new Muscle("Ягодичные", 5)
            },
            new StrengthTraining(12, 94, 66)
        ));

        // Ягодичные (3 упражнения)
        exercises.Add(new Exercise(
            "Румынская тяга",
            new List<Muscle>
            {
        new Muscle("Ягодичные", 60),
        new Muscle("Бицепс бедра", 30),
        new Muscle("Поясница", 10)
            },
            new StrengthTraining(8, 118, 82)
        ));

        exercises.Add(new Exercise(
            "Выпады со штангой",
            new List<Muscle>
            {
        new Muscle("Ягодичные", 70),
        new Muscle("Квадрицепс", 20),
        new Muscle("Бицепс бедра", 10)
            },
            new StrengthTraining(10, 88, 62)
        ));

        exercises.Add(new Exercise(
            "Ягодичный мост со штангой",
            new List<Muscle>
            {
        new Muscle("Ягодичные", 85),
        new Muscle("Бицепс бедра", 10),
        new Muscle("Поясница", 5)
            },
            new StrengthTraining(10, 176, 124)
        ));

        // Бицепс бедра (2 упражнения)
        exercises.Add(new Exercise(
            "Сгибания ног лежа в тренажере",
            new List<Muscle>
            {
        new Muscle("Бицепс бедра", 95),
        new Muscle("Ягодичные", 5)
            },
            new StrengthTraining(12, 59, 41)
        ));

        exercises.Add(new Exercise(
            "Становая тяга на прямых ногах",
            new List<Muscle>
            {
        new Muscle("Бицепс бедра", 70),
        new Muscle("Ягодичные", 20),
        new Muscle("Поясница", 10)
            },
            new StrengthTraining(8, 124, 87)
        ));

        // Икры (2 упражнения)
        exercises.Add(new Exercise(
            "Подъемы на носки стоя в тренажере",
            new List<Muscle>
            {
        new Muscle("Икры", 100)
            },
            new StrengthTraining(15, 176, 124)
        ));

        exercises.Add(new Exercise(
            "Подъемы на носки сидя",
            new List<Muscle>
            {
        new Muscle("Икры", 100)
            },
            new StrengthTraining(15, 141, 99)
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
    new StrengthTraining(15, 35, 25)
));

        // Низ пресса
        exercises.Add(new Exercise(
            "Подъемы ног в висе",
            new List<Muscle>
            {
        new Muscle("Низ пресса", 85),
        new Muscle("Верх пресса", 10),
        new Muscle("Косые мышцы", 5)
            },
            new Static(1,0)
        ));

        // Косые мышцы
        exercises.Add(new Exercise(
            "Боковые скручивания на полу",
            new List<Muscle>
            {
        new Muscle("Косые мышцы", 90),
        new Muscle("Верх пресса", 10)
            },
            new Static(1,0)
        ));
        #endregion

        return exercises;
    }
    #endregion

    #region Взаимодействие с упражнением

    public static Exercise DeepClone(Exercise exercise)
    {
        Exercise cloneExercise = new Exercise()
        {
            name = exercise.name,
            specificParameters = exercise.specificParameters.DeepClone(exercise.specificParameters),
            muscles = exercise.muscles?.Select(m => Muscle.DeepClone(m)).ToList(),
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
} 
#endregion
