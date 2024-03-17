﻿using CommunityToolkit.Mvvm.ComponentModel;
using ModernWpf.Controls;
using SoundScapes.Classes;
using SoundScapes.Interfaces;
using SoundScapes.Models;
using SpotifyExplode;
using System.Net.Http;
using System.Windows;

namespace SoundScapes.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    private readonly IMusicPlayer _musicPlayer;
    private readonly SpotifyClient _spotifyClient = new();

    [ObservableProperty]
    private string? _searchText;
    [ObservableProperty]
    private string? _errorText;
    [ObservableProperty]
    private Visibility? _errorTextVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private Visibility? _resultsBoxVisibility = Visibility.Visible;
    [ObservableProperty]
    private List<SongModel> _songsList;
    [ObservableProperty]
    private SongModel _currentSong;

    public SearchViewModel(IMusicPlayer musicPlayer)
    {
        _musicPlayer = musicPlayer;
        _currentSong = _musicPlayer.CurrentSong;
        _songsList = _musicPlayer.Songs;
        _musicPlayer.SongChanged += (o, e) =>
        {
            _currentSong = e;
        };
    }

    partial void OnCurrentSongChanged(SongModel value)
    {
        if (value != null) // when the list in search is getting changed it can send null
        {
            _musicPlayer.CurrentSong = value;
            _musicPlayer.Play();
        }
    }

    partial void OnSongsListChanged(List<SongModel> value)
    {
        _musicPlayer.Songs = value;
    }

    public void RegisterSearchBox(AutoSuggestBox searchBox) => searchBox!.QuerySubmitted += SearchBox_QuerySubmitted;

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        Task.Run(async () =>
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                ErrorText = string.Empty;
                GC.Collect();
                try
                {
                    var tracks = await _spotifyClient.Search.GetTracksAsync(SearchText);
                    List<SongModel> trackList = [];
                    foreach (var track in tracks)
                    {
                        trackList.Add(new SongModel() { Title = track.Title, Artist = ArtistConverter.FormatArtists(track.Artists), SongID = track.Id, Duration = TimeConverter.ConvertMsToTime(track.DurationMs), Icon = track.Album.Images[0].Url });
                    }
                    if (trackList.Count > 1)
                    {
                        SongsList = trackList;
                        ErrorTextVisibility = Visibility.Collapsed;
                        ResultsBoxVisibility = Visibility.Visible;
                    }
                    else
                    {
                        ErrorText = $"За запитом '{SearchText}' нічого не знайдено.";
                        ErrorTextVisibility = Visibility.Visible;
                        ResultsBoxVisibility = Visibility.Collapsed;
                    }
                    CurrentSong = _musicPlayer.CurrentSong;
                }
                catch (HttpRequestException) 
                {
                    ErrorText = "Схоже у вас проблеми з інтернетом, спробуйте ще раз!";
                    ErrorTextVisibility = Visibility.Visible;
                    ResultsBoxVisibility = Visibility.Collapsed;
                }
                catch
                {
                    ErrorText = "Щось трапилось у програмі, спробуйте ще раз!";
                    ErrorTextVisibility = Visibility.Visible;
                    ResultsBoxVisibility = Visibility.Collapsed;
                }
            }
        });
    }
}