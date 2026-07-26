using System.Text.Json;

namespace TripleG3.Bible;

/// <summary>Provides lazy access to and query operations over the bundled KJV translation.</summary>
public static class BibleLoader
{
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true
    };
    
    private static readonly Lazy<Bible> KingJamesVersion = new(LoadKingJamesVersion, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>Gets the lazily loaded King James Version Bible.</summary>
    public static Bible KJV => KingJamesVersion.Value;

    /// <summary>Gets translation metadata and counts, optionally limited to a testament.</summary>
    /// <param name="testament">The testament code, such as <c>OT</c> or <c>NT</c>; blank returns all books.</param>
    /// <returns>The matching metadata.</returns>
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

    /// <summary>Gets books, optionally filtered by testament.</summary>
    /// <param name="testament">The testament code, such as <c>OT</c> or <c>NT</c>.</param>
    /// <returns>A read-only list of matching books, or an empty list when no testament matches.</returns>
    public static IReadOnlyList<BibleBook> GetBooks(string? testament = null)
    {
        var books = KJV.Books.AsEnumerable();
        return string.IsNullOrWhiteSpace(testament)
            ? [.. books]
            : [.. books.Where(book => string.Equals(book.Testament, testament, StringComparison.OrdinalIgnoreCase))];
    }

    /// <summary>Gets the number of verses in a book.</summary>
    /// <param name="book">The book abbreviation or English name.</param>
    /// <returns>The verse count, or zero when the book is not found.</returns>
    public static int GetVerseCount(string book)
    {
        return GetBook(book).Chapters.Sum(chapter => chapter.Verses.Length);
    }

    /// <summary>Gets the chapter numbers in a book.</summary>
    /// <param name="book">The book abbreviation or English name.</param>
    /// <returns>The chapter numbers, or an empty list when the book is not found.</returns>
    public static IReadOnlyList<int> GetChapterNumbers(string book)
    {
        return [.. GetBook(book).Chapters.Select(chapter => chapter.ChapterNumber)];
    }

    /// <summary>Gets the abbreviations for all books in canonical order.</summary>
    /// <returns>A read-only list of book abbreviations.</returns>
    public static IReadOnlyList<string> GetAllBookTitles()
    {
        var books = KJV.Books.Select(book => book.Book);
        return [.. books];
    }

    /// <summary>Finds a book by abbreviation or English name.</summary>
    /// <param name="book">The book abbreviation or English name.</param>
    /// <returns>The matching book, or <see cref="BibleBook.Empty"/> when not found.</returns>
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

    /// <summary>Finds a chapter in a book.</summary>
    /// <param name="book">The book abbreviation or English name.</param>
    /// <param name="chapterNumber">The one-based chapter number.</param>
    /// <returns>The matching chapter, or <see cref="Chapter.Empty"/> when not found.</returns>
    public static Chapter GetChapter(string book, int chapterNumber)
    {
        if (chapterNumber < 1)
        {
            return Chapter.Empty;
        }

        return GetBook(book).Chapters.FirstOrDefault(chapter => chapter.ChapterNumber == chapterNumber)
            ?? Chapter.Empty;
    }

    /// <summary>Finds a verse in a chapter.</summary>
    /// <param name="book">The book abbreviation or English name.</param>
    /// <param name="chapterNumber">The one-based chapter number.</param>
    /// <param name="verseNumber">The one-based verse number.</param>
    /// <returns>The matching verse, or <see cref="Verse.Empty"/> when not found.</returns>
    public static Verse GetVerse(string book, int chapterNumber, int verseNumber)
    {
        if (chapterNumber < 1 || verseNumber < 1)
        {
            return Verse.Empty;
        }

        return GetChapter(book, chapterNumber).Verses.FirstOrDefault(verse => verse.Number == verseNumber)
            ?? Verse.Empty;
    }

    /// <summary>Searches verse text and returns matching verse references.</summary>
    /// <param name="text">The text to find.</param>
    /// <param name="comparison">The string comparison used for matching.</param>
    /// <returns>Matching verse references, or an empty sequence for blank or unmatched text.</returns>
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

    /// <summary>Gets all verses in a chapter with their containing context.</summary>
    /// <param name="book">The book abbreviation or English name.</param>
    /// <param name="chapterNumber">The one-based chapter number.</param>
    /// <returns>The chapter's verse references, or an empty sequence when not found.</returns>
    public static IEnumerable<VerseReference> GetVerses(string book, int chapterNumber)
    {
        var bibleBook = GetBook(book);
        return bibleBook.Chapters.FirstOrDefault(chapter => chapter.ChapterNumber == chapterNumber) is { } chapter
            ? chapter.Verses.Select(verse => new VerseReference(bibleBook, chapter, verse))
            : [];
    }

    /// <summary>Gets a range of chapters from a book.</summary>
    /// <param name="book">The book abbreviation or English name.</param>
    /// <param name="chapterRange">A zero-based C# range over the book's chapters.</param>
    /// <returns>The selected chapters, or an empty sequence for an invalid range or book.</returns>
    public static IEnumerable<Chapter> GetChapters(string book, Range chapterRange)
    {
        var bibleBook = GetBook(book);
        var chapters = bibleBook.Chapters;
        if (!TryGetOffsetAndLength(chapterRange, chapters.Length, out var start, out var count))
        {
            return [];
        }

        return chapters.Skip(start).Take(count);
    }

    /// <summary>Gets a range of verses from a chapter with their containing context.</summary>
    /// <param name="book">The book abbreviation or English name.</param>
    /// <param name="chapterNumber">The one-based chapter number.</param>
    /// <param name="verseRange">A zero-based C# range over the chapter's verses.</param>
    /// <returns>The selected verse references, or an empty sequence for an invalid range or chapter.</returns>
    public static IEnumerable<VerseReference> GetVerses(string book, int chapterNumber, Range verseRange)
    {
        var bibleBook = GetBook(book);
        var chapter = bibleBook.Chapters.FirstOrDefault(candidate => candidate.ChapterNumber == chapterNumber);
        if (chapter is null)
        {
            return [];
        }

        if (!TryGetOffsetAndLength(verseRange, chapter.Verses.Length, out var start, out var count))
        {
            return [];
        }

        return chapter.Verses
            .Skip(start)
            .Take(count)
            .Select(verse => new VerseReference(bibleBook, chapter, verse));
    }

    /// <summary>Gets the verse immediately after the specified verse.</summary>
    /// <returns>The next verse reference, or <see cref="VerseReference.Empty"/> at the end or when not found.</returns>
    public static VerseReference GetNextVerse(string book, int chapterNumber, int verseNumber)
    {
        var references = GetAllVerses().ToArray();
        var index = Array.FindIndex(references, reference =>
            string.Equals(reference.Book.Book, book, StringComparison.OrdinalIgnoreCase) &&
            reference.Chapter.ChapterNumber == chapterNumber && reference.Verse.Number == verseNumber);

        return index >= 0 && index + 1 < references.Length ? references[index + 1] : VerseReference.Empty;
    }

    /// <summary>Gets the verse immediately before the specified verse.</summary>
    /// <returns>The previous verse reference, or <see cref="VerseReference.Empty"/> at the beginning or when not found.</returns>
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

    private static bool TryGetOffsetAndLength(Range range, int collectionLength, out int offset, out int length)
    {
        try
        {
            (offset, length) = range.GetOffsetAndLength(collectionLength);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            offset = 0;
            length = 0;
            return false;
        }
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
