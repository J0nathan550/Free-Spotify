using SoundScapes.Interfaces;
using SoundScapes.Models;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace SoundScapes.Services;

/// <summary>
/// Служба для збереження та завантаження налаштувань.
/// </summary>
public class SettingsService : ISettings
{
    // Шлях до файлу налаштувань.
    public string FilePath { get; private set; } = "settings.json";

    // Модель налаштувань.
    public SettingsModel SettingsModel { get; private set; } = new SettingsModel();

    /// <summary>
    /// Метод завантаження налаштувань.
    /// </summary>
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
            Trace.WriteLine($"Помилка завантаження налаштувань: {ex.Message}");
            File.Delete(FilePath);
        }
    }

    /// <summary>
    /// Метод збереження налаштувань.
    /// </summary>
    public void Save()
    {
        try
        {
            string json = JsonSerializer.Serialize(SettingsModel);
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Помилка збереження налаштувань: {ex.Message}");
        }
    }
}
