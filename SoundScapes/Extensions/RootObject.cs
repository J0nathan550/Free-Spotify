using System.Windows.Controls;
using System.Windows.Markup;
using System.Xaml;

namespace SoundScapes.Extensions;

/// <summary>
/// Розширення для отримання кореневого об'єкта. Зроблено для контексного меню.
/// </summary>
[MarkupExtensionReturnType(typeof(ContentControl))]
public class RootObject : MarkupExtension
{
    /// <summary>
    /// Повертає кореневий об'єкт.
    /// </summary>
    /// <param name="serviceProvider">Постачальник служб.</param>
    /// <returns>Кореневий об'єкт.</returns>
    public override object? ProvideValue(IServiceProvider serviceProvider)
    {
        // Отримання постачальника кореневого об'єкта.
        IRootObjectProvider? rootObjectProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;

        // Повернення кореневого об'єкта.
        return rootObjectProvider?.RootObject;
    }
}