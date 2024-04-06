using SoundScapes.Interfaces;
using SoundScapes.Models;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace SoundScapes.Services;

public class SettingsService : ISettings
{
    public string FilePath { get; private set; } = "settings.json";
    public SettingsModel SettingsModel { get; private set; } = new SettingsModel();

    public void Load()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                string json = File.ReadAllText(FilePath);
                SettingsModel = JsonSerializer.Deserialize<SettingsModel>(json) ?? new();
            }
            else SettingsModel = new();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error loading settings: {ex.Message}");
            File.Delete(FilePath);
        }
    }

    public void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(SettingsModel);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error saving settings: {ex.Message}");
        }
    }
}