using LibVLCSharp.Shared;
using SoundScapes.Interfaces;
using SoundScapes.Models;
using SpotifyExplode;
using SpotifyExplode.Tracks;
using System.Diagnostics;
using System.IO;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace SoundScapes.Services;

/// <summary>
/// Служба відтворення музики.
/// </summary>
public class MusicPlayerService : IMusicPlayer
{
    // Об'єкт LibVLC для відтворення медіа.
    public LibVLC LibVLC { get; private set; } = new();

    // Відтворювач медіа.
    public MediaPlayer MediaPlayer { get; private set; }

    // Клієнт YouTube.
    public YoutubeClient YoutubeClient { get; private set; } = new();

    // Клієнт Spotify.
    public SpotifyClient SpotifyClient { get; private set; } = new();

    // Токен для відміни відтворення музики.
    public CancellationTokenSource CancellationTokenSourcePlay { get; private set; } = new();

    // Прапорець, що вказує, чи призупинено відтворення музики.
    public bool IsPaused { get; private set; }

    // Прапорець, що вказує, чи ввімкнено повторення музики.
    public bool IsRepeating { get; private set; }

    /// <summary>
    /// Конструктор класу MusicPlayerService.
    /// </summary>
    public MusicPlayerService() => MediaPlayer = new MediaPlayer(LibVLC);

    // Подія винятку.
    public event EventHandler? ExceptionThrown;

    /// <summary>
    /// Метод відтворення пісні.
    /// </summary>
    /// <param name="currentSong">Поточна пісня, яка буде відтворена.</param>
    public async Task OnPlaySong(SongModel currentSong)
    {
        try
        {
            CreateMusicSaveFolderIfNeeded();

            CancellationTokenSourcePlay?.Cancel();
            CancellationTokenSourcePlay = new CancellationTokenSource();

            CancelPlayingMusic();

            bool fileExists = false;
            if (File.Exists(@$"SavedMusic\{currentSong.Artist[0]} - {currentSong.Title}.mp3"))
            {
                fileExists = true;
            }

            CancelPlayingMusic();

            Media? media;
            if (fileExists)
            {
                media = new Media(LibVLC, @$"SavedMusic\{currentSong.Artist[0]} - {currentSong.Title}.mp3", FromType.FromPath);
            }
            else
            {
                string? youtubeID = await SpotifyClient.Tracks.GetYoutubeIdAsync(TrackId.Parse(currentSong.SongID), CancellationTokenSourcePlay.Token);
                StreamManifest streamInfo = await YoutubeClient.Videos.Streams.GetManifestAsync($"https://youtube.com/watch?v={youtubeID}", CancellationTokenSourcePlay.Token);
                CancelPlayingMusic();
                IStreamInfo content = streamInfo.GetAudioOnlyStreams().GetWithHighestBitrate();
                media = new Media(LibVLC, content.Url, FromType.FromLocation);
            }
            media.AddOption(":no-video");

            CancelPlayingMusic();

            MediaPlayer.Media = media;
            MediaPlayer.Play();
            if (IsPaused) MediaPlayer.SetPause(true);

            CancelPlayingMusic();
        }
        catch (TaskCanceledException ex)
        {
            Trace.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
            ExceptionThrown?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Метод відміни відтворення музики.
    /// </summary>
    public void CancelPlayingMusic()
    {
        if (CancellationTokenSourcePlay.IsCancellationRequested)
        {
            MediaPlayer.Stop();
            MediaPlayer.Media?.Dispose();
        }
    }

    /// <summary>
    /// Метод призупинення відтворення пісні.
    /// </summary>
    public void OnPauseSong()
    {
        IsPaused = !IsPaused;
        MediaPlayer.SetPause(IsPaused);
    }

    /// <summary>
    /// Метод ввімкнення/вимкнення повторення пісні.
    /// </summary>
    public void OnRepeatingSong() => IsRepeating = !IsRepeating;

    /// <summary>
    /// Метод для створення папки для збереження музики, якщо вона не існує.
    /// </summary>
    private static void CreateMusicSaveFolderIfNeeded()
    {
        if (!Directory.Exists("SavedMusic"))
        {
            Directory.CreateDirectory("SavedMusic");
        }
    }
}
