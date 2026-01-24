using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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
    public void SaveUserMetrics(long userId, float weightKg, float bodyFatPercent, int age, int experienceMonths)
    {
        StartCoroutine(SaveOrUpdateUserMetrics(userId, weightKg, bodyFatPercent, age, experienceMonths));
    }
    public void LoadUserMetrics(long userId, System.Action<float, float, int, int> callback)
    {
        StartCoroutine(LoadUserMetricsCoroutine(userId, callback));
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
    IEnumerator SaveOrUpdateUserMetrics(long userId, float weightKg, float bodyFatPercent, int age, int experienceMonths)
    {
        Debug.Log($"Сохранение метрик для user {userId}: вес={weightKg}кг, жир={bodyFatPercent}%, возраст={age}, опыт={experienceMonths}мес");

        // 1. Проверяем, есть ли уже запись у пользователя
        bool userExists = false;
        yield return StartCoroutine(CheckUserExists(userId, (exists) => userExists = exists));

        string method;
        string url;
        string json;

        // Форматируем дату в формате "YYYY-MM-DD"
        string currentDate = System.DateTime.Now.ToString("yyyy-MM-dd");

        if (userExists)
        {
            // Обновляем существующую запись
            method = "PATCH";
            url = $"{supabaseUrl}/rest/v1/user_metrics?user_id=eq.{userId}";
            json = $"{{\"weight_kg\":{weightKg.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                   $"\"body_fat_percent\":{bodyFatPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                   $"\"age\":{age}," +
                   $"\"experience_months\":{experienceMonths}," +
                   $"\"measurement_date\":\"{currentDate}\"}}";
        }
        else
        {
            // Создаем новую запись
            method = "POST";
            url = $"{supabaseUrl}/rest/v1/user_metrics";
            json = $"{{\"user_id\":{userId}," +
                   $"\"weight_kg\":{weightKg.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                   $"\"body_fat_percent\":{bodyFatPercent.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                   $"\"age\":{age}," +
                   $"\"experience_months\":{experienceMonths}," +
                   $"\"measurement_date\":\"{currentDate}\"}}";
        }

        // Отправляем запрос
        UnityWebRequest request = new UnityWebRequest(url, method);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
        request.SetRequestHeader("Prefer", "return=minimal");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            if (userExists)
                Debug.Log($"✅ Метрики обновлены для user {userId}");
            else
                Debug.Log($"✅ Метрики созданы для нового user {userId}");
        }
        else
        {
            Debug.LogError($"❌ Ошибка сохранения метрик: {request.error}");
            if (request.downloadHandler != null)
                Debug.LogError($"Ответ: {request.downloadHandler.text}");
        }

        request.Dispose();
    }

    // ============ ПРОВЕРИТЬ СУЩЕСТВОВАНИЕ ПОЛЬЗОВАТЕЛЯ ============
    IEnumerator CheckUserExists(long userId, System.Action<bool> callback)
    {
        string url = $"{supabaseUrl}/rest/v1/user_metrics?user_id=eq.{userId}&select=user_id";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");

        yield return request.SendWebRequest();

        bool exists = false;

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            // Если не пустой массив [], значит пользователь существует
            exists = (response != "[]" && response.Length > 2);
        }

        callback?.Invoke(exists);
        request.Dispose();
    }

    // ============ СОХРАНИТЬ ВЕСЬ ДЕНЬ С ПОДХОДАМИ ============
    public void SaveTrainingDayWithSets(long userId, string dayOfWeek, List<TrainingSet> sets)
    {
        StartCoroutine(SaveTrainingDayWithSetsCoroutine(userId, dayOfWeek, sets));
    }

    IEnumerator SaveTrainingDayWithSetsCoroutine(long userId, string dayOfWeek, List<TrainingSet> sets)
    {
        Debug.Log($"Сохранение {sets.Count} подходов для дня: {dayOfWeek}");

        // 1. Удаляем старые подходы этого дня
        yield return StartCoroutine(DeleteTrainingDayCoroutine(userId, dayOfWeek));

        // 2. Добавляем новые подходы (если они есть)
        if (sets.Count > 0)
        {
            yield return StartCoroutine(InsertTrainingSets(userId, dayOfWeek, sets));
            Debug.Log($"✅ День '{dayOfWeek}' сохранен ({sets.Count} подходов)");
            Debug.Log($"в неделе в дне {dayOfWeek} сейчас {SetOfExercises.Count(Week.week.Days.FirstOrDefault(d=>d.name== dayOfWeek).setsOfExercises)}");
        }
        else
        {
            Debug.Log($"✅ День '{dayOfWeek}' очищен");
        }
    }

    // ============ УДАЛИТЬ ВЕСЬ ТРЕНИРОВОЧНЫЙ ДЕНЬ ============
    public void DeleteTrainingDay(long userId, string dayOfWeek)
    {
        StartCoroutine(DeleteTrainingDayCoroutine(userId, dayOfWeek));
    }

    IEnumerator DeleteTrainingDayCoroutine(long userId, string dayOfWeek)
    {
        string url = $"{supabaseUrl}/rest/v1/training_diary?user_id=eq.{userId}&day_of_week=eq.{UnityWebRequest.EscapeURL(dayOfWeek)}";

        UnityWebRequest request = UnityWebRequest.Delete(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
        request.SetRequestHeader("Prefer", "return=minimal");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Тренировочный день '{dayOfWeek}' полностью удален");
        }
        else if (request.responseCode == 404)
        {
            Debug.Log($"ℹ️ День '{dayOfWeek}' не найден для удаления");
        }
        else
        {
            Debug.LogError($"❌ Ошибка удаления дня '{dayOfWeek}': {request.error}");
        }

        request.Dispose();
    }
    // ============ ДОБАВИТЬ НОВЫЕ ПОДХОДЫ ============
    IEnumerator InsertTrainingSets(long userId, string dayOfWeek, List<TrainingSet> sets)
    {
        StringBuilder jsonBuilder = new StringBuilder();
        jsonBuilder.Append("[");

        for (int i = 0; i < sets.Count; i++)
        {
            var set = sets[i];
            jsonBuilder.Append("{");
            jsonBuilder.Append($"\"user_id\":{userId},");
            jsonBuilder.Append($"\"day_of_week\":\"{EscapeJson(dayOfWeek)}\",");
            jsonBuilder.Append($"\"exercise_id\":{set.exercise_id},");
            jsonBuilder.Append($"\"exercise_name\":\"{EscapeJson(set.exercise_name)}\",");
            jsonBuilder.Append($"\"set_number\":{set.set_number},");
            jsonBuilder.Append($"\"working_weight_kg\":{set.working_weight_kg.ToString(System.Globalization.CultureInfo.InvariantCulture)},");
            jsonBuilder.Append($"\"repetitions\":{set.repetitions}");
            jsonBuilder.Append("}");

            if (i < sets.Count - 1)
                jsonBuilder.Append(",");
        }

        jsonBuilder.Append("]");

        string url = $"{supabaseUrl}/rest/v1/training_diary";
        string json = jsonBuilder.ToString();

        // ДЕБАГ: Выводим JSON перед отправкой
        Debug.Log($"Отправляемый JSON: {json}");

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
        request.SetRequestHeader("Prefer", "return=representation"); // Изменяем на representation для получения ответа

        yield return request.SendWebRequest();

        Debug.Log($"Статус код: {request.responseCode}");
        Debug.Log($"Ответ: {request.downloadHandler?.text}");

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"❌ Ошибка сохранения подходов: {request.error}");
        }

        request.Dispose();
    }

    // ============ ЗАГРУЗИТЬ ВЕСЬ ДЕНЬ С ПОДХОДАМИ ============
    public void LoadTrainingDayWithSets(long userId, string dayOfWeek, System.Action<List<TrainingSet>> callback)
    {
        StartCoroutine(LoadTrainingDayWithSetsCoroutine(userId, dayOfWeek, callback));
    }

    IEnumerator LoadTrainingDayWithSetsCoroutine(long userId, string dayOfWeek, System.Action<List<TrainingSet>> callback)
    {
        string url = $"{supabaseUrl}/rest/v1/training_diary?user_id=eq.{userId}&day_of_week=eq.{UnityWebRequest.EscapeURL(dayOfWeek)}&select=exercise_id,exercise_name,set_number,working_weight_kg,repetitions&order=exercise_id,set_number";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");

        yield return request.SendWebRequest();

        List<TrainingSet> sets = new List<TrainingSet>();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            sets = ParseTrainingSets(json);
            Debug.Log($"Загружено {sets.Count} подходов для дня '{dayOfWeek}'");
        }
        else
        {
            Debug.LogError($"Ошибка загрузки дня '{dayOfWeek}': {request.error}");
        }

        callback?.Invoke(sets);
        request.Dispose();
    }

    // ============ ДОБАВИТЬ ОДИН ПОДХОД ============
    public void AddTrainingSet(long userId, string dayOfWeek, TrainingSet set)
    {
        StartCoroutine(AddTrainingSetCoroutine(userId, dayOfWeek, set));
    }

    IEnumerator AddTrainingSetCoroutine(long userId, string dayOfWeek, TrainingSet set)
    {
        string url = $"{supabaseUrl}/rest/v1/training_diary";
        string json = $"{{\"user_id\":{userId}," +
                      $"\"day_of_week\":\"{EscapeJson(dayOfWeek)}\"," +
                      $"\"exercise_id\":{set.exercise_id}," +
                      $"\"exercise_name\":\"{EscapeJson(set.exercise_name)}\"," +
                      $"\"set_number\":{set.set_number}," +
                      $"\"working_weight_kg\":{set.working_weight_kg.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                      $"\"repetitions\":{set.repetitions}}}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
        request.SetRequestHeader("Prefer", "return=minimal");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Подход #{set.set_number} упражнения '{set.exercise_name}' добавлен");
        }
        else
        {
            Debug.LogError($"❌ Ошибка добавления подхода: {request.error}");
        }

        request.Dispose();
    }

    // ============ УДАЛИТЬ ПОДХОД УПРАЖНЕНИЯ ============
    public void DeleteExerciseSet(long userId, string dayOfWeek, int exerciseId, int setNumber)
    {
        StartCoroutine(DeleteExerciseSetCoroutine(userId, dayOfWeek, exerciseId, setNumber));
    }

    IEnumerator DeleteExerciseSetCoroutine(long userId, string dayOfWeek, int exerciseId, int setNumber)
    {
        string url = $"{supabaseUrl}/rest/v1/training_diary?user_id=eq.{userId}&day_of_week=eq.{UnityWebRequest.EscapeURL(dayOfWeek)}&exercise_id=eq.{exerciseId}&set_number=eq.{setNumber}";

        UnityWebRequest request = UnityWebRequest.Delete(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
        request.SetRequestHeader("Prefer", "return=minimal");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Подход #{setNumber} упражнения ID={exerciseId} удален");
        }
        else
        {
            Debug.LogError($"❌ Ошибка удаления подхода: {request.error}");
        }

        request.Dispose();
    }

    // ============ ОБНОВИТЬ ПОДХОД ============
    public void UpdateTrainingSet(long userId, string dayOfWeek, TrainingSet set)
    {
        StartCoroutine(UpdateTrainingSetCoroutine(userId, dayOfWeek, set));
    }

    IEnumerator UpdateTrainingSetCoroutine(long userId, string dayOfWeek, TrainingSet set)
    {
        string url = $"{supabaseUrl}/rest/v1/training_diary?user_id=eq.{userId}&day_of_week=eq.{UnityWebRequest.EscapeURL(dayOfWeek)}&exercise_id=eq.{set.exercise_id}&set_number=eq.{set.set_number}";

        string json = $"{{\"working_weight_kg\":{set.working_weight_kg.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                      $"\"repetitions\":{set.repetitions}}}";

        UnityWebRequest request = new UnityWebRequest(url, "PATCH");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
        request.SetRequestHeader("Prefer", "return=minimal");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Подход #{set.set_number} упражнения '{set.exercise_name}' обновлен");
        }
        else
        {
            Debug.LogError($"❌ Ошибка обновления подхода: {request.error}");
        }

        request.Dispose();
    }

    // ============ УДАЛИТЬ ВСЕ ПОДХОДЫ УПРАЖНЕНИЯ ============
    public void DeleteAllExerciseSets(long userId, string dayOfWeek, int exerciseId)
    {
        StartCoroutine(DeleteAllExerciseSetsCoroutine(userId, dayOfWeek, exerciseId));
    }

    IEnumerator DeleteAllExerciseSetsCoroutine(long userId, string dayOfWeek, int exerciseId)
    {
        string url = $"{supabaseUrl}/rest/v1/training_diary?user_id=eq.{userId}&day_of_week=eq.{UnityWebRequest.EscapeURL(dayOfWeek)}&exercise_id=eq.{exerciseId}";

        UnityWebRequest request = UnityWebRequest.Delete(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
        request.SetRequestHeader("Prefer", "return=minimal");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Все подходы упражнения ID={exerciseId} удалены");
        }
        else
        {
            Debug.LogError($"❌ Ошибка удаления подходов: {request.error}");
        }

        request.Dispose();
    }
    // ============ ПРОВЕРИТЬ СУЩЕСТВУЕТ ЛИ ДЕНЬ ============
    public void CheckDayExists(long userId, string dayOfWeek, System.Action<bool> callback)
    {
        StartCoroutine(CheckDayExistsCoroutine(userId, dayOfWeek, callback));
    }

    IEnumerator CheckDayExistsCoroutine(long userId, string dayOfWeek, System.Action<bool> callback)
    {
        string url = $"{supabaseUrl}/rest/v1/training_diary?user_id=eq.{userId}&day_of_week=eq.{UnityWebRequest.EscapeURL(dayOfWeek)}&select=id&limit=1";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");

        yield return request.SendWebRequest();

        bool exists = false;

        if (request.result == UnityWebRequest.Result.Success)
        {
            string response = request.downloadHandler.text;
            exists = !string.IsNullOrEmpty(response) && response != "[]";
            Debug.Log($"День '{dayOfWeek}' существует: {exists}");
        }
        else
        {
            Debug.LogError($"Ошибка проверки дня: {request.error}");
        }

        callback?.Invoke(exists);
        request.Dispose();
    }
    // ============ ПАРСИНГ ПОДХОДОВ ИЗ JSON ============
    private List<TrainingSet> ParseTrainingSets(string json)
    {
        List<TrainingSet> sets = new List<TrainingSet>();

        if (json == "[]" || string.IsNullOrEmpty(json))
            return sets;

        try
        {
            json = json.Trim('[', ']');
            string[] records = json.Split(new[] { "}," }, StringSplitOptions.None);

            foreach (string record in records)
            {
                string cleanRecord = record.Trim('{', '}');
                string[] pairs = cleanRecord.Split(',');

                int exId = 0;
                string name = "";
                int setNum = 0;
                float weight = 0;
                int reps = 0;

                foreach (string pair in pairs)
                {
                    string[] keyValue = pair.Split(':');
                    if (keyValue.Length < 2) continue;

                    string key = keyValue[0].Trim().Trim('"');
                    string value = keyValue[1].Trim();

                    switch (key)
                    {
                        case "exercise_id":
                            int.TryParse(value, out exId);
                            break;
                        case "exercise_name":
                            name = value.Trim('"');
                            break;
                        case "set_number":
                            int.TryParse(value, out setNum);
                            break;
                        case "working_weight_kg":
                            float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out weight);
                            break;
                        case "repetitions":
                            int.TryParse(value, out reps);
                            break;
                    }
                }

                if (!string.IsNullOrEmpty(name))
                {
                    sets.Add(new TrainingSet(exId, name, setNum, weight, reps));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка парсинга подходов: {e.Message}");
        }

        return sets;
    }
    IEnumerator LoadUserMetricsCoroutine(long userId, System.Action<float, float, int, int> callback)
    {
        string url = $"{supabaseUrl}/rest/v1/user_metrics?user_id=eq.{userId}&select=weight_kg,body_fat_percent,age,experience_months&order=measurement_date.desc&limit=1";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            ParseMetricsFromJson(json, callback);
        }
        else
        {
            Debug.LogError($"❌ Ошибка загрузки метрик: {request.error}");
            callback?.Invoke(0, 0, 0, 0);
        }

        request.Dispose();
    }
    // ============ ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ============
    private void ParseMetricsFromJson(string json, System.Action<float, float, int, int> callback)
    {
        float weight = 0;
        float bodyFat = 0;
        int age = 0;
        int experience = 0;

        // Если пустой массив []
        if (json == "[]" || string.IsNullOrEmpty(json) || json.Length < 3)
        {
            callback?.Invoke(weight, bodyFat, age, experience);
            return;
        }

        try
        {
            // Ищем значения в JSON
            // Формат: [{"weight_kg":75.5,"body_fat_percent":15.2,"age":25,"experience_months":12}]

            int weightStart = json.IndexOf("\"weight_kg\":") + 12;
            if (weightStart >= 12)
            {
                int weightEnd = json.IndexOf(",", weightStart);
                string weightStr = json.Substring(weightStart, weightEnd - weightStart);
                float.TryParse(weightStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out weight);
            }

            int fatStart = json.IndexOf("\"body_fat_percent\":") + 19;
            if (fatStart >= 19)
            {
                int fatEnd = json.IndexOf(",", fatStart);
                string fatStr = json.Substring(fatStart, fatEnd - fatStart);
                float.TryParse(fatStr, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out bodyFat);
            }

            int ageStart = json.IndexOf("\"age\":") + 6;
            if (ageStart >= 6)
            {
                int ageEnd = json.IndexOf(",", ageStart);
                string ageStr = json.Substring(ageStart, ageEnd - ageStart);
                int.TryParse(ageStr, out age);
            }

            int expStart = json.IndexOf("\"experience_months\":") + 20;
            if (expStart >= 20)
            {
                int expEnd = json.IndexOf("}", expStart);
                string expStr = json.Substring(expStart, expEnd - expStart);
                int.TryParse(expStr, out experience);
            }

            callback?.Invoke(weight, bodyFat, age, experience);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка парсинга метрик: {e.Message}");
            callback?.Invoke(0, 0, 0, 0);
        }
    }


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

[System.Serializable]
public class TrainingSet
{
    public int exercise_id;          // Личный номер упражнения
    public string exercise_name;     // Название упражнения
    public int set_number;           // Номер сета (1, 2, 3...)
    public float working_weight_kg;  // Вес для этого сета
    public int repetitions;          // Повторения для этого сета

    public TrainingSet(int exId, string name, int setNum, float weight, int reps)
    {
        exercise_id = exId;
        exercise_name = name;
        set_number = setNum;
        working_weight_kg = weight;
        repetitions = reps;
    }
}