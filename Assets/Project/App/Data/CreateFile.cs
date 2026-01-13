using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
public class CreateFile : MonoBehaviour
{
    private void Awake()
    {
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
        if (!File.Exists(Muscle.path))
        {
            var muscles = Muscle.Muscles;
            Muscle.Muscles = muscles;
        }
    }
    public static void Test()
    {
        StringBuilder stringBuilder = new StringBuilder();
        CreateProgram.DistributeExercises(ExerciseManager.Exercises,Muscle.Muscles,MuscleGroup.muscleGroups,150, stringBuilder);
        
        Debug.Log(stringBuilder.ToString());
    }
    public static void DebugLog(string message) {  Debug.Log(message); }
}
