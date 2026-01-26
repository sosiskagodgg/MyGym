using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static WeeklyTrainingSchedule;

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
    // ============ ЗАГРУЗИТЬ УПРАЖНЕНИЯ ПОЛЬЗОВАТЕЛЯ ============
    public void LoadUserExercises(long userId, System.Action<List<ExerciseData>> callback)
    {
        StartCoroutine(GetUserExercises(userId, callback));
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
    public IEnumerator GetUserExercises(long userId, System.Action<List<ExerciseData>> callback)
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
    private bool isSaving = false;
    public void SaveTrainingDayWithSets(long userId, string dayOfWeek, List<TrainingSet> sets)
    {
        if (!isSaving) StartCoroutine(SaveTrainingDayWithSetsCoroutine(userId, dayOfWeek, sets));
    }

    IEnumerator SaveTrainingDayWithSetsCoroutine(long userId, string dayOfWeek, List<TrainingSet> sets)
    {
        Debug.Log($"Сохранение {sets.Count} подходов для дня: {dayOfWeek}");
        isSaving = true;
        // 1. Удаляем старые подходы этого дня
        yield return StartCoroutine(DeleteTrainingDayCoroutine(userId, dayOfWeek));

        // 2. Добавляем новые подходы (если они есть)
        if (sets.Count > 0)
        {
            yield return StartCoroutine(InsertTrainingSets(userId, dayOfWeek, sets));
            Debug.Log($"✅ День '{dayOfWeek}' сохранен ({sets.Count} подходов)");
            Debug.Log($"в неделе в дне {dayOfWeek} сейчас {SetOfExercises.Count(Week.week.Days.FirstOrDefault(d => d.name == dayOfWeek).setsOfExercises)}");
        }
        else
        {
            Debug.Log($"✅ День '{dayOfWeek}' очищен");
        }
        isSaving = false;
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
            Debug.Log($"✅ Подход #{set.set_number} упражнения '{set.exercise_name}' обновлен, новый вес -{set.working_weight_kg}");
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
    // ============ СОХРАНИТЬ НЕДЕЛЬНОЕ РАСПИСАНИЕ ============
    public void SaveWeeklyTrainingSchedule(long userId, WeeklyTrainingSchedule schedule)
    {
        StartCoroutine(SaveWeeklyTrainingScheduleCoroutine(userId, schedule));
    }
    public void SaveEntireWeek(long userId, WeeklyTrainingSchedule schedule)
    {
        if(!isSaving)StartCoroutine(SaveEntireWeekCoroutine(userId, schedule));
    }

    IEnumerator SaveEntireWeekCoroutine(long userId, WeeklyTrainingSchedule schedule)
    {
        isSaving = true;
        Debug.Log($"Начинаю сохранение всей недели для пользователя {userId}");

        // Сохраняем каждый день
        foreach (var day in schedule.days)
        {
            Debug.Log($"Сохранение дня: {day.day_of_week}, упражнений: {day.exercises?.Count ?? 0}");
            yield return StartCoroutine(SaveTrainingDayToSchedule(userId, day));
        }

        Debug.Log($"✅ Вся неделя сохранена (дней: {schedule.days.Count})");
        isSaving = false;
    }
    IEnumerator SaveWeeklyTrainingScheduleCoroutine(long userId, WeeklyTrainingSchedule schedule)
    {
        Debug.Log($"Сохранение недельного расписания для user {userId}");

        foreach (var dayData in schedule.days)
        {
            yield return StartCoroutine(SaveTrainingDayToSchedule(userId, dayData));
        }

        Debug.Log($"✅ Недельное расписание сохранено");
    }

    IEnumerator SaveTrainingDayToSchedule(long userId, TrainingDaySchedule dayData)
    {
        Debug.Log($"Сохранение дня: {dayData.day_of_week}, упражнений: {dayData.exercises?.Count ?? 0}");

        // 1. Удаляем ВСЕ старые записи этого дня
        yield return StartCoroutine(DeleteDayExercisesFromSchedule(userId, dayData.day_of_week));

        // 2. Ждем, чтобы удаление точно завершилось
        yield return new WaitForSeconds(0.5f);

        // 3. Если нет упражнений - создаем одну запись с is_active=false
        if (dayData.exercises == null || dayData.exercises.Count == 0)
        {
            // Для пустого дня создаем одну запись с пустым упражнением
            string emptyDayJson = $"{{\"user_id\":{userId}," +
                                 $"\"day_of_week\":\"{EscapeJson(dayData.day_of_week)}\"," +
                                 $"\"exercise_id\":0," +
                                 $"\"exercise_name\":\"\"," +
                                 $"\"set_number\":0," +
                                 $"\"working_weight_kg\":0," +
                                 $"\"repetitions\":0," +
                                 $"\"is_active\":false," +
                                 $"\"notes\":\"{EscapeJson(dayData.notes ?? "")}\"}}";

            string json = "[" + emptyDayJson + "]";
            Debug.Log($"Отправляемый JSON для пустого дня: {json}");

            yield return StartCoroutine(SendBatchToSupabase(json, dayData.day_of_week, 0));
            yield break;
        }

        // 4. Создаем список всех упражнений
        List<string> records = new List<string>();

        // Группируем упражнения по set_number, чтобы обрабатывать подходы
        var groupedBySet = dayData.exercises
            .GroupBy(e => e.set_number)
            .OrderBy(g => g.Key)
            .ToList();

        // 5. Для каждого сета создаем записи подходов
        foreach (var setGroup in groupedBySet)
        {
            int setNumber = setGroup.Key;

            // Сортируем подходы в сете по exercise_id
            var approachesInSet = setGroup.OrderBy(e => e.exercise_id).ToList();

            for (int approachIndex = 0; approachIndex < approachesInSet.Count; approachIndex++)
            {
                var set = approachesInSet[approachIndex];

                // Ключевое: exercise_id должен быть уникальным в пределах сета
                // approachIndex - это номер подхода в сете (0, 1, 2...)
                string exerciseJson = $"{{\"user_id\":{userId}," +
                                     $"\"day_of_week\":\"{EscapeJson(dayData.day_of_week)}\"," +
                                     $"\"exercise_id\":{approachIndex}," + // подход 0, 1, 2...
                                     $"\"exercise_name\":\"{EscapeJson(set.exercise_name)}\"," +
                                     $"\"set_number\":{setNumber}," + // номер сета
                                     $"\"working_weight_kg\":{set.working_weight_kg.ToString(CultureInfo.InvariantCulture)}," +
                                     $"\"repetitions\":{set.repetitions}," +
                                     $"\"is_active\":true," +
                                     $"\"notes\":\"{EscapeJson(dayData.notes ?? "")}\"}}";
                records.Add(exerciseJson);
            }
        }

        // 6. Отправляем все записи батчем
        if (records.Count > 0)
        {
            string json = "[" + string.Join(",", records) + "]";
            Debug.Log($"Отправляемый JSON: {json}");

            yield return StartCoroutine(SendBatchToSupabase(json, dayData.day_of_week, records.Count));
        }
    }

    // Вспомогательный метод для отправки батча
    IEnumerator SendBatchToSupabase(string json, string dayName, int recordCount)
    {
        string url = $"{supabaseUrl}/rest/v1/user_training_schedule";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] body = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
        request.SetRequestHeader("Prefer", "return=representation");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ День '{dayName}' сохранен с {recordCount} записями");
        }
        else
        {
            Debug.LogError($"❌ Ошибка сохранения дня '{dayName}': {request.error}");
            Debug.LogError($"Статус код: {request.responseCode}");
            if (request.downloadHandler != null)
                Debug.LogError($"Ответ: {request.downloadHandler.text}");
        }

        request.Dispose();
    }
    IEnumerator DeleteDayExercisesFromSchedule(long userId, string dayOfWeek)
    {
        // Правильный фильтр для удаления упражнений дня
        string url = $"{supabaseUrl}/rest/v1/user_training_schedule?user_id=eq.{userId}&day_of_week=eq.{UnityWebRequest.EscapeURL(dayOfWeek)}";

        // Добавляем условие, что exercise_id не равен null ИЛИ удаляем все записи дня
        // В Supabase нужно делать либо так:
        // string url = $"{supabaseUrl}/rest/v1/user_training_schedule?user_id=eq.{userId}&day_of_week=eq.{dayOfWeek}&exercise_id=gt.0";
        // Или просто удаляем все записи дня (и заголовок, и упражнения)

        UnityWebRequest request = UnityWebRequest.Delete(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
        request.SetRequestHeader("Prefer", "return=minimal");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ Упражнения дня '{dayOfWeek}' удалены");
        }
        else
        {
            Debug.LogError($"❌ Ошибка удаления упражнений дня '{dayOfWeek}': {request.error}");
            Debug.LogError($"URL: {url}");
            if (request.downloadHandler != null)
                Debug.LogError($"Ответ: {request.downloadHandler.text}");
        }

        request.Dispose();
    }

    // ============ ЗАГРУЗИТЬ НЕДЕЛЬНОЕ РАСПИСАНИЕ ============
    public void LoadWeeklyTrainingSchedule(long userId, System.Action<WeeklyTrainingSchedule> callback)
    {
        StartCoroutine(LoadWeeklyTrainingScheduleCoroutine(userId, callback));
    }
    IEnumerator LoadWeeklyTrainingScheduleCoroutine(long userId, System.Action<WeeklyTrainingSchedule> callback)
    {
        // Добавьте notes в список полей для выборки
        string url = $"{supabaseUrl}/rest/v1/user_training_schedule?user_id=eq.{userId}&select=day_of_week,exercise_id,exercise_name,set_number,working_weight_kg,repetitions,is_active,notes&order=day_of_week,exercise_id,set_number";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");

        yield return request.SendWebRequest();

        WeeklyTrainingSchedule schedule = new WeeklyTrainingSchedule(userId);

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;

            // ДЕБАГ: Выводим что пришло
            Debug.Log($"Полученный JSON от сервера: {json}");

            ParseWeeklyScheduleFromNewTable(json, schedule);
            Debug.Log($"Загружено недельное расписание для user {userId}");
        }
        else
        {
            Debug.LogError($"❌ Ошибка загрузки расписания: {request.error}");
            Debug.LogError($"URL: {url}");
        }

        callback?.Invoke(schedule);
        request.Dispose();
    }
    private void ParseWeeklyScheduleFromNewTable(string json, WeeklyTrainingSchedule schedule)
    {
        if (string.IsNullOrEmpty(json) || json == "[]")
        {
            Debug.Log("Расписание пустое или не найдено");

            // Создаем пустые дни недели
            string[] daysOfWeek = { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };
            foreach (var dayName in daysOfWeek)
            {
                schedule.days.Add(new TrainingDaySchedule(dayName)
                {
                    is_active = false,
                    exercises = new List<TrainingSet>(),
                    notes = ""
                });
            }
            return;
        }

        try
        {
            // Временный словарь для отладки
            Dictionary<string, List<object>> debugData = new Dictionary<string, List<object>>();

            // Словарь для группировки данных: день → (notes, словарь сетов)
            Dictionary<string, (string notes, Dictionary<int, List<TrainingSet>> sets)> dayData =
                new Dictionary<string, (string, Dictionary<int, List<TrainingSet>>)>();

            // Инициализируем структуру для всех дней
            string[] daysOfWeek = { "Понедельник", "Вторник", "Среда", "Четверг", "Пятница", "Суббота", "Воскресенье" };
            foreach (var dayName in daysOfWeek)
            {
                dayData[dayName] = ("", new Dictionary<int, List<TrainingSet>>());
                debugData[dayName] = new List<object>();
            }

            // Парсим JSON через JsonUtility (более надежный способ)
            List<ScheduleRecord> records = ParseScheduleRecords(json);

            Debug.Log($"Загружено {records.Count} записей");

            // Сначала собираем notes для каждого дня
            Dictionary<string, string> dayNotes = new Dictionary<string, string>();
            foreach (var record in records)
            {
                if (!string.IsNullOrEmpty(record.day_of_week) && !string.IsNullOrEmpty(record.notes))
                {
                    // Берем последние notes для дня
                    dayNotes[record.day_of_week] = record.notes;

                    // ДЕБАГ
                    Debug.Log($"Найдены notes для дня {record.day_of_week}: {record.notes}");
                }
            }

            // Затем группируем упражнения
            foreach (var record in records)
            {
                if (string.IsNullOrEmpty(record.day_of_week)) continue;

                string dayName = record.day_of_week;
                int setNumber = record.set_number;
                string exerciseName = record.exercise_name ?? "";
                float weight = record.working_weight_kg;
                int reps = record.repetitions;

                // ДЕБАГ
                debugData[dayName].Add(new
                {
                    exerciseName,
                    setNumber,
                    weight,
                    reps,
                    exerciseId = record.exercise_id
                });

                // Пропускаем пустые записи (заголовки дней)
                if (string.IsNullOrEmpty(exerciseName) || exerciseName.Trim() == "")
                {
                    continue;
                }

                // Инициализируем сет если его нет
                if (!dayData[dayName].sets.ContainsKey(setNumber))
                {
                    dayData[dayName].sets[setNumber] = new List<TrainingSet>();
                }

                // Добавляем подход
                dayData[dayName].sets[setNumber].Add(new TrainingSet(
                    record.exercise_id,
                    exerciseName,
                    setNumber,
                    weight,
                    reps
                ));
            }

            // ДЕБАГ: выводим все данные
            foreach (var dayName in daysOfWeek)
            {
                Debug.Log($"День {dayName}: {debugData[dayName].Count} записей");
                foreach (var item in debugData[dayName])
                {
                    Debug.Log($"  {item}");
                }
            }

            // Преобразуем в структуру WeeklyTrainingSchedule
            foreach (var dayName in daysOfWeek)
            {
                var (notes, setsForDay) = dayData[dayName];

                // Получаем notes из словаря или из данных дня
                string dayNotesValue = dayNotes.ContainsKey(dayName) ?
                    dayNotes[dayName] : notes;

                var daySchedule = new TrainingDaySchedule(dayName);
                var allExercises = new List<TrainingSet>();

                // Собираем все подходы из всех сетов
                foreach (var setPair in setsForDay.OrderBy(s => s.Key))
                {
                    allExercises.AddRange(setPair.Value);
                }

                daySchedule.exercises = allExercises;
                daySchedule.is_active = allExercises.Count > 0;
                daySchedule.notes = dayNotesValue; // Устанавливаем notes

                // ДЕБАГ
                Debug.Log($"Создан день {dayName}: {allExercises.Count} упражнений, notes: {dayNotesValue}");

                schedule.days.Add(daySchedule);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка парсинга расписания: {e.Message}\n{e.StackTrace}");
        }
    }

    // Новый метод для парсинга записей
    private List<ScheduleRecord> ParseScheduleRecords(string json)
    {
        List<ScheduleRecord> records = new List<ScheduleRecord>();

        if (string.IsNullOrEmpty(json) || json == "[]")
            return records;

        try
        {
            // Альтернативный способ парсинга для дебага
            // 1. Пробуем через JsonHelper
            try
            {
                var helperRecords = JsonHelper.FromJson<ScheduleRecord>(json);
                if (helperRecords != null && helperRecords.Length > 0)
                {
                    records = helperRecords.ToList();
                    Debug.Log($"Успешно распарсено через JsonHelper: {records.Count} записей");
                    return records;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"JsonHelper не сработал: {e.Message}");
            }

            // 2. Ручной парсинг JSON
            if (json.StartsWith("[") && json.EndsWith("]"))
            {
                string cleanJson = json.Substring(1, json.Length - 2);
                string[] recordStrings = cleanJson.Split(new[] { "},{" }, StringSplitOptions.None);

                foreach (var recordStr in recordStrings)
                {
                    string cleanRecord = recordStr.Trim('{', '}');
                    var record = ParseSingleScheduleRecord(cleanRecord);
                    if (record != null)
                    {
                        records.Add(record);
                    }
                }
                Debug.Log($"Распарсено вручную: {records.Count} записей");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка в ParseScheduleRecords: {e.Message}");
        }

        return records;
    }

    private ScheduleRecord ParseSingleScheduleRecord(string recordStr)
    {
        try
        {
            var record = new ScheduleRecord();
            string[] pairs = recordStr.Split(',');

            foreach (string pair in pairs)
            {
                string[] keyValue = pair.Split(new[] { ':' }, 2);
                if (keyValue.Length < 2) continue;

                string key = keyValue[0].Trim().Trim('"');
                string value = keyValue[1].Trim().Trim('"');

                switch (key)
                {
                    case "user_id":
                        long.TryParse(value, out record.user_id);
                        break;
                    case "day_of_week":
                        record.day_of_week = value;
                        break;
                    case "exercise_id":
                        int.TryParse(value, out record.exercise_id);
                        break;
                    case "exercise_name":
                        record.exercise_name = value;
                        break;
                    case "set_number":
                        int.TryParse(value, out record.set_number);
                        break;
                    case "working_weight_kg":
                        float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out record.working_weight_kg);
                        break;
                    case "repetitions":
                        int.TryParse(value, out record.repetitions);
                        break;
                    case "is_active":
                        record.is_active = value.ToLower() == "true";
                        break;
                    case "notes":
                        record.notes = value;
                        break;
                }
            }

            return record;
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка парсинга записи: {e.Message}");
            return null;
        }
    }

    // Запасной метод на случай проблем с JSON

    // Вспомогательный класс для парсинга JSON
    [System.Serializable]

    private class ScheduleRecord
    {
        public long user_id;
        public string day_of_week;
        public int exercise_id;
        public string exercise_name;
        public int set_number;
        public float working_weight_kg; // Изменяем на не nullable
        public int repetitions;         // Изменяем на не nullable
        public bool is_active;
        public string notes;
    }

    // Вспомогательный класс для парсинга JSON массива
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{\"items\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.items;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }
    }


    // ============ ОБНОВИТЬ ОДИН ДЕНЬ В РАСПИСАНИИ ============
    public void UpdateDayInSchedule(long userId, string dayOfWeek, bool isActive, List<TrainingSet> exercises, string notes = "")
    {
        var dayData = new TrainingDaySchedule(dayOfWeek, isActive)
        {
            exercises = exercises,
            notes = notes
        };

        StartCoroutine(SaveTrainingDayToSchedule(userId, dayData));
    }

    // ============ ПРОВЕРИТЬ АКТИВНЫЕ ДНИ ============
    public void GetActiveTrainingDays(long userId, System.Action<List<string>> callback)
    {
        StartCoroutine(GetActiveTrainingDaysCoroutine(userId, callback));
    }

    IEnumerator GetActiveTrainingDaysCoroutine(long userId, System.Action<List<string>> callback)
    {
        string url = $"{supabaseUrl}/rest/v1/user_training_schedule?user_id=eq.{userId}&is_active=eq.true&select=day_of_week";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");

        yield return request.SendWebRequest();

        List<string> activeDays = new List<string>();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            activeDays = ParseActiveDaysJson(json);
        }

        callback?.Invoke(activeDays);
        request.Dispose();
    }
    public void UpdateTrainingSetInSchedule(long userId, string dayOfWeek, TrainingSet set)
    {
        StartCoroutine(UpdateTrainingSetInScheduleCoroutine(userId, dayOfWeek, set));
    }

    IEnumerator UpdateTrainingSetInScheduleCoroutine(long userId, string dayOfWeek, TrainingSet set)
    {
        // Используем ту же логику фильтрации, что и при сохранении
        string url = $"{supabaseUrl}/rest/v1/user_training_schedule?user_id=eq.{userId}&day_of_week=eq.{UnityWebRequest.EscapeURL(dayOfWeek)}&exercise_id=eq.{set.exercise_id}&set_number=eq.{set.set_number}";

        string json = $"{{\"working_weight_kg\":{set.working_weight_kg.ToString(CultureInfo.InvariantCulture)}," +
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
            Debug.Log($"✅ Подход #{set.set_number} упражнения '{set.exercise_name}' обновлен, новый вес: {set.working_weight_kg}");

            // Дополнительная проверка - загрузим обновленные данные
        }
        else
        {
            Debug.LogError($"❌ Ошибка обновления подхода: {request.error}");
            Debug.LogError($"URL: {url}");
            Debug.LogError($"JSON: {json}");
            if (request.downloadHandler != null)
                Debug.LogError($"Ответ: {request.downloadHandler.text}");
        }

        request.Dispose();
    }
    private List<string> ParseActiveDaysJson(string json)
    {
        List<string> activeDays = new List<string>();

        if (string.IsNullOrEmpty(json) || json == "[]")
            return activeDays;

        try
        {
            json = json.Trim('[', ']');
            string[] records = json.Split(new[] { "}," }, StringSplitOptions.None);

            foreach (string record in records)
            {
                string cleanRecord = record.Trim('{', '}');
                string[] pairs = cleanRecord.Split(',');

                foreach (string pair in pairs)
                {
                    string[] keyValue = pair.Split(':');
                    if (keyValue.Length < 2) continue;

                    string key = keyValue[0].Trim().Trim('"');
                    string value = keyValue[1].Trim();

                    if (key == "day_of_week")
                    {
                        activeDays.Add(value.Trim('"'));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка парсинга активных дней: {e.Message}");
        }

        return activeDays;
    }

    // ============ УДАЛИТЬ ДЕНЬ ИЗ РАСПИСАНИЯ ============
    public void DeleteDayFromSchedule(long userId, string dayOfWeek)
    {
        StartCoroutine(DeleteDayFromScheduleCoroutine(userId, dayOfWeek));
    }

    IEnumerator DeleteDayFromScheduleCoroutine(long userId, string dayOfWeek)
    {
        string url = $"{supabaseUrl}/rest/v1/user_training_schedule?user_id=eq.{userId}&day_of_week=eq.{UnityWebRequest.EscapeURL(dayOfWeek)}";

        UnityWebRequest request = UnityWebRequest.Delete(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
        request.SetRequestHeader("Prefer", "return=minimal");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"✅ День '{dayOfWeek}' удален из расписания");
        }
        else if (request.responseCode == 404)
        {
            Debug.Log($"ℹ️ День '{dayOfWeek}' не найден в расписании");
        }
        else
        {
            Debug.LogError($"❌ Ошибка удаления дня '{dayOfWeek}': {request.error}");
        }

        request.Dispose();
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
    public IEnumerator LoadUserMetricsCoroutine(long userId, System.Action<float, float, int, int> callback)
    {
        string url = $"{supabaseUrl}/rest/v1/user_metrics?user_id=eq.{userId}&select=weight_kg,body_fat_percent,age,experience_months&order=measurement_date.desc&limit=1";

        Debug.Log($"Загрузка метрик по URL: {url}");

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", supabaseKey);
        request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log($"Полученный JSON от сервера: {json}");

            ParseMetricsFromJson(json, callback);
        }
        else
        {
            Debug.LogError($"❌ Ошибка загрузки метрик: {request.error}");
            Debug.LogError($"Статус код: {request.responseCode}");
            if (request.downloadHandler != null)
                Debug.LogError($"Ответ сервера: {request.downloadHandler.text}");
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

        Debug.Log($"Парсим JSON метрик: {json}");

        // Если пустой массив []
        if (json == "[]" || string.IsNullOrEmpty(json) || json.Length < 3)
        {
            Debug.LogWarning("JSON метрик пустой");
            callback?.Invoke(weight, bodyFat, age, experience);
            return;
        }

        try
        {
            // Убираем квадратные скобки
            if (json.StartsWith("[") && json.EndsWith("]"))
            {
                json = json.Substring(1, json.Length - 2);
            }

            Debug.Log($"Очищенный JSON: {json}");

            // Убираем фигурные скобки
            json = json.Trim('{', '}');
            Debug.Log($"После удаления фигурных скобок: {json}");

            // Разбиваем по запятым
            string[] pairs = json.Split(',');

            foreach (string pair in pairs)
            {
                // Разбиваем каждую пару по двоеточию
                string[] keyValue = pair.Split(':');
                if (keyValue.Length < 2) continue;

                // Очищаем ключ и значение от кавычек и пробелов
                string key = keyValue[0].Trim().Trim('"');
                string value = keyValue[1].Trim();

                // Удаляем возможные кавычки в конце
                if (value.EndsWith("}")) value = value.Substring(0, value.Length - 1);
                value = value.Trim('"');

                Debug.Log($"Ключ: '{key}', Значение: '{value}'");

                switch (key)
                {
                    case "weight_kg":
                        if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float w))
                        {
                            weight = w;
                            Debug.Log($"Вес: {weight}");
                        }
                        else
                        {
                            Debug.LogWarning($"Не удалось распарсить вес: {value}");
                        }
                        break;

                    case "body_fat_percent":
                        if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f))
                        {
                            bodyFat = f;
                            Debug.Log($"Процент жира: {bodyFat}");
                        }
                        else
                        {
                            Debug.LogWarning($"Не удалось распарсить процент жира: {value}");
                        }
                        break;

                    case "age":
                        if (int.TryParse(value, out int a))
                        {
                            age = a;
                            Debug.Log($"Возраст: {age}");
                        }
                        else
                        {
                            Debug.LogWarning($"Не удалось распарсить возраст: {value}");
                        }
                        break;

                    case "experience_months":
                        if (int.TryParse(value, out int e))
                        {
                            experience = e;
                            Debug.Log($"Опыт (месяцы): {experience}");
                        }
                        else
                        {
                            Debug.LogWarning($"Не удалось распарсить опыт: {value}");
                        }
                        break;

                    default:
                        Debug.LogWarning($"Неизвестный ключ: {key}");
                        break;
                }
            }

            Debug.Log($"Итог: вес={weight}, жир={bodyFat}, возраст={age}, опыт={experience}");
            callback?.Invoke(weight, bodyFat, age, experience);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка парсинга метрик: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            callback?.Invoke(0, 0, 0, 0);
        }
    }


    private List<ExerciseData> ParseExercisesJson(string json)
    {
        List<ExerciseData> exercises = new List<ExerciseData>();

        if (json == "[]" || string.IsNullOrEmpty(json))
        {
            Debug.Log("JSON пустой: " + json);
            return exercises;
        }

        try
        {
            Debug.Log($"Парсим JSON упражнений: {json}");

            // Используем JsonHelper для парсинга
            var records = JsonHelper.FromJson<ExerciseRecord>(json);

            if (records != null && records.Length > 0)
            {
                foreach (var record in records)
                {
                    if (!string.IsNullOrEmpty(record.exercise_name))
                    {
                        exercises.Add(new ExerciseData(record.exercise_name, record.coefficient));
                        Debug.Log($"Добавлено упражнение: {record.exercise_name}, коэффициент: {record.coefficient}");
                    }
                }
                Debug.Log($"Успешно распарсено через JsonHelper: {exercises.Count} упражнений");
            }
            else
            {
                Debug.LogWarning("JsonHelper вернул null или пустой массив");
                // Пробуем старый метод как запасной вариант
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка парсинга JSON упражнений: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            // Пробуем старый метод как запасной вариант
        }

        return exercises;
    }

    // Добавьте этот вспомогательный класс для парсинга
    [System.Serializable]
    private class ExerciseRecord
    {
        public string exercise_name;
        public float coefficient;
    }

    private string EscapeJson(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }


}
[System.Serializable]
public class TrainingDaySchedule
{
    public string day_of_week;
    public bool is_active;
    public List<TrainingSet> exercises;
    public string notes;

    public TrainingDaySchedule(string v)
    {
        this.day_of_week = v;
        this.is_active = false;
        this.exercises = new List<TrainingSet>();
        this.notes = "";
    }

    public TrainingDaySchedule(string v, bool isActive)
    {
        this.day_of_week = v;
        this.is_active = isActive;
        this.exercises = new List<TrainingSet>();
        this.notes = "";
    }

    public TrainingDaySchedule(string day, List<TrainingSet> exercises, string notes)
    {
        day_of_week = day;
        this.exercises = exercises ?? new List<TrainingSet>();
        this.notes = notes;
        this.is_active = (exercises != null && exercises.Count > 0);
    }
}

[System.Serializable]
public class WeeklyTrainingSchedule
{
    public long user_id;
    public List<TrainingDaySchedule> days = new List<TrainingDaySchedule>();

    public WeeklyTrainingSchedule(long userId)
    {
        this.user_id = userId;
    }

    public WeeklyTrainingSchedule(long userId, List<TrainingDaySchedule> days)
    {
        user_id = userId;
        this.days = days;
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