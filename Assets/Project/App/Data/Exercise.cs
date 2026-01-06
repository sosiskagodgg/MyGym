using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[System.Serializable]
public class Exercise
{
    public string name { get; private set; }
    public SpecificParameters specificParameters;
    public List<Muscle> muscles;

    
    #region auxiliary classes
    public abstract class SpecificParameters 
    {
        public abstract override string ToString();
        public abstract string GetDescription();
        public abstract void SetParametrs(Player player);

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
        private  string _description;
        private  byte _kmPerHour;
        private  byte _kilometers;
        private  short _meters;
        private  byte _angle;
        public Walk( byte kmPerHour,byte kilometers, short meters, string description,byte angle = 0)
        {
            // Валидация
            if (kmPerHour == 0)
                throw new ArgumentException("Скорость не может быть нулевой");
            if (meters >= 1000)
                throw new ArgumentException("Метры должны быть меньше 1000");

            _description = description ?? string.Empty;
            _kmPerHour = kmPerHour;
            _kilometers = kilometers;
            _meters = meters;
            _angle = angle;
        }
        #endregion
        private (byte Hours, byte Minutes) CalculateTime()
        {
            float totalHours = (_kilometers + _meters / 1000f) / _kmPerHour;

            // Защита от переполнения
            if (totalHours > 255f)
                totalHours = 255f;

            byte hours = (byte)MathF.Floor(totalHours);
            byte minutes = (byte)((totalHours - hours) * 60f);

            // Корректировка округления
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
        public override void SetParametrs(Player player) { throw new NotImplementedException(); }
        public override string ToString() => $"{_kilometers}.{_meters:D3}";
        public override string GetDescription()
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
        private byte _workWeight;
        private byte _repetitions;
        private byte _onePm;
        private byte _twelvePm;
        private byte _ApproachNumber;
        private readonly string _description;
        public StrengthTraining(byte workWeight, byte repetitions, byte onePm, byte twelvePm, byte approachNumber, string description)
        {
            _workWeight = workWeight;
            _repetitions = repetitions;
            _onePm = onePm;
            _twelvePm = twelvePm;
            _ApproachNumber = approachNumber;
            _description = description;
        }

        #endregion
        #region Методы для расчета параметров



        #endregion
        #region Публичные методы
        public override void SetParametrs(Player player) { throw new NotImplementedException(); }
        public override string ToString() 
        {
            if (_workWeight > 0 && _repetitions > 0) { return $"{_workWeight}кг на {_repetitions} раз"; }
            else if (_workWeight <= 0 && _repetitions > 0) return $"{_repetitions} раз";
            else return "Ошибка : повторений < 1";
        }
        public override string GetDescription()
        {
            return _description;
        }
        #endregion
    }
    public class Static : SpecificParameters
    {
        #region Переменные и конструкторы 
        private readonly string _description;
        private byte _minutes;
        private byte _seconds;
        public Static(string description, byte minutes, byte seconds)
        {
            _description = description;
            _minutes = minutes;
            _seconds = seconds;
        }

        #endregion
        #region Публичные методы
        public override string ToString()
        {
            string result = "";
            if(_minutes > 0)
            {
                result += _minutes + " " +GetMinuteWord(_minutes);
                if (_seconds > 0) result += " ";
            }
            if (_seconds > 0)
            {
                result += _seconds + " " + GetSecondsWord(_seconds);
            }
            return result;
        }
        public override string GetDescription() => _description;
        public override void SetParametrs(Player player)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
    #endregion
}
