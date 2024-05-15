using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace SoundScapes.Views;

/// <summary>
/// Клас представлення для відображення пошуку.
/// </summary>
public partial class SearchView : UserControl
{
    /// <summary>
    /// Конструктор класу SearchView.
    /// </summary>
    public SearchView()
    {
        // Отримуємо сервіс SearchViewModel з контейнера служб
        SearchViewModel? searchViewModel = App.AppHost?.Services.GetRequiredService<SearchViewModel>();

        // Встановлюємо контекст даних для відображення
        DataContext = searchViewModel;

        // Ініціалізуємо компоненти відображення
        InitializeComponent();

        // Реєструємо пошукове поле в моделі представлення
        searchViewModel?.RegisterSearchBox(SearchBox);
    }
}
