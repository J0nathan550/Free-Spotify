using LibVLCSharp.Shared;
using SoundScapes.Interfaces;
using SoundScapes.Models;
using SpotifyExplode;
using System.Diagnostics;
using System.Windows.Threading;
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

    public async Task OnPlaySong(SongModel currentSong)
    {
        try
        {
            // Cancel any existing tasks
            CancellationTokenSourcePlay?.Cancel();
            CancellationTokenSourcePlay = new CancellationTokenSource();

            CancelPlayingMusic();

            // Get YouTube ID and stream info
            var youtubeID = await SpotifyClient.Tracks.GetYoutubeIdAsync(currentSong.SongID, CancellationTokenSourcePlay.Token);
            var streamInfo = await YoutubeClient.Videos.Streams.GetManifestAsync($"https://youtube.com/watch?v={youtubeID}", CancellationTokenSourcePlay.Token);

            // If cancellation was requested, stop playback and return
            CancelPlayingMusic();

            // Get the audio stream with the highest bitrate
            var content = streamInfo.GetAudioStreams().GetWithHighestBitrate();

            // If cancellation was requested, stop playback and return
            CancelPlayingMusic();

            // Create media and set options
            var media = new Media(LibVLC, content.Url, FromType.FromLocation);
            media.AddOption(":no-video");

            // If cancellation was requested, stop playback and return
            CancelPlayingMusic();

            // Set media and start playback
            MediaPlayer.Media = media;
            MediaPlayer.Play();

            // If cancellation was requested, stop playback and return
            CancelPlayingMusic();
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.Message);
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
}