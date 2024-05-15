namespace SoundScapes.Classes;

/// <summary>
/// Клас конвертера часу.
/// </summary>
public class TimeConverter
{
    /// <summary>
    /// Метод конвертації мілісекунд у формат часу (години:хвилини:секунди).
    /// </summary>
    /// <param name="milliseconds">Кількість мілісекунд.</param>
    /// <returns>Час у форматі години:хвилини:секунди.</returns>
    public static string ConvertMsToTime(long milliseconds)
    {
        // Створення об'єкту TimeSpan з мілісекунд.
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        // Обчислення загальної кількості годин (максимум 99).
        int totalHours = Math.Min(timeSpan.Days * 24 + timeSpan.Hours, 99);

        string formattedTime;

        // Форматування часу залежно від кількості годин.
        if (totalHours == 0)
        {
            formattedTime = $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}";
        }
        else if (totalHours < 100)
        {
            formattedTime = $"{totalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        else
        {
            formattedTime = $"100:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        return formattedTime;
    }
}