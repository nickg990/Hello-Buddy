# Canine Physio App - Developer README

## Design Token System Documentation

This document describes the design token architecture for the Canine Physio App ("Hello Buddy"). All UI elements should consume tokens rather than raw values to ensure consistency and maintainability.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Token Taxonomy](#token-taxonomy)
3. [Responsive Strategy](#responsive-strategy)
4. [How to Use Tokens](#how-to-use-tokens)
5. [Adding New Tokens](#adding-new-tokens)
6. [Control Styles](#control-styles)
7. [Components](#components)
8. [Helpers](#helpers)
9. [Core Principles](#core-principles)
10. [iOS-Safe Rules](#ios-safe-rules)

---

## Architecture Overview

### Folder Structure

```
Canine Physio App/
├── Theming/
│   ├── Colors.xaml       # Raw color tokens
│   ├── Typography.xaml   # Font families, sizes, text styles
│   ├── Spacing.xaml      # Margin/padding scale
│   ├── Radii.xaml        # Corner radius scale
│   ├── Sizing.xaml       # Control heights, tap targets, icon sizes
│   └── Controls.xaml     # Global control styles (consumes all above)
├── Helpers/
│   └── CharacterSpacingHelper.cs  # Shared text spacing calculations
├── Components/
│   ├── Controls/         # Reusable controls (LogoDisc, LoadingSpinner, ExerciseTile)
│   ├── Decor/            # Decorative elements (SineWaveBlock)
│   ├── Header/           # Header components (HeaderBlock)
│   └── Layout/           # Layout templates (AppPageTemplate)
├── App.xaml              # Merges all theme dictionaries
└── ...
```

### Merge Order (Critical!)

Dictionaries are merged in `App.xaml` in this order:

1. **Colors.xaml** - Must be first (other tokens reference colors)
2. **Typography.xaml** - References colors for text
3. **Spacing.xaml** - Independent scale values
4. **Radii.xaml** - Independent scale values
5. **Sizing.xaml** - Independent scale values
6. **Controls.xaml** - Must be LAST (references all tokens)

> ⚠️ **Do not change the merge order** or you will get missing resource exceptions.

---

## Token Taxonomy

### Colors

The Hello Buddy palette is intentionally minimal: 5 brand colors + 8 utility tokens. **Light theme only** (dark mode deferred).

#### Brand Colors (fixed — never change with theme)

| Token | Value | Purpose |
|-------|-------|---------|
| `BrandPrimary` | #6392AE | Primary brand teal |
| `BrandPrimaryLight` | #B3CDD6 | Light variant for backgrounds, hover |
| `BrandPrimaryDark` | #28404F | Dark variant for emphasis |
| `BrandMist` | #D4E0E5 | Soft muted background |
| `White` | #FFFFFF | Pure white for text-on-primary |

#### Utility Tokens (Light Theme Only)

| Token | Value | Purpose |
|-------|-------|---------|
| `PageBackground` | #D4E0E5 | Root page background (BrandMist) |
| `Surface` | #FFFFFF | Cards, inputs, containers |
| `TextPrimary` | #111111 | Body text (near-black for max contrast) |
| `TextMuted` | #5A7A8A | Secondary text, captions (grey-blue) |
| `BorderDefault` | #B3CDD6 | Strokes, dividers (BrandPrimaryLight) |
| `DisabledSurface` | #E0E0E0 | Disabled control bg |
| `DisabledText` | #9E9E9E | Disabled control text |
| `Error` | #DC3545 | Validation errors |
| `Success` | #4CAF50 | Completion/success states |
| `TextOnPrimary` | #FFFFFF | White text on coloured backgrounds |

#### Alias Tokens

| Token | Value | Purpose |
|-------|-------|---------|
| `Shadow` | #4028404F | BrandPrimaryDark @ 25% alpha (for drop shadows) |
| `LogoGold` | #F5D4A8 | Gold accent matching dog logo body |
| `OverlayLight` | #80FFFFFF | Semi-transparent white for loading overlays |
| `ErrorOnMist` | #D97706 | Error text on BrandMist background (warm amber) |

> **Note:** Dark theme tokens are not implemented in Section 4.1. All styles use `StaticResource` directly.

### Typography

| Token | Phone | Tablet | Usage |
|-------|-------|--------|-------|
| `TextXs` | 11 | 12 | Captions, fine print |
| `TextSm` | 13 | 14 | Secondary text |
| `TextMd` | 15 | 16 | Body text (default) |
| `TextLg` | 17 | 18 | Emphasized body |
| `TextXl` | 19 | 21 | Large body |
| `TextH3` | 16 | 18 | Subheading (section headers) |
| `TextH2` | 22 | 26 | Section heading |
| `TextH1` | 28 | 34 | Page title |

**Styles:** Use named styles like `HeadingH1`, `TextMdStyle`, `TextCaption`.

### Spacing

| Token | Value | Usage |
|-------|-------|-------|
| `Space2` | 2 | Micro spacing |
| `Space4` | 4 | Tight spacing |
| `Space8` | 8 | Default small |
| `Space12` | 12 | Medium-small |
| `Space16` | 16 | Default medium |
| `Space24` | 24 | Large |
| `Space32` | 32 | Section gap |
| `Space48` | 48 | Major section |
| `Space64` | 64 | Page-level |

**Responsive Thickness:**
- `SpacePage` - Page padding (16 phone / 24 tablet)
- `SpaceCardPadding` - Card internal padding
- `SpaceButtonPadding` - Button internal padding

**Layout Spacing (VerticalStackLayout Spacing):**
- `SpaceStackXs` - Extra-small stack spacing (4 phone / 6 tablet)
- `SpaceStackSm` - Small stack spacing
- `SpaceStackMd` - Medium stack spacing

### Corner Radii

| Token | Value | Usage |
|-------|-------|-------|
| `RadiusNone` | 0 | Sharp corners |
| `RadiusSm` | 4 | Subtle rounding |
| `RadiusMd` | 8 | Default (buttons, inputs) |
| `RadiusLg` | 12 | Cards, modals |
| `RadiusXl` | 16 | Large containers |
| `RadiusFull` | 9999 | Pills, circles |

**Int Versions:** Use `RadiusMdInt` for `Button.CornerRadius` (requires int).

### Sizing

| Token | Phone | Tablet | Purpose |
|-------|-------|--------|---------|
| `MinTapHeight` | 44 | 44 | Accessibility minimum |
| `ButtonHeightDefault` | 48 | 52 | Standard button |
| `EntryHeightDefault` | 48 | 52 | Standard input |
| `IconSizeSm` | 16 | 18 | Small icons |
| `IconSizeMd` | 24 | 28 | Default icons |
| `IconSizeLg` | 32 | 36 | Large icons |

---

## Responsive Strategy

### Approach: OnIdiom

We use `OnIdiom` for responsive sizing:

```xml
<OnIdiom x:Key="TextMd" x:TypeArguments="x:Double" 
         Phone="15" 
         Tablet="16" 
         Default="15"/>
```

**Device Breakpoints:**
- **Phone** (includes SmallPhone, 0-599dp): Baseline sizing optimized for Samsung S9 class devices
- **Tablet** (600dp+): Slightly larger for improved readability

### Why OnIdiom?

1. **Simplicity** - Built-in MAUI feature, no custom code
2. **Predictability** - Clear phone/tablet split
3. **Maintainability** - Easy to understand and modify
4. **SmallPhone Safe** - Phone baseline works down to 320dp width

### Light/Dark Theme

Use `AppThemeBinding` for theme-aware values:

```xml
<Setter Property="BackgroundColor" 
        Value="{AppThemeBinding Light={StaticResource SurfaceBackground}, 
                                Dark={StaticResource SurfaceBackgroundDark}}"/>
```

---

## How to Use Tokens

### In XAML Pages/Components

```xml
<!-- Use color tokens -->
<Label TextColor="{StaticResource TextPrimary}"/>

<!-- Use spacing tokens -->
<VerticalStackLayout Spacing="{StaticResource Space16}"/>

<!-- Use Thickness tokens -->
<Grid Padding="{StaticResource SpacePage}"/>

<!-- Use named styles -->
<Label Text="Title" Style="{StaticResource HeadingH1}"/>
<Button Text="Submit" Style="{StaticResource ButtonPrimary}"/>
```

### Implicit vs Named Styles

**Implicit styles** apply automatically to all controls of that type:
```xml
<!-- No Style attribute needed - implicit Button style applies -->
<Button Text="Default Button"/>
```

**Named styles** must be explicitly applied:
```xml
<Button Text="Outlined" Style="{StaticResource ButtonSecondary}"/>
```

### StaticResource vs DynamicResource

- Use `StaticResource` for tokens that don't change at runtime (recommended)
- Use `DynamicResource` only if you need runtime theme switching beyond AppThemeBinding

---

## Adding New Tokens

### 1. Add to Appropriate Dictionary

```xml
<!-- In Colors.xaml -->
<Color x:Key="MyNewColor">#123456</Color>

<!-- In Spacing.xaml -->
<x:Double x:Key="Space96">96</x:Double>
```

### 2. For Responsive Values

```xml
<!-- In Sizing.xaml -->
<OnIdiom x:Key="MyNewSize" x:TypeArguments="x:Double" 
         Phone="40" 
         Tablet="48" 
         Default="40"/>
```

### 3. Consume in Controls.xaml or Pages

```xml
<Setter Property="HeightRequest" Value="{StaticResource MyNewSize}"/>
```

---

## Control Styles

### Available Button Styles

| Style | Description |
|-------|-------------|
| (default) | Primary BrandPrimary filled button |
| `ButtonPrimary` | Explicit primary style (BrandPrimary bg + White text) |
| `ButtonSecondary` | Outlined style (Surface bg + BrandPrimary border/text) |

### Available Text Styles

| Style | Usage |
|-------|-------|
| `HeadingH1` | Page titles |
| `HeadingH2` | Section headings |
| `HeadingH3` | Subheadings |
| `TextXlStyle` - `TextXsStyle` | Body text variants |
| `TextCaption` | Fine print, hints |
| `TextLink` | Hyperlinks |
| `TextError` | Validation errors |

### Available Container Styles

| Style | Usage |
|-------|-------|
| (default Border) | Generic bordered container |
| `CardBorder` | Card with rounded corners |
| `Divider` | Horizontal line separator |

---

## Quick Reference

### Common Patterns

```xml
<!-- Card with content -->
<Border Style="{StaticResource CardBorder}">
    <VerticalStackLayout Spacing="{StaticResource SpaceStackSm}">
        <Label Text="Title" Style="{StaticResource HeadingH3}"/>
        <Label Text="Body text" Style="{StaticResource TextMdStyle}"/>
    </VerticalStackLayout>
</Border>

<!-- Switch row -->
<Border Style="{StaticResource CardBorder}">
    <Grid ColumnDefinitions="*,Auto" HeightRequest="{StaticResource SwitchRowHeight}">
        <Label Text="Setting Name" VerticalOptions="Center"/>
        <Switch Grid.Column="1" VerticalOptions="Center"/>
    </Grid>
</Border>

<!-- Button group -->
<HorizontalStackLayout Spacing="{StaticResource Space8}">
    <Button Text="Primary" Style="{StaticResource ButtonPrimary}"/>
    <Button Text="Secondary" Style="{StaticResource ButtonSecondary}"/>
</HorizontalStackLayout>
```

---

## Android Platform Configuration

### Edge-to-Edge Status Bar

The app uses a transparent status bar with edge-to-edge mode, allowing the page content to show through the status bar area.

**Key files:**

| File | Purpose |
|------|--------|
| `Platforms/Android/MainActivity.cs` | Sets transparent status bar + light icons |
| `Platforms/Android/Resources/values/colors.xml` | Defines Android theme colors |

**MainActivity.cs approach:**

```csharp
// Enable edge-to-edge with transparent bars
WindowCompat.SetDecorFitsSystemWindows(Window, false);

// Make status bar transparent so app content shows through
Window.SetStatusBarColor(Android.Graphics.Color.Transparent);

// Set dark icons for visibility on light background
var windowInsetsController = WindowCompat.GetInsetsController(Window, Window.DecorView);
windowInsetsController.AppearanceLightStatusBars = true;
```

### Shell Navigation Bar

The Shell navigation bar is **hidden** on all pages to ensure a clean status bar area:

```xml
<ContentPage Shell.NavBarIsVisible="False" ... >
```

> ⚠️ **Important:** Every page must include `Shell.NavBarIsVisible="False"` or the Shell's nav bar (using `colorPrimary`) will show through the transparent status bar.

### Safe Area Considerations

With edge-to-edge mode, content can draw behind the status bar. Ensure pages have appropriate top padding to prevent content from being clipped under the status bar (~24-48dp depending on device).

---

## Maintenance Notes

- **Token Playground**: `MainPage.xaml` demonstrates all tokens - use it to verify changes
- **No raw values**: Never use hex colors or magic numbers directly in pages
- **Test on devices**: Verify both Phone and Tablet idioms render correctly
- **Dark mode**: Deferred to future section (light theme only in 4.1)
- **Android status bar**: Use transparent + `Shell.NavBarIsVisible="False"` pattern
- **Event lifecycle**: Components must use `OnHandlerChanged`/`OnHandlerChanging` for event subscriptions (see Decision 21, 37)
- **INotifyPropertyChanged**: Pages should NOT shadow `PropertyChanged` event; use `base.OnPropertyChanged()` directly
- **iOS safe area**: SafeAreaService uses UIWindowScene API for iOS 15+ (see Decision 36)
- **Header Info icon**: All exercise pages use the HeaderBlock info icon (`info_icon.svg`) for consistent one-tap access to InformationPage. Do NOT add in-body INFO buttons (see Decision 66)
- **Icon colors**: Header and tab bar icons must use `BrandPrimaryDark`. For SVG icons, embed the color (#28404F) in the SVG paths (see Decision 68)
- **Tab back navigation**: Skip/Progress BACK uses `TabBar.CurrentItem = PreviousTabSection` (not `GoToAsync`) to preserve source tab's navigation stack (see Decision 72)
- **Tab reselection**: Platform-specific `CustomShellRenderer` handles re-tapping the active tab — pops to root (see Decision 71)
- **Skip tab icon**: Dynamically swapped in `AppShell.OnNavigated` — `>>` on MainExercisesPage, `>` on ExerciseDetailPage (see Decision 77)
- **Tab bar hiding**: ProgressTabPage and ExerciseProgressPage hide tab bar via FlyoutItemIsVisible + Shell.SetTabBarIsVisible in AppShell.OnNavigated (see Decision 81)
- **Exercise skip context**: `SessionStateService.CurrentExerciseKey` set in ExerciseDetailPage.OnAppearing, cleared in MainExercisesPage.OnAppearing (see Decision 79)
- **AM always first**: `ResolveCurrentPeriod()` always returns first session; auto-advance handles AM→PM progression (see Decision 83)

---

## Reusable Layout Components (Section 4.2)

### Architecture

```
Components/
├── Controls/
│   ├── LogoDisc.xaml         # Brand logo disc component
│   ├── LogoDisc.xaml.cs
│   ├── LoadingSpinner.xaml   # Activity indicator with message
│   ├── LoadingSpinner.xaml.cs
│   ├── ExerciseTile.xaml     # Exercise tile with image/title/completion
│   └── ExerciseTile.xaml.cs
├── Decor/
│   ├── WaveEdge.cs           # Enum: Bottom, Top
│   ├── SineWaveBlock.xaml    # Decorative wave component
│   └── SineWaveBlock.xaml.cs
├── Header/
│   ├── HeaderBlock.xaml      # Page header with title/subtitle
│   └── HeaderBlock.xaml.cs
├── Layout/
│   ├── AppPageTemplate.xaml  # Standard page scaffold
│   └── AppPageTemplate.xaml.cs
Helpers/
└── CharacterSpacingHelper.cs # Shared text spacing calculations
Services/
├── ISafeAreaService.cs       # Safe area interface
└── SafeAreaService.cs        # Platform-specific implementation
```

### LogoDisc

Brand logo component displaying the Hello Buddy dog logo with "GETTING BETTER" tagline inside a circular disc.

**Bindable Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `DiscSize` | double | 0 (auto) | Explicit disc diameter; 0 = auto-size |
| `LogoSource` | ImageSource | hellobuddy_logo.png | Optional custom logo |

**Sizing Behavior:**
- If `DiscSize > 0`: Uses explicit size
- If `DiscSize == 0`: Auto-sizes based on allocated space (76.5% of min dimension)
- All internal elements (shadow, border, logo, tagline) scale proportionally

**Key Ratios (in code-behind):**
- `AutoSizeRatio = 0.765` — disc relative to container
- `LogoSizeRatio = 0.66` — logo relative to disc
- `TaglineWidthRatio = 0.70` — tagline width relative to disc

**Usage:**

```xml
<controls:LogoDisc />              <!-- Auto-size -->
<controls:LogoDisc DiscSize="200" /> <!-- Fixed 200dp -->
```

### LoadingSpinner

Activity indicator with optional message text.

**Bindable Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IsLoading` | bool | false | Show/hide and animate spinner |
| `Message` | string | "Loading..." | Message text below spinner |
| `ShowMessage` | bool | true | Show/hide message label |

**Responsive Sizing:**
- Phone: 40dp spinner
- Tablet: 56dp spinner

**Usage:**

```xml
<controls:LoadingSpinner 
    IsLoading="{Binding IsBusy}"
    Message="Please wait..." />
```

### ExerciseTile

Tile component for displaying an exercise with image, title, completion, and skip states.

**Bindable Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ExerciseKey` | string | "" | Unique identifier for the exercise |
| `ExerciseName` | string | "" | Display name |
| `ImageSource` | ImageSource | null | Exercise thumbnail image |
| `IsComplete` | bool | false | Shows green tick badge when true |
| `IsSkipped` | bool | false | Shows red X badge when true |
| `Command` | ICommand | null | Tap command (passes ExerciseKey) |

**Badge Overlays:**
- **Completion tick** (green): `check_white.svg` on `Success` background
- **Skipped cross** (red): `cross_white.svg` on `Error` background
- Badges are mutually exclusive via `IsVisible` bindings
- If a skipped exercise is later completed, the cross converts to a tick

**Responsive Sizing:**
- Image maintains 3:2 aspect ratio (67% height of width)
- Badge circle: 24dp (Phone) / 32dp (Tablet)
- Icon inside badge: 60% of badge size

**Usage:**

```xml
<Grid ColumnDefinitions="*,*" RowSpacing="{StaticResource Space8}">
    <controls:ExerciseTile 
        ExerciseKey="stretch_1"
        ExerciseName="Back Stretch"
        ImageSource="stretch_1.jpg"
        IsComplete="True"
        IsSkipped="False"
        Command="{Binding OpenExerciseCommand}" />
</Grid>
```

### SineWaveBlock

A decorative sine wave component used as a visual transition between page sections.

**Bindable Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `FillColor` | Color | BrandPrimaryLight | Wave fill color |
| `WaveHeight` | double | 140 (token) | Overall block height |
| `WaveEdge` | WaveEdge | Bottom | Curve orientation |

**WaveEdge Values:**
- `Bottom`: Curve dips downward (header-to-body transition)
- `Top`: Curve dips upward (inverted)

**Usage:**

```xml
<decor:SineWaveBlock 
    WaveEdge="Bottom"
    FillColor="{StaticResource Surface}" />
```

### CharacterSpacingHelper

A shared utility for calculating character spacing to fill a target width. Used by `HeaderBlock` (subtitle) and `LogoDisc` (tagline).

**Location:** `Helpers/CharacterSpacingHelper.cs`

**Method:**
```csharp
public static double Calculate(
    string text,
    double targetWidth,
    double fontSize,
    double avgCharWidthRatio = 0.50,  // Character width as ratio of fontSize
    double spaceWidthRatio = 0.25)    // Space width as ratio of fontSize
```

**Usage:**
```csharp
// In HeaderBlock
subtitleLabel.CharacterSpacing = CharacterSpacingHelper.Calculate(
    Subtitle.ToUpperInvariant(),
    availableWidth,
    fontSize);

// In LogoDisc (with custom ratios for bold text)
taglineLabel.CharacterSpacing = CharacterSpacingHelper.Calculate(
    "GETTING BETTER",
    availableWidth,
    fontSize,
    avgCharWidthRatio: 0.60,
    spaceWidthRatio: 0.30);
```

**Rationale:** Centralizes the spacing algorithm so HeaderBlock and LogoDisc stay in sync. Adjustable ratios allow fine-tuning for different font weights.

---

### HeaderBlock

A reusable page header with title, optional subtitle, and optional icon button.

**Bindable Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Title` | string | "Hello Buddy" | Main heading text |
| `Subtitle` | string | "CANINE PHYSIOTHERAPY" | Subtitle text (uppercase) |
| `ShowSubtitle` | bool | true | Whether to display subtitle |
| `ShowIcon` | bool | false | Whether to show icon button |
| `IconSource` | ImageSource | null | Icon image source |
| `IconCommand` | ICommand | null | Icon tap command |

**Features:**
- Title centered, uses HeadingH1 style
- Subtitle dynamically adjusts character spacing to fill ~90% width
- Background is transparent (inherits from parent)

**Usage:**

```xml
<header:HeaderBlock 
    Title="Settings"
    Subtitle="ACCOUNT PREFERENCES"
    ShowSubtitle="True" />
```

### AppPageTemplate

A standard page layout scaffold composing header, wave, body, and footer.

**Structure (4 rows):**

| Row | Content | Background |
|-----|---------|------------|
| 0 | Safe Area Top | BrandMist |
| 1 | HeaderBlock | BrandMist |
| 2 | Page Scroll (Hero + Wave + Body) OR FullBodyContent | Mixed |
| 3 | Footer (VerticalStackLayout: buttons + safe area spacer) | Surface |

**Note:** The footer row combines button Grid + safe area spacer into a single VerticalStackLayout with Surface background. Safe area height is set via `safeAreaBottomSpacer.HeightRequest` in code-behind. See Decision 57 for refactoring rationale.

**Two Layout Modes:**

1. **Standard Mode** (`ShowFullBody=False`): Hero content + Wave + Body content
2. **Full Body Mode** (`ShowFullBody=True`): Transparent overlay scrolls over hero/wave/body backgrounds

**Bindable Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Title` | string | "Hello Buddy" | Header title |
| `Subtitle` | string | "CANINE PHYSIOTHERAPY" | Header subtitle |
| `ShowSubtitle` | bool | true | Show/hide subtitle |
| `ShowHeaderIcon` | bool | false | Show/hide header icon |
| `HeaderIconSource` | ImageSource | null | Header icon image |
| `HeaderIconCommand` | ICommand | null | Header icon command |
| `HeroContent` | View | null | Content in hero stage area |
| `BodyContent` | View | null | Main page content (standard mode) |
| `FullBodyContent` | View | null | Overlay content (full body mode) |
| `ShowFullBody` | bool | false | Enable full body overlay mode |
| `IsBodyScrollable` | bool | true | Wrap body in ScrollView |
| `ShowFooter` | bool | false | Show footer buttons (fixed at bottom) |
| `PrimaryButtonText` | string | "" | Primary button label (right side) |
| `PrimaryCommand` | ICommand | null | Primary button command |
| `PrimaryButtonIsEnabled` | bool | true | Enable/disable primary button |
| `SecondaryButtonText` | string | "" | Secondary button label (left side) |
| `SecondaryCommand` | ICommand | null | Secondary button command |
| `ShowOverlay` | bool | false | Show loading spinner overlay |
| `IsTablet` | bool | false | Read-only: true when screen width ≥ 600dp |
| `IsPhone` | bool | true | Read-only: true when screen width < 600dp |

**Standard Mode Usage:**

```xml
<layout:AppPageTemplate
    Title="Welcome"
    Subtitle="GET STARTED"
    ShowSubtitle="True"
    IsBodyScrollable="True"
    ShowFooter="True"
    PrimaryButtonText="Next"
    SecondaryButtonText="Back">
    
    <layout:AppPageTemplate.HeroContent>
        <controls:LogoDisc />
    </layout:AppPageTemplate.HeroContent>
    
    <layout:AppPageTemplate.BodyContent>
        <VerticalStackLayout Padding="{StaticResource SpacePage}">
            <!-- Page content here -->
        </VerticalStackLayout>
    </layout:AppPageTemplate.BodyContent>
</layout:AppPageTemplate>
```

**Full Body Mode Usage:**

```xml
<layout:AppPageTemplate
    Title="Settings"
    ShowFullBody="True"
    ShowFooter="True"
    SecondaryButtonText="BACK"
    SecondaryCommand="{Binding NavigateBackCommand}">
    
    <layout:AppPageTemplate.FullBodyContent>
        <VerticalStackLayout Padding="{StaticResource SpacePage}">
            <!-- Content scrolls over hero/wave backgrounds -->
        </VerticalStackLayout>
    </layout:AppPageTemplate.FullBodyContent>
</layout:AppPageTemplate>
```

**Loading Overlay Usage:**

```xml
<layout:AppPageTemplate
    ShowOverlay="{Binding IsLoading}"
    ...>
```

The overlay displays a centered LoadingSpinner with "Loading..." message and a semi-transparent white background (`OverlayLight`). It blocks interaction with the page content while visible.

### SafeAreaService

Platform-specific service for retrieving safe area insets (status bar, navigation bar, notch).

**Interface:**

```csharp
public interface ISafeAreaService
{
    double TopInset { get; }
    double BottomInset { get; }
    double LeftInset { get; }
    double RightInset { get; }
    void Refresh();
}
```

**Registration (MauiProgram.cs):**

```csharp
builder.Services.AddSingleton<ISafeAreaService, SafeAreaService>();
```

**Platform Support:**
- **Android**: Uses `WindowInsetsCompat.Type.SystemBars()` for top inset; **BottomInset = 0** (nav bar handles its own space, see Decision 58)
- **iOS**: Uses `SafeAreaInsets` from UIWindow (typically 24-34dp bottom for home indicator)
- **Windows**: Returns 0 (no safe area concerns)

**Note:** Android's navigation bar (3-button or gesture) reserves its own space even in edge-to-edge mode. Adding bottom safe area padding causes double-spacing. iOS still requires bottom inset for the home indicator area.

---

## Views / Pages

### Page Architecture

All pages use `AppPageTemplate` for consistent layout. Navigation is handled via Shell routing.

**Route Registration (AppShell.xaml.cs):**

```csharp
Routing.RegisterRoute(nameof(InformationPage), typeof(InformationPage));
Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
Routing.RegisterRoute("StyleGuide", typeof(StyleGuidePage));
Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
Routing.RegisterRoute(nameof(ForgottenPasswordPage), typeof(ForgottenPasswordPage));
Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
Routing.RegisterRoute(nameof(TermsConditionsPage), typeof(TermsConditionsPage));
Routing.RegisterRoute(nameof(ExerciseDetailPage), typeof(ExerciseDetailPage));
Routing.RegisterRoute(nameof(ExerciseProgressPage), typeof(ExerciseProgressPage));
```

**Tab Navigation (AppShell.xaml — Post-Login):**

| Tab | Route | Page | Icon |
|-----|-------|------|------|
| Exercises | `//PostLogin/MainExercises` | MainExercisesPage | `tab_exercises.svg` |
| Progress | `//PostLogin/ProgressTab` | ProgressTabPage | `tab_progress.svg` |
| Skip | `//PostLogin/SkipSession` | SkipToNextSessionPage | `tab_skip.svg` / `tab_skip_exercise.svg` |

**Dynamic Skip Tab Icon (AppShell.OnNavigated):**
- `>>` double chevron (`tab_skip.svg`) — shown on MainExercisesPage and most pages; indicates session skip
- `>` single chevron (`tab_skip_exercise.svg`) — shown on ExerciseDetailPage; indicates exercise skip
- Icon swapped dynamically via `skipTab.Icon` in `AppShell.OnNavigated` based on current route

**Tab Bar Visibility (AppShell.OnNavigated):**
- Tab bar is **hidden** on ProgressTabPage and ExerciseProgressPage
- Uses `FlyoutItemIsVisible = false` on all tab items + `Shell.SetTabBarIsVisible(CurrentPage, false)`
- Restored when navigating away from those pages

**Tab Back Navigation:**
- Skip and Progress tabs preserve the source tab's navigation stack
- BACK switches tabs via `TabBar.CurrentItem = PreviousTabSection` (not `GoToAsync`)
- Only the Exercises tab pops the previous tab's stack (as the hub/home)
- `AppShell.PreviousTabSection` stores the `ShellSection` reference for stack-preserving switches

**Tab Reselection:**
- Tapping the already-active tab pops its navigation stack to root
- Handled by platform-specific `CustomShellRenderer` (Android: `OnTabReselected`, iOS: `UITabBarControllerDelegate`)

### HelloBuddyPage

Landing page with logo and navigation buttons.

**Features:**
- LogoDisc in hero area (proportional to screen height)
- Four navigation buttons: INFO, SETTINGS, START PHYSIO, REGISTRATION
- Buttons vertically centered in body area

**Navigation:**
- INFO → InformationPage
- SETTINGS → SettingsPage
- START PHYSIO → LoginPage
- REGISTRATION → RegistrationPage

### InformationPage

Displays informational content about the app, loaded from JSON.

**Features:**
- Uses `ShowFullBody="True"` mode
- Content loaded from `Resources/Raw/AppContent.json` via `TextContentLoader`
- Content loading in `OnAppearing` for reliable tablet support
- Sections: ABOUT THIS APP, PHYSIOTHERAPY, RECOGNISING PAIN
- BACK button always visible in footer (page is always pushed, `GoToAsync("..")` pops correctly)

### SettingsPage

App preferences with toggle switches.

**Features:**
- Uses `ShowFullBody="True"` mode
- Settings persisted via MAUI `Preferences` API
- Settings: Program Code (read-only), Download Videos, Offline Caching, Notifications, Notification Time
- Notification Time picker only visible when Notifications enabled
- BACK button in template footer

### LoginPage

User authentication page.

**Features:**
- Email and password entries in hero area (75% width via Grid columns `*,6*,*`)
- Entry fields styled with BrandMist background and BrandPrimary border
- Error message area with fixed height (Space24) to prevent layout shift
- LOGIN button in body area
- FORGOTTEN PASSWORD text button with pressed state feedback
- Loading overlay during authentication
- BACK button in template footer

**Validation:**
- Email required + valid format
- Password required

**Navigation:**
- LOGIN (success) → TermsConditionsPage
- FORGOTTEN PASSWORD → ForgottenPasswordPage
- BACK → HelloBuddyPage

### ForgottenPasswordPage

Password reset request page.

**Features:**
- Email entry in hero area (75% width)
- Entry field styled with BrandMist background and BrandPrimary border
- Error message area with fixed height
- SEND RESET button in hero area
- On success: Form hides, shows "Password reset sent!" + "Please check your email."
- Loading overlay during API call
- BACK button in template footer

**Validation:**
- Email required + valid format

**Navigation:**
- BACK → LoginPage

### RegistrationPage

New user registration page.

**Features:**
- Email and password entries in hero area (75% width)
- Entry fields styled with BrandMist background and BrandPrimary border
- Error/success message area with fixed height
- REGISTER button in body area
- Loading overlay during registration (500ms simulated)
- BACK button in template footer

**Validation:**
- Email required + valid format
- Password required
- (Password min 8 chars - temporarily disabled for prototype)

**Navigation:**
- REGISTER (success) → Shows success message, stays on page
- BACK → HelloBuddyPage

### TermsConditionsPage

Legal terms with acceptance requirement.

**Features:**
- Uses `ShowFullBody="True"` mode
- Collapsible sections (tap to expand/collapse)
- Content loaded from `Resources/Raw/AppContent.json`
- Sections: TERMS OF SERVICE, PRIVACY POLICY, ACCEPTABLE USE POLICY
- Agreement toggle enables NEXT button
- **Preloads exercise data** while spinner runs (primes PhysioContentService cache)
- BACK/NEXT buttons in template footer

**Acceptance Flow:**
1. User reads terms (can expand each section)
2. User toggles "I agree..." switch
3. NEXT button becomes enabled
4. Tap NEXT → spinner shows while preloading exercise data
5. Navigate to MainExercisesPage (loads instantly from cache)

### MainExercisesPage

Main exercise display page with progress tracking and session management.

**Features:**
- Uses standard AppPageTemplate mode with hero content
- Responsive hero layout (larger text on tablets, compact on phones)
- Instruction card in hero: description, progress bar, warning text
- Exercise tiles grid in body (2 columns)
- Data loaded from `PhysioContent.json` via `PhysioContentService`
- **Header info icon** (top-right) navigates to InformationPage
- No footer (clean hub page)
- **Exercise completion tracking** via `completedExerciseKey` query parameter
- **Exercise skip tracking** via `skippedExerciseKey` query parameter
- **Cross → tick conversion** when a skipped exercise is subsequently completed
- **Auto-advance** to next session when all exercises are completed or skipped
- Clears `SessionStateService.CurrentExerciseKey` in `OnAppearing`

**Hero Content (Responsive):**

| Element | Phone (< 600dp) | Tablet (≥ 600dp) |
|---------|-----------------|------------------|
| Description | TextSmStyle | TextMdStyle |
| Progress % | TextXsStyle | TextSmStyle |
| Warning | TextXsStyle | TextSmStyle |
| Card padding | Space12 | SpaceCardPadding |

**Data Loading:**
- Exercise data preloaded on TermsConditionsPage while spinner runs
- `PhysioContentService` caches data for instant MainExercisesPage load
- Warning text loaded from `AppContent.json` via `TextContentLoader`

**Exercise Skip Tracking:**
- `_skippedExerciseKeys` HashSet tracks skipped exercises (in-memory, not persisted)
- `SkippedExerciseKey` query property received from `SkipToNextSessionPage` (exercise mode)
- `MarkExerciseSkipped()` adds to skipped set and shows red X badge
- `MarkExerciseComplete()` removes from skipped set if previously skipped (cross → tick)

**Auto-Advance Logic:**
- Triggers when `_exercises.All(e => e.IsComplete || e.IsSkipped)`
- AM → PM (multi-session): Shows "Session Complete" alert, loads PM exercises
- PM → Done or Single → Done: Shows "All Done!" alert

**Navigation:**
- Header info icon → InformationPage
- Exercise tile tap → ExerciseDetailPage
- Tab bar → Progress, Skip tabs

### SkipToNextSessionPage

Context-aware skip page that handles both exercise-level and session-level skips.

**Features:**
- Uses standard AppPageTemplate mode
- Hero card with instruction text (matches MainExercisesPage card styling)
- Comments editor in body area for optional feedback
- BACK/CONFIRM buttons in template footer
- Loading overlay during confirmation
- **Dual mode**: Exercise skip or Session skip (auto-detected)

**Mode Detection:**
- Reads `SessionStateService.CurrentExerciseKey` in `OnAppearing`
- If set → **Exercise skip mode** (user came from ExerciseDetailPage)
- If null → **Session skip mode** (user came from MainExercisesPage)

**Exercise Skip Mode:**
- Header title: Exercise name (e.g., "Baited Back Stretch")
- Hero text: "Skipping this exercise will mark it with a red cross..."
- Comments placeholder: "Why are you skipping this exercise?"
- CONFIRM → `//MainExercises?skippedExerciseKey={key}` (marks tile with red X)
- No revert button

**Session Skip Mode:**
- Header title: "Skip to Next Session"
- Hero text: "Skipping this session will mark it as complete..."
- Comments placeholder: "Why are you skipping this session?"
- CONFIRM → Calls `SessionStateService.TrySkip()` then `//MainExercises`
- Revert button available when skip targets tomorrow

**Layout:**
- Hero: Bordered card (BrandMist background, BrandPrimary stroke) with context-dependent text
- Body: Multi-line Editor with `CommentsEditorHeight` token for internal scrolling
- Footer: BACK (returns to previous tab), CONFIRM (completes skip action)

**Navigation:**
- BACK → Returns to previous tab (via `CurrentItem` assignment, preserves source stack)
- CONFIRM → MainExercisesPage (via `//MainExercises` with optional `skippedExerciseKey` parameter)

### ProgressTabPage

Placeholder progress overview page (tab root).

**Features:**
- Uses standard AppPageTemplate mode (non-scrollable)
- Centered placeholder text: "Progress tracking" / "Your exercise progress overview will appear here."
- Header info icon → InformationPage
- Footer BACK button → Returns to previous tab (via `CurrentItem` assignment)
- **Tab bar hidden** via `Shell.TabBarIsVisible="False"` + AppShell.OnNavigated FlyoutItemIsVisible suppression

**Navigation:**
- Header info icon → InformationPage
- BACK → Returns to previous tab (preserves source stack)

**Registration:** `builder.Services.AddTransient<ProgressTabPage>()` in MauiProgram.cs

**Future:** Will be replaced with full progress overview showing current vs previous session data.

### ExerciseDetailPage

Exercise detail page with video playback and instructions.

**Features:**
- Uses `ShowFullBody="True"` mode for immersive content
- Responsive layout (tablet: side-by-side; phone: stacked)
- Video thumbnail with play overlay
- Fullscreen video playback via CommunityToolkit.Maui.MediaElement
- Numbered instruction steps using BindableLayout
- Reps/sets display
- **Header info icon** (top-right) navigates to InformationPage
- BACK/DONE sticky footer buttons
- **Sets `SessionStateService.CurrentExerciseKey`** in `OnAppearing` for exercise-level skip context
- Tab bar shows `>` (single chevron) icon on Skip tab when on this page

**Layout (Responsive):**

| Element | Phone (< 600dp) | Tablet (≥ 600dp) |
|---------|-----------------|------------------|
| Layout | Vertical stacked | Horizontal (video left, content right) |
| Video thumbnail | Full width, AspectFill | Square, rounded corners |
| Instructions | Full width | Scrollable panel |
| Footer | Full width buttons | Centered max-width buttons |

**Data Flow:**
1. Navigation passes `exerciseKey` via QueryProperty
2. `ExerciseDetailPage.ExerciseKey` setter triggers data load
3. `PhysioContentService.GetExerciseByKeyAsync()` returns exercise
4. Properties populated: Title, Summary, Reps, Sets, Instructions, Thumbnail
5. Video loaded on-demand when play tapped

**Video Playback:**
- Thumbnail shows static image from exercise data
- Play overlay button triggers fullscreen video
- Video loads from `Resources/Raw/{VideoName}` via `MediaSource.FromResource()`
- Close button stops video and hides overlay
- `OnDisappearing` ensures video cleanup

**Package Dependency:**
```xml
<PackageReference Include="CommunityToolkit.Maui.MediaElement" Version="4.1.1" />
```

**MauiProgram.cs Configuration:**
```csharp
using CommunityToolkit.Maui;

builder.UseMauiCommunityToolkitMediaElement()
```

**Navigation:**
- BACK → Previous page (Shell.GoToAsync(".."))
- DONE → Placeholder completion alert, then navigate back

---

## Content Management

### TextContentLoader

Helper class for loading content sections from `AppContent.json`.

**Registration (MauiProgram.cs):**

```csharp
builder.Services.AddSingleton<TextContentLoader>();
```

**Usage:**

```csharp
public partial class InformationPage : ContentPage
{
    private readonly TextContentLoader _contentLoader;
    
    public InformationPage(TextContentLoader contentLoader)
    {
        _contentLoader = contentLoader;
        LoadContentAsync();
    }
    
    private async void LoadContentAsync()
    {
        var sections = await _contentLoader.GetSectionsAsync("information");
        Sections = new ObservableCollection<ContentSection>(sections);
    }
}
```

### AppContent.json Structure

Located at `Resources/Raw/AppContent.json`:

```json
{
  "information": [
    { "key": "about", "header": "ABOUT THIS APP", "body": "..." },
    { "key": "physiotherapy", "header": "PHYSIOTHERAPY", "body": "..." },
    { "key": "pain", "header": "RECOGNISING PAIN", "body": "..." }
  ],
  "termsConditions": [
    { "key": "termsOfService", "header": "TERMS OF SERVICE", "body": "..." },
    { "key": "privacyPolicy", "header": "PRIVACY POLICY", "body": "..." },
    { "key": "acceptableUse", "header": "ACCEPTABLE USE POLICY", "body": "..." }
  ],
  "warnings": [
    { "key": "exerciseDisclaimer", "text": "Always consult your vet..." }
  ]
}
```

### PhysioContentService

Service for loading exercise data from `PhysioContent.json` with thread-safe caching.

**Registration (MauiProgram.cs):**

```csharp
builder.Services.AddSingleton<PhysioContentService>();
```

**Methods:**

| Method | Returns | Description |
|--------|---------|-------------|
| `LoadContentAsync()` | `Task` | Preloads and caches all content |
| `GetFirstExerciseSetAsync()` | `Task<ExerciseSet?>` | Returns first exercise set |
| `GetExerciseByKeyAsync(key)` | `Task<Exercise?>` | Returns exercise by key |
| `GetProgrammeAsync()` | `Task<Programme?>` | Returns full programme with daily sessions |
| `GetExerciseSetByPeriodAsync(period)` | `Task<ExerciseSet?>` | Returns exercise set for a given period (AM/PM) |

**Usage:**

```csharp
public partial class MainExercisesPage : ContentPage
{
    private readonly PhysioContentService _physioContentService;
    
    public MainExercisesPage(PhysioContentService physioContentService)
    {
        _physioContentService = physioContentService;
    }
    
    private async Task LoadContentAsync()
    {
        var exerciseSet = await _physioContentService.GetFirstExerciseSetAsync();
        // exerciseSet.Description, exerciseSet.Exercises, etc.
    }
}
```

### SessionStateService

Singleton service managing session state, skip logic, and active session resolution using `Preferences` for persistence.

**Registration (MauiProgram.cs):**

```csharp
builder.Services.AddSingleton<SessionStateService>();
```

**Key Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `CurrentExerciseKey` | `string?` | Exercise key currently being viewed (in-memory, not persisted) |
| `IsSkipActive` | `bool` | Whether a session skip is currently active |

**Key Methods:**

| Method | Returns | Description |
|--------|---------|-------------|
| `GetActiveSession(dailySessions)` | `(DateTime, string)` | Resolves current session date/period accounting for skips |
| `TrySkip(date, period, dailySessions)` | `SkipResult` | Attempts to skip to next session (AM→PM or PM→tomorrow) |
| `RevertSkip(dailySessions)` | `(DateTime, string)` | Reverts active skip to today's first session |
| `ClearSkip()` | `void` | Clears all skip state from Preferences |

**Skip Persistence:**
- Uses `Preferences` API with keys: `SkipActiveDate`, `SkipActivePeriod`, `SkipOriginDate`
- Auto-expiry: Skip clears when origin date is older than today
- `ResolveCurrentPeriod()` always returns first session (AM comes first)

**CurrentExerciseKey Lifecycle:**
1. Set in `ExerciseDetailPage.OnAppearing()` — user is viewing an exercise
2. Read in `SkipToNextSessionPage.OnAppearing()` — determines skip mode
3. Cleared in `MainExercisesPage.OnAppearing()` — no longer viewing an exercise
4. Cleared in `SkipToNextSessionPage.ConfirmSkipAsync()` — exercise skip confirmed

**Usage:**

```csharp
// Resolve active session
var (date, period) = _sessionStateService.GetActiveSession(dailySessions);

// Skip to next session
var result = _sessionStateService.TrySkip(date, period, dailySessions);
if (!result.IsBlocked)
{
    // Navigate to new session
}
```

### PhysioContent.json Structure

Located at `Resources/Raw/PhysioContent.json`:

```json
{
  "exerciseSets": [
    {
      "key": "hindlimbCore",
      "description": "These canine physiotherapy videos...",
      "exercises": [
        {
          "key": "baitedBackStretch",
          "title": "Baited back stretch",
          "image": "baited_back_stretch",
          "summary": "Neck and spine stretches...",
          "instructions": ["Step 1...", "Step 2..."],
          "reps": 3,
          "sets": 1,
          "videoName": "baited_back_stretch_video.mp4"
        }
      ]
    }
  ]
}
```

**Models:**
- `PhysioContent` - Root container with `ExerciseSets` list
- `ExerciseSet` - Group of related exercises with description
- `Exercise` - Individual exercise with title, image, instructions, reps/sets, video

**Resources:**
- Images: `Resources/Images/` (e.g., `baited_back_stretch.jpg`)
- Videos: `Resources/Raw/` (e.g., `baited_back_stretch_video.mp4`)

---

## Layout Tokens (Section 4.2 Additions)

### Sizing Tokens

| Token | Phone | Tablet | Purpose |
|-------|-------|--------|---------|
| `HeaderRegionHeight` | 320 | 380 | Total height of header region |
| `WaveHeight` | 140 | 220 | Height of SineWaveBlock |
| `ProgressBarHeight` | 10 | 12 | Height of progress bar indicators |
| `ProgressBarCornerRadius` | 5 | 6 | Corner radius for progress bars |

### Spacing Tokens

| Token | Phone | Tablet | Purpose |
|-------|-------|--------|---------|
| `SpaceHeaderPadding` | 16,12,16,16 | 24,16,24,20 | HeaderBlock internal padding |
| `SpaceFooterPadding` | 16,12,16,16 | 24,16,24,20 | Footer button area padding |

---

## Core Principles

### Foundational Principles

All pages and components must adhere to these principles:

1. **Responsive by design** - Components scale predictably across SmallPhone/Phone/Tablet without manual device hacks.
2. **Token-driven styling** - Colors, typography, spacing, and radii use Theming dictionaries. No hard-coded hex values or magic numbers.
3. **Minimal new tokens** - Only add what is genuinely missing. Avoid creating parallel token systems.
4. **Predictable layout** - All sizing derived from a single source of truth (e.g., DiscSize for LogoDisc).
5. **Safe defaults** - Controls should size themselves sensibly within their container if no explicit size is set.
6. **No coupling to specific pages** - Components should be reusable across the app.

> **See also:** [iOS-Safe Rules](#ios-safe-rules) — additional rules ensuring cross-platform compatibility, especially for iOS.

---

## iOS-Safe Rules

These rules ensure every component/page is "iOS-safe by default", even when developing on Windows without local iOS testing. Rules are platform-neutral where possible, with iOS-specific behaviour only when essential.

**Applies to:** AppPageTemplate, HeaderBlock, LogoDisc, SineWaveBlock, ExerciseTile, LoadingSpinner, and all new pages/components.

### 1) Safe Areas Are Owned by the Template

- Only `AppPageTemplate` may handle safe-area padding/rows.
- Pages/components **MUST NOT** use negative margins or absolute positioning to "fake" safe area alignment.
- Any bottom overlay (e.g., scroll Home pill) **MUST** sit above the bottom safe-area row (never under the home indicator).

### 2) No Fixed Heights for Text-Containing UI

- Do not set fixed `HeightRequest` on containers that include Labels or variable text (headers, cards, tiles).
- Prefer `MinHeightRequest` + `Padding` over fixed height.
- If a height must be constrained, use adaptive tokens and ensure text can wrap or scroll.

### 3) Dynamic Type / Large Text Friendly

- All labels must tolerate increased font sizes:
  - Avoid `NoWrap` for anything longer than a short title unless there is a clear UX reason.
  - If `NoWrap` is required (e.g., header subtitle), use character spacing logic with clamping and test at larger font sizes.
- Ensure text does not clip vertically (allow enough padding/line height).

### 4) Minimum Tap Targets

- Any tappable control (ImageButton, Button, Tile, icon) must be at least **~44×44 logical units**.
- For small icons, wrap them in a larger hit-area container.
- Use `MinTapHeight` and `MinTapWidth` tokens from Sizing.xaml.

### 5) Scroll + Gestures Must Not Fight iOS Expectations

- Avoid custom pan/swipe handlers that can interfere with:
  - Scroll gestures inside ScrollView/CollectionView
  - Back-swipe gesture (if Shell/Navigation stack is used)
- Prefer standard controls over gesture recognizers unless essential.

### 6) Forms Must Be Keyboard-Safe

- All forms (login, registration, comments) must be in a `ScrollView` or equivalent so fields can be scrolled into view when the keyboard appears.
- Do not place critical inputs in fixed bottom areas that can be obscured by the iOS keyboard.
- Set appropriate keyboard types (Email, Numeric, etc.) using MAUI `Keyboard` settings.

### 7) Layout Must Be Responsive via Tokens + Breakpoints

- Use only SmallPhone/Phone/Tablet adaptive tokens for sizing, spacing, radii, typography.
- Avoid device-model checks.
- No "pixel-perfect" alignment assumptions; allow breathing room via tokens.

### 8) Bottom Overlays Must Be Inset-Aware

- Any floating UI (toast, bottom nav pill) must:
  - Respect safe-area bottom inset
  - Never overlap your fixed footer row (if `ShowFooter="True"`, auto-disable/hide)
  - Use animation guards to prevent jitter/stacking

### 9) Images Must Scale Cleanly

- Avoid assuming a PNG will look identical across platforms.
- Use aspect modes deliberately (`AspectFit` for logos, `AspectFill` for tiles).
- Avoid layout depending on tiny image transparent padding; instead use explicit sizing and offsets (as with LogoDisc).

### 10) Lifecycle / Backgrounding Safe Defaults

- Assume iOS can suspend the app quickly.
- Persist any important in-progress state (e.g., exercise logging inputs) when leaving a page or app sleeps.
- Do not rely on long-running operations completing without cancellation support.

### 11) Networking Safe Defaults (ATS-Friendly)

- Assume iOS will block non-HTTPS by default.
- All endpoints must be HTTPS unless explicitly configured otherwise.
- Centralize HTTP configuration; do not scatter platform exceptions.

### 12) Platform-Specific Code Must Be Centralized

- Avoid sprinkling `#if IOS` across pages.
- If iOS-specific tweaks are needed later, put them in:
  - The template, or
  - A small service/handler mapper layer
- Keep components platform-agnostic.

---

## iOS-Safe Checklist (PR Review)

Use this checklist when reviewing pages/components:

- [ ] Safe area owned by template only?
- [ ] No fixed heights for text containers?
- [ ] Tap targets >= 44×44?
- [ ] Forms inside ScrollView for keyboard safety?
- [ ] Token-driven sizing only (no magic numbers)?
- [ ] Bottom overlays inset-aware?
- [ ] No custom gestures conflicting with scroll/back-swipe?
- [ ] Images using appropriate aspect mode?
- [ ] HTTPS-only networking?
- [ ] Platform-specific code centralized (not scattered)?

---

## Known Issues

### Android Emulator Keyboard Not Appearing for Numeric Entry Fields

**Symptom:** On Android emulators, tapping numeric Entry fields (e.g., reps/sets on ExerciseProgressPage) shows a "Choose input method" dialog with a black bar instead of the keyboard.

**Affected:** Android emulators only. Physical devices (S22, tablets) work correctly.

**Root Cause:** Android emulator IME (Input Method Editor) handling for `Keyboard="Numeric"` in constrained layouts (Entry inside Border with fixed dimensions) does not trigger the soft keyboard correctly.

**Workarounds Attempted:**
- Changed `Keyboard="Numeric"` to `Keyboard="Telephone"` - no effect
- Added `HorizontalOptions="Fill"` and `VerticalOptions="Fill"` to Entry - no effect
- Added `TapGestureRecognizer` to programmatically call `Entry.Focus()` - no effect
- Changed binding from `int?` to `string` properties - no effect on emulator

**Recommendation:** Test Entry field workflows on physical devices. This is a known Android emulator limitation, not an app issue.

**Reference:** See Decision Log entries 50-51.

---

## Icons and Symbols

### Using Icons in the App

**DO NOT** use Unicode symbols (✓, ✗, ★, etc.) directly in Labels - the app's OpenSans font does not include these glyphs, causing them to render as blank/invisible on Android.

**DO** use one of these approaches:

1. **SVG Images (Recommended)**
   ```xaml
   <Image Source="check_white.svg" WidthRequest="16" HeightRequest="16" />
   ```
   - Place SVG files in `Resources/Images/`
   - MAUI automatically converts to platform-appropriate format
   - Scales cleanly, consistent cross-platform

2. **PNG Images**
   - Use when SVG is not suitable
   - Provide multiple resolutions if needed

**Available Icons:**
| Icon | File | Description |
|------|------|-------------|
| ✓ (white) | `check_white.svg` | Checkmark for completion indicators |
| ✗ (white) | `cross_white.svg` | Cross for skipped exercise indicators |
| >> | `tab_skip.svg` | Double chevron for session skip (tab icon) |
| > | `tab_skip_exercise.svg` | Single chevron for exercise skip (tab icon) |
| (i) | `info_icon.svg` | Info circle for header icon |
| Exercises | `tab_exercises.svg` | Exercises tab icon |
| Progress | `tab_progress.svg` | Progress tab icon |

**Reference:** See Decision Log entries 54, 78.

---

## Exercise Completion & Skip Tracking

### Current Implementation (MVP)

Exercise completion and skip state are tracked in-memory using two `HashSet<string>` collections in `MainExercisesPage`:

```csharp
private readonly HashSet<string> _completedExerciseKeys = new();
private readonly HashSet<string> _skippedExerciseKeys = new();
```

Keys are compound strings scoped to date+period: `"2026-03-10_AM_baitedBackStretch"`.

**Completion Flow:**
1. User completes exercise on `ExerciseProgressPage` and taps SAVE
2. Page navigates back with query parameter: `../../?completedExerciseKey={key}`
3. `MainExercisesPage` receives key via `[QueryProperty]`
4. `MarkExerciseComplete()` adds key to completed set, removes from skipped set
5. `ExerciseTile` shows green tick (cross → tick if previously skipped)

**Skip Flow:**
1. User taps Skip tab (`>` icon) from `ExerciseDetailPage`
2. `SkipToNextSessionPage` detects exercise mode via `CurrentExerciseKey`
3. User optionally adds comments, taps CONFIRM
4. Page navigates to `//MainExercises?skippedExerciseKey={key}`
5. `MainExercisesPage` receives key via `[QueryProperty]`
6. `MarkExerciseSkipped()` adds key to skipped set
7. `ExerciseTile` shows red X badge

**Auto-Advance:**
- Triggers when ALL exercises are either completed or skipped
- AM → PM (multi-session): Loads afternoon exercises
- PM → Done (or single-session): Shows "All Done!" alert

**Cross → Tick Conversion:**
- If user goes back to a skipped exercise and completes it, `MarkExerciseComplete()` removes the key from `_skippedExerciseKeys` and sets `IsSkipped=false`, `IsComplete=true` on the view model

**Limitations:**
- State is lost on app restart
- No persistence to Preferences or database yet
- Skip comments not persisted (TODO for backend)

**Future Enhancement:** Persist completion and skip state to `Preferences` or local database.

**Reference:** See Decision Log entries 53, 78, 80, 82.

---

## Google Play Store Deployment

### Build Configuration

The app is configured for Google Play Store deployment with the following settings:

**Application Identity:**
```xml
<ApplicationId>com.yourcompany.caninephysio</ApplicationId>
<ApplicationDisplayVersion>1.0.5</ApplicationDisplayVersion>
<ApplicationVersion>5</ApplicationVersion>
```

**Signing Configuration:**
```xml
<PropertyGroup Condition="$(TargetFramework.Contains('-android')) and '$(Configuration)' == 'Release'">
    <AndroidKeyStore>true</AndroidKeyStore>
    <AndroidSigningKeyStore>..\canine-physio.keystore</AndroidSigningKeyStore>
    <AndroidSigningKeyAlias>caninephysio</AndroidSigningKeyAlias>
    <AndroidSigningKeyPass>canineDemo2026</AndroidSigningKeyPass>
    <AndroidSigningStorePass>canineDemo2026</AndroidSigningStorePass>
    <AndroidPackageFormat>aab</AndroidPackageFormat>
</PropertyGroup>
```

**Linker & Runtime Settings (Critical for Release):**
```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <PublishTrimmed>false</PublishTrimmed>
    <RunAOTCompilation>false</RunAOTCompilation>
</PropertyGroup>

<PropertyGroup Condition="$(TargetFramework.Contains('-android')) and '$(Configuration)' == 'Release'">
    <AndroidLinkMode>None</AndroidLinkMode>
    <AndroidEnableProfiledAot>false</AndroidEnableProfiledAot>
    <AndroidEnableMarshalMethods>false</AndroidEnableMarshalMethods>
</PropertyGroup>
```

> ⚠️ **AndroidEnableMarshalMethods=false** is essential. .NET 10's marshal methods feature generates static JNI registrations that are incompatible with ExoPlayer bindings (CommunityToolkit.Maui.MediaElement). Without this setting, the AAB will crash on startup with `UnsatisfiedLinkError`. See Decision Log entry 65.

### Building for Release

```powershell
cd "c:\Projects\Canine Physio App"
dotnet publish "Canine Physio App\Canine Physio App.csproj" -f net10.0-android -c Release
```

**Output:** `Canine Physio App\bin\Release\net10.0-android\publish\com.yourcompany.caninephysio-Signed.aab`

### Keystore Location

⚠️ **IMPORTANT:** Keep the keystore file safe! You need it for all future updates.

- **Location:** `c:\Projects\Canine Physio App\canine-physio.keystore`
- **Alias:** `caninephysio`
- **Password:** `canineDemo2026`

### Google Play Console Upload Steps

1. Go to [play.google.com/console](https://play.google.com/console)
2. Select the **Canine Physio** app
3. Navigate to **Testing → Internal testing**
4. Click **Create new release**
5. Upload the signed AAB file
6. Add release notes
7. Click **Next** → **Save and publish**

### Installing on Test Device

1. Wait ~10 minutes for Google to process the release
2. In Play Console, go to **Testing → Internal testing → Testers**
3. Copy the invite link
4. Open that link on the test device
5. Install/Update from the Play Store

---

## Known Issues & Troubleshooting

### Release Build Crashes (IL Linker / Marshal Methods)

**Symptom:** App works in Debug mode but crashes in Release — either when navigating to certain pages or immediately on startup.

**Root Causes Identified:**
1. **Missing StaticResource definitions** — Pages referencing undefined resources (e.g. `SpaceStackXs`) crash silently in Release but work in Debug due to lenient resource fallback.
2. **IL Trimmer** — Strips types needed at runtime for DI, Shell navigation, and XAML bindings.
3. **Marshal Methods (.NET 10)** — Generates static JNI registrations incompatible with ExoPlayer bindings, causing `UnsatisfiedLinkError: MauiApplication.n_onCreate()`.

**Solution:** All three mitigations are required in the .csproj:
```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <PublishTrimmed>false</PublishTrimmed>
    <RunAOTCompilation>false</RunAOTCompilation>
</PropertyGroup>

<PropertyGroup Condition="$(TargetFramework.Contains('-android')) and '$(Configuration)' == 'Release'">
    <AndroidLinkMode>None</AndroidLinkMode>
    <AndroidEnableProfiledAot>false</AndroidEnableProfiledAot>
    <AndroidEnableMarshalMethods>false</AndroidEnableMarshalMethods>
</PropertyGroup>
```

**Trade-off:** Larger APK/AAB size (~86MB vs ~54MB) but stable runtime behavior.

**Reference:** See Decision Log entries 60, 64, 65.

### AAB Startup Crash (UnsatisfiedLinkError)

**Symptom:** AAB installs from Google Play but crashes immediately on launch with `UnsatisfiedLinkError: No implementation found for void MauiApplication.n_onCreate()`.

**Root Cause:** .NET 10 Android marshal methods generate invalid JNI stubs for ExoPlayer library bindings (used by CommunityToolkit.Maui.MediaElement v4.1.1). The Boolean/Byte type mismatches in ExoPlayer's Java bindings produce 290+ build warnings and broken native method registrations.

**Solution:** Add `<AndroidEnableMarshalMethods>false</AndroidEnableMarshalMethods>` to the Android Release PropertyGroup. Build warnings should drop from ~456 to ~166 (confirming the fix).

**Diagnosis Tip:** If you see hundreds of `ExoPlayer` warnings mentioning `Boolean` or `Byte` marshal method mismatches, marshal methods are the issue.

**Reference:** See Decision Log entry 65.

### Missing StaticResource Crashes

**Symptom:** Certain pages crash when navigated to in Release mode but work fine in Debug. No clear error message.

**Root Cause:** XAML referencing a `StaticResource` key that doesn't exist in any merged resource dictionary. Debug mode handles missing resources gracefully; Release mode throws.

**Solution:** Audit all `StaticResource` references against definitions in Theming/*.xaml files. Ensure every referenced key has a corresponding definition.

**Reference:** See Decision Log entry 64.

### Version Code Already Used Error

**Symptom:** Google Play Console rejects upload with "Version code X has already been used"

**Solution:** Increment `<ApplicationVersion>` in the .csproj before rebuilding:
```xml
<ApplicationDisplayVersion>1.0.1</ApplicationDisplayVersion>
<ApplicationVersion>2</ApplicationVersion>  <!-- Increment this -->
```

### XAML Thickness Syntax Error

**Symptom:** Runtime crash on page load with no clear error message.

**Root Cause:** Invalid XAML Thickness syntax combining StaticResource with comma separators:
```xml
<!-- INVALID - causes runtime crash -->
Padding="{StaticResource Space16},{StaticResource Space8}"

<!-- VALID - use literal values -->
Padding="16,8"
```

**Solution:** Use literal values when combining multiple dimensions, or create a dedicated Thickness resource.
