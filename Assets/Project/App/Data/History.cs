using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AdaptiveCoefficients
{
    [SerializeField] private float _strengthCoefficient = 1.0f;
    [SerializeField] private float _enduranceCoefficient = 1.0f;
    [SerializeField] private float _volumeTolerance = 1.0f;
    [SerializeField] private DateTime _lastUpdated;

    // История изменений для отладки
    [SerializeField] private List<CoefficientChange> _changeHistory = new();

    public float StrengthCoefficient
    {
        get => _strengthCoefficient;
        set => SetCoefficient(ref _strengthCoefficient, value, "Strength");
    }

    public float EnduranceCoefficient
    {
        get => _enduranceCoefficient;
        set => SetCoefficient(ref _enduranceCoefficient, value, "Endurance");
    }

    public float VolumeTolerance
    {
        get => _volumeTolerance;
        set => SetCoefficient(ref _volumeTolerance, value, "Volume");
    }

    private void SetCoefficient(ref float field, float value, string name)
    {
        float oldValue = field;
        field = Mathf.Clamp(value, 0.5f, 2.0f);
        _lastUpdated = DateTime.Now;

        _changeHistory.Add(new CoefficientChange
        {
            Date = DateTime.Now,
            Name = name,
            OldValue = oldValue,
            NewValue = field,
            Change = field - oldValue
        });

        // Сохраняем историю
        SaveHistory();
    }

    public void AdaptBasedOnPerformance(StrengthTraining exercise,
        float actualWeight, int actualReps, float expectedWeight, int expectedReps)
    {
        float weightDiffPercent = (actualWeight - expectedWeight) / expectedWeight;
        float repsDiff = actualReps - expectedReps;

        // Если сделали больше веса
        if (weightDiffPercent > 0.05f) // +5%
        {
            float adjustment = weightDiffPercent * 0.1f; // 10% от разницы
            StrengthCoefficient += adjustment;
            Debug.Log($"StrengthCoefficient увеличен на {adjustment:F3} (+{weightDiffPercent * 100:F1}% веса)");
        }
        // Если сделали меньше веса
        else if (weightDiffPercent < -0.05f) // -5%
        {
            float adjustment = weightDiffPercent * 0.05f; // 5% от разницы
            StrengthCoefficient += adjustment;
            Debug.Log($"StrengthCoefficient уменьшен на {Mathf.Abs(adjustment):F3} (-{Mathf.Abs(weightDiffPercent) * 100:F1}% веса)");
        }

        // Если сделали больше повторений
        if (repsDiff > 2)
        {
            float adjustment = repsDiff * 0.01f;
            EnduranceCoefficient += adjustment;
            Debug.Log($"EnduranceCoefficient увеличен на {adjustment:F3} (+{repsDiff} повторений)");
        }
    }

    private void SaveHistory()
    {
        // Сохраняем в PlayerPrefs или файл
        string json = JsonUtility.ToJson(this, true);
        PlayerPrefs.SetString("AdaptiveCoefficients", json);
        PlayerPrefs.Save();
    }

    public static AdaptiveCoefficients Load()
    {
        string json = PlayerPrefs.GetString("AdaptiveCoefficients", "{}");
        AdaptiveCoefficients loaded = JsonUtility.FromJson<AdaptiveCoefficients>(json);
        return loaded ?? new AdaptiveCoefficients();
    }

    [System.Serializable]
    public class CoefficientChange
    {
        public DateTime Date;
        public string Name;
        public float OldValue;
        public float NewValue;
        public float Change;
    }
}