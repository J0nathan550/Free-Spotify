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

public class MusicPlayerService : IMusicPlayer
{
    public LibVLC LibVLC { get; private set; } = new();
    public MediaPlayer MediaPlayer { get; private set; }
    public YoutubeClient YoutubeClient { get; private set; } = new();
    public SpotifyClient SpotifyClient { get; private set; } = new();
    public CancellationTokenSource CancellationTokenSourcePlay { get; private set; } = new();
    public bool IsPaused { get; private set; }
    public bool IsRepeating { get; private set; }
    public MusicPlayerService() => MediaPlayer = new(LibVLC);

    public event EventHandler? ExceptionThrown;

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

    public void CancelPlayingMusic()
    {
        if (CancellationTokenSourcePlay.IsCancellationRequested)
        {
            MediaPlayer.Stop();
            MediaPlayer.Media?.Dispose();
        }
    }

    public void OnPauseSong()
    {
        IsPaused = !IsPaused;
        MediaPlayer.SetPause(IsPaused);
    }

    public void OnRepeatingSong() => IsRepeating = !IsRepeating;

    private static void CreateMusicSaveFolderIfNeeded()
    {
        if (!Directory.Exists("SavedMusic"))
        {
            Directory.CreateDirectory("SavedMusic");
        }
    }
}