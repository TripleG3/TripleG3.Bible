using System.Text.Json.Serialization;

namespace TripleG3.Bible;

public record Bible(string Version, string Name, string Language, string License, BibleBook[] Books)
{
    public static Bible Empty { get; } = new(string.Empty, string.Empty, string.Empty, string.Empty, []);
}

public record BibleBook(string Book, int BookId, string EnglishName, string Testament, Chapter[] Chapters)
{
    public static BibleBook Empty { get; } = new(string.Empty, 0, string.Empty, string.Empty, []);
}

public record Chapter([property: JsonPropertyName("chapter")] int ChapterNumber, Verse[] Verses)
{
    public static Chapter Empty { get; } = new(0, []);
}

public record Verse(int Number, string Text)
{
    public static Verse Empty { get; } = new(0, string.Empty);
}

public record Metadata(string Version, string Name, string Language, string License, int BookCount, int ChapterCount, int VerseCount)
{
    public static Metadata Empty { get; } = new(string.Empty, string.Empty, string.Empty, string.Empty, 0, 0, 0);
}