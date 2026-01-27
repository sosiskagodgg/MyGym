using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
public class CreateFile : MonoBehaviour
{
    [SerializeField] GameObject description;
    [SerializeField] PlayerVisual playerVisual;
    private void Awake()
    {
        StartCoroutine(Initialization());
    }
    private IEnumerator Initialization()
    {
        description.SetActive(true);
        description.SetActive(false);
        yield return StartCoroutine(LoadPlayer());
        playerVisual.Initialization();
        yield return StartCoroutine(SetCof(ExerciseManager.GetBaseExercises()));
    } 
    static private IEnumerator SetCof(List<Exercise> exercises)
    {
        yield return new WaitForSeconds(1f);
        List<ExerciseData> data = new List<ExerciseData>();
        yield return DataManager.SEM.GetUserExercises(DataManager.id, (ex) => data = ex);
        Debug.Log($"Всего упражнений игрока в базе - {data.Count}");
        for (int i = 0; i < exercises.Count; i++)
        {
            if (data.Any(d => d.name == exercises[i].name))
            {
                if (exercises[i].specificParameters is StrengthTraining)
                    (exercises[i].specificParameters as StrengthTraining).weightCof
                        = data.First(ex => ex.name == exercises[i].name).coefficient;
                else if (exercises[i].specificParameters is Calisthenics)
                    (exercises[i].specificParameters as Calisthenics).repCof
                        = data.First(ex => ex.name == exercises[i].name).coefficient;
            }
        }
        ExerciseManager._cachedExercises = exercises;
    }
    IEnumerator LoadPlayer()
    {
        yield return null;
        yield return DataManager.SEM.LoadUserMetricsCoroutine(DataManager.id,
        (weight, percentageOfFat, age, experience,height) =>
        {
            Player._cachedPlayer.weight = weight;
            Player._cachedPlayer.age = age;
            Player._cachedPlayer.percentageOfFat = percentageOfFat;
            Player._cachedPlayer.experience = experience;
            Player._cachedPlayer.height = height;
        });
    }
}
