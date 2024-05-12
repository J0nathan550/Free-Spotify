using Microsoft.Extensions.DependencyInjection;
using SoundScapes.ViewModels;

namespace SoundScapes.Views;

public partial class SearchView : System.Windows.Controls.UserControl
{
    public SearchView()
    {
        SearchViewModel? searchViewModel = App.AppHost?.Services.GetRequiredService<SearchViewModel>();
        DataContext = searchViewModel;
        InitializeComponent();
        searchViewModel?.RegisterSearchBox(SearchBox);
    }
}