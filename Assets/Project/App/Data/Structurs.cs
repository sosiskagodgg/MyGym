using System;
using System.Diagnostics;

/// <summary>
/// Представляет физическое расстояние в метрах и километрах.
/// Неизменяемая структура для безопасности потоков и производительности.
/// </summary>
[DebuggerDisplay("{ToString()}")]
[System.Serializable]
public struct Distance : IComparable<Distance>, IEquatable<Distance>, IFormattable
{
    #region Константы

    private const float MetersPerKilometer = 1000.0f;

    public static readonly Distance Zero = new Distance(0);
    public static readonly Distance OneMeter = new Distance(1);
    public static readonly Distance OneKilometer = FromKilometers(1);

    #endregion

    #region Свойства

    public float Meters;

    /// <summary>
    /// Полное количество километров (с дробной частью)
    /// </summary>
    public float TotalKilometers => Meters / 1000f;

    /// <summary>
    /// Целая часть километров
    /// </summary>
    public int WholeKilometers => (int)(Meters / 1000f);

    /// <summary>
    /// Оставшиеся метры после целых километров
    /// </summary>
    public float RemainingMeters => Meters % 1000f;

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает расстояние из метров
    /// </summary>
    /// <param name="meters">Количество метров</param>
    public Distance(float meters)
    {
        if (float.IsNaN(meters))
            throw new ArgumentException("Значение не может быть NaN", nameof(meters));
        if (float.IsInfinity(meters))
            throw new ArgumentException("Значение не может быть бесконечностью", nameof(meters));
        // Опционально: if (meters < 0) throw new ArgumentOutOfRangeException(...)

        Meters = meters;
    }

    /// <summary>
    /// Создает расстояние из километров и метров
    /// </summary>
    /// <param name="kilometers">Целое количество километров</param>
    /// <param name="meters">Количество метров (дробная часть километра)</param>
    /// <remarks>Пример: new Distance(1, 500f) = 1500 метров (1 км 500 м)</remarks>
    public Distance(int kilometers, float meters)
    {
        if (float.IsNaN(meters))
            throw new ArgumentException("Значение не может быть NaN", nameof(meters));
        if (float.IsInfinity(meters))
            throw new ArgumentException("Значение не может быть бесконечностью", nameof(meters));

        // Для int проверяем только особые случаи
        if (kilometers < 0)
            throw new ArgumentOutOfRangeException(nameof(kilometers), "Километры не могут быть отрицательными");

        // Проверяем, что meters в диапазоне [0, 1000)
        if (meters < 0 || meters >= 1000)
            throw new ArgumentOutOfRangeException(nameof(meters), "Метры должны быть в диапазоне от 0 до 999.999...");

        Meters = kilometers * 1000f + meters;
    }

    #endregion

    #region Фабричные методы

    /// <summary>
    /// Создает расстояние из метров
    /// </summary>
    public static Distance FromMeters(float meters) => new Distance(meters);

    /// <summary>
    /// Создает расстояние из километров
    /// </summary>
    public static Distance FromKilometers(float kilometers) => new Distance(kilometers * MetersPerKilometer);

    #endregion

    #region Операторы

    public static Distance operator +(Distance left, Distance right) =>
        new Distance(left.Meters + right.Meters);

    public static Distance operator -(Distance left, Distance right) =>
        new Distance(left.Meters - right.Meters);

    public static Distance operator *(Distance distance, float multiplier) =>
        new Distance(distance.Meters * multiplier);

    public static Distance operator *(float multiplier, Distance distance) =>
        distance * multiplier;

    public static Distance operator /(Distance distance, float divisor)
    {
        if (MathF.Abs(divisor) < float.Epsilon)
            throw new DivideByZeroException("Делитель не может быть равен нулю");
        return new Distance(distance.Meters / divisor);
    }

    public static bool operator ==(Distance left, Distance right) =>
        MathF.Abs(left.Meters - right.Meters) < float.Epsilon;

    public static bool operator !=(Distance left, Distance right) =>
        !(left == right);

    public static bool operator <(Distance left, Distance right) =>
        left.Meters < right.Meters;

