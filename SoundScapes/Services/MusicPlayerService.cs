using LibVLCSharp.Shared;
using SoundScapes.Interfaces;
using SoundScapes.Models;
using SpotifyExplode;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace SoundScapes.Services;

public class MusicPlayerService : IMusicPlayer
{
    public List<SongModel> Songs { get; set; } = [];
    public SongModel CurrentSong { get; set; } = new SongModel();
    public LibVLC LibVLC { get; } = new();
    public MediaPlayer MediaPlayer { get; private set; }
    public YoutubeClient YoutubeClient { get; private set; } = new YoutubeClient();
    public SpotifyClient SpotifyClient { get; private set; } = new SpotifyClient();

    public event EventHandler<SongModel>? SongChanged;

    public MusicPlayerService()
    {
        MediaPlayer = new MediaPlayer(LibVLC);
    }

    public void ChangeVolume(double volume)
    {
        MediaPlayer.Volume = (int)volume;
    }

    public void NextSong()
    {
        SongChanged?.Invoke(this, CurrentSong);
    }

    public void Pause()
    {

    }

    public async void Play()
    {
        Stop();
        await Task.Run(() =>
        {
            var youTubeID = SpotifyClient.Tracks.GetYoutubeIdAsync(CurrentSong.SongID).Result;
            if (youTubeID != null)
            {
                var streamManifest = YoutubeClient.Videos.Streams.GetManifestAsync(youTubeID);
                var streamInfo = streamManifest.Result.GetAudioStreams().GetWithHighestBitrate();
                var audioMedia = new Media(LibVLC, streamInfo.Url, FromType.FromLocation);
                audioMedia.AddOption(":no-video");
                MediaPlayer.Media = audioMedia;
                MediaPlayer.Play();
                SongChanged?.Invoke(this, CurrentSong);
            }
        });
    }

    public void PreviousSong()
    {
        SongChanged?.Invoke(this, CurrentSong);
    }

    public void RepeatSong()
    {

    }

    public void ShuffleSong()
    {
        SongChanged?.Invoke(this, CurrentSong);
    }

    public void Stop()
    {
        Task.Run(() =>
        {
            MediaPlayer.Stop();
            MediaPlayer.Media?.Dispose();
            SongChanged?.Invoke(this, CurrentSong);
        });
    }
}