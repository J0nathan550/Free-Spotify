using System.Windows.Controls;
using System.Windows.Markup;
using System.Xaml;

namespace SoundScapes.Extensions;

[MarkupExtensionReturnType(typeof(ContentControl))]
public class RootObject : MarkupExtension
{
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        IRootObjectProvider? rootObjectProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
        return rootObjectProvider?.RootObject;
    }
}