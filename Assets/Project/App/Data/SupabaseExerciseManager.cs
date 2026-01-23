using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

[System.Serializable]
public class ExerciseData
{
    public string name;
    public float coefficient;

    public ExerciseData(string name, float coefficient)
    {
        this.name = name;
        this.coefficient = coefficient;
    }
}

public class SupabaseExerciseManager : MonoBehaviour
{
    [Header("Supabase настройки")]
    [SerializeField] private string supabaseUrl = "https://cvzfnvqpzgzhyckrpkau.supabase.co";
    [SerializeField] private string supabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImN2emZudnFwemd6aHlja3Jwa2F1Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjkxMjIwNTIsImV4cCI6MjA4NDY5ODA1Mn0.RcFI8IVvfbfqTa31b_EfkT-OsItp_yL_On6KmKKeQwY";

    // ============ СОХРАНИТЬ УПРАЖНЕНИЯ (с заменой) ============
    public void SaveUserExercises(long userId, List<ExerciseData> newExercises)
    {
        StartCoroutine(SaveExercisesWithUpdate(userId, newExercises));
    }

    IEnumerator SaveExercisesWithUpdate(long userId, List<ExerciseData> newExercises)
    {
        Debug.Log($"Начинаю сохранение {newExercises.Count} упражнений для user {userId}");

        // 1. Получаем текущие упражнения пользователя
        List<ExerciseData> currentExercises = new List<ExerciseData>();
        yield return StartCoroutine(GetUserExercises(userId, (exercises) => currentExercises = exercises));

        Debug.Log($"Найдено {currentExercises.Count} текущих упражнений");

        // 2. Разделяем на обновляемые и новые
        var exercisesToUpdate = new List<ExerciseData>();
        var exercisesToInsert = new List<ExerciseData>();

        foreach (var newEx in newExercises)
        {
            var existing = currentExercises.FirstOrDefault(e => e.name == newEx.name);
            if (existing != null)
            {
                // Обновляем коэффициент если изменился
                if (existing.coefficient != newEx.coefficient)
                {
                    exercisesToUpdate.Add(newEx);
                }
            }
            else
            {
                // Новое упражнение
                exercisesToInsert.Add(newEx);
            }
        }

        Debug.Log($"Будет обновлено: {exercisesToUpdate.Count}, добавлено новых: {exercisesToInsert.Count}");

        // 3. Обновляем существующие
        if (exercisesToUpdate.Count > 0)
        {
            yield return StartCoroutine(UpdateExercises(userId, exercisesToUpdate));
        }

        // 4. Добавляем новые
        if (exercisesToInsert.Count > 0)
        {
            yield return StartCoroutine(InsertExercises(userId, exercisesToInsert));
        }

        Debug.Log($"✅ Упражнения сохранены! Всего: {newExercises.Count} (обновлено: {exercisesToUpdate.Count}, добавлено: {exercisesToInsert.Count})");
    }

    // ============ ПОЛУЧИТЬ ТЕКУЩИЕ УПРАЖНЕНИЯ ============
    IEnumerator GetUserExercises(long userId, System.Action<List<ExerciseData>> callback)
    {
        string url = $"{supabaseUrl}/rest/v1/user_exercises?user_id=eq.{userId}&select=exercise_name,coefficient";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");

        yield return request.SendWebRequest();

        List<ExerciseData> exercises = new List<ExerciseData>();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            exercises = ParseExercisesJson(json);
        }

        callback?.Invoke(exercises);
        request.Dispose();
    }

    // ============ ОБНОВИТЬ СУЩЕСТВУЮЩИЕ УПРАЖНЕНИЯ ============
    IEnumerator UpdateExercises(long userId, List<ExerciseData> exercisesToUpdate)
    {
        foreach (var exercise in exercisesToUpdate)
        {
            string url = $"{supabaseUrl}/rest/v1/user_exercises?user_id=eq.{userId}&exercise_name=eq.{UnityWebRequest.EscapeURL(exercise.name)}";
            string json = $"{{\"coefficient\":{exercise.coefficient.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}";

            UnityWebRequest request = new UnityWebRequest(url, "PATCH");
            byte[] body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", supabaseKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
            request.SetRequestHeader("Prefer", "return=minimal");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Ошибка обновления упражнения {exercise.name}: {request.error}");
            }

            request.Dispose();
            yield return new WaitForSeconds(0.05f); // Небольшая пауза
        }

        Debug.Log($"Обновлено {exercisesToUpdate.Count} упражнений");
    }

    // ============ ДОБАВИТЬ НОВЫЕ УПРАЖНЕНИЯ ============
    IEnumerator InsertExercises(long userId, List<ExerciseData> exercisesToInsert)
    {
        // Если много упражнений - отправляем батчами по 10
        int batchSize = 10;

        for (int i = 0; i < exercisesToInsert.Count; i += batchSize)
        {
            int count = Mathf.Min(batchSize, exercisesToInsert.Count - i);
            var batch = exercisesToInsert.GetRange(i, count);

            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("[");

            for (int j = 0; j < batch.Count; j++)
            {
                jsonBuilder.Append("{");
                jsonBuilder.Append($"\"user_id\":{userId},");
                jsonBuilder.Append($"\"exercise_name\":\"{EscapeJson(batch[j].name)}\",");
                jsonBuilder.Append($"\"coefficient\":{batch[j].coefficient.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
                jsonBuilder.Append("}");

                if (j < batch.Count - 1)
                    jsonBuilder.Append(",");
            }

            jsonBuilder.Append("]");

            string url = $"{supabaseUrl}/rest/v1/user_exercises";
            string json = jsonBuilder.ToString();

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] body = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", supabaseKey);
            request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
            request.SetRequestHeader("Prefer", "return=minimal");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Ошибка добавления упражнений: {request.error}");
            }

            request.Dispose();
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log($"Добавлено {exercisesToInsert.Count} новых упражнений");
    }

    // ============ ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ============

    private List<ExerciseData> ParseExercisesJson(string json)
    {
        List<ExerciseData> exercises = new List<ExerciseData>();

        if (json == "[]" || string.IsNullOrEmpty(json))
            return exercises;

        try
        {
            // Простой парсинг JSON массива
            json = json.Trim('[', ']');
            string[] items = json.Split(new[] { "}," }, System.StringSplitOptions.None);

            foreach (string item in items)
            {
                string cleanItem = item.Trim('{', '}');
                string[] pairs = cleanItem.Split(',');

                string name = "";
                float coefficient = 1.0f;

                foreach (string pair in pairs)
                {
                    string[] keyValue = pair.Split(':');
                    if (keyValue.Length < 2) continue;

                    string key = keyValue[0].Trim().Trim('"');
                    string value = keyValue[1].Trim();

                    if (key == "exercise_name")
                    {
                        name = value.Trim('"');
                    }
                    else if (key == "coefficient")
                    {
                        float.TryParse(value, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out coefficient);
                    }
                }

                if (!string.IsNullOrEmpty(name))
                {
                    exercises.Add(new ExerciseData(name, coefficient));
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка парсинга JSON: {e.Message}");
        }

        return exercises;
    }

    private string EscapeJson(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }


}