using SoundScapes.Interfaces;
using SoundScapes.Models;

namespace SoundScapes.Services;

public class MusicPlayerService : IMusicPlayer
{
    public List<SongModel> Songs { get; set; } = [];
    public SongModel CurrentSong { get; set; } = new SongModel();
    public event EventHandler<SongModel>? SongChanged;

    public void ChangeVolume()
    {
    }

    public void NextSong()
    {
        SongChanged?.Invoke(this, CurrentSong);
    }

    public void Pause()
    {
    }

    public void Play()
    {
        SongChanged?.Invoke(this, CurrentSong);
    }

    public void PreviousSong()
    {
        SongChanged?.Invoke(this, CurrentSong);
    }

    public void ShuffleSong()
    {
        SongChanged?.Invoke(this, CurrentSong);
    }

    public void Stop()
    {
        SongChanged?.Invoke(this, CurrentSong);
    }
}