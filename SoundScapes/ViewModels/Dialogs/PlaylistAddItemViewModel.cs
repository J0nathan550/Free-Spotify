using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;

namespace SoundScapes.ViewModels;

/// <summary>
/// ViewModel для додавання плейлисту.
/// </summary>
public partial class PlaylistAddItemViewModel : ObservableObject
{
    // Назва плейлисту.
    [ObservableProperty]
    private string _title = string.Empty;

    // Іконка плейлисту.
    [ObservableProperty]
    private string _icon = "pack://application:,,,/SoundScapes;component/Assets/SoundScapesIcon.ico";

    // Прапорець, що вказує, чи є вмикана основна кнопка.
    [ObservableProperty]
    private bool _isPrimaryButtonEnabled = false;

    // Команда вибору зображення.
    [ObservableProperty]
    private RelayCommand _selectImageCommand;

    /// <summary>
    /// Конструктор класу PlaylistAddItemViewModel.
    /// </summary>
    public PlaylistAddItemViewModel() => SelectImageCommand = new RelayCommand(SelectImageCommand_Execute);

    /// <summary>
    /// Метод виконання команди вибору зображення.
    /// </summary>
    private void SelectImageCommand_Execute()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog()
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

    /// <summary>
    /// Реєстрація текстового поля для введення назви плейлисту.
    /// </summary>
    /// <param name="textBox">Текстове поле для реєстрації.</param>
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

    // Метод, який викликається при зміні значення іконки.
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

    // Метод для перевірки, чи є файл зображенням.
    private static bool IsImageAsync(string filePath)
    {
        try
        {
            using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
            BitmapDecoder.Create(fs, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Метод для завантаження за замовчуванням іконки.
    private void LoadDefaultImage() => Icon = "pack://application:,,,/SoundScapes;component/Assets/SoundScapesIcon.ico";
}