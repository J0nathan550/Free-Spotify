using Free_Spotify.Pages;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Free_Spotify.Classes
{
    public static class Playlist_Holder
    {
        public static List<Playlist> Playlists { get; private set; } = new List<Playlist>();

        public static void CreatePlaylist(string name, string image)
        {
            Playlists.Add(new Playlist() { PlaylistName = name, PlaylistImage = image });
            // Create the Border
            Border mainBorder = new Border
            {
                Name = $"i{Playlists.Count - 1}",
                Background = new SolidColorBrush(Color.FromRgb(35, 35, 35)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(5),
                Margin = new Thickness(0,0,0,5),
            };

            Playlists[Playlists.Count - 1].MainBorder = mainBorder;

            // Create the main Grid
            Grid mainGrid = new Grid
            {
                Height = 40
            };

            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });

            // Create the inner Border with Image
            Border imageBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(48, 48, 48)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(5)
            };

            Image imageVisual = new Image
            {
                Source = new BitmapImage(new Uri(image))
            };

            imageBorder.Child = imageVisual;

            // Create the inner Grid
            Grid innerGrid = new Grid
            {
                Margin = new Thickness(5, 0, 0, 0),
            };
            Grid.SetColumn(innerGrid, 1);

            innerGrid.RowDefinitions.Add(new RowDefinition());
            innerGrid.RowDefinitions.Add(new RowDefinition());

            Label label1 = new Label
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = new Thickness(0),
                FontSize = 10,
                FontWeight = FontWeights.Medium,
                Foreground = Brushes.White,
                Content = name
            };

            Label label2 = new Label
            {
                VerticalContentAlignment = VerticalAlignment.Top,
                Padding = new Thickness(0),
                FontSize = 10,
                FontWeight = FontWeights.Medium,
                Foreground = Brushes.Gray,
                Content = "Количество треков: 0"
            };
            Grid.SetRow(label2, 1);

            innerGrid.Children.Add(label1);
            innerGrid.Children.Add(label2);

            // Add the inner elements to the main Grid
            mainGrid.Children.Add(imageBorder);
            mainGrid.Children.Add(innerGrid);

            // Add the main Grid to the Border
            mainBorder.Child = mainGrid;

            MainScreenPage.instance.playListLoader.Children.Add(mainBorder);
        }
        public static void CreatePlaylist(string name)
        {
            Playlists.Add(new Playlist() { PlaylistName = name, PlaylistImage = "" });
            // Create the Border
            Border mainBorder = new Border
            {
                Name = $"i{Playlists.Count - 1}",
                Background = new SolidColorBrush(Color.FromRgb(35, 35, 35)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(5),
                Margin = new Thickness(0, 0, 0, 5)
            };

            Playlists[Playlists.Count - 1].MainBorder = mainBorder;

            mainBorder.MouseDown += (sender, e) =>
            {
                MainScreenPage.instance.PlaylistPage();
            };

            // Create the main Grid
            Grid mainGrid = new Grid
            {
                Height = 40
            };

            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });

            // Create the inner Border with Image
            Border imageBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(48, 48, 48)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(5)
            };

            Image imageVisual = new Image
            {
                Source = new BitmapImage(new Uri("/Assets/Spotify_icon.png", UriKind.Relative))
            };

            imageBorder.Child = imageVisual;

            // Create the inner Grid
            Grid innerGrid = new Grid
            {
                Margin = new Thickness(5, 0, 0, 0),
            };
            Grid.SetColumn(innerGrid, 1);

            innerGrid.RowDefinitions.Add(new RowDefinition());
            innerGrid.RowDefinitions.Add(new RowDefinition());

            Label label1 = new Label
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = new Thickness(0),
                FontSize = 10,
                FontWeight = FontWeights.Medium,
                Foreground = Brushes.White,
                Content = name
            };

            Label label2 = new Label
            {
                VerticalContentAlignment = VerticalAlignment.Top,
                Padding = new Thickness(0),
                FontSize = 10,
                FontWeight = FontWeights.Medium,
                Foreground = Brushes.Gray,
                Content = "Количество треков: 0"
            };
            Grid.SetRow(label2, 1);

            innerGrid.Children.Add(label1);
            innerGrid.Children.Add(label2);

            // Add the inner elements to the main Grid
            mainGrid.Children.Add(imageBorder);
            mainGrid.Children.Add(innerGrid);

            // Add the main Grid to the Border
            mainBorder.Child = mainGrid;

            MainScreenPage.instance.playListLoader.Children.Add(mainBorder);
        }
        public static void RemovePlaylist(Border playlistBorderID)
        {
            int index = int.Parse(playlistBorderID.Name.Substring(1));
            MainScreenPage.instance.playListLoader.Children.RemoveAt(index);
            Playlists.RemoveAt(index);
            UpdatePlaylist();
        }
        public static void UpdatePlaylist()
        {
            int indexRearanger = 0; 
            foreach (var item in Playlists)
            {
                item.MainBorder.Name = $"i{indexRearanger}";
                indexRearanger++;
            }
        }
        public static int GetLastPlaylistID()
        {
            return Playlists.Count + 1;
        }
    }
}