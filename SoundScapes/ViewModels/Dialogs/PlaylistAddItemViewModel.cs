using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

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
    private IAsyncRelayCommand _selectImageCommand;

    public PlaylistAddItemViewModel()
    {
        SelectImageCommand = new AsyncRelayCommand(SelectImageCommand_Execute);
    }

    private async Task SelectImageCommand_Execute()
    {
        OpenFileDialog openFileDialog = new()
        {
            Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
        };

        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            string filePath = openFileDialog.FileName;
            if (!await IsImageAsync(filePath)) return;
            Icon = filePath;
        }
    }

    partial void OnTitleChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            IsPrimaryButtonEnabled = false;
            return;
        }
        IsPrimaryButtonEnabled = true;
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

        Task<bool> isValidImageTask = IsImageAsync(value);

        isValidImageTask.ContinueWith(task =>
        {
            bool isValidImage = task.Result;
            if (!isValidImage) LoadDefaultImage();
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private static async Task<bool> IsImageAsync(string filePath)
    {
        try
        {
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            await Task.Run(() => Image.FromStream(fs));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void LoadDefaultImage() => Icon = "pack://application:,,,/SoundScapes;component/Assets/SoundScapesIcon.ico";
}