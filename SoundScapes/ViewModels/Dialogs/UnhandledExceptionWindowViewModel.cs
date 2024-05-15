using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace SoundScapes.ViewModels;

/// <summary>
/// ViewModel для вікна неперехопленого винятку.
/// </summary>
public partial class UnhandledExceptionWindowViewModel : ObservableObject
{
    /// <summary>
    /// Повідомлення про помилку.
    /// </summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Команда для відправлення помилки на GitHub.
    /// </summary>
    [ObservableProperty]
    private RelayCommand _sendToGithubCommand;

    /// <summary>
    /// Ініціалізує новий екземпляр класу <see cref="UnhandledExceptionWindowViewModel"/>.
    /// </summary>
    public UnhandledExceptionWindowViewModel() => SendToGithubCommand = new RelayCommand(SendToGithubCommand_Execute);

    /// <summary>
    /// Виконує команду відправлення помилки на GitHub.
    /// </summary>
    private void SendToGithubCommand_Execute()
    {
        try
        {
            // Створення об'єкту для запуску процесу браузера і переходу на GitHub Issues
            ProcessStartInfo processStartInfo = new()
            {
                CreateNoWindow = true,
                UseShellExecute = true,
                FileName = "https://github.com/J0nathan550/Free-Spotify/issues"
            };
            Process.Start(processStartInfo); // Запуск процесу
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message); // Логування помилки, якщо її неможливо обробити
        }
    }
}
