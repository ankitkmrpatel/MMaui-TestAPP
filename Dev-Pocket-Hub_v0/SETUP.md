# DevPocketCMS — Setup Guide

## Prerequisites

| Tool | Version |
|------|---------|
| Visual Studio 2022 | 17.8+ (Community or higher) |
| .NET SDK | 8.0 |
| MAUI Workload | Latest |
| Android / iOS emulator | or physical device |

### Install the MAUI workload (if not already done)
```bash
dotnet workload install maui
```

---

## Getting Started

1. **Open the solution**
   ```
   DevPocketCMS/DevPocketCMS.sln
   ```

2. **Restore NuGet packages**
   Visual Studio does this automatically on first open, or run:
   ```bash
   dotnet restore
   ```

3. **Add Inter fonts** *(optional but recommended)*
   Download from https://fonts.google.com/specimen/Inter
   Copy these files to `DevPocketCMS/Resources/Fonts/`:
   - `Inter-Regular.ttf`
   - `Inter-Medium.ttf`
   - `Inter-SemiBold.ttf`
   - `Inter-Bold.ttf`

   Without fonts the app still works — it falls back to system fonts.

4. **Add tab bar images** *(optional)*
   Add small 24×24 PNG icons to `DevPocketCMS/Resources/Images/`:
   - `feed.png`
   - `lists.png`
   - `search.png`
   - `dashboard.png`
   - `settings.png`

   Without them MAUI will use default placeholders.

5. **Select a target**
   - Android emulator / device
   - iOS simulator (macOS only)
   - Mac Catalyst

6. **Run / Debug**
   Press **F5** or click the green play button in Visual Studio.

---

## Project Structure

```
DevPocketCMS/
├── DevPocketCMS.sln
│
├── DevPocketCMS.Core/              ← Entities, Interfaces, Models (no deps)
│   ├── Entities/
│   │   ├── Article.cs
│   │   ├── ArticleList.cs
│   │   ├── ArticleTag.cs          ← junction table
│   │   ├── Tag.cs
│   │   └── UserPreferences.cs
│   ├── Interfaces/                 ← repository + service contracts
│   └── Models/                    ← SearchOptions, DashboardStats, LinkPreviewData
│
├── DevPocketCMS.Infrastructure/    ← SQLite, HTTP, notifications
│   ├── Data/AppDatabase.cs         ← SQLiteAsyncConnection singleton + seeding
│   ├── Repositories/               ← concrete repo implementations
│   ├── Services/
│   │   ├── LinkPreviewService.cs   ← fetches OG tags with HtmlAgilityPack
│   │   ├── PocketModeService.cs    ← saves full HTML offline
│   │   └── NotificationService.cs ← Plugin.LocalNotification daily reminders
│   └── DependencyInjection.cs
│
├── DevPocketCMS.Application/       ← business logic, DTOs
│   ├── DTOs/
│   ├── Services/
│   │   ├── ArticleService.cs       ← save/read/delete articles + preview + pocket
│   │   ├── DashboardService.cs     ← stats aggregation
│   │   ├── ListService.cs
│   │   ├── SearchService.cs        ← AND/OR multi-field search
│   │   └── TagService.cs
│   └── DependencyInjection.cs
│
└── DevPocketCMS/                   ← MAUI app project (pure MauiReactor — no XAML)
    ├── MauiProgram.cs              ← DI setup, DB init, MauiReactor entry
    ├── AppRoot.cs                  ← root component: onboarding vs main shell
    ├── UI/
    │   ├── Theme/AppColors.cs      ← single source of truth for colours
    │   ├── Components/             ← ArticleCard, StatCard, EmptyState, FilterChip
    │   └── Pages/
    │       ├── OnboardingPage.cs   ← 5-step first-launch wizard
    │       ├── MainShell.cs        ← Shell with 5 tabs
    │       ├── HomePage.cs         ← feed + list filter + FAB
    │       ├── AddLinkSheet.cs     ← bottom sheet: save link + options
    │       ├── ListsPage.cs        ← CRUD for lists
    │       ├── SearchPage.cs       ← AND/OR advanced search
    │       ├── DashboardPage.cs    ← stats, progress bars, tag cloud
    │       └── SettingsPage.cs     ← theme, notifications, profile
    └── Platforms/                  ← Android, iOS, MacCatalyst, Windows
```

---

## Features

| # | Feature | Where |
|---|---------|-------|
| 1 | Save links with metadata | `AddLinkSheet.cs` + `ArticleService.cs` |
| 2 | Auto-fetch preview (OG title/desc/image) | `LinkPreviewService.cs` (HtmlAgilityPack) |
| 3 | Pocket mode (save HTML offline) | `PocketModeService.cs` |
| 4 | Multiple Lists | `ListsPage.cs` + `ListService.cs` |
| 5 | Multiple Tags | Tag chips in `AddLinkSheet.cs` + `TagService.cs` |
| 6 | Advanced Search (AND / OR) | `SearchPage.cs` + `SearchService.cs` |
| 7 | Dashboard with statistics | `DashboardPage.cs` + `DashboardService.cs` |
| 8 | Local notifications (daily unread reminder) | `NotificationService.cs` + `SettingsPage.cs` |
| 9 | Onboarding (first launch only) | `OnboardingPage.cs` |
| 10 | User preferences (theme, user type) | `SettingsPage.cs` + `UserPreferences.cs` |

---

## Key NuGet Packages

| Package | Purpose |
|---------|---------|
| `Reactor.Maui` | MauiReactor — pure C# reactive UI |
| `Reactor.Maui.Scaffold` | Code-generation helpers for MauiReactor |
| `sqlite-net-pcl` | Lightweight SQLite ORM |
| `SQLitePCLRaw.bundle_green` | Native SQLite bindings |
| `HtmlAgilityPack` | HTML parsing for link previews |
| `Plugin.LocalNotification` | Cross-platform local push notifications |

---

## Hot Reload

MauiReactor supports hot reload during debug builds:
```csharp
#if DEBUG
.EnableMauiReactorHotReload()
#endif
```
Changes to component `Render()` methods apply instantly without restarting the app.
