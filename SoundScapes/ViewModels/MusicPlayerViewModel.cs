using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FontAwesome.WPF;
using Microsoft.Extensions.DependencyInjection;
using ModernWpf.Controls;
using SoundScapes.Classes;
using SoundScapes.Interfaces;
using SoundScapes.Models;
using SoundScapes.Views.Dialogs;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SoundScapes.ViewModels;

public partial class MusicPlayerViewModel : ObservableObject
{
    public readonly IMusicPlayer _musicPlayer; // Посилання на інтерфейс IMusicPlayer
    private readonly ISettings _settings; // Посилання на інтерфейс ISettings

    // Властивості, які автоматично генерують сповіщення при зміні значення
    [ObservableProperty]
    private SongModel _currentSong = new(); // Поточна пісня
    [ObservableProperty]
    private FontAwesomeIcon _playMediaIcon = FontAwesomeIcon.Play; // Іконка кнопки відтворення медіа
    [ObservableProperty]
    private SolidColorBrush _shuffleMediaBrush = new(Colors.White); // Колір кнопки перемішування
    [ObservableProperty]
    private SolidColorBrush _repeatMediaBrush = new(Colors.White); // Колір кнопки повторення
    [ObservableProperty]
    private RelayCommand _playSongCommand; // Команда для відтворення пісні
    [ObservableProperty]
    private RelayCommand _nextSongCommand; // Команда для переходу до наступної пісні
    [ObservableProperty]
    private RelayCommand _previousSongCommand; // Команда для переходу до попередньої пісні
    [ObservableProperty]
    private RelayCommand _shuffleSongCommand; // Команда для перемішування пісень
    [ObservableProperty]
    private RelayCommand _repeatSongCommand; // Команда для повторення пісні
    [ObservableProperty]
    private IAsyncRelayCommand _addFavoriteSongCommand; // Асинхронна команда для додавання улюбленої пісні
    [ObservableProperty]
    private double _volumeValue = 100.0; // Гучність відтворення
    [ObservableProperty]
    private double _musicPosition = 0.0; // Поточна позиція відтворення музики
    [ObservableProperty]
    private string _songDuration = "0:00"; // Тривалість пісні
    [ObservableProperty]
    private bool _isPlayingFromPlaylist = false; // Показує, чи відтворюються пісні з плейлисту

    // Події, що виникають при зміні пісні або плейлисту
    public event EventHandler<SongModel>? SongChanged;
    public event EventHandler<SongModel>? SongPlaylistChanged;

    // Таймер для оновлення позиції музики
    public readonly System.Timers.Timer _musicPositionUpdate = new() { Interval = 500 };

    // Конструктор класу MusicPlayerViewModel
    public MusicPlayerViewModel(IMusicPlayer musicPlayer, ISettings settings)
    {
        // Ініціалізуємо сервіси та налаштування
        _settings = settings;
        _settings.Load();
        _musicPlayer = musicPlayer;
        _musicPlayer.MediaPlayer.Volume = _settings.SettingsModel.Volume;
        VolumeValue = _settings.SettingsModel.Volume;

        // Ініціалізуємо команди
        _playSongCommand = new RelayCommand(PlaySongCommand_Execute);
        _nextSongCommand = new RelayCommand(NextSongCommand_Execute);
        _previousSongCommand = new RelayCommand(PreviousSongCommand_Execute);
        _shuffleSongCommand = new RelayCommand(ShuffleSongCommand_Execute);
        _repeatSongCommand = new RelayCommand(RepeatSongCommand_Execute);
        _addFavoriteSongCommand = new AsyncRelayCommand(AddFavoriteSongCommand_Execute);

        // Обробники подій для таймера оновлення позиції музики та винятків відтворення
        _musicPlayer.MediaPlayer.Volume = (int)VolumeValue;
        _musicPositionUpdate.Elapsed += (o, e) => CheckMediaPlayerPosition();
        _musicPlayer.ExceptionThrown += (o, e) => NextSongCommand_Execute();

        // Встановлюємо прапорець, що відтворюються пісні з плейлисту
        IsPlayingFromPlaylist = true;
    }

