using System;
using UnityEngine;
using UnityEngine.UI;

public class LowerCard : MonoBehaviour
{
    public Exercise exercise;
    public void DebugStringBilder() 
    {
        if(exercise == null)  throw new Exception("Упражнение пустое!"); 
        if(exercise.specificParameters == null) throw new Exception("Специальные параметры пустые!");
        if(exercise.specificParameters.debugString == null ) throw new Exception("Дебаг стринг пустой!");
        if (exercise.specificParameters.debugString == null) Debug.Log("Дебаг стринг пустой");
        Debug.Log(exercise.specificParameters.debugString.ToString());
    }
}
