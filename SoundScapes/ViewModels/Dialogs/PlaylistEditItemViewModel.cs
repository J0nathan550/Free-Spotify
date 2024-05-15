using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SoundScapes.ViewModels;

/// <summary>
/// ViewModel для редагування елемента списку відтворення.
/// </summary>
public partial class PlaylistEditItemViewModel : ObservableObject
{
    /// <summary>
    /// Заголовок елемента списку відтворення.
    /// </summary>
    [ObservableProperty]
    private string _title = string.Empty;

    /// <summary>
    /// Іконка елемента списку відтворення.
    /// </summary>
    [ObservableProperty]
    private string _icon = "pack://application:,,,/SoundScapes;component/Assets/SoundScapesIcon.ico";

    /// <summary>
    /// Прапорець, що показує, чи є кнопка первинної дії увімкненою.
    /// </summary>
    [ObservableProperty]
    private bool _isPrimaryButtonEnabled = false;

    /// <summary>
    /// Команда вибору зображення.
    /// </summary>
    [ObservableProperty]
    private RelayCommand _selectImageCommand;

    /// <summary>
    /// Ініціалізує новий екземпляр класу PlaylistEditItemViewModel.
    /// </summary>
    public PlaylistEditItemViewModel() => SelectImageCommand = new RelayCommand(SelectImageCommand_Execute);

    /// <summary>
    /// Обробник виконання команди вибору зображення.
    /// </summary>
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

    /// <summary>
    /// Реєструє текстове поле для введення заголовка.
    /// </summary>
    /// <param name="textBox">Текстове поле заголовка.</param>
    public void RegisterTitleTextBox(TextBox textBox)
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

    /// <summary>
    /// Перевірка на зміни зображення
    /// </summary>
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

    /// <summary>
    /// Перевіряє, чи є файл зображення дійсним.
    /// </summary>
    /// <param name="filePath">Шлях до файлу.</param>
    /// <returns><c>true</c>, якщо файл є зображенням; інакше - <c>false</c>.</returns>
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

    /// <summary>
    /// Завантажує значок за замовчуванням.
    /// </summary>
    private void LoadDefaultImage() => Icon = "pack://application:,,,/SoundScapes;component/Assets/SoundScapesIcon.ico";
}