    // Метод для виконання команди додавання улюбленої пісні
    private async Task AddFavoriteSongCommand_Execute()
    {
        // Отримуємо необхідні сервіси та дані
        PlaylistAddSongItemView? contentAddDialog = App.AppHost?.Services.GetRequiredService<PlaylistAddSongItemView>();
        PlaylistViewModel? playlistViewModel = App.AppHost?.Services.GetRequiredService<PlaylistViewModel>();
        MusicPlayerViewModel? musicPlayerViewModel = App.AppHost?.Services.GetRequiredService<MusicPlayerViewModel>();

        // Перевіряємо, чи відтворюються пісні з плейлисту
        if (musicPlayerViewModel!.IsPlayingFromPlaylist) return;

        // Перевіряємо наявність необхідних сервісів та даних
        if (CurrentSong == null || contentAddDialog?.viewModel == null || playlistViewModel?.OriginalPlaylists == null) return;

        if (string.IsNullOrEmpty(CurrentSong.SongID))
        {
            return;
        }

        // Відображаємо діалогове вікно та очікуємо результат
        ContentDialogResult result = await contentAddDialog!.ShowAsync();

        // Якщо користувач не підтвердив діалог, виходимо
        if (result != ContentDialogResult.Primary) return;

        List<PlaylistModel> playlistsSelected = contentAddDialog.viewModel.PlaylistsSelected;
        List<PlaylistModel> originalPlaylists = playlistViewModel.OriginalPlaylists;

        // Додаємо поточну пісню до кожного вибраного плейлисту
        foreach (PlaylistModel playlist in playlistsSelected)
        {
            int index = originalPlaylists.FindIndex(p => p.Title == playlist.Title && p.Duration == playlist.Duration);
            if (index != -1)
            {
                if (!originalPlaylists[index].SongsInPlaylist.Contains(CurrentSong))
                {
                    originalPlaylists[index].SongsInPlaylist.Add(CurrentSong);
                }
            }
        }

        // Оновлюємо дані та вигляд плейлисту
        if (playlistViewModel.IsInsidePlaylist)
        {
            playlistViewModel.Songs = null;
            playlistViewModel.Songs = playlistViewModel.CurrentPlaylistSelected?.SongsInPlaylist;
        }

        _settings.Save();

        playlistViewModel.CheckAmountOfItemsInPlaylist();
        playlistViewModel.RecalculatePlaylists();
    }

    // Метод для оновлення позиції музики
    public void CheckMediaPlayerPosition()
    {
        SongDuration = TimeConverter.ConvertMsToTime(_musicPlayer.MediaPlayer.Time);
        MusicPosition = _musicPlayer.MediaPlayer.Position;
        _musicPlayer.MediaPlayer.Volume = (int)VolumeValue;
        if (_musicPlayer.MediaPlayer.Position >= 0.99f)
        {
            if (_musicPlayer.IsRepeating)
            {
                RepeatSong();
                return;
            }
            NextSongCommand.Execute(null);
        }
    }

    // Метод для відтворення пісні
    private void PlaySongCommand_Execute()
    {
        _musicPlayer.OnPauseSong();
        PlayMediaIcon = _musicPlayer.IsPaused ? FontAwesomeIcon.Play : FontAwesomeIcon.Pause;
        if (_musicPlayer.IsPaused) _musicPositionUpdate.Stop();
        else _musicPositionUpdate.Start();
    }

    // Метод для повторення пісні
    private void RepeatSong()
    {
        SongDuration = "0:00";
        MusicPosition = 0;
        _musicPlayer.MediaPlayer.Position = 0;
    }

    // Метод для відтворення пісні
    public void PlaySong()
    {
        SongDuration = "0:00";
        MusicPosition = 0;
        _musicPositionUpdate.Stop();
        _musicPlayer.CancellationTokenSourcePlay.Cancel();
        _musicPlayer.CancelPlayingMusic();
        if (_musicPlayer.IsPaused)
        {
            _musicPlayer.OnPauseSong();
            PlayMediaIcon = FontAwesomeIcon.Pause;
        }
        _musicPlayer.MediaPlayer.Position = 0;
        _musicPlayer.OnPlaySong(CurrentSong);
        _musicPositionUpdate.Start();
    }

    // Метод для переходу до наступної пісні
    private void NextSongCommand_Execute()
    {
        PlaylistViewModel? model = App.AppHost?.Services.GetRequiredService<PlaylistViewModel>();
        List<SongModel>? songs = IsPlayingFromPlaylist ? model?.Songs : App.AppHost?.Services.GetRequiredService<SearchViewModel>()?.SongsList;

        if (songs == null)
        {
            return;
        }

        int currentIndex = songs.FindIndex(n => n == CurrentSong);
        if (currentIndex == -1)
        {
            GoToNextPlaylist(model);
            return;
        }

        int index = (currentIndex + 1) % songs.Count;

        if (IsPlayingFromPlaylist && index == 0)
        {
            GoToNextPlaylist(model);
            return;
        }

        int loopCount = 0;
        while (loopCount < songs.Count)
        {
            if (songs[index] != CurrentSong)
            {
                CurrentSong = songs[index];
                SongDuration = "0:00";
                MusicPosition = 0;
                _musicPlayer.MediaPlayer.Position = 0;

                if (IsPlayingFromPlaylist)
                    SongPlaylistChanged?.Invoke(this, CurrentSong);
                else
                    SongChanged?.Invoke(this, CurrentSong);

                return;
            }

            index = (index + 1) % songs.Count;
            loopCount++;
        }
    }