    public static bool operator >(Distance left, Distance right) =>
        left.Meters > right.Meters;

    public static bool operator <=(Distance left, Distance right) =>
        left.Meters <= right.Meters;

    public static bool operator >=(Distance left, Distance right) =>
        left.Meters >= right.Meters;

    /// <summary>
    /// Деление расстояния на время дает скорость
    /// </summary>
    public static Speed operator /(Distance distance, TimeSpan time) =>
        Speed.FromDistanceAndTime(distance, time);
    #endregion

    #region Методы сравнения

    public int CompareTo(Distance other) => Meters.CompareTo(other.Meters);

    public bool Equals(Distance other) => this == other;

    public override bool Equals(object obj) => obj is Distance other && Equals(other);

    public override int GetHashCode() => Meters.GetHashCode();

    #endregion

    #region Методы преобразования

    /// <summary>
    /// Возвращает расстояние с абсолютным значением
    /// </summary>
    public Distance Abs() => new Distance(MathF.Abs(Meters));

    /// <summary>
    /// Возвращает расстояние, округленное до ближайшего целого числа метров
    /// </summary>
    public Distance Round() => new Distance(MathF.Round(Meters));

    /// <summary>
    /// Возвращает расстояние, округленное до указанного количества знаков после запятой
    /// </summary>
    public Distance Round(int decimals) => new Distance(MathF.Round(Meters, decimals));

    /// <summary>
    /// Возвращает расстояние, округленное вниз
    /// </summary>
    public Distance Floor() => new Distance(MathF.Floor(Meters));

    /// <summary>
    /// Возвращает расстояние, округленное вверх
    /// </summary>
    public Distance Ceiling() => new Distance(MathF.Ceiling(Meters));

    /// <summary>
    /// Возвращает расстояние, обрезанное до указанного количества знаков после запятой
    /// </summary>
    public Distance Truncate(int decimals)
    {
        float multiplier = MathF.Pow(10, decimals);
        return new Distance(MathF.Truncate(Meters * multiplier) / multiplier);
    }

    #endregion

    #region Методы форматирования

    /// <summary>
    /// Возвращает строковое представление расстояния в метрах
    /// </summary>
    public override string ToString() => ToString(null, null);

    /// <summary>
    /// Возвращает строковое представление расстояния с использованием указанного формата
    /// </summary>
    public string ToString(string format) => ToString(format, null);

    /// <summary>
    /// Возвращает строковое представление расстояния
    /// </summary>
    /// <param name="format">
    /// Формат: 
    /// "m" или null - в метрах (100 м)
    /// "km" - в километрах (0.1 км)
    /// "auto" - автоматически выбирает лучшую единицу (0.1 км при > 1000м, иначе 100 м)
    /// "F" - с указанием единицы (100.00 м)
    /// </param>
    public string ToString(string format, IFormatProvider formatProvider)
    {
        formatProvider ??= System.Globalization.CultureInfo.CurrentCulture;

        switch (format?.ToLower())
        {
            case null:
            case "m":
            case "meters":
                return $"{Meters:F1} м";

            case "km":
            case "kilometers":
                return $"{TotalKilometers:F3} км";

            case "auto":
                return Meters >= 1000 ? $"{TotalKilometers:F2} км" : $"{Meters:F0} м";

            case "short":
                return Meters >= 1000 ? $"{TotalKilometers:F1}км" : $"{Meters:F0}м";

            case "f":
            case "full":
                return $"{(Meters >= 1000 ? TotalKilometers : Meters):F2} {(Meters >= 1000 ? "километров" : "метров")}";

            default:
                throw new FormatException($"Неизвестный формат: {format}");
        }
    }

    /// <summary>
    /// Возвращает краткое строковое представление (100м или 1.5км)
    /// </summary>
    public string ToShortString() => ToString("short", null);

    /// <summary>
    /// Возвращает автоматически форматированное представление
    /// </summary>
    public string ToAutoString() => ToString("auto", null);

    #endregion

    #region Статические методы

    /// <summary>
    /// Возвращает минимальное из двух расстояний
    /// </summary>
    public static Distance Min(Distance a, Distance b) => a < b ? a : b;

