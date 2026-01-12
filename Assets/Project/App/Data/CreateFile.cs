using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
public class CreateFile : MonoBehaviour
{
    string ap;
    private void Awake()
    {
        ap = Application.persistentDataPath;
        Create();
        Test();
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
    public static void Test()
    {
        StringBuilder stringBilder = new StringBuilder();
        SetOfExercises.GetExercisesByMuscleWeekWA(Muscle.GetMuscleByName("Середина груди"),20, stringBilder);
        Debug.Log(stringBilder.ToString());
    }
}
