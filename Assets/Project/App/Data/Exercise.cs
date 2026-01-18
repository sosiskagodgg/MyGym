
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
public abstract class SpecificParameters
{
    public abstract override string ToString();
    public abstract string GetDescription(string name);
    public abstract void SetParametrs(Player player,byte ApproachNumber = 0);
    public abstract SpecificParameters DeepClone(SpecificParameters specificParameters);
    public abstract void SetNewParametrs(List<float> newParametrs);
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
public class Walk : SpecificParameters
{
    #region Переменные и конструкторы
    private string _description;
    public byte _kmPerHour;
    public byte _kilometers;
    public short _meters;
    public byte _angle;
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
    public override void SetParametrs(Player player, byte ApproachNumber = 0) {  }
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
    public override void SetNewParametrs(List<float> newParametrs)
    {
        _kilometers = (byte)newParametrs[0];
        _meters = (byte)newParametrs[1];
    }
    public override List<float> GetParametrs()
    {
        return new List<float> { _kilometers, _meters };
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
        if (workWeight > 0 && repetitions > 0) { return $"{workWeight} кг на {repetitions} раз"; }
        else if (workWeight <= 0 && repetitions > 0) return $"{repetitions} раз";
        else return "Ошибка : повторений < 1";
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
public class Stretching : SpecificParameters
{
    public float seconds;

    #region публичные методы
    public override SpecificParameters DeepClone(SpecificParameters specificParameters)
    {
        return new Stretching()
        {
            seconds = (specificParameters as Stretching).seconds
        };
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
public class Calisthenics : SpecificParameters
{
    public int replications;
    public Calisthenics(int replications)
    {
        this.replications = replications;
    }
    #region публичные методы
    public override SpecificParameters DeepClone(SpecificParameters specificParameters)
    {
        return new Calisthenics((specificParameters as Calisthenics).replications);
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
        replications = (int)(replications * ExerciseManager.Coefficient.EnduranceCoefficient*ExerciseManager.Coefficient.StrengthCoefficient);
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
            "Отжимания на брусьях (акцент на трицепс)",
            new List<Muscle>
            {
        new Muscle("Трицепс", 85),
        new Muscle("Низ груди", 10),
        new Muscle("Передние дельты", 5)
            },
            new StrengthTraining(12, 18, 0),
            1 // Высокий приоритет (базовое упражнение)
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
            new StrengthTraining(6, 141, 94),
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

        #endregion

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
    public static List<Exercise> GetExercisesByMuscle(Muscle muscle,int minPercentageOfWork = 50)
    {
        List<Exercise> exercises = ExerciseManager.Exercises;
        exercises = exercises.Where(ex => ex.muscles.Where(m => m.percentageOfWork > minPercentageOfWork).Any(mus => mus.name == muscle.name)).ToList();
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

            return finalCoefficient;
        }
    }
    #endregion
} 
#endregion