    /// <summary>
    /// Возвращает максимальное из двух расстояний
    /// </summary>
    public static Distance Max(Distance a, Distance b) => a > b ? a : b;

    /// <summary>
    /// Пытается преобразовать строку в расстояние
    /// </summary>
    public static bool TryParse(string s, out Distance result)
    {
        result = Zero;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        s = s.Trim().ToLower();

        try
        {
            if (s.EndsWith("km") || s.EndsWith("км"))
            {
                string numberStr = s.Substring(0, s.Length - 2).Trim();
                if (float.TryParse(numberStr, out float km))
                {
                    result = FromKilometers(km);
                    return true;
                }
            }
            else if (s.EndsWith("m") || s.EndsWith("м"))
            {
                string numberStr = s.Substring(0, s.Length - 1).Trim();
                if (float.TryParse(numberStr, out float m))
                {
                    result = FromMeters(m);
                    return true;
                }
            }
            else if (float.TryParse(s, out float value))
            {
                // По умолчанию считаем метрами
                result = FromMeters(value);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    /// <summary>
    /// Преобразует строку в расстояние
    /// </summary>
    public static Distance Parse(string s)
    {
        if (TryParse(s, out Distance result))
            return result;

        throw new FormatException($"Не удалось преобразовать строку '{s}' в Distance");
    }

    #endregion
}



/// <summary>
/// Представляет физическую скорость с поддержкой различных единиц измерения.
/// Интегрирована со структурой Distance для расчетов пути и времени.
/// </summary>
[DebuggerDisplay("{ToString()}")]
[System.Serializable]
public struct Speed : IComparable<Speed>, IEquatable<Speed>, IFormattable
{
    #region Константы

    private const float MetersPerSecondToKmPerHour = 3.6f;
    private const float MetersPerSecondToMilesPerHour = 2.23694f;
    private const float MetersPerSecondToKnots = 1.94384f;
    private const float KmPerHourToMetersPerSecond = 1f / 3.6f;

    public static readonly Speed Zero = new Speed(0);
    public static readonly Speed Walking = FromKilometersPerHour(5f);      // 5 км/ч
    public static readonly Speed Running = FromKilometersPerHour(12f);     // 12 км/ч
    public static readonly Speed Cycling = FromKilometersPerHour(25f);     // 25 км/ч
    public static readonly Speed CityDriving = FromKilometersPerHour(50f); // 50 км/ч
    public static readonly Speed Highway = FromKilometersPerHour(110f);    // 110 км/ч

    #endregion

    #region Свойства

    /// <summary>
    /// Скорость в метрах в секунду (основная единица хранения)
    /// </summary>
    public float MetersPerSecond;

    /// <summary>
    /// Скорость в километрах в час
    /// </summary>
    public float KilometersPerHour => MetersPerSecond * MetersPerSecondToKmPerHour;

    /// <summary>
    /// Скорость в милях в час
    /// </summary>
    public float MilesPerHour => MetersPerSecond * MetersPerSecondToMilesPerHour;

    /// <summary>
    /// Скорость в узлах (морских милях в час)
    /// </summary>
    public float Knots => MetersPerSecond * MetersPerSecondToKnots;

    /// <summary>
    /// Возвращает true, если скорость равна нулю
    /// </summary>
    public bool IsZero => MathF.Abs(MetersPerSecond) < float.Epsilon;

    /// <summary>
    /// Возвращает true, если скорость положительная (движение вперед)
    /// </summary>
    public bool IsForward => MetersPerSecond > 0;

    /// <summary>
    /// Возвращает true, если скорость отрицательная (движение назад)
    /// </summary>
    public bool IsBackward => MetersPerSecond < 0;

    #endregion

    #region Конструкторы

    /// <summary>
    /// Создает скорость с указанием единицы измерения
    /// </summary>
    /// <param name="value">Значение скорости</param>
    /// <param name="unit">Единица измерения: "MPS" - м/с, "KPH" - км/ч, "MPH" - миль/ч</param>
    /// <exception cref="ArgumentException">При неизвестной единице измерения</exception>
    public Speed(float value, string unit = "MPS")
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            throw new ArgumentException("Скорость не может быть NaN или бесконечностью");

        MetersPerSecond = unit.ToUpperInvariant() switch
        {
            "MPS" or "M/S" => value,                           // Метры в секунду
            "KPH" or "KM/H" => value / 3.6f,                   // Километры в час
            "MPH" => value / 2.23694f,                         // Мили в час
            _ => throw new ArgumentException($"Неизвестная единица измерения: {unit}. " +
                  "Допустимые значения: MPS, KPH, MPH")
        };
    }

    // Для удобства можно добавить статические методы-помощники
    public static Speed FromMps(float mps) => new Speed(mps, "MPS");
    public static Speed FromKph(float kph) => new Speed(kph, "KPH");
    public static Speed FromMph(float mph) => new Speed(mph, "MPH");


#endregion

#region Фабричные методы

/// <summary>
/// Создает скорость из метров в секунду
/// </summary>
public static Speed FromMetersPerSecond(float mps) => new Speed(mps);

    /// <summary>
    /// Создает скорость из километров в час
    /// </summary>
    public static Speed FromKilometersPerHour(float kmh) => new Speed(kmh * KmPerHourToMetersPerSecond);

    /// <summary>
    /// Создает скорость из миль в час
    /// </summary>
    public static Speed FromMilesPerHour(float mph) => new Speed(mph / MetersPerSecondToMilesPerHour);

    /// <summary>
    /// Создает скорость из узлов
    /// </summary>
    public static Speed FromKnots(float knots) => new Speed(knots / MetersPerSecondToKnots);

    /// <summary>
    /// Рассчитывает скорость на основе пройденного расстояния и затраченного времени
    /// </summary>
    public static Speed FromDistanceAndTime(Distance distance, TimeSpan time)
    {
        if (time.TotalSeconds <= 0)
            throw new ArgumentException("Время должно быть положительным", nameof(time));

        return new Speed(distance.Meters / (float)time.TotalSeconds);
    }

    /// <summary>
    /// Рассчитывает среднюю скорость на основе нескольких скоростей
    /// </summary>
    public static Speed Average(params Speed[] speeds)
    {
        if (speeds == null || speeds.Length == 0)
            return Zero;

        float sum = 0;
        foreach (var speed in speeds)
            sum += speed.MetersPerSecond;

        return new Speed(sum / speeds.Length);
    }

    #endregion

    #region Операторы

    public static Speed operator +(Speed left, Speed right) =>
        new Speed(left.MetersPerSecond + right.MetersPerSecond);

    public static Speed operator -(Speed left, Speed right) =>
        new Speed(left.MetersPerSecond - right.MetersPerSecond);

    public static Speed operator *(Speed speed, float multiplier) =>
        new Speed(speed.MetersPerSecond * multiplier);

    public static Speed operator *(float multiplier, Speed speed) =>
        speed * multiplier;

    public static Speed operator /(Speed speed, float divisor)
    {
        if (MathF.Abs(divisor) < float.Epsilon)
            throw new DivideByZeroException("Делитель не может быть равен нулю");
        return new Speed(speed.MetersPerSecond / divisor);
    }

    public static bool operator ==(Speed left, Speed right) =>
        MathF.Abs(left.MetersPerSecond - right.MetersPerSecond) < float.Epsilon;

    public static bool operator !=(Speed left, Speed right) =>
        !(left == right);

    public static bool operator <(Speed left, Speed right) =>
        left.MetersPerSecond < right.MetersPerSecond;

    public static bool operator >(Speed left, Speed right) =>
        left.MetersPerSecond > right.MetersPerSecond;

    public static bool operator <=(Speed left, Speed right) =>
        left.MetersPerSecond <= right.MetersPerSecond;

    public static bool operator >=(Speed left, Speed right) =>
        left.MetersPerSecond >= right.MetersPerSecond;

    #endregion

    #region Методы для работы со временем и расстоянием

    /// <summary>
    /// Рассчитывает время, необходимое для преодоления указанного расстояния
    /// </summary>
    public TimeSpan CalculateTime(Distance distance)
    {
        if (IsZero)
            return TimeSpan.MaxValue; // Бесконечное время при нулевой скорости

        float seconds = distance.Meters / MathF.Abs(MetersPerSecond);
        return TimeSpan.FromSeconds(seconds);
    }

    /// <summary>
    /// Рассчитывает расстояние, которое можно преодолеть за указанное время
    /// </summary>
    public Distance CalculateDistance(TimeSpan time)
    {
        float meters = MetersPerSecond * (float)time.TotalSeconds;
        return Distance.FromMeters(meters);
    }

    /// <summary>
    /// Рассчитывает расстояние, которое можно преодолеть за указанное время (в секундах)
    /// </summary>
    public Distance CalculateDistance(float seconds) =>
        Distance.FromMeters(MetersPerSecond * seconds);

    /// <summary>
    /// Рассчитывает расстояние, которое можно преодолеть за указанное время (в минутах)
    /// </summary>
    public Distance CalculateDistanceFromMinutes(float minutes) =>
        CalculateDistance(minutes * 60f);

    /// <summary>
    /// Рассчитывает время, необходимое для преодоления расстояния между двумя точками
    /// </summary>
    public TimeSpan CalculateTimeBetween(Distance from, Distance to)
    {
        Distance delta = Distance.FromMeters(to.Meters - from.Meters);
        return CalculateTime(delta);
    }

    #endregion

    #region Методы преобразования

    /// <summary>
    /// Возвращает скорость с абсолютным значением
    /// </summary>
    public Speed Abs() => new Speed(MathF.Abs(MetersPerSecond));

    /// <summary>
    /// Возвращает обратную скорость (1 / скорость)
    /// </summary>
    public Speed Reciprocal()
    {
        if (IsZero)
            throw new InvalidOperationException("Невозможно получить обратную скорость к нулю");
        return new Speed(1f / MetersPerSecond);
    }

    /// <summary>
    /// Ограничивает скорость указанными границами
    /// </summary>
    public Speed Clamp(Speed min, Speed max)
    {
        float value = MetersPerSecond;
        if (value < min.MetersPerSecond) value = min.MetersPerSecond;
        if (value > max.MetersPerSecond) value = max.MetersPerSecond;
        return new Speed(value);
    }

    /// <summary>
    /// Возвращает скорость с противоположным направлением
    /// </summary>
    public Speed Reverse() => new Speed(-MetersPerSecond);

    /// <summary>
    /// Преобразует скорость в темп (время на километр)
    /// </summary>
    public TimeSpan GetPacePerKilometer()
    {
        if (IsZero)
            return TimeSpan.MaxValue;

        float secondsPerKm = 1000f / MathF.Abs(MetersPerSecond);
        return TimeSpan.FromSeconds(secondsPerKm);
    }

    /// <summary>
    /// Преобразует скорость в темп (время на милю)
    /// </summary>
    public TimeSpan GetPacePerMile()
    {
        if (IsZero)
            return TimeSpan.MaxValue;

        float secondsPerMile = 1609.344f / MathF.Abs(MetersPerSecond);
        return TimeSpan.FromSeconds(secondsPerMile);
    }

    #endregion

    #region Методы сравнения

    public int CompareTo(Speed other) => MetersPerSecond.CompareTo(other.MetersPerSecond);

    public bool Equals(Speed other) => this == other;

    public override bool Equals(object obj) => obj is Speed other && Equals(other);

    public override int GetHashCode() => MetersPerSecond.GetHashCode();

    #endregion

    #region Методы форматирования

    public override string ToString() => ToString("auto", null);

    public string ToString(string format) => ToString(format, null);

    public string ToString(string format, IFormatProvider formatProvider)
    {
        formatProvider ??= System.Globalization.CultureInfo.CurrentCulture;

        switch (format?.ToLower())
        {
            case null:
            case "auto":
                // Автоматически выбираем наиболее подходящую единицу
                float absKmh = MathF.Abs(KilometersPerHour);
                return absKmh < 1 ? $"{MetersPerSecond:F1} м/с" : $"{KilometersPerHour:F1} км/ч";

            case "mps":
            case "m/s":
                return $"{MetersPerSecond:F1} м/с";

            case "kmh":
            case "km/h":
                return $"{KilometersPerHour:F1} км/ч";

            case "mph":
                return $"{MilesPerHour:F1} миль/ч";

            case "knots":
                return $"{Knots:F1} узлов";

            case "pace":
                TimeSpan pace = GetPacePerKilometer();
                return pace == TimeSpan.MaxValue ? "∞" : $"{pace:mm\\:ss} мин/км";

            case "full":
                return $"{(KilometersPerHour >= 1 ? KilometersPerHour : MetersPerSecond):F2} " +
                       $"{(KilometersPerHour >= 1 ? "км/ч" : "м/с")}";

            case "short":
                return KilometersPerHour >= 1 ?
                    $"{KilometersPerHour:F0}км/ч" :
                    $"{MetersPerSecond:F0}м/с";

            default:
                throw new FormatException($"Неизвестный формат: {format}");
        }
    }

    /// <summary>
    /// Возвращает строку с темпом (время на километр)
    /// </summary>
    public string ToPaceString()
    {
        TimeSpan pace = GetPacePerKilometer();
        return pace == TimeSpan.MaxValue ? "∞" : $"{pace:mm\\:ss}";
    }

    /// <summary>
    /// Возвращает строку скорости в зависимости от контекста (бег, авто и т.д.)
    /// </summary>
    public string ToContextString()
    {
        float kmh = KilometersPerHour;
        if (kmh < 10) return ToPaceString(); // Для бега/ходьбы показываем темп
        return $"{kmh:F0} км/ч"; // Для транспорта показываем скорость
    }

    #endregion

    #region Статические методы

    public static Speed Min(Speed a, Speed b) => a < b ? a : b;

    public static Speed Max(Speed a, Speed b) => a > b ? a : b;

    /// <summary>
    /// Линейная интерполяция между двумя скоростями
    /// </summary>
    public static Speed Lerp(Speed a, Speed b, float t)
    {
        t = Math.Clamp(t, 0, 1);
        return new Speed(a.MetersPerSecond + (b.MetersPerSecond - a.MetersPerSecond) * t);
    }

    /// <summary>
    /// Рассчитывает относительную скорость (разность скоростей)
    /// </summary>
    public static Speed RelativeSpeed(Speed observer, Speed target) => target - observer;

    #endregion

    #region Операторы для работы с Distance и TimeSpan

    /// <summary>
    /// Умножение скорости на время дает расстояние
    /// </summary>
    public static Distance operator *(Speed speed, TimeSpan time) =>
        speed.CalculateDistance(time);

    /// <summary>
    /// Умножение времени на скорость дает расстояние
    /// </summary>
    public static Distance operator *(TimeSpan time, Speed speed) =>
        speed.CalculateDistance(time);


    /// <summary>
    /// Деление расстояния на скорость дает время
    /// </summary>
    public static TimeSpan operator /(Distance distance, Speed speed) =>
        speed.CalculateTime(distance);


    #endregion
}

/// <summary>
/// Методы расширения для работы со Speed, Distance и TimeSpan
/// </summary>
public static class SpeedExtensions
{
    /// <summary>
    /// Рассчитывает среднюю скорость для преодоления расстояния
    /// </summary>
    public static Speed CalculateSpeed(this Distance distance, TimeSpan time) =>
        Speed.FromDistanceAndTime(distance, time);


    /// <summary>
    /// Рассчитывает время для преодоления расстояния с заданной скоростью
    /// </summary>
    public static TimeSpan CalculateTime(this Distance distance, Speed speed) =>
        speed.CalculateTime(distance);

    /// <summary>
    /// Рассчитывает расстояние, которое можно преодолеть за время с заданной скоростью
    /// </summary>
    public static Distance CalculateDistance(this Speed speed, TimeSpan time) =>
        speed.CalculateDistance(time);

    /// <summary>
    /// Рассчитывает темп (время на километр) для скорости
    /// </summary>
    public static TimeSpan GetPace(this Speed speed) =>
        speed.GetPacePerKilometer();
}
