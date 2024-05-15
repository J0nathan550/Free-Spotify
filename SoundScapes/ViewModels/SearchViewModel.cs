using CommunityToolkit.Mvvm.ComponentModel;
using FontAwesome.WPF;
using ModernWpf.Controls;
using SoundScapes.Classes;
using SoundScapes.Models;
using SpotifyExplode;
using System.Net.Http;
using System.Windows;

namespace SoundScapes.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    private readonly SpotifyClient _spotifyClient = new();
    private readonly MusicPlayerViewModel _musicPlayerView;

    [ObservableProperty]
    private string? _searchText;
    [ObservableProperty]
    private string? _errorText;
    [ObservableProperty]
    private Visibility? _errorTextVisibility = Visibility.Collapsed;
    [ObservableProperty]
    private Visibility? _resultsBoxVisibility = Visibility.Visible;
    [ObservableProperty]
    private List<SongModel>? _songsList = [];
    [ObservableProperty]
    private SongModel? _currentSong = null;
    private int globalIndex = 0;

    public SearchViewModel(MusicPlayerViewModel musicPlayerView)
    {
        _musicPlayerView = musicPlayerView;
        _musicPlayerView.SongChanged += (o, e) => CurrentSong = e;
        ErrorText = "Нема жодного знайденого трека. Використовуйте поле зверху щоб почати пошук!";
        ErrorTextVisibility = Visibility.Visible;
    }

    partial void OnCurrentSongChanged(SongModel? value)
    {
        if (value != null) // when the list in search is getting changed it can send null
        {
            _musicPlayerView.CurrentSong = value;
            _musicPlayerView.PlayMediaIcon = _musicPlayerView._musicPlayer.IsPaused ? FontAwesomeIcon.Play : FontAwesomeIcon.Pause;
            _musicPlayerView.IsPlayingFromPlaylist = false;
            _musicPlayerView.PlaySong();
        }
    }
    
    public void RegisterSearchBox(AutoSuggestBox searchBox) => searchBox!.QuerySubmitted += SearchBox_QuerySubmitted;

    public void UpdateViewList()
    {
        List<SongModel>? temp = SongsList;
        SongsList = null;
        SongsList = temp;
    }

    private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (!string.IsNullOrEmpty(SearchText))
        {
            ErrorText = "Завантаження...";
            ErrorTextVisibility = Visibility.Visible;
            ResultsBoxVisibility = Visibility.Collapsed;
            try
            {
                List<SpotifyExplode.Search.TrackSearchResult> tracks = await _spotifyClient.Search.GetTracksAsync(SearchText);
                List<SongModel> trackList = [];
                foreach (SpotifyExplode.Search.TrackSearchResult track in tracks)
                {
                    trackList.Add(new SongModel() { Index = globalIndex++, Title = track.Title, Artist = ArtistConverter.FormatArtists(track.Artists), SongID = track.Id, Duration = TimeConverter.ConvertMsToTime(track.DurationMs), Icon = track.Album.Images[0].Url, DurationLong = track.DurationMs });
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
    }
}