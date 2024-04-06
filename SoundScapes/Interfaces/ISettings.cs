using SoundScapes.Models;

namespace SoundScapes.Interfaces;

public interface ISettings
{
    string FilePath { get; }
    SettingsModel SettingsModel { get; }
    void Load();
    void Save();
}