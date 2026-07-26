using Xunit;

namespace TripleG3.Bible.Tests;

public sealed class BibleLoaderTests
{
    [Fact]
    public void GetBooksWithoutTestamentReturnsAllBooks()
    {
        var books = BibleLoader.GetBooks();

        Assert.Equal(BibleLoader.KJV.Books, books);
    }

    [Fact]
    public void GetBooksFiltersByTestamentCaseInsensitively()
    {
        var uppercaseBooks = BibleLoader.GetBooks("OT");
        var lowercaseBooks = BibleLoader.GetBooks("ot");

        Assert.NotEmpty(uppercaseBooks);
        Assert.Equal(uppercaseBooks, lowercaseBooks);
        Assert.All(uppercaseBooks, book => Assert.Equal("OT", book.Testament));
    }

    [Fact]
    public void GetBooksWithUnknownTestamentReturnsEmpty()
    {
        Assert.Empty(BibleLoader.GetBooks("Unknown"));
    }

    [Fact]
    public void GetMetadataReturnsBibleCounts()
    {
        var metadata = BibleLoader.GetMetadata();

        Assert.Equal(BibleLoader.KJV.Version, metadata.Version);
        Assert.Equal(BibleLoader.KJV.Name, metadata.Name);
        Assert.Equal(BibleLoader.KJV.Language, metadata.Language);
        Assert.Equal(BibleLoader.KJV.License, metadata.License);
        Assert.Equal(BibleLoader.KJV.Books.Length, metadata.BookCount);
        Assert.Equal(BibleLoader.KJV.Books.Sum(book => book.Chapters.Length), metadata.ChapterCount);
        Assert.Equal(
            BibleLoader.KJV.Books.SelectMany(book => book.Chapters).Sum(chapter => chapter.Verses.Length),
            metadata.VerseCount);
    }

    [Fact]
    public void GetBookChapterAndVerseReturnEmptyForUnknownValues()
    {
        Assert.Equal(BibleBook.Empty, BibleLoader.GetBook("Unknown"));
        Assert.Equal(Chapter.Empty, BibleLoader.GetChapter("Unknown", 1));
        Assert.Equal(Verse.Empty, BibleLoader.GetVerse("Unknown", 1, 1));
    }

    [Fact]
    public void SearchReturnsMatchingVerseReferences()
    {
        var results = BibleLoader.Search("In the beginning").ToArray();

        Assert.NotEmpty(results);
        Assert.Contains(results, result => result.Book.Book == "Gen" && result.Chapter.ChapterNumber == 1 && result.Verse.Number == 1);
    }

    [Fact]
    public void GetChaptersReturnsRequestedRange()
    {
        var chapters = BibleLoader.GetChapters("Gen", 0..3).ToArray();

        Assert.Equal([1, 2, 3], chapters.Select(chapter => chapter.ChapterNumber));
    }

    [Fact]
    public void GetVersesReturnsRequestedRange()
    {
        var verses = BibleLoader.GetVerses("John", 3, 15..18).ToArray();

        Assert.Equal([16, 17, 18], verses.Select(reference => reference.Verse.Number));
        Assert.All(verses, reference => Assert.Equal("John", reference.Book.EnglishName));
        Assert.All(verses, reference => Assert.Equal(3, reference.Chapter.ChapterNumber));
    }

    [Fact]
    public void RangeQueriesReturnEmptyForUnknownValues()
    {
        Assert.Empty(BibleLoader.GetChapters("Unknown", 0..3));
        Assert.Empty(BibleLoader.GetVerses("John", 999, 0..3));
    }
}
