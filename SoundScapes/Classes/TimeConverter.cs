namespace SoundScapes.Classes;

public class TimeConverter
{
    public static string ConvertMsToTime(long milliseconds)
    {
        // Convert milliseconds to TimeSpan
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);

        // Check if total hours exceed 99, if yes, cap it at 99
        int totalHours = Math.Min(timeSpan.Days * 24 + timeSpan.Hours, 99);

        // Construct the formatted time string based on the total hours
        string formattedTime;

        // If total hours are less than 1 hour
        if (totalHours == 0)
        {
            formattedTime = $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}";
        }
        // If total hours are less than 100 hours
        else if (totalHours < 100)
        {
            formattedTime = $"{totalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        // If total hours are 100 or more
        else
        {
            formattedTime = $"100:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        return formattedTime;
    }
}