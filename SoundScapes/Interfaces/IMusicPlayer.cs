using LibVLCSharp.Shared;
using SoundScapes.Models;
using SpotifyExplode;
using YoutubeExplode;

namespace SoundScapes.Interfaces;

public interface IMusicPlayer
{
    List<SongModel> Songs { get; set; }
    public SongModel CurrentSong { get; set; }
    event EventHandler<SongModel> SongChanged;
    LibVLC LibVLC { get; }
    MediaPlayer MediaPlayer { get; }
    YoutubeClient YoutubeClient { get; }
    SpotifyClient SpotifyClient { get; }
    void Play();
    void Stop();
    void Pause();
    void NextSong();
    void PreviousSong();
    void ShuffleSong();
    void RepeatSong();
    void ChangeVolume(double volume);
}