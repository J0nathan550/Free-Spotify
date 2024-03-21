using LibVLCSharp.Shared;
using SoundScapes.Models;
using SpotifyExplode;
using YoutubeExplode;

namespace SoundScapes.Interfaces;

public interface IMusicPlayer
{
    LibVLC LibVLC { get; }
    MediaPlayer MediaPlayer { get; }
    YoutubeClient YoutubeClient { get; }
    SpotifyClient SpotifyClient { get; }
    CancellationTokenSource CancellationTokenSourcePlay { get; }
    bool IsPaused { get; }
    bool IsRepeating { get; }
    Task OnPlaySong(SongModel currentSong);
    void OnPauseSong();
    void OnRepeatingSong();
    void CancelPlayingMusic();
}