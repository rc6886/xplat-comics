# XplatComics — Cross-Platform Comic Reader (MVP)

## Context

Build a cross-platform comic reader supporting **macOS, Windows (desktop)** and **iOS (mobile)** using **Avalonia UI**. The app reads **CBR** (RAR archive) and **CBZ** (ZIP archive) comic files.

**MVP Scope**: Library view + single-page reader with basic navigation and progress tracking. No RTL/manga support, no double-page mode, no zoom — those are future iterations.

---

## Tech Stack

| Component | Choice | Version | Rationale |
|-----------|--------|---------|-----------|
| UI Framework | Avalonia | 11.3.12 | Cross-platform, Skia rendering, iOS support |
| .NET | net10.0 | 10.0.102 | Matches installed SDK |
| MVVM | CommunityToolkit.Mvvm | 8.4.0 | Source generators, lightweight |
| CBZ extraction | System.IO.Compression | BCL | Built-in |
| CBR extraction | SharpCompress | 0.46.2 | Pure C#, RAR4/RAR5 |
| DI | Microsoft.Extensions.DependencyInjection | 10.0.3 | Standard .NET DI |
| Persistence | System.Text.Json | BCL | Simple JSON for library state |

---

## Implementation Phases

### Phase 1: Project Scaffolding — COMPLETED

- [x] Install Avalonia templates (`dotnet new install Avalonia.Templates`)
- [x] Scaffold cross-platform project (`dotnet new avalonia.xplat -n XplatComics`)
- [x] Delete Android and Browser projects, remove from .sln
- [x] Clean up Directory.Packages.props (remove Android/Browser packages)
- [x] Add NuGet packages: SharpCompress 0.46.2, Microsoft.Extensions.DependencyInjection 10.0.3
- [x] Create models: `ComicBook`, `ComicPage`, `ComicArchiveType`
- [x] Create service interfaces and implementations:
  - `IComicArchive` / `CbzArchive` / `CbrArchive` / `ComicArchiveFactory`
  - `INavigationService` / `NavigationService`
  - `ILibraryStore` / `JsonLibraryStore`
  - `IPageCacheService` / `PageCacheService` (LRU, 10-page cache)
  - `IThumbnailService` / `ThumbnailService`
  - `ServiceLocator`
- [x] Set up DI container in `App.axaml.cs`
- [x] Wire ViewModel-first navigation: `MainViewModel.CurrentPage` + `ViewLocator`
- [x] Create placeholder `LibraryView` and `ReaderView`
- [x] Verify build (0 warnings, 0 errors) and desktop launch

**Note**: iOS workload not yet installed (`dotnet workload install ios` needed to build iOS target).

---

### Phase 2: Core Comic Engine — TODO

Flesh out the archive and caching implementations built in Phase 1:

- [ ] Add `SemaphoreSlim` to archive classes for thread-safe stream access
- [ ] Implement `Bitmap.DecodeToHeight()` in `PageCacheService` for memory efficiency
- [ ] Add ±2 page preloading to `PageCacheService`
- [ ] Test CBZ extraction with real .cbz files
- [ ] Test CBR extraction with real .cbr files
- [ ] Verify `JsonLibraryStore` round-trip (save/load)

---

### Phase 3: Library View — TODO

- [ ] `LibraryViewModel`: `ObservableCollection<ComicBookViewModel>`, open/remove commands
- [ ] File picker: `StorageProvider.OpenFilePickerAsync()` with .cbz/.cbr filter
- [ ] Cover grid: `WrapPanel` + `ScrollViewer` with thumbnails (300px height)
- [ ] `ThumbnailService`: Extract first page, cache to disk as `{id}.jpg`
- [ ] Progress bar: Per-comic reading progress indicator
- [ ] Wire "Open Comic" to add book to library and display cover

---

### Phase 4: Single-Page Reader — TODO

- [ ] Page display: `Image` control bound to `CurrentPageBitmap`
- [ ] Tap zones: Left third = previous, right third = next, center = toggle HUD
- [ ] HUD overlay: Top bar (title, back, page counter), bottom bar (page slider)
- [ ] Page slider: `Slider` control to scrub to any page
- [ ] Progress persistence: Save `LastReadPage` on each page turn
- [ ] Keyboard shortcuts (desktop): Left/Right arrows, Space, Escape
- [ ] Fit-to-screen: Images scaled with `Stretch="Uniform"`

---

### Phase 5: Basic Platform Polish — TODO

- [ ] Desktop: Keyboard shortcuts (arrows, space, escape), basic `NativeMenu` (File > Open)
- [ ] iOS: Safe area handling, swipe gesture for back navigation
- [ ] Performance: `DecodeToHeight()` for all bitmaps, explicit `Dispose()` on eviction
- [ ] `SemaphoreSlim` for thread-safe archive access
- [ ] Final verification against all 9 test cases

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
