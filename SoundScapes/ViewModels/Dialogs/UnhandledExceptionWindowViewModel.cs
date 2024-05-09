using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace SoundScapes.ViewModels;

public partial class UnhandledExceptionWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _errorMessage = string.Empty;
    [ObservableProperty]
    private RelayCommand _sendToGithubCommand;

    public UnhandledExceptionWindowViewModel() => SendToGithubCommand = new RelayCommand(SendToGithubCommand_Execute);

    private void SendToGithubCommand_Execute()
    {
        try
        {
            ProcessStartInfo processStartInfo = new()
            {
                CreateNoWindow = true,
                UseShellExecute = true,
                FileName = "https://github.com/J0nathan550/Free-Spotify/issues"
            };
            Process.Start(processStartInfo);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
        }
    }
}