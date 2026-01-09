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
    public static void Test(string s)
    {
        Debug.Log(s);
    }
}
