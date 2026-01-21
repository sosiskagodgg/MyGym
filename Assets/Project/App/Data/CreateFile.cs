using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;
public class CreateFile : MonoBehaviour
{
    [SerializeField] GameObject description;
    private void Awake()
    {
        Test();
        Create();
        description.SetActive(true);
        description.SetActive(false);
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
            var grup = MuscleGroup.muscleGroups;
            var muscles = Muscle.Muscles;
            Muscle.Muscles = muscles;
        }
        if (!File.Exists(Day.path))
        {
            var f = Day.ActiveDay;
        }
    }


    void Test()
    {
        Walk.AutoCreateWalk(new TimeSpan(1, 0, 0), 800);
    }
    public static void DebugLog(string message) {  Debug.Log(message); }
}
