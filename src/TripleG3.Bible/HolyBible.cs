using System.Text.Json.Serialization;

namespace TripleG3.Bible;

/// <summary>Represents a complete Bible translation.</summary>
public record Bible(string Version, string Name, string Language, string License, BibleBook[] Books)
{
    /// <summary>Gets an empty Bible value.</summary>
    public static Bible Empty { get; } = new(string.Empty, string.Empty, string.Empty, string.Empty, []);
}

/// <summary>Represents a Bible book and its chapters.</summary>
public record BibleBook(string Book, int BookId, string EnglishName, string Testament, Chapter[] Chapters)
{
    /// <summary>Gets an empty Bible book value.</summary>
    public static BibleBook Empty { get; } = new(string.Empty, 0, string.Empty, string.Empty, []);
}

/// <summary>Represents a chapter and its verses.</summary>
public record Chapter([property: JsonPropertyName("chapter")] int ChapterNumber, Verse[] Verses)
{
    /// <summary>Gets an empty chapter value.</summary>
    public static Chapter Empty { get; } = new(0, []);
}

/// <summary>Represents a numbered Bible verse.</summary>
public record Verse(int Number, string Text)
{
    /// <summary>Gets an empty verse value.</summary>
    public static Verse Empty { get; } = new(0, string.Empty);
}

/// <summary>Contains translation identity and aggregate Bible counts.</summary>
public record Metadata(string Version, string Name, string Language, string License, int BookCount, int ChapterCount, int VerseCount)
{
    /// <summary>Gets an empty metadata value.</summary>
    public static Metadata Empty { get; } = new(string.Empty, string.Empty, string.Empty, string.Empty, 0, 0, 0);
}