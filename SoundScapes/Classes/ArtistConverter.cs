using SpotifyExplode.Artists;

namespace SoundScapes.Classes;

/// <summary>
/// Клас конвертера виконавців.
/// </summary>
public class ArtistConverter
{
    /// <summary>
    /// Метод форматування списку виконавців у рядок.
    /// </summary>
    /// <param name="artists">Список виконавців.</param>
    /// <returns>Рядок з виконавцями.</returns>
    public static string FormatArtists(List<Artist> artists)
    {
        // Перевірка на наявність виконавців.
        if (artists == null || artists.Count == 0) return string.Empty;
        // Якщо є лише один виконавець, повертаємо його ім'я.
        if (artists.Count == 1) return artists[0].Name;
        // Форматуємо імена всіх виконавців та об'єднуємо їх через кому.
        string formattedNames = string.Join(", ", artists.Select(artist => artist.Name));
        return $"{formattedNames}";
    }
}
