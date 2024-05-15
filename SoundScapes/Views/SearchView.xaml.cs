using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;
using System.Windows.Controls;

namespace SoundScapes.Views;

public partial class SearchView : UserControl
{
    public SearchView()
    {
        SearchViewModel? searchViewModel = App.AppHost?.Services.GetRequiredService<SearchViewModel>();
        DataContext = searchViewModel;
        InitializeComponent();
        searchViewModel?.RegisterSearchBox(SearchBox);
    }
}