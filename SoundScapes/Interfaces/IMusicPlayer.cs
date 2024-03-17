using SoundScapes.Models;

namespace SoundScapes.Interfaces;

public interface IMusicPlayer
{
    List<SongModel> Songs { get; set; }
    public SongModel CurrentSong { get; set; }
    event EventHandler<SongModel> SongChanged;
    void Play();
    void Stop();
    void Pause();
    void NextSong();
    void PreviousSong();
    void ShuffleSong();
    void ChangeVolume();
}