    // Метод для переходу до наступного плейлисту
    private void GoToNextPlaylist(PlaylistViewModel? model)
    {
        if (model != null)
        {
            int currentIndexPlaylist = model.Playlists!.IndexOf(model.CurrentPlaylistSelected!);
            int nextIndex = (currentIndexPlaylist + 1) % model.Playlists.Count;

            int loopCount = 0;
            while (loopCount < model.Playlists.Count)
            {
                if (model.Playlists[nextIndex].SongsInPlaylist.Count > 0)
                {
                    model.CurrentPlaylistSelected = null;
                    model.CurrentPlaylistSelected = model.Playlists[nextIndex];

                    if (IsPlayingFromPlaylist)
                        SongPlaylistChanged?.Invoke(this, CurrentSong);
                    else
                        SongChanged?.Invoke(this, CurrentSong);

                    return;
                }

                nextIndex = (nextIndex + 1) % model.Playlists.Count;
                loopCount++;
            }
        }
    }

    // Метод для переходу до попередньої пісні
    private void PreviousSongCommand_Execute()
    {
        PlaylistViewModel? model = App.AppHost?.Services.GetRequiredService<PlaylistViewModel>();
        List<SongModel>? songs = IsPlayingFromPlaylist ? model?.Songs : App.AppHost?.Services.GetRequiredService<SearchViewModel>()?.SongsList;

        if (songs == null)
        {
            return;
        }

        int currentIndex = songs.FindIndex(n => n == CurrentSong);
        if (currentIndex == -1)
        {
            GoToPreviousPlaylist(model);
            return;
        }
        int index = (currentIndex - 1 + songs.Count) % songs.Count;

        if (IsPlayingFromPlaylist && index == songs.Count - 1)
        {
            GoToPreviousPlaylist(model);
            return;
        }

        int loopCount = 0;
        while (loopCount < songs.Count)
        {
            if (songs[index] != CurrentSong)
            {
                CurrentSong = songs[index];
                SongDuration = "0:00";
                MusicPosition = 0;
                _musicPlayer.MediaPlayer.Position = 0;

                if (IsPlayingFromPlaylist)
                    SongPlaylistChanged?.Invoke(this, CurrentSong);
                else
                    SongChanged?.Invoke(this, CurrentSong);

                return;
            }

            index = (index - 1 + songs.Count) % songs.Count;
            loopCount++;
        }
    }

    // Метод для переходу до попереднього плейлисту
    private void GoToPreviousPlaylist(PlaylistViewModel? model)
    {
        if (model != null)
        {
            int currentIndexPlaylist = model.Playlists!.IndexOf(model.CurrentPlaylistSelected!);
            int previousIndex = (currentIndexPlaylist - 1 + model.Playlists.Count) % model.Playlists.Count;

            int loopCount = 0;
            while (loopCount < model.Playlists.Count)
            {
                if (model.Playlists[previousIndex].SongsInPlaylist.Count > 0)
                {
                    model.CurrentPlaylistSelected = null;
                    model.CurrentPlaylistSelected = model.Playlists[previousIndex];

                    if (IsPlayingFromPlaylist)
                        SongPlaylistChanged?.Invoke(this, CurrentSong);
                    else
                        SongChanged?.Invoke(this, CurrentSong);

                    return;
                }

                previousIndex = (previousIndex - 1 + model.Playlists.Count) % model.Playlists.Count;
                loopCount++;
            }
        }
    }

    // Метод для виконання команди повторення пісні
    private void RepeatSongCommand_Execute()
    {
        _musicPlayer.OnRepeatingSong();
        RepeatMediaBrush = _musicPlayer.IsRepeating ? new SolidColorBrush(Colors.Lime) : new SolidColorBrush(Colors.White);
    }

    // Прапорець для перемішування пісень
    private bool IsShuffling = false;

    // Метод для виконання команди перемішування пісень
    private void ShuffleSongCommand_Execute()
    {
        IsShuffling = !IsShuffling;
        if (!IsPlayingFromPlaylist)
        {
            SearchViewModel? searchViewModel = App.AppHost?.Services.GetRequiredService<SearchViewModel>();
            if (searchViewModel == null || searchViewModel.SongsList == null) return;
            ToggleShuffle(searchViewModel.SongsList);
            return;
        }
        PlaylistViewModel? playlistViewModel = App.AppHost?.Services.GetRequiredService<PlaylistViewModel>();
        if (playlistViewModel == null || playlistViewModel.OriginalPlaylists == null) return;
        for (int i = 0; i < playlistViewModel.OriginalPlaylists.Count; i++)
        {
            if (playlistViewModel.OriginalPlaylists[i].SongsInPlaylist.Count == 0) continue;
            ToggleShuffle(playlistViewModel.OriginalPlaylists[i].SongsInPlaylist);
        }
    }


