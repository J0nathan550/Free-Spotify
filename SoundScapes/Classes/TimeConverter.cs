namespace SoundScapes.Classes;

public class TimeConverter
{
    public static string ConvertMsToTime(long milliseconds)
    {
        // Convert milliseconds to TimeSpan
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);

        // Use TimeSpan's ToString method to get the desired time format
        string formattedTime = $"{timeSpan.Minutes}:{timeSpan.Seconds:D2}";

        return formattedTime;
    }
}