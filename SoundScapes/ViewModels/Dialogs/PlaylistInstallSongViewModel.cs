using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Downloader;
using SoundScapes.Models;
using SpotifyExplode;
using System.Diagnostics;
using System.IO;
using System.Windows;

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
    private TextAlignment _trackDescriptionAlignment = TextAlignment.Left;
    [ObservableProperty]
    private TextWrapping _trackDescriptionWrapping = TextWrapping.NoWrap;
    [ObservableProperty]
    private bool _isDownloadFinished;
    [ObservableProperty]
    private double _downloadProgress;
    [ObservableProperty]
    private string? _trackDescription;
    [ObservableProperty]
    private string? _downloadProgressPercent;
    [ObservableProperty]
    private string? _dataInstalled;

    private readonly DownloadService _downloader = new(new DownloadConfiguration() { Timeout = 5000, ClearPackageOnCompletionWithFailure = true });
    private readonly SpotifyClient spotifyClient = new();
    private int _currentFilesDownloaded = 0;

    public PlaylistInstallSongViewModel()
    {
        _downloader.DownloadProgressChanged += DownloadService_DownloadProgressChanged;
        _downloader.DownloadFileCompleted += DownloadService_DownloadFileCompleted;
        
        CancelDownloadCommand = new AsyncRelayCommand(CancelDownloadCommand_Execute);
        
        TrackDescription = $"Трек: ... ({_currentFilesDownloaded} / {DownloadedListQueue.Count})";
        DataInstalled = "Завантажено: [0 МБ] з [... МБ]";
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
        try
        {
            var link = await spotifyClient.Tracks.GetDownloadUrlAsync(DownloadListQueue[_currentFilesDownloaded].SongID, default);
            CreateMusicSaveFolder();
            await _downloader.DownloadFileTaskAsync(new DownloadPackage() { Address = link, FileName = $@"SavedMusic\{DownloadListQueue[_currentFilesDownloaded].Artist[0]} - {DownloadListQueue[_currentFilesDownloaded].Title}.mp3" }, default);
        }
        catch
        {
            StartInstalling();
        }
    }

    private void DownloadService_DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        if(DownloadListQueue == null) return;
        TrackDescription = $"Трек: {DownloadListQueue[_currentFilesDownloaded].Artist[0]} - {DownloadListQueue[_currentFilesDownloaded].Title} ({_currentFilesDownloaded + 1} / {DownloadListQueue.Count})";
        DataInstalled = $"Завантажено: [{CalculateBytesSize(e.ReceivedBytesSize)} з {CalculateBytesSize(e.TotalBytesToReceive)}]";
        DownloadProgressPercent = $"Прогресс: {e.ProgressPercentage:F1}%";
        DownloadProgress = e.ProgressPercentage;
    }

    private async Task CancelDownloadCommand_Execute()
    {
        await CancelDownload();
    }

    const double INV_KB = 1 / 1024.0;
    const double INV_MB = 1 / (1024.0 * 1024.0);
    const double INV_GB = 1 / (1024.0 * 1024.0 * 1024.0);
    private static string CalculateBytesSize(long size)
    {
        if (size < 1024)
        {
            return $"{size} Б";
        }
        if (size < 1024 * 1024)
        {
            return $"{(size * INV_KB):F1} КБ";
        }
        if (size < 1024 * 1024 * 1024)
        {
            return $"{(size * INV_MB):F1} МБ";
        }
        return $"{(size * INV_GB):F1} ГБ";
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
        try
        {
            var link = await spotifyClient.Tracks.GetDownloadUrlAsync(DownloadListQueue[_currentFilesDownloaded].SongID, default);
            await _downloader.DownloadFileTaskAsync(new DownloadPackage() { Address = link, FileName = $@"SavedMusic\{DownloadListQueue[_currentFilesDownloaded].Artist[0]} - {DownloadListQueue[_currentFilesDownloaded].Title}.mp3" }, default);
        }
        catch
        {
            _currentFilesDownloaded++;
            if (_currentFilesDownloaded >= DownloadListQueue.Count)
            {
                TrackDescription = "Сталася помилка під час завантаження треку. Будь ласка, закрийте це вікно та спробуйте ще раз. Проблема може бути у поганому інтернет-з'єднанні або у тому, що трек, який ви намагалися додати до плейлисту, не має посилання для завантаження. Розгляньте можливість видалення треку, щоб уникнути подальших помилок. Крім того, якщо бажаєте, можете зберегти успішно завантажені файли, натиснувши кнопку \"Закрити та зберегти\".";
                TrackDescriptionAlignment = TextAlignment.Center;
                TrackDescriptionWrapping = TextWrapping.WrapWithOverflow;
                DataInstalled = string.Empty;
                DownloadProgressPercent = string.Empty;
                DownloadProgress = 0;
                IsDownloadFinished = true;
                return;
            }
            StartInstalling();
            return;
        }
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
        try
        {
            string saveFolderPath = "SavedMusic";
            if (!Directory.Exists(saveFolderPath))
            {
                Directory.CreateDirectory(saveFolderPath);
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error creating the 'SavedMusic' folder: {ex.Message}");
        }
    }
}