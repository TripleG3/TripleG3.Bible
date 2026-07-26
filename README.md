# TripleG3.Bible

A .NET library for querying the public-domain **King James Version (KJV)** Bible.

The project is based on the Bible dataset from [midvash/bible-data](https://github.com/midvash/bible-data).

## Features

- Lazy loading of the bundled `kjv.json` data file.
- Strongly typed records for the Bible, books, chapters, verses, metadata, and verse references.
- Case-insensitive book lookup by abbreviation or English name.
- Filtering by Old Testament (`OT`) or New Testament (`NT`).
- Verse text search.
- Previous and next verse navigation.
- Non-throwing query methods that return empty models or collections when no match exists.

## Installation

Install the NuGet package:

```bash
dotnet add package TripleG3.Bible
```

The package targets .NET 10.

## Usage

Import the namespace and access the KJV through `BibleLoader`:

```csharp
using TripleG3.Bible;

Bible bible = BibleLoader.KJV;
```

The Bible data is loaded on the first access to `BibleLoader.KJV` and then reused for subsequent queries.

### Metadata

```csharp
Metadata metadata = BibleLoader.GetMetadata();

Console.WriteLine($"{metadata.Name} ({metadata.Version})");
Console.WriteLine($"Books: {metadata.BookCount}");
Console.WriteLine($"Chapters: {metadata.ChapterCount}");
Console.WriteLine($"Verses: {metadata.VerseCount}");
```

Metadata can be limited to one testament:

```csharp
Metadata oldTestament = BibleLoader.GetMetadata("OT");
Metadata newTestament = BibleLoader.GetMetadata("NT");
```

### Books and chapters

```csharp
IReadOnlyList<BibleBook> books = BibleLoader.GetBooks();
IReadOnlyList<BibleBook> oldTestamentBooks = BibleLoader.GetBooks("OT");
IReadOnlyList<string> titles = BibleLoader.GetAllBookTitles();

BibleBook genesis = BibleLoader.GetBook("Genesis");
BibleBook genesisByAbbreviation = BibleLoader.GetBook("Gen");
IReadOnlyList<int> chapterNumbers = BibleLoader.GetChapterNumbers("Gen");
```

Testament filtering is case-insensitive. Passing a missing or unknown testament returns an empty collection, while a blank testament returns all books.

### Chapters and verses

```csharp
Chapter chapter = BibleLoader.GetChapter("John", 3);
Verse verse = BibleLoader.GetVerse("John", 3, 16);
IEnumerable<Chapter> chapters = BibleLoader.GetChapters("John", 0..3);
IEnumerable<VerseReference> verses = BibleLoader.GetVerses("John", 3);
IEnumerable<VerseReference> verseRange = BibleLoader.GetVerses("John", 3, 15..18);
```

The range overloads use standard zero-based C# `Range` semantics. The examples above select the first three chapters and verses 16 through 18, respectively.

A `VerseReference` includes the matching `BibleBook`, `Chapter`, and `Verse`:

```csharp
foreach (VerseReference reference in verses)
{
    Console.WriteLine($"{reference.Book.EnglishName} " +
        $"{reference.Chapter.ChapterNumber}:{reference.Verse.Number} " +
        reference.Verse.Text);
}
```

### Search

```csharp
IEnumerable<VerseReference> results = BibleLoader.Search("In the beginning");

foreach (VerseReference result in results)
{
    Console.WriteLine($"{result.Book.Book} " +
        $"{result.Chapter.ChapterNumber}:{result.Verse.Number} - " +
        result.Verse.Text);
}
```

Search is case-insensitive by default. A different `StringComparison` can be supplied:

```csharp
IEnumerable<VerseReference> results = BibleLoader.Search(
    "faith",
    StringComparison.Ordinal);
```

### Verse navigation

```csharp
VerseReference next = BibleLoader.GetNextVerse("John", 3, 16);
VerseReference previous = BibleLoader.GetPreviousVerse("John", 3, 16);
```

When a book, chapter, verse, or navigation target cannot be found, the query returns the associated static `Empty` value, such as `Verse.Empty` or `VerseReference.Empty`. Collection queries return an empty collection.

## Development

Build the solution:

```bash
dotnet build TripleG3.Bible.slnx
```

Run the unit tests:

```bash
dotnet test TripleG3.Bible.slnx
```

The NuGet package is published by `.github/workflows/nuget-publish.yml` when changes are pushed to `main`. The workflow builds and tests the project, creates a versioned package, and publishes it to nuget.org using the `NUGET_API_KEY` repository secret.

## License

The KJV Bible data is identified as public domain in the bundled dataset. See `LICENSE` for the repository license.
