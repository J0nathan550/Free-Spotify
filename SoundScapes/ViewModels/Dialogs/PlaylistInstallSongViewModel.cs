using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Downloader;
using SoundScapes.Models;
using SpotifyExplode;
using System.Diagnostics;
using System.IO;

namespace SoundScapes.ViewModels;

public partial class PlaylistInstallSongViewModel : ObservableObject
{
    [ObservableProperty]
    private IAsyncRelayCommand _cancelDownloadCommand;
    [ObservableProperty]
    private List<SongModel> _downloadListQueue = [];
    [ObservableProperty]
    private List<SongModel> _downloadedListQueue = [];
    [ObservableProperty]
    private bool _isDownloadFinished;
    [ObservableProperty]
    private double _downloadProgress;
    [ObservableProperty]
    private string? _trackDescription;
    [ObservableProperty]
    private string? _estimationTime;
    [ObservableProperty]
    private string? _downloadProgressPercent;
    [ObservableProperty]
    private string? _dataInstalled;
    [ObservableProperty]
    private string? _internetSpeed;

    private readonly DownloadService _downloader = new(new DownloadConfiguration() { Timeout = 5000, ClearPackageOnCompletionWithFailure = true });
    private readonly SpotifyClient spotifyClient = new();
    private int _currentFilesDownloaded = 0;

    public PlaylistInstallSongViewModel()
    {
        _downloader.DownloadProgressChanged += DownloadService_DownloadProgressChanged;
        _downloader.DownloadFileCompleted += DownloadService_DownloadFileCompleted;
        
        CancelDownloadCommand = new AsyncRelayCommand(CancelDownloadCommand_Execute);
        
        TrackDescription = $"Трек: ... ({_currentFilesDownloaded} / {DownloadedListQueue.Count})";
        DataInstalled = "Завантажено: [0 МБ] з [??? МБ]";
        InternetSpeed = "Швидкість інтернету: ??? МБ/C";
        EstimationTime = "Залишилось часу: ??:??:??";
        DownloadProgressPercent = "Прогресс: 0%";
        DownloadProgress = 0.0;

        CreateMusicSaveFolder();
    }

    private async Task FinishDownload()
    {
        IsDownloadFinished = true;
        await _downloader.CancelTaskAsync();
        await _downloader.Clear();
    }

    private async void DownloadService_DownloadFileCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        DownloadedListQueue.Add(DownloadListQueue[_currentFilesDownloaded]);
        _currentFilesDownloaded++;
        if (_currentFilesDownloaded == DownloadListQueue.Count)
        {
            await FinishDownload();
            return;
        }
        var link = await spotifyClient.Tracks.GetDownloadUrlAsync(DownloadListQueue[_currentFilesDownloaded].SongID, default);
        await _downloader.DownloadFileTaskAsync(new DownloadPackage() { Address = link, FileName = $@"SavedMusic\{DownloadListQueue[_currentFilesDownloaded].Artist[0]} - {DownloadListQueue[_currentFilesDownloaded].Title}.mp3" }, default);
    }

    private void DownloadService_DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        if(DownloadListQueue == null) return;
        TrackDescription = $"Трек: {DownloadListQueue[_currentFilesDownloaded].Artist[0]} - {DownloadListQueue[_currentFilesDownloaded].Title} ({_currentFilesDownloaded + 1} / {DownloadListQueue.Count})";
        DataInstalled = $"Завантажено: [{e.ReceivedBytesSize} з {e.TotalBytesToReceive}]";
        InternetSpeed = $"Швидкість інтернету: {e.AverageBytesPerSecondSpeed}";
        EstimationTime = $"Залишилось часу: {e.AverageBytesPerSecondSpeed}";
        DownloadProgressPercent = $"Прогресс: {e.ProgressPercentage}%";
        DownloadProgress = e.ProgressPercentage;
    }

    private async Task CancelDownloadCommand_Execute()
    {
        await CancelDownload();
    }

    private async Task CancelDownload()
    {
        if (_currentFilesDownloaded != DownloadListQueue.Count)
        {
            await _downloader.CancelTaskAsync();
            await _downloader.Clear();
        }
        foreach (var item in DownloadedListQueue)
        {
            try
            {
                if (Directory.Exists("SavedMusic"))
                {
                    File.Delete(@$"SavedMusic\{item.Artist[0]} - {item.Title}.mp3");
                }
            }
            catch
            {
                break;
            }
        }
    }

    private async void StartInstalling() => await StartInstallingAsync();

    private async Task StartInstallingAsync()
    {
        if (DownloadListQueue.Count == 0) return;
        var link = await spotifyClient.Tracks.GetDownloadUrlAsync(DownloadListQueue[_currentFilesDownloaded].SongID, default);
        await _downloader.DownloadFileTaskAsync(new DownloadPackage() { Address = link, FileName = $@"SavedMusic\{DownloadListQueue[_currentFilesDownloaded].Artist[0]} - {DownloadListQueue[_currentFilesDownloaded].Title}.mp3" }, default);
    }

    partial void OnDownloadListQueueChanged(List<SongModel> value)
    {
        if (value != null && !IsDownloadFinished)
        {
            StartInstalling();
        }
    }

    private static void CreateMusicSaveFolder()
    {
        if (!Directory.Exists("SavedMusic"))
        {
            Directory.CreateDirectory("SavedMusic");
        }
    }
}