namespace TripleG3.Bible;

public record VerseReference(BibleBook Book, Chapter Chapter, Verse Verse)
{
	public static VerseReference Empty { get; } = new(BibleBook.Empty, Chapter.Empty, Verse.Empty);
}