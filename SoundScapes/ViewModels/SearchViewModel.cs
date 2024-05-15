using CommunityToolkit.Mvvm.ComponentModel;
using FontAwesome.WPF;
using ModernWpf.Controls;
using SoundScapes.Classes;
using SoundScapes.Models;
using SoundScapes.ViewModels;
using SpotifyExplode;
using System.Net.Http;
using System.Windows;

/// <summary>
/// ViewModel для пошуку треків.
/// </summary>
public partial class SearchViewModel : ObservableObject
{
    private readonly SpotifyClient _spotifyClient = new();
    private readonly MusicPlayerViewModel _musicPlayerView;

    [ObservableProperty]
    private string? _searchText; // Текст для пошуку
    [ObservableProperty]
    private string? _errorText; // Текст помилки
    [ObservableProperty]
    private Visibility? _errorTextVisibility = Visibility.Collapsed; // Видимість тексту помилки
    [ObservableProperty]
    private Visibility? _resultsBoxVisibility = Visibility.Visible; // Видимість результатів пошуку
    [ObservableProperty]
    private List<SongModel>? _songsList = []; // Список треків
    [ObservableProperty]
    private SongModel? _currentSong = null; // Поточний трек
    private int globalIndex = 0;

    /// <summary>
    /// Конструктор класу SearchViewModel.
    /// </summary>
    /// <param name="musicPlayerView">ViewModel плеєра музики.</param>
    public SearchViewModel(MusicPlayerViewModel musicPlayerView)
    {
        _musicPlayerView = musicPlayerView;
        _musicPlayerView.SongChanged += (o, e) => CurrentSong = e;
        ErrorText = "Нема жодного знайденого трека. Використовуйте поле зверху щоб почати пошук!";
        ErrorTextVisibility = Visibility.Visible;
    }

    // Подія, що відбувається при зміні поточного треку
    partial void OnCurrentSongChanged(SongModel? value)
    {
        if (value != null) // коли список у пошуку змінюється, може відправити null
        {
            _musicPlayerView.CurrentSong = value;
            _musicPlayerView.PlayMediaIcon = _musicPlayerView._musicPlayer.IsPaused ? FontAwesomeIcon.Play : FontAwesomeIcon.Pause;
            _musicPlayerView.IsPlayingFromPlaylist = false;
            _musicPlayerView.PlaySong();
        }
    }

    // Реєстрація полів для пошуку
    public void RegisterSearchBox(AutoSuggestBox searchBox) => searchBox!.QuerySubmitted += SearchBox_QuerySubmitted;

    // Оновлення списку відображення
    public void UpdateViewList()
    {
        List<SongModel>? temp = SongsList;
        SongsList = null;
        SongsList = temp;
    }

    // Обробник події при поданні запиту пошуку
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