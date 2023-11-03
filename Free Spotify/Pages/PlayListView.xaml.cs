using Free_Spotify.Classes;
using Free_Spotify.Dialogs;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using XamlAnimatedGif;

namespace Free_Spotify.Pages
{
    public partial class PlayListView : Page
    {
        private static PlayListView? instance;

        /// <summary>
        /// Constructor for PlayListView. Initializes the component and creates playlists.
        /// </summary>
        public PlayListView()
        {
            InitializeComponent();
            yourPlaylist.Text = Settings.GetLocalizationString("YourPlaylistText");
            CreatePlaylists();
            Instance = this;
        }

        // for renaming selected playlist.
        public PlayListBriefView? playListBriefView;
        public int currentSelectedPlaylist;

        // Property to get or set the instance of PlayListView.
        public static PlayListView? Instance { get => instance; set => instance = value; }

        /// <summary>
        /// Creates and displays the playlists on the user interface.
        /// </summary>
        public void CreatePlaylists()
        {
            // Clears the playlist rendering area
            // If there are no playlists, displays a message
            // Otherwise, iterates through playlists, creates UI elements, and handles interactions.

            playlistRender.Children.Clear();
            if (Settings.SettingsData.playlists.Count == 0)
            {
                TextBlock textBlock = new()
                {
                    TextAlignment = TextAlignment.Center,
                    FontSize = 14,
                    Style = (Style)FindResource("fontMontserrat"),
                    FontWeight = FontWeights.Medium,
                    Text = Settings.GetLocalizationString("NoPlaylistDefaultText"),
                    Foreground = new SolidColorBrush(Colors.Gray),
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                RenderOptions.SetBitmapScalingMode(textBlock, BitmapScalingMode.Fant);
                playlistRender.Children.Add(textBlock);
                return;
            }
            int index = 0;
            foreach (var playlist in Settings.SettingsData.playlists)
            {
                Border border = new()
                {
                    Style = (Style)FindResource("PlaylistItem"),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new CornerRadius(6),
                    Background = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                    Margin = new Thickness(0, 0, 0, 10),
                    Name = $"i{index}" // for interaction when you click on playlist
                };
                RenderOptions.SetBitmapScalingMode(border, BitmapScalingMode.Fant);

                border.MouseLeftButtonDown += (o, e) =>
                {
                    if (MainWindow.Window != null)
                    {
                        if (playListBriefView != null)
                        {
                            playListBriefView = null;
                        }
                        int indexSelection = int.Parse(border.Name[1..]);
                        playListBriefView = new(indexSelection);
                        currentSelectedPlaylist = indexSelection;
                        MainWindow.Window.LoadingPagesFrame.Content = null;
                        MainWindow.Window.LoadingPagesFrame.NavigationService.Navigate(null);
                        MainWindow.Window.LoadingPagesFrame.NavigationService.RemoveBackEntry();
                        MainWindow.Window.LoadingPagesFrame.Navigate(playListBriefView);
                    }
                };

                // Create a ContextMenu
                ContextMenu contextMenu = new()
                {
                    Background = new SolidColorBrush(Colors.Black),
                    Foreground = new SolidColorBrush(Colors.White)
                };

                MenuItem renameMenuItem = new() { Header = Settings.GetLocalizationString("RenameDefaultText"), Background = new SolidColorBrush(Colors.Black), Foreground = new SolidColorBrush(Colors.White), BorderThickness = new Thickness(0) };
                MenuItem moveUpMenuItem = new() { Header = Settings.GetLocalizationString("MoveUpDefaultText"), Background = new SolidColorBrush(Colors.Black), Foreground = new SolidColorBrush(Colors.White), BorderThickness = new Thickness(0) };
                MenuItem moveDownMenuItem = new() { Header = Settings.GetLocalizationString("MoveDownDefaultText"), Background = new SolidColorBrush(Colors.Black), Foreground = new SolidColorBrush(Colors.White), BorderThickness = new Thickness(0) };
                MenuItem deleteMenuItem = new() { Header = Settings.GetLocalizationString("DeleteDefaultText"), Background = new SolidColorBrush(Colors.Black), Foreground = new SolidColorBrush(Colors.White), BorderThickness = new Thickness(0) };
                deleteMenuItem.Click += (o, e) =>
                {
                    PlayListRemoveDialog dialogRemove = new(PlayListRemoveDialog.TypeRemove.Playlist);
                    if (dialogRemove.ShowDialog() == true)
                    {
                        int index = int.Parse(border.Name[1..]);
                        Settings.SettingsData.playlists.RemoveAt(index);
                        Settings.SaveSettings();
                        CreatePlaylists();
                        if (MainWindow.Window == null)
                        {
                            return;
                        }
                        MainWindow.Window.LoadingPagesFrame.Content = null;
                        if (SearchViewPage.SearchWindow != null)
                        {
                            SearchViewPage.SearchWindow = null;
                            SearchViewPage.SearchWindow = new();
                        }
                        MusicPlayerPage.Instance?.ClearMusic();
                        MainWindow.Window.LoadingPagesFrame.Content = null;
                        MainWindow.Window.LoadingPagesFrame.NavigationService.Navigate(null);
                        MainWindow.Window.LoadingPagesFrame.NavigationService.RemoveBackEntry();
                        MainWindow.Window.LoadingPagesFrame.Navigate(SearchViewPage.SearchWindow);
                    }
                    return;
                };
                renameMenuItem.Click += (o, e) =>
                {
                    int index = int.Parse(border.Name[1..]);
                    PlayListRenameDialog renameDialog = new(index);
                    if (renameDialog.ShowDialog() == true)
                    {
                        CreatePlaylists();
                    }
                    return;
                };
                moveUpMenuItem.Click += (o, e) =>
                {
                    int index = int.Parse(border.Name[1..]);
                    int oldIndex = index;
                    index--;
                    if (index < 0)
                    {
                        index = Settings.SettingsData.playlists.Count - 1;
                        var item = Settings.SettingsData.playlists[oldIndex];
                        Settings.SettingsData.playlists.RemoveAt(oldIndex);
                        Settings.SettingsData.playlists.Insert(index, item);
                        Settings.SaveSettings();
                        CreatePlaylists();
                        return;
                    }
                    var item1 = Settings.SettingsData.playlists[oldIndex];
                    Settings.SettingsData.playlists.RemoveAt(oldIndex);
                    Settings.SettingsData.playlists.Insert(index, item1);
                    Settings.SaveSettings();
                    CreatePlaylists();
                    return;
                };
                moveDownMenuItem.Click += (o, e) =>
                {
                    int index = int.Parse(border.Name[1..]);
                    int oldIndex = index;
                    index++;
                    if (index >= Settings.SettingsData.playlists.Count)
                    {
                        index = 0;
                        var item = Settings.SettingsData.playlists[oldIndex];
                        Settings.SettingsData.playlists.RemoveAt(oldIndex);
                        Settings.SettingsData.playlists.Insert(index, item);
                        Settings.SaveSettings();
                        CreatePlaylists();
                        return;
                    }
                    var item1 = Settings.SettingsData.playlists[oldIndex];
                    Settings.SettingsData.playlists.RemoveAt(oldIndex);
                    Settings.SettingsData.playlists.Insert(index, item1);
                    Settings.SaveSettings();
                    CreatePlaylists();
                    return;
                };
                contextMenu.Items.Add(renameMenuItem);
                contextMenu.Items.Add(moveUpMenuItem);
                contextMenu.Items.Add(moveDownMenuItem);
                contextMenu.Items.Add(deleteMenuItem);
                border.ContextMenu = contextMenu;

                // Create a Grid
                Grid grid = new();
                RenderOptions.SetBitmapScalingMode(grid, BitmapScalingMode.Fant);

                // Define column definitions
                ColumnDefinition column1 = new() { Width = new GridLength(70) };
                ColumnDefinition column2 = new();
                grid.ColumnDefinitions.Add(column1);
                grid.ColumnDefinitions.Add(column2);

                Image actualImage = new();
                RenderOptions.SetBitmapScalingMode(actualImage, BitmapScalingMode.Fant);
                try
                {

                    Uri uri = new(Settings.SettingsData.playlists[index].ImagePath, UriKind.RelativeOrAbsolute);

                    // Create an Image
                    Image image = new()
                    {
                        Margin = new Thickness(5),
                        Stretch = Stretch.Uniform
                    };

                    AnimationBehavior.SetSourceUri(image, uri);
                    AnimationBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
                    actualImage = image;
                }
                catch
                {
                    Uri uri = new(Utils.DefaultImagePath);

                    // Create an Image
                    Image image = new()
                    {
                        Margin = new Thickness(5),
                        Stretch = Stretch.Uniform
                    };

                    AnimationBehavior.SetSourceUri(image, uri);
                    AnimationBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
                    actualImage = image;
                }

                // Create a nested Grid for labels
                Grid labelsGrid = new();

                // Define row definitions
                RowDefinition labelRow1 = new();
                RowDefinition labelRow2 = new();
                labelsGrid.RowDefinitions.Add(labelRow1);
                labelsGrid.RowDefinitions.Add(labelRow2);

                // Create the first Label
                Label titleLabel = new()
                {
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Colors.White),
                    Style = (Style)FindResource("fontLabelMontserrat"),
                    FontWeight = FontWeights.Medium,
                    VerticalContentAlignment = VerticalAlignment.Bottom,
                    HorizontalContentAlignment = HorizontalAlignment.Left
                };

                TextBlock titleTextBlock = new()
                {
                    Text = playlist.Title,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                // Create the second Label
                Label infoLabel = new()
                {
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Colors.White),
                    Style = (Style)FindResource("fontLabelMontserrat"),
                    FontWeight = FontWeights.Medium,
                    VerticalContentAlignment = VerticalAlignment.Top,
                    HorizontalContentAlignment = HorizontalAlignment.Left
                };

                TextBlock infoTextBlock = new()
                {
                    Text = $"{Settings.GetLocalizationString("AmountTracksDefaultText")} {playlist.TracksInPlaylist.Count} | {playlist.CalculateAmountOfTimeToListenWholePlayList()}",
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                Grid.SetRow(titleLabel, 0);
                Grid.SetRow(infoLabel, 1);

                titleLabel.Content = titleTextBlock;
                infoLabel.Content = infoTextBlock;

                labelsGrid.Children.Add(titleLabel);
                labelsGrid.Children.Add(infoLabel);

                Grid.SetColumn(actualImage, 0);
                Grid.SetColumn(labelsGrid, 1);

                grid.Children.Add(actualImage);
                grid.Children.Add(labelsGrid);

                border.Child = grid;

                playlistRender.Children.Add(border);
                index++;
            }
        }

        /// <summary>
        /// Opens a dialog to create a new playlist with title and optional image.
        /// </summary>
        private void AddPlayListItemIcon_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Opens a dialog for adding a new playlist and refreshes the playlists if one is created.
            PlayListAddDialog dlg = new();
            if (dlg.ShowDialog() == true)
            {
                CreatePlaylists();
            }
        }
    }
}