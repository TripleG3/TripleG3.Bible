# Repository instructions

- 2026-07-25: Keep Bible JSON loading lazy and copy `kjv.json` to build/publish output so `BibleLoader.KJV` can load it at runtime.
- 2026-07-25: Expose Bible queries through `BibleLoader` and return `VerseReference` for verse results that need book and chapter context.
- 2026-07-25: Bible query methods must not throw or return null; unmatched scalar queries return empty model records and collection queries return empty sequences.
- 2026-07-25: Metadata queries expose Bible identity and book/testament chapter and verse counts through `BibleLoader`.
- 2026-07-25: Expose metadata through the single public `BibleLoader.GetMetadata` query returning `Metadata`; keep component count helpers private.
- 2026-07-25: NuGet publishing targets `src/TripleG3.Bible/TripleG3.Bible.csproj`; this repository has no test project, so the publish workflow restores, builds, packs, and publishes the library directly.
- 2026-07-25: Unit tests live in `tests/TripleG3.Bible.Tests` and the NuGet workflow tests that project before packing.
- 2026-07-25: Keep `README.md` aligned with the public `BibleLoader` API, NuGet installation, development commands, and publishing workflow.
- 2026-07-25: Range queries use standard zero-based C# `Range` semantics and return empty collections for invalid or unmatched ranges.
- 2026-07-25: The NuGet package includes the repository `README.md`, `LICENSE`, and generated XML API documentation through project package metadata.
