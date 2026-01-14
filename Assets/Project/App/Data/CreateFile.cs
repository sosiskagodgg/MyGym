using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;
public class CreateFile : MonoBehaviour
{
    private void Awake()
    {
        Create();
    }
    void Create()
    {
        if (!File.Exists(ExerciseManager.path))
        {
            var ex = ExerciseManager.Exercises;
            Debug.Log(ex.Count);
            ExerciseManager.Save(ExerciseManager.Exercises);
        }
        if (!File.Exists(Week.path))
        {
            var week = Week.week;
            week.SaveWeek();
        }
        if (!File.Exists(Muscle.path))
        {
            var muscles = Muscle.Muscles;
            Muscle.Muscles = muscles;
        }
    }



    public static void DebugLog(string message) {  Debug.Log(message); }
}
