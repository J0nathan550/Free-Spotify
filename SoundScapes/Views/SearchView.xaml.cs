using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;
using System.Windows.Controls;

namespace SoundScapes.Views;

public partial class SearchView : UserControl
{
    public SearchView()
    {
        var searchViewModel = App.AppHost?.Services.GetService<SearchViewModel>();
        DataContext = searchViewModel;
        InitializeComponent();
        searchViewModel?.RegisterSearchBox(SearchBox);
    }
}