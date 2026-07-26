using System.Text.Json;

namespace TripleG3.Bible;

public static class BibleLoader
{
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    private static readonly Lazy<Bible> KingJamesVersion = new(LoadKingJamesVersion, LazyThreadSafetyMode.ExecutionAndPublication);

    public static Bible KJV => KingJamesVersion.Value;

    public static Metadata GetMetadata(string? testament = null)
    {
        var books = GetBooks(testament);
        return new Metadata(
            KJV.Version,
            KJV.Name,
            KJV.Language,
            KJV.License,
            GetBookCount(books),
            GetChapterCount(books),
            GetVerseCount(books));
    }

    public static IReadOnlyList<BibleBook> GetBooks(string? testament = null)
    {
        var books = KJV.Books.AsEnumerable();
        return string.IsNullOrWhiteSpace(testament)
            ? [.. books]
            : [.. books.Where(book => string.Equals(book.Testament, testament, StringComparison.OrdinalIgnoreCase))];
    }

    public static int GetVerseCount(string book)
    {
        return GetBook(book).Chapters.Sum(chapter => chapter.Verses.Length);
    }

    public static IReadOnlyList<int> GetChapterNumbers(string book)
    {
        return [.. GetBook(book).Chapters.Select(chapter => chapter.ChapterNumber)];
    }

    public static IReadOnlyList<string> GetAllBookTitles()
    {
        var books = KJV.Books.Select(book => book.Book);
        return [.. books];
    }

    public static BibleBook GetBook(string book)
    {
        if (string.IsNullOrWhiteSpace(book))
        {
            return BibleBook.Empty;
        }

        return KJV.Books.FirstOrDefault(candidate =>
            string.Equals(candidate.Book, book, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(candidate.EnglishName, book, StringComparison.OrdinalIgnoreCase))
            ?? BibleBook.Empty;
    }

    public static Chapter GetChapter(string book, int chapterNumber)
    {
        if (chapterNumber < 1)
        {
            return Chapter.Empty;
        }

        return GetBook(book).Chapters.FirstOrDefault(chapter => chapter.ChapterNumber == chapterNumber)
            ?? Chapter.Empty;
    }

    public static Verse GetVerse(string book, int chapterNumber, int verseNumber)
    {
        if (chapterNumber < 1 || verseNumber < 1)
        {
            return Verse.Empty;
        }

        return GetChapter(book, chapterNumber).Verses.FirstOrDefault(verse => verse.Number == verseNumber)
            ?? Verse.Empty;
    }

    public static IEnumerable<VerseReference> Search(string text, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return KJV.Books.SelectMany(book => book.Chapters.SelectMany(chapter => chapter.Verses
            .Where(verse => verse.Text.Contains(text, comparison))
            .Select(verse => new VerseReference(book, chapter, verse))));
    }

    public static IEnumerable<VerseReference> GetVerses(string book, int chapterNumber)
    {
        var bibleBook = GetBook(book);
        return bibleBook.Chapters.FirstOrDefault(chapter => chapter.ChapterNumber == chapterNumber) is { } chapter
            ? chapter.Verses.Select(verse => new VerseReference(bibleBook, chapter, verse))
            : [];
    }

    public static VerseReference GetNextVerse(string book, int chapterNumber, int verseNumber)
    {
        var references = GetAllVerses().ToArray();
        var index = Array.FindIndex(references, reference =>
            string.Equals(reference.Book.Book, book, StringComparison.OrdinalIgnoreCase) &&
            reference.Chapter.ChapterNumber == chapterNumber && reference.Verse.Number == verseNumber);

        return index >= 0 && index + 1 < references.Length ? references[index + 1] : VerseReference.Empty;
    }

    public static VerseReference GetPreviousVerse(string book, int chapterNumber, int verseNumber)
    {
        var references = GetAllVerses().ToArray();
        var index = Array.FindIndex(references, reference =>
            string.Equals(reference.Book.Book, book, StringComparison.OrdinalIgnoreCase) &&
            reference.Chapter.ChapterNumber == chapterNumber && reference.Verse.Number == verseNumber);

        return index > 0 ? references[index - 1] : VerseReference.Empty;
    }

    private static int GetBookCount(IReadOnlyCollection<BibleBook> books)
    {
        return books.Count;
    }

    private static int GetChapterCount(IReadOnlyCollection<BibleBook> books)
    {
        return books.Sum(book => book.Chapters.Length);
    }

    private static int GetVerseCount(IReadOnlyCollection<BibleBook> books)
    {
        return books.SelectMany(book => book.Chapters).Sum(chapter => chapter.Verses.Length);
    }

    private static IEnumerable<VerseReference> GetAllVerses()
    {
        return KJV.Books.SelectMany(book => book.Chapters.SelectMany(chapter => chapter.Verses
            .Select(verse => new VerseReference(book, chapter, verse))));
    }

    private static Bible LoadKingJamesVersion()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "kjv.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Bible>(json, options) ?? throw new InvalidDataException($"The Bible data in '{path}' is empty or invalid.");
    }
}
