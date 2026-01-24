using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
[System.Serializable]
public class Player
{
    #region Параметры и конструкторы
    public string name;
    public int age, experience;
    public float weight, percentageOfFat;
    public TreningParametrs treningParametrs = new TreningParametrs();
    #endregion

    #region Загрузка - Сохранение

    private static Player _cachedPlayer;

    public static Player player
    {
        get
        {
            if(_cachedPlayer == null)
            {
                _cachedPlayer = new Player();
                    DataManager.SEM.LoadUserMetrics(DataManager.id,
                    (weight,percentageOfFat, age, experience) =>
                        {
                            _cachedPlayer.weight = weight;
                            _cachedPlayer.age = age;
                            _cachedPlayer.percentageOfFat = percentageOfFat;
                            _cachedPlayer.experience = experience;
                        }
                    );
            }
            return _cachedPlayer;
        }
        set
        {
            _cachedPlayer = value;
        }
    }

    #endregion

    #region Данные по упражнениям

    #endregion
}
public class DataPath : MonoBehaviour
{
    static public string Path() => $"{Application.persistentDataPath}";
}