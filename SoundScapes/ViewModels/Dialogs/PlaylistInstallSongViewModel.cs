using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Downloader;
using SoundScapes.Models;
using SpotifyExplode;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace SoundScapes.ViewModels;

/// <summary>
/// ViewModel для установки плейлисту пісень
/// </summary>
public partial class PlaylistInstallSongViewModel : ObservableObject
{
    // Команда для скасування завантаження
    [ObservableProperty]
    private RelayCommand _cancelDownloadCommand;
    // Команда для завершення завантаження
    [ObservableProperty]
    private RelayCommand _finishDownloadCommand;
    // Черга для завантаження
    [ObservableProperty]
    private List<SongModel> _downloadListQueue = [];
    // Черга завантажених пісень
    [ObservableProperty]
    private List<SongModel> _downloadedListQueue = [];
    // Вирівнювання опису треку
    [ObservableProperty]
    private TextAlignment _trackDescriptionAlignment = TextAlignment.Left;
    // Обертання опису треку
    [ObservableProperty]
    private TextWrapping _trackDescriptionWrapping = TextWrapping.NoWrap;
    // Флаг, що вказує, чи завершено завантаження
    [ObservableProperty]
    private bool _isDownloadFinished;
    // Прогрес завантаження
    [ObservableProperty]
    private double _downloadProgress;
    // Опис треку
    [ObservableProperty]
    private string? _trackDescription;
    // Відсоток завантаження
    [ObservableProperty]
    private string? _downloadProgressPercent;
    // Дані про встановлені файли
    [ObservableProperty]
    private string? _dataInstalled;

    // Токен для скасування завантаження
    private CancellationTokenSource? cancelDownloadingSongs = null;
    // Сервіс для завантаження файлів
    private readonly DownloadService _downloader = new(new DownloadConfiguration() { Timeout = 5000, ClearPackageOnCompletionWithFailure = true });
    // Клієнт Spotify API
    private readonly SpotifyClient spotifyClient = new();
    // Лічильник завантажених файлів
    private int _currentFilesDownloaded = 0;

    /// <summary>
    /// Конструктор класу
    /// </summary>
    public PlaylistInstallSongViewModel()
    {
        // Підписка на події сервісу завантаження
        _downloader.DownloadProgressChanged += DownloadService_DownloadProgressChanged;
        _downloader.DownloadFileCompleted += DownloadService_DownloadFileCompleted;

        // Ініціалізація команд
        CancelDownloadCommand = new RelayCommand(CancelDownloadCommand_Execute);
        FinishDownloadCommand = new RelayCommand(FinishDownloadCommand_Execute);

        // Ініціалізація даних
        TrackDescription = $"Трек: ... ({_currentFilesDownloaded} / {DownloadedListQueue.Count})";
        DataInstalled = "Завантажено: [0 МБ] з [... МБ]";
        DownloadProgressPercent = "Прогресс: 0%";
        DownloadProgress = 0.0;

        // Створення папки для збереження музики
        CreateMusicSaveFolder();
    }

    // Метод для виконання команди завершення завантаження
    private void FinishDownloadCommand_Execute() => FinishDownload();

    // Метод для завершення завантаження
    private void FinishDownload()
    {
        IsDownloadFinished = true;
        _downloader.CancelAsync();
        cancelDownloadingSongs?.Cancel();
        cancelDownloadingSongs = null;
    }

    // Метод, що викликається при завершенні завантаження файлу
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

    // Метод, що викликається при зміні прогресу завантаження
    private void DownloadService_DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        if (DownloadListQueue == null) return;
        TrackDescription = $"Трек: {DownloadListQueue[_currentFilesDownloaded].Artist[0]} - {DownloadListQueue[_currentFilesDownloaded].Title} ({_currentFilesDownloaded + 1} / {DownloadListQueue.Count})";
        DataInstalled = $"Завантажено: [{CalculateBytesSize(e.ReceivedBytesSize)} з {CalculateBytesSize(e.TotalBytesToReceive)}]";
        DownloadProgressPercent = $"Прогресс: {e.ProgressPercentage:F1}%";
        DownloadProgress = e.ProgressPercentage;
    }

    // Метод для виконання команди скасування завантаження
    private void CancelDownloadCommand_Execute()
    {
        CancelDownload();
    }

    // Константи для переведення байтів у розмір файлу
    const double INV_KB = 1 / 1024.0;
    const double INV_MB = 1 / (1024.0 * 1024.0);
    const double INV_GB = 1 / (1024.0 * 1024.0 * 1024.0);

    // Метод для обчислення розміру файлу у зручному форматі
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

    // Метод для скасування завантаження
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

    // Метод для початку встановлення
    private async void StartInstalling(CancellationToken token) => await StartInstallingAsync(token);

    // Асинхронний метод для початку встановлення
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

    // Метод, що викликається при зміні черги для завантаження
    partial void OnDownloadListQueueChanged(List<SongModel> value)
    {
        if (value != null && !IsDownloadFinished)
        {
            cancelDownloadingSongs ??= new();
            StartInstalling(cancelDownloadingSongs.Token);
        }
    }

    // Метод для створення папки для збереження музики
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