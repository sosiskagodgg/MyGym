using UnityEngine;
[System.Serializable]
public class TreningParametrs
{
    public GymOrStreet gymOrStreet;
    public Goal goal;

    public static string GetDescription(string goal)
    {
        string WeightLoss = "Похудение и снижение веса за счет сжигания жировой массы. Тренировки направлены на дефицит калорий, кардионагрузки и силовые упражнения для сохранения мышечной ткани.";

        string GainingMuscleMass = "Набор мышечной массы и увеличение объема мускулатуры. Программа включает силовые тренировки с прогрессией нагрузок, профицит калорий и достаточное количество белка.";

        string IncreasedStrength = "Увеличение максимальной силы без существенного роста мышечной массы. Акцент на базовые упражнения с малым количеством повторений и большими весами.";

        string IncreasedEndurance = "Повышение выносливости и способности выдерживать длительные нагрузки. Тренировки включают высокообъемные занятия, интервальные нагрузки и кардио.";

        string Recomposition = "Одновременное сжигание жира и наращивание мышечной массы. Балансировка между дефицитом/профицитом калорий, сочетание силовых и кардионагрузок.";

        string Flexibility = "Развитие гибкости, увеличение диапазона движений в суставах. Программа состоит из стретчинга, йоги, мобильности и упражнений на растяжку.";

        string Recovery = "Восстановление после травм, операций или длительных перерывов. Щадящие тренировки для постепенного возвращения к нормальным нагрузкам, реабилитационные упражнения.";

        switch (goal)
        {
            case "WeightLoss": return WeightLoss; 
            case "GainingMuscleMass": return GainingMuscleMass;
            case "IncreasedStrength": return IncreasedStrength;
            case "IncreasedEndurance": return IncreasedEndurance;
            case "Recomposition": return Recomposition;
            case "Flexibility": return Flexibility;
            case "Recovery": return Recovery;
            default: return($"Ошибка цель {goal} не существует");
        }
    }
}



[System.Serializable]
public enum GymOrStreet
{
    Gym,
    Street
}
[System.Serializable]
public enum Goal
{
    WeightLoss,
    GainingMuscleMass,
    IncreasedStrength,
    IncreasedEndurance,
    Recomposition,
    Flexibility,
    Recovery
}
