
using System;
using UnityEngine;
using UnityEngine.Analytics;
using System.IO;
using System.Collections.Generic;
public  class CompleteExercises : MonoBehaviour
{
    [SerializeField] SupabaseExerciseManager supabaseExerciseManager;
    static SupabaseExerciseManager _supabaseExerciseManager;
    private void Awake()
    {
        _supabaseExerciseManager = supabaseExerciseManager;
    }
    static public void CompleteExercise(Exercise exercise,bool failure)
    {
        if (failure)
        {
            float cof = 0;
            float lastCof = 0;
            if (exercise.specificParameters is StrengthTraining)
            {
                lastCof = (exercise.specificParameters as StrengthTraining).weightCof;
                StrengthTraining strengthTraining = (StrengthTraining)exercise.specificParameters;
                StrengthTraining referenceStrengthTraining = (StrengthTraining)strengthTraining.DeepClone(exercise.specificParameters);
                referenceStrengthTraining.SetWorkWeight(Player.player);
                strengthTraining.weightCof *= strengthTraining.workWeight / referenceStrengthTraining.workWeight;
                Debug.Log($"новый weightCof - {strengthTraining.weightCof}");
                ExerciseManager.UpdateExercise(exercise);
                cof = strengthTraining.weightCof;
            }

            else if (exercise.specificParameters is Calisthenics)
            {
                lastCof = (exercise.specificParameters as Calisthenics).repCof;
                Calisthenics strengthTraining = (Calisthenics)exercise.specificParameters;
                Calisthenics referenceStrengthTraining = (Calisthenics)strengthTraining.DeepClone(exercise.specificParameters);
                referenceStrengthTraining.SetParametrs(Player.player);
                strengthTraining.repCof *= (float)strengthTraining.replications / (float)referenceStrengthTraining.replications;
                Debug.Log($"новый repCof - {strengthTraining.repCof}");
                ExerciseManager.UpdateExercise(exercise);
                cof = strengthTraining.repCof;
            }



            if (cof != 0 && cof!= lastCof)
            {
                _supabaseExerciseManager.SaveUserExercises(DataManager.id, new List<ExerciseData>
                {
                    new ExerciseData($"{exercise.name}",cof)
                });
            }
        }
    }
}
