# XplatComics — Cross-Platform Comic Reader (MVP)

## Context

Build a cross-platform comic reader supporting **macOS, Windows (desktop)** and **iOS (mobile)** using **Avalonia UI**. The app reads **CBR** (RAR archive) and **CBZ** (ZIP archive) comic files. Starting from an empty directory with .NET 10.0.102 SDK installed.

**MVP Scope**: Library view + single-page reader with basic navigation and progress tracking. No RTL/manga support, no double-page mode, no zoom — those are future iterations.

---

## Tech Stack

| Component | Choice | Rationale |
|-----------|--------|-----------|
| UI Framework | Avalonia 11.3.x | Cross-platform, Skia rendering, iOS support |
| .NET | net10.0 | Matches installed SDK |
| MVVM | CommunityToolkit.Mvvm | Source generators, lightweight |
| CBZ extraction | System.IO.Compression (BCL) | Built-in |
| CBR extraction | SharpCompress ~0.46 | Pure C#, RAR4/RAR5 |
| DI | Microsoft.Extensions.DependencyInjection | Standard .NET DI |
| Persistence | System.Text.Json (BCL) | Simple JSON for library state |

---

## Solution Structure

```
xplat-comics/
  Directory.Packages.props
  XplatComics.sln
  XplatComics/                        # Shared core
    Models/
      ComicBook.cs                    # Library entry (path, title, progress)
      ComicPage.cs                    # Page metadata (index, entry key)
      ComicArchiveType.cs             # Enum: Cbz, Cbr
    Services/
      Archives/
        IComicArchive.cs              # Core abstraction
        CbzArchive.cs                 # ZIP impl (System.IO.Compression)
        CbrArchive.cs                 # RAR impl (SharpCompress)
        ComicArchiveFactory.cs        # Extension -> correct impl
      INavigationService.cs
      NavigationService.cs
      IPageCacheService.cs            # LRU bitmap cache with preloading
      PageCacheService.cs
      ILibraryStore.cs
      JsonLibraryStore.cs
      IThumbnailService.cs            # Cover thumbnail generation
      ThumbnailService.cs
      ServiceLocator.cs
    ViewModels/
      ViewModelBase.cs
      MainViewModel.cs                # Holds CurrentPage for navigation
      LibraryViewModel.cs             # Library grid, file picker
      ComicBookViewModel.cs           # Per-comic wrapper (cover, progress)
      ReaderViewModel.cs              # Page nav, HUD, progress
    Views/
      MainWindow.axaml/.cs
      MainView.axaml/.cs              # ContentControl bound to CurrentPage
      LibraryView.axaml/.cs           # Cover grid
      ReaderView.axaml/.cs            # Single-page reader with tap zones
    App.axaml/.cs
    ViewLocator.cs
  XplatComics.Desktop/
    Program.cs, app.manifest
  XplatComics.iOS/
    AppDelegate.cs, Main.cs, Info.plist
```

---

## Implementation Phases

### Phase 1: Project Scaffolding

1. Install Avalonia templates: `dotnet new install Avalonia.Templates`
2. Scaffold: `dotnet new avalonia.xplat -n XplatComics`
3. Delete generated Android and Browser projects
4. Remove their references from .sln and Directory.Packages.props
5. Add NuGet packages: `SharpCompress`, `Microsoft.Extensions.DependencyInjection`
6. Set up DI container in `App.axaml.cs`
7. Wire navigation: `MainViewModel.CurrentPage` + `ViewLocator`
8. Verify build and launch on desktop

### Phase 2: Core Comic Engine

**`IComicArchive`** — central interface:
```csharp
public interface IComicArchive : IDisposable
{
    int PageCount { get; }
    IReadOnlyList<ComicPage> Pages { get; }
    Task<Stream> GetPageStreamAsync(int pageIndex, CancellationToken ct = default);
    Task<Stream> GetCoverStreamAsync(CancellationToken ct = default);
}
```

- `CbzArchive`: `ZipArchive` → filter image entries → sort alphabetically
- `CbrArchive`: `SharpCompress.ArchiveFactory` → same filter/sort
- Both accept `Stream` constructor for iOS sandbox compatibility
- `ComicArchiveFactory.Open(path)` dispatches by extension

**`PageCacheService`**: `ConcurrentDictionary<int, Bitmap>`, LRU eviction, `Bitmap.DecodeToHeight()`, preloads ±2 pages

**`JsonLibraryStore`**: `List<ComicBook>` → JSON file in app data directory

### Phase 3: Library View

- **LibraryViewModel**: `ObservableCollection<ComicBookViewModel>`, open/remove commands
- **File picker**: `StorageProvider.OpenFilePickerAsync()` with .cbz/.cbr filter
- **Cover grid**: `WrapPanel` + `ScrollViewer` with thumbnails (300px height)
- **ThumbnailService**: Extract first page, cache to disk as `{id}.jpg`
- **Progress bar**: Per-comic reading progress indicator

### Phase 4: Single-Page Reader

- **Page display**: `Image` control bound to `CurrentPageBitmap`
- **Tap zones**: Left third = previous page, right third = next page, center = toggle HUD
- **HUD overlay**: Top bar (title, back, page counter), bottom bar (page slider) — toggled on center tap
- **Page slider**: `Slider` control to scrub to any page
- **Progress persistence**: Save `LastReadPage` on each page turn
- **Keyboard shortcuts** (desktop): Left/Right arrows, Space, Escape
- **Fit-to-screen**: Images scaled with `Stretch="Uniform"` to fit viewport

### Phase 5: Basic Platform Polish

- **Desktop**: Keyboard shortcuts (arrows, space, escape), basic `NativeMenu` (File > Open)
- **iOS**: Safe area handling, swipe gesture for back navigation
- **Performance**: `DecodeToHeight()` for all bitmaps, explicit `Dispose()` on eviction, `SemaphoreSlim` for thread-safe archive access

---

## Key Architectural Decisions

1. **ViewModel-first navigation** via `MainViewModel.CurrentPage` + `ViewLocator` — no routing framework
2. **Static factory** for archives — disposable per-file resources, not DI-managed
3. **JSON persistence** — sufficient for hundreds of comics, no SQLite dependency
4. **Stream-based archive constructors** — required for iOS sandbox
5. **Simple `Image` control** for MVP reader (no custom zoom control yet)

---

## Future Iterations (Post-MVP)

- Zoom and pan (custom `ZoomableImage` control with pinch + scroll wheel)
- Double-page spread mode
- RTL / manga reading direction
- Thumbnail strip in reader
- Fullscreen toggle
- iOS "Open In" document type registration
- Search/filter in library

---

## Verification Plan

1. `dotnet build` — all 3 projects compile cleanly
2. `dotnet run --project XplatComics.Desktop` — app launches, shows library view
3. Open a .cbz file via file picker — cover appears in library grid
4. Open a .cbr file — same behavior
5. Tap a comic — reader opens at page 1 (or last-read page)
6. Tap right/left zones — pages advance/go back correctly
7. Use page slider — jumps to correct page
8. Close reader, reopen same comic — resumes at last-read page
9. Close and relaunch app — library persists, progress preserved