    // Метод для перемикання режиму перемішування
    private void ToggleShuffle(List<SongModel> songs)
    {
        List<SongModel> temp = new(songs); // Create a copy of the list

        if (IsShuffling)
        {
            ShuffleList(temp);
            ShuffleMediaBrush = new SolidColorBrush(Colors.Lime);
        }
        else
        {
            temp.Sort((a, b) => a.Index.CompareTo(b.Index));
            ShuffleMediaBrush = new SolidColorBrush(Colors.White);
        }

        songs.Clear(); // Clear the original list
        songs.AddRange(temp); // Update the original list with the shuffled or unshuffled temp list
        UpdateViewList();
    }

    // Метод для перемішування списку пісень
    private static void ShuffleList(List<SongModel> list)
    {
        Random rand = new();
        for (int i = 0; i < list.Count * 2; i++)
        {
            int a = rand.Next(0, list.Count - 1);
            int b = rand.Next(0, list.Count - 1);
            (list[b], list[a]) = (list[a], list[b]);
        }
    }

    // Метод для оновлення відображення списку пісень
    private void UpdateViewList()
    {
        if (!IsPlayingFromPlaylist)
        {
            SearchViewModel? searchViewModel = App.AppHost?.Services.GetRequiredService<SearchViewModel>();
            searchViewModel?.UpdateViewList();
        }
        else
        {
            PlaylistViewModel? playlistViewModel = App.AppHost?.Services.GetRequiredService<PlaylistViewModel>();
            playlistViewModel?.UpdateViewSongList();
        }
    }

    public void CancelPlayingMusic()
    {
        _musicPlayer.MediaPlayer.Stop();
        _musicPositionUpdate.Stop();
        CurrentSong = new();
        MusicPosition = 0;
        SongDuration = "0:00";
    }

    // Метод для реєстрації слайдера музики
    public void RegisterMusicSlider(Slider slider)
    {
        // Event handlers for drag start, delta, and completion
        slider.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler((o, e) =>
        {
            if (!IsValidMediaPlayerState()) return;
            _musicPlayer.MediaPlayer.SetPause(true);
            _musicPositionUpdate.Stop();
        }));

        slider.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler((o, e) =>
        {
            if (!IsValidMediaPlayerState()) return;
            long progress = (long)(MusicPosition * _musicPlayer.MediaPlayer.Length);
            SongDuration = TimeConverter.ConvertMsToTime(progress);
        }));

        slider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((o, e) =>
        {
            if (!IsValidMediaPlayerState()) return;
            _musicPlayer.MediaPlayer.Position = (float)MusicPosition;
            CheckMediaPlayerPosition();
            _musicPlayer.MediaPlayer.SetPause(false);
            _musicPositionUpdate.Start();
        }));

        // Handle mouse left button down event for smoother dragging
        slider.AddHandler(System.Windows.UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler((o, e) =>
        {
            if (!IsValidMediaPlayerState() || !slider.IsMoveToPointEnabled) return;
            if (slider.Template.FindName("PART_Track", slider) is not Track track || track.Thumb == null || track.Thumb.IsMouseOver) return;

            track.Thumb.UpdateLayout();
            track.Thumb.RaiseEvent(new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left)
            {
                RoutedEvent = System.Windows.UIElement.MouseLeftButtonDownEvent,
                Source = track.Thumb
            });

            track.Thumb.RaiseEvent(new DragDeltaEventArgs(0, 0)
            {
                RoutedEvent = Thumb.DragDeltaEvent,
                Source = track.Thumb
            });
        }), true);
    }

    private bool IsValidMediaPlayerState()
    {
        return _musicPlayer.MediaPlayer.State != LibVLCSharp.Shared.VLCState.NothingSpecial &&
               _musicPlayer.MediaPlayer.State != LibVLCSharp.Shared.VLCState.Stopped &&
               _musicPlayer.MediaPlayer.State != LibVLCSharp.Shared.VLCState.Ended &&
               _musicPlayer.MediaPlayer.State != LibVLCSharp.Shared.VLCState.Error;
    }

    partial void OnVolumeValueChanging(double value)
    {
        _musicPlayer.MediaPlayer.Volume = (int)value;
        _settings.SettingsModel.Volume = (int)value;
        _settings.Save();
    }
}