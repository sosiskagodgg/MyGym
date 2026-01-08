using UnityEngine;
using System.IO;
using System.Collections.Generic;
public class CreateFile : MonoBehaviour
{
    string ap;
    private void Awake()
    {
        ap = Application.persistentDataPath;
        Create();
        //Test();
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
    }
    void Test()
    {
        try
        {
        Week.week.Days[0].setsOfExercises[0] = new SetOfExercises(ExerciseManager.GetExercisesByName("∆им лежа"), 4);
        }
        catch
        {
            Week.week.Days[0].setsOfExercises.Add(new SetOfExercises(ExerciseManager.GetExercisesByName("∆им лежа"), 4));
        }
        Week.week.SaveWeek();
    }
}
