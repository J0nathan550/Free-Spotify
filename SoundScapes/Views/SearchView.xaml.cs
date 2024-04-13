using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;

namespace SoundScapes.Views;

public partial class SearchView : System.Windows.Controls.UserControl
{
    public SearchView()
    {
        var searchViewModel = App.AppHost?.Services.GetService<SearchViewModel>();
        DataContext = searchViewModel;
        InitializeComponent();
        searchViewModel?.RegisterSearchBox(SearchBox);
    }
}