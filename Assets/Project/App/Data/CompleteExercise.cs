
using System;
using UnityEngine;
using UnityEngine.Analytics;
using System.IO;
public static class CompleteExercises 
{
    static public void CompleteExercise(Exercise exercise,bool failure)
    {
        if (failure)
        {
            if (exercise.specificParameters is StrengthTraining)
            {
                StrengthTraining strengthTraining = (StrengthTraining)exercise.specificParameters;
                StrengthTraining referenceStrengthTraining = (StrengthTraining)strengthTraining.DeepClone(exercise.specificParameters);
                referenceStrengthTraining.SetWorkWeight(Player.player);
                strengthTraining.weightCof *= strengthTraining.workWeight / referenceStrengthTraining.workWeight;
                Debug.Log($"новый weightCof - {strengthTraining.weightCof}");
                ExerciseManager.UpdateExercise(exercise);
            }

            else if (exercise.specificParameters is Calisthenics)
            {
                Calisthenics strengthTraining = (Calisthenics)exercise.specificParameters;
                Calisthenics referenceStrengthTraining = (Calisthenics)strengthTraining.DeepClone(exercise.specificParameters);
                referenceStrengthTraining.SetParametrs(Player.player);
                strengthTraining.repCof *= (float)strengthTraining.replications / (float)referenceStrengthTraining.replications;
                Debug.Log($"новый repCof - {strengthTraining.repCof}");
                ExerciseManager.UpdateExercise(exercise);
            }
        }
    }
}
