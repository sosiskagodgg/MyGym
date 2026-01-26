using UnityEngine;
using System.Collections.Generic;
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
        long id;
#if UNITY_WEBGL
        string url = Application.absoluteURL;
        int i = url.IndexOf("user_id=");
        if (i > 0)
        {
            i += 8;
            int j = url.IndexOfAny("&#".ToCharArray(), i);
            if (j < 0) j = url.Length;
            displayText.text = url.Substring(i, j - i);
            id = Convert.ToInt64(url.Substring(i, j - i));
            SupabaseSaveManager.id = id;
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
