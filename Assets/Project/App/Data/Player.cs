using System;
using System.Collections;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
[System.Serializable]
public class Player 
{
    #region Параметры и конструкторы
    public string name;
    public int age, experience,height;
    public float weight, percentageOfFat;
    public TreningParametrs treningParametrs = new TreningParametrs();
    #endregion

    #region Загрузка - Сохранение

    public static Player _cachedPlayer = new();

    public static Player player
    {
        get
        {
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