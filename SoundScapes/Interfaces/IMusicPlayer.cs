using LibVLCSharp.Shared;
using SoundScapes.Models;
using SpotifyExplode;
using YoutubeExplode;

namespace SoundScapes.Interfaces;

/// <summary>
/// Інтерфейс для відтворення музики.
/// </summary>
public interface IMusicPlayer
{
    // Об'єкт LibVLC для відтворення медіа.
    LibVLC LibVLC { get; }

    // Відтворювач медіа.
    MediaPlayer MediaPlayer { get; }

    // Клієнт YouTube.
    YoutubeClient YoutubeClient { get; }

    // Клієнт Spotify.
    SpotifyClient SpotifyClient { get; }

    // Токен для відміни відтворення музики.
    CancellationTokenSource CancellationTokenSourcePlay { get; }

    // Прапорець, що вказує, чи призупинено відтворення музики.
    bool IsPaused { get; }

    // Прапорець, що вказує, чи ввімкнено повторення музики.
    bool IsRepeating { get; }

    // Подія винятку.
    event EventHandler ExceptionThrown;

    // Метод відтворення пісні.
    Task OnPlaySong(SongModel currentSong);

    // Метод призупинення відтворення пісні.
    void OnPauseSong();

    // Метод ввімкнення/вимкнення повторення пісні.
    void OnRepeatingSong();

    // Метод для відміни відтворення музики.
    void CancelPlayingMusic();
}