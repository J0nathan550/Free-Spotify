using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;

namespace SoundScapes.ViewModels;

public partial class PlaylistAddItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;
    [ObservableProperty]
    private string _icon = "pack://application:,,,/SoundScapes;component/Assets/SoundScapesIcon.ico";
    [ObservableProperty]
    private bool _isPrimaryButtonEnabled = false;
    [ObservableProperty]
    private RelayCommand _selectImageCommand;

    public PlaylistAddItemViewModel() => SelectImageCommand = new RelayCommand(SelectImageCommand_Execute);

    private void SelectImageCommand_Execute()
    {
        OpenFileDialog openFileDialog = new()
        {
            Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        };

        if (openFileDialog.ShowDialog() == true)
        {
            string filePath = openFileDialog.FileName;
            if (!IsImageAsync(filePath)) return;
            Icon = filePath;
        }
    }

    public void RegisterTitleTextBox(System.Windows.Controls.TextBox textBox)
    {
        textBox.TextChanged += (o, e) =>
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                IsPrimaryButtonEnabled = false;
                return;
            }
            IsPrimaryButtonEnabled = true;
        };
    }

    partial void OnIconChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            LoadDefaultImage();
            return;
        }

        if (!File.Exists(value))
        {
            LoadDefaultImage();
            return;
        }

        if (!IsImageAsync(value)) LoadDefaultImage();
    }

    private static bool IsImageAsync(string filePath)
    {
        try
        {
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            try
            {
                BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void LoadDefaultImage() => Icon = "pack://application:,,,/SoundScapes;component/Assets/SoundScapesIcon.ico";
}