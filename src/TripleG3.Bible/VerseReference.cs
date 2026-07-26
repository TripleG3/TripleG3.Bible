namespace TripleG3.Bible;

/// <summary>Associates a verse with its containing book and chapter.</summary>
public record VerseReference(BibleBook Book, Chapter Chapter, Verse Verse)
{
	/// <summary>Gets an empty verse reference value.</summary>
	public static VerseReference Empty { get; } = new(BibleBook.Empty, Chapter.Empty, Verse.Empty);
}