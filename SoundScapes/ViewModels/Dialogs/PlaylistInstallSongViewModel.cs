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
    private RelayCommand _cancelDownloadCommand;
    [ObservableProperty]
    private RelayCommand _finishDownloadCommand;
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

    private CancellationTokenSource? cancelDownloadingSongs = null;
    private readonly DownloadService _downloader = new(new DownloadConfiguration() { Timeout = 5000, ClearPackageOnCompletionWithFailure = true });
    private readonly SpotifyClient spotifyClient = new();
    private int _currentFilesDownloaded = 0;

    public PlaylistInstallSongViewModel()
    {
        _downloader.DownloadProgressChanged += DownloadService_DownloadProgressChanged;
        _downloader.DownloadFileCompleted += DownloadService_DownloadFileCompleted;

        CancelDownloadCommand = new RelayCommand(CancelDownloadCommand_Execute);
        FinishDownloadCommand = new RelayCommand(FinishDownloadCommand_Execute);

        TrackDescription = $"Трек: ... ({_currentFilesDownloaded} / {DownloadedListQueue.Count})";
        DataInstalled = "Завантажено: [0 МБ] з [... МБ]";
        DownloadProgressPercent = "Прогресс: 0%";
        DownloadProgress = 0.0;

        CreateMusicSaveFolder();
    }

    private void FinishDownloadCommand_Execute() => FinishDownload();

    private void FinishDownload()
    {
        IsDownloadFinished = true;
        _downloader.CancelAsync();
        cancelDownloadingSongs?.Cancel();
        cancelDownloadingSongs = null;
    }

    private void DownloadService_DownloadFileCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        DownloadedListQueue.Add(DownloadListQueue[_currentFilesDownloaded]);
        _currentFilesDownloaded++;
        if (_currentFilesDownloaded == DownloadListQueue.Count)
        {
            FinishDownload();
            return;
        }
        cancelDownloadingSongs ??= new();
        if (cancelDownloadingSongs.Token.IsCancellationRequested) return;
        StartInstalling(cancelDownloadingSongs.Token);
    }

    private void DownloadService_DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        if (DownloadListQueue == null) return;
        TrackDescription = $"Трек: {DownloadListQueue[_currentFilesDownloaded].Artist[0]} - {DownloadListQueue[_currentFilesDownloaded].Title} ({_currentFilesDownloaded + 1} / {DownloadListQueue.Count})";
        DataInstalled = $"Завантажено: [{CalculateBytesSize(e.ReceivedBytesSize)} з {CalculateBytesSize(e.TotalBytesToReceive)}]";
        DownloadProgressPercent = $"Прогресс: {e.ProgressPercentage:F1}%";
        DownloadProgress = e.ProgressPercentage;
    }

    private void CancelDownloadCommand_Execute()
    {
        CancelDownload();
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

    private void CancelDownload()
    {
        cancelDownloadingSongs?.Cancel();
        cancelDownloadingSongs = null;
        if (_currentFilesDownloaded != DownloadListQueue.Count)
        {
            _downloader.CancelAsync();
        }
        foreach (SongModel item in DownloadedListQueue)
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

    private async void StartInstalling(CancellationToken token) => await StartInstallingAsync(token);

    private async Task StartInstallingAsync(CancellationToken cancellationToken)
    {
        if (DownloadListQueue.Count == 0) return;
        try
        {
            string? link = await spotifyClient.Tracks.GetDownloadUrlAsync(DownloadListQueue[_currentFilesDownloaded].SongID, cancellationToken);
            await _downloader.DownloadFileTaskAsync(new DownloadPackage() { Address = link, FileName = $@"SavedMusic\{DownloadListQueue[_currentFilesDownloaded].Artist[0]} - {DownloadListQueue[_currentFilesDownloaded].Title}.mp3" }, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return;
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
            StartInstalling(cancellationToken);
            return;
        }
    }

    partial void OnDownloadListQueueChanged(List<SongModel> value)
    {
        if (value != null && !IsDownloadFinished)
        {
            cancelDownloadingSongs ??= new();
            StartInstalling(cancelDownloadingSongs.Token);
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