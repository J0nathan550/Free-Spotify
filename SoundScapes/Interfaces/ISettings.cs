using SoundScapes.Models;

namespace SoundScapes.Interfaces;

/// <summary>
/// Інтерфейс для роботи з налаштуваннями.
/// </summary>
public interface ISettings
{
    // Шлях до файлу налаштувань.
    string FilePath { get; }

    // Модель налаштувань.
    SettingsModel SettingsModel { get; }

    // Метод завантаження налаштувань.
    void Load();

    // Метод збереження налаштувань.
    void Save();
}