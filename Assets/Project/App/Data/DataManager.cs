using UnityEngine;
using System.Collections.Generic;
using System;
public class DataManager : MonoBehaviour
{
    [SerializeField] SupabaseExerciseManager supabaseExerciseManager;
    public static SupabaseExerciseManager SEM;
    public static List<ExerciseData> exerciseDatas;
    public static long id;
    private void Awake()
    {
        SEM = supabaseExerciseManager;
    }
    void Start()
    {
        DataManager.id = GetID();
    }
    static long GetID() 
    {
        long id = 0;
#if UNITY_WEBGL
        string url = Application.absoluteURL;
        int i = url.IndexOf("user_id=");
        if (i > 0)
        {
            i += 8;
            int j = url.IndexOfAny("&#".ToCharArray(), i);
            if (j < 0) j = url.Length;
            id = Convert.ToInt64(url.Substring(i, j - i));
        }
#else
        id = 12345;
#endif
#if UNITY_STANDALONE_WIN
        id = 54321;
#endif
        return id;
    }
}
