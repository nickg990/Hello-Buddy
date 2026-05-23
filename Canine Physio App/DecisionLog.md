# Canine Physio App - Decision Log

This document records architectural decisions, assumptions, and trade-offs made during development.

---

## Section 4.1: Design Token System

**Date:** 2026-01-29  
**Author:** Lead Engineer

---

### Decision 1: Responsive Strategy - OnIdiom

**Decision:** Use `OnIdiom` for responsive token sizing instead of VisualStateManager.

**Alternatives Considered:**
1. VisualStateManager with adaptive states (more complex, requires custom implementation)
2. Custom markup extensions (requires C# code, harder to maintain)
3. OnIdiom with Phone/Tablet (simple, built-in, predictable)

**Rationale:**
- OnIdiom is a first-party MAUI feature with excellent tooling support
- Phone idiom works well for SmallPhone (320-359dp) baseline - tested
- Tablet idiom provides natural scaling for 600dp+
- No custom code required; purely declarative XAML
- Easy for junior developers to understand and extend

**Trade-off:** Cannot distinguish SmallPhone from Phone programmatically with OnIdiom alone. Mitigated by designing Phone baseline to work on 320dp minimum.

---

### Decision 2: Hello Buddy Color Palette

**Decision:** Use exact Hello Buddy brand colors with minimal utility tokens.

**Brand Colors:**
- `BrandPrimary` (#6392AE) - Primary brand teal
- `BrandPrimaryLight` (#B3CDD6) - Light variant
- `BrandPrimaryDark` (#28404F) - Dark variant
- `BrandMist` (#D4E0E5) - Soft muted background
- `White` (#FFFFFF) - Pure white

**Utility Tokens:** 
- `PageBackground`, `Surface` - Layout backgrounds
- `TextPrimary`, `TextMuted` - Typography colors
- `BorderDefault`, `DisabledSurface`, `DisabledText` - Control states
- `Error` - Validation errors only

**Rationale:** Minimal palette reduces decision fatigue. All semantic/surface colors derive from just 5 brand values + 8 utility tokens. Zero neutral ladder (no Neutral000-900).

---

### Decision 3: No Neutral Scale

**Decision:** Removed numeric neutral scale entirely. Use utility tokens only.

**Previous Approach:** Neutral000=white through Neutral900=near-black.

**Current Approach:** Named utility tokens: `PageBackground`, `Surface`, `TextPrimary`, `TextMuted`, `BorderDefault`, `DisabledSurface`, `DisabledText`.

**Rationale:** Semantic naming is clearer than numeric values. Developers know exactly what each token is for without consulting a reference.

---

### Decision 4: Button Corner Radius - RadiusMd (8dp)

**Decision:** Default button corner radius is 8dp (RadiusMd).

**Rationale:**
- Friendly but professional appearance
- Not too rounded (playful/childish) nor too sharp (corporate)
- Matches the "Hello Buddy" friendly brand feel

---

### Decision 5: Minimum Tap Target - 44dp

**Decision:** All interactive elements have minimum 44dp tap target.

**Rationale:**
- iOS HIG specifies 44pt minimum
- Material Design recommends 48dp, but 44dp saves vertical space on small phones
- Accessibility requirement for motor impairment support

**Implementation:** `MinTapHeight` and `MinTapWidth` tokens set to 44.

---

### Decision 6: Typography Scale

**Decision:** Use body text scale (Xs/Sm/Md/Lg/Xl) separate from heading scale (H1/H2/H3).

**Values:**
| Token | Phone | Tablet |
|-------|-------|--------|
| TextXs | 11 | 12 |
| TextSm | 13 | 14 |
| TextMd | 15 | 16 |
| TextLg | 17 | 18 |
| TextXl | 19 | 21 |
| TextH3 | 18 | 20 |
| TextH2 | 22 | 26 |
| TextH1 | 28 | 34 |

**Rationale:**
- 15pt base (TextMd) is readable on small phones without being too large
- Heading sizes provide clear visual hierarchy
- Tablet sizes are ~10-15% larger for improved readability at viewing distance

---

### Decision 7: Spacing Scale - 4px Grid

**Decision:** Spacing tokens follow 4px grid: 2, 4, 8, 12, 16, 24, 32, 48, 64.

**Rationale:**
- Industry standard (Material, iOS HIG)
- Provides enough granularity without too many options
- Easy mental math (multiples of 4)

---

### Decision 8: No Brushes.xaml or Elevation.xaml

**Decision:** Skip separate Brushes.xaml and Elevation.xaml files for now.

**Rationale:**
- Brushes: Colors suffice; SolidColorBrush can be created inline when needed
- Elevation: MAUI's Shadow support is limited; defer until needed for specific components
- Principle: Minimal + deliberate resources - don't create files until needed

**Future:** Add these files in later sections if shadow/gradient tokens are required.

---

### Decision 9: Control Styles - Implicit + Named

**Decision:** Provide both implicit styles (auto-apply) and named variants.

**Pattern:**
- `TargetType="Button"` (no Key) = implicit, applies to all Buttons
- `x:Key="ButtonSecondary"` = explicit, opt-in for specific buttons

**Rationale:**
- Implicit styles reduce boilerplate (most buttons are primary)
- Named styles provide flexibility for variants
- Standard MAUI pattern, familiar to .NET developers

---

### Decision 10: Template Bloat Removal

**Files Removed:**
- `Resources/Styles/Colors.xaml` - Replaced by `Theming/Colors.xaml`
- `Resources/Styles/Styles.xaml` - Replaced by modular theme files
- `Resources/Images/dotnet_bot.png` - Sample image not used by app

**Files Modified:**
- `MainPage.xaml` - Replaced welcome template with Token Playground
- `MainPage.xaml.cs` - Removed counter logic
- `AppShell.xaml` - Updated title to "Hello Buddy"
- `App.xaml` - Updated to merge new theme dictionaries

**Rationale:** Clean slate per requirements; no template content should remain.

---

### Decision 11: Dark Theme Deferred

**Decision:** Dark theme is NOT implemented in Section 4.1. All tokens and styles are light theme only.

**Rationale:**
- Reduces complexity for initial implementation
- Hello Buddy brand colors are defined; dark mode palette not yet finalized
- `StaticResource` is simpler than `AppThemeBinding` (no dual-token overhead)
- Dark mode can be added in a future section when palette is approved

**Implementation:**
- Colors.xaml contains only light-theme utility tokens
- No `*Dark` suffix tokens exist
- All control styles use `StaticResource` directly (no `AppThemeBinding`)

**Future:** When dark mode is approved, add `*Dark` tokens and update styles to use `AppThemeBinding`.

---

### Open Questions / Future Decisions

1. **Custom Fonts:** Currently using OpenSans (MAUI default). Need to confirm if brand fonts will be added.
2. **Animation Tokens:** Not included yet. May add duration/easing tokens when animations are implemented.
3. **Figma Integration:** When Figma file is available, verify/update color values in Colors.xaml.
4. **Accessibility:** Need to verify contrast ratios meet WCAG AA once final colors are confirmed.

---

### Decision 12: Android Status Bar - Edge-to-Edge Transparent

**Decision:** Use transparent status bar with edge-to-edge mode on Android.

**Problem:** Material 3 theme default colors include pink-tinted neutrals (`m3_ref_palette_neutral98` = #FEF7FF). Setting `SetStatusBarColor(Color.White)` alone was ineffective because MAUI/Material themes were overriding it.

**Solution:**
1. Enable edge-to-edge mode: `WindowCompat.SetDecorFitsSystemWindows(Window, false)`
2. Set status bar to transparent (not white): `Window.SetStatusBarColor(Color.Transparent)`
3. Use light status bar icons: `windowInsetsController.AppearanceLightStatusBars = true`
4. Hide Shell navigation bar: `Shell.NavBarIsVisible="False"` on all pages

**Key Insight:** With transparent status bar, the app's page background shows through. If the Shell nav bar is visible, its `colorPrimary` background shows through instead. Hiding the nav bar ensures only the page content (white) is visible.

**Files Modified:**
- `Platforms/Android/MainActivity.cs` - Edge-to-edge + transparent status bar code
- `MainPage.xaml` - Added `Shell.NavBarIsVisible="False"`
- `AppShell.xaml` - Added `Shell.BackgroundColor="{StaticResource White}"`

**Approach Source:** Archive codebase (`Archive/Canine Physiotherapy/App/CaninePhysioApp`) had working implementation.

**Trade-off:** Edge-to-edge mode requires pages to account for safe areas (top padding for status bar, bottom for navigation bar). Archive used a custom `PageTemplate` control with built-in safe area handling.

---

## Changelog

| Date | Section | Change |
|------|---------|--------|
| 2026-01-29 | 4.1 | Initial token system created |
| 2026-01-29 | 4.1 | Stripped palette to Hello Buddy brand only; removed Neutral scale; updated utility tokens |
| 2026-01-29 | 4.1 | Removed all dark theme tokens; light theme only; TextPrimary=#111111, TextMuted=#5A7A8A |
| 2026-01-29 | 4.1 | Android status bar: edge-to-edge transparent + Shell.NavBarIsVisible=False |
| 2026-01-30 | 4.2 | Created SineWaveBlock, HeaderBlock, AppPageTemplate components |
| 2026-01-30 | 4.2 | Added SafeAreaService for platform-specific safe area insets |
| 2026-01-30 | 4.2 | Added HeaderRegionHeight, WaveHeight, SpaceHeaderPadding, SpaceFooterPadding tokens |
| 2026-01-30 | 4.2 | Fixed BoxView implicit style issue causing wrong background color |
| 2026-01-31 | 4.2 | Refactored AppPageTemplate: fixed header, scrollable hero+wave+body |
| 2026-01-31 | 4.2 | Added HeroHeight token (400/460 Phone/Tablet) replacing HeaderRegionHeight |
| 2026-01-31 | 4.2 | Added HeroContent slot for logo disc and illustrations |
| 2026-02-05 | 4.3 | Created LoginPage with email/password validation |
| 2026-02-05 | 4.3 | Created ForgottenPasswordPage with success state |
| 2026-02-06 | 4.3 | Created RegistrationPage with email/password validation |
| 2026-02-06 | 4.3 | Added ErrorOnMist color token (#D97706) for error text on BrandMist background |
| 2026-02-06 | 4.3 | Fixed content loading on tablet - moved from constructor to OnAppearing |
| 2026-02-09 | 4.4 | Created MainExercisesPage with PhysioContentService and responsive hero |
| 2026-02-09 | 4.4 | Added PhysioContentService for JSON-based exercise data loading |
| 2026-02-09 | 4.4 | Added IsTablet/IsPhone properties to AppPageTemplate for responsive layouts |
| 2026-02-09 | 4.4 | Implemented T&C preloading of exercise data while spinner runs |
| 2026-02-09 | 4.5 | Code review: Fixed iOS deprecated KeyWindow API in SafeAreaService |
| 2026-02-09 | 4.5 | Code review: Added event lifecycle handlers to AppPageTemplate |
| 2026-02-09 | 4.5 | Code review: Removed shadowed PropertyChanged events from pages |
| 2026-02-09 | 4.5 | Code review: Added CancellationToken support to PhysioContentService |
| 2026-02-09 | 4.5 | Code review: Added ProgressBarHeight/ProgressBarCornerRadius tokens |
| 2026-02-16 | 4.6 | Created SkipToNextSessionPage with hero card and comments editor |
| 2026-02-16 | 4.6 | Added navigation from MainExercisesPage SKIP button to SkipToNextSessionPage |
| 2026-02-16 | 4.6 | Refactored AppPageTemplate: Simplified from 5 rows to 4 rows |
| 2026-02-16 | 4.6 | Refactored AppPageTemplate: Combined footer + safe area into VerticalStackLayout |
| 2026-02-16 | 4.6 | Android bottom safe area set to 0 (nav bar handles its own space) |
| 2026-02-16 | 4.6 | Updated SpaceFooterPadding with 8dp bottom padding |
| 2026-02-25 | 5 | Fixed missing SpaceStackXs token in Spacing.xaml (caused Release navigation crashes) |
| 2026-02-25 | 5 | Fixed incorrect style references in ForgottenPasswordPage (TextH3Style→HeadingH3, TextBaseStyle→TextMdStyle) |
| 2026-02-25 | 5 | Fixed AAB startup crash: disabled AndroidEnableMarshalMethods for .NET 10 Android |
| 2026-02-25 | 5 | Disabled PublishTrimmed to prevent IL trimmer stripping required types |
| 2026-02-25 | 5 | Version bumped to 1.0.5 (versionCode 5) - confirmed working on device |
| 2026-02-26 | 5.1 | Replaced in-page INFO buttons with persistent HeaderBlock info icon on exercise pages |
| 2026-02-26 | 5.1 | Removed SKIP from MainExercisesPage footer; relocated to ExerciseProgressPage body |
| 2026-02-26 | 5.1 | Added info_icon.svg (BrandPrimaryDark #28404F) to Resources/Images |
| 2026-02-26 | 5.1 | Updated Shell tab bar colors: TabBarForegroundColor/TitleColor → BrandPrimaryDark |
| 2026-02-26 | 5.1 | MainExercisesPage footer removed (ShowFooter=False); clean hub page |
| 2026-02-26 | 5.2 | Added post-login tab bar: Exercises, Progress, Skip (3 tabs via Shell TabBar) |
| 2026-02-26 | 5.2 | Created ProgressTabPage placeholder with BACK button |
| 2026-02-26 | 5.2 | Created tab SVG icons: tab_exercises.svg, tab_progress.svg, tab_skip.svg |
| 2026-02-26 | 5.2 | Added TabBarSurface color token (#D9FFFFFF, 85% white opacity) |
| 2026-02-26 | 5.2 | Added platform-specific CustomShellRenderer for tab reselection (Android + iOS) |
| 2026-02-26 | 5.2 | Stack-preserving tab back: CurrentItem assignment instead of GoToAsync for BACK |
| 2026-02-26 | 5.2 | InformationPage BACK button now always visible (removed post-login hiding logic) |
| 2026-02-26 | 5.2 | Removed SKIP button from ExerciseProgressPage (tab handles skip now) |
| 2026-03-10 | 5.3 | Skip Exercise feature: SkipToNextSessionPage dual-mode (exercise skip + session skip) |
| 2026-03-10 | 5.3 | Dynamic tab icon swapping: >> (session skip) on MainExercisesPage, > (exercise skip) on ExerciseDetailPage |
| 2026-03-10 | 5.3 | Created tab_skip_exercise.svg (single chevron >) for exercise-level skip |
| 2026-03-10 | 5.3 | Created cross_white.svg (white X) for skipped exercise badge |
| 2026-03-10 | 5.3 | Added IsSkipped property to ExerciseTile with red X badge overlay |
| 2026-03-10 | 5.3 | Added IsSkipped property to ExerciseViewModel |
| 2026-03-10 | 5.3 | Added CurrentExerciseKey to SessionStateService for exercise-level skip context |
| 2026-03-10 | 5.3 | MainExercisesPage: skippedExerciseKey QueryProperty + MarkExerciseSkipped() |
| 2026-03-10 | 5.3 | Skipped exercise subsequently completed: cross → tick (remove from skipped set) |
| 2026-03-10 | 5.3 | Auto-advance triggers when all exercises are complete OR skipped |
| 2026-03-10 | 5.3 | Tab bar hidden on ProgressTabPage and ExerciseProgressPage via AppShell.OnNavigated |
| 2026-03-10 | 5.3 | ExerciseDetailPage sets CurrentExerciseKey in OnAppearing |
| 2026-03-10 | 5.3 | MainExercisesPage clears CurrentExerciseKey in OnAppearing |
| 2026-03-10 | 5.3 | Simplified ResolveCurrentPeriod: always returns first session (AM loads first) |
| 2026-03-10 | 5.3 | Added tertiary button support to AppPageTemplate (TertiaryButtonText, TertiaryCommand) |

---

## Section 4.2: Reusable Layout Foundation

**Date:** 2026-01-30  
**Author:** Lead Engineer

---

### Decision 13: SineWaveBlock - Sine vs Cosine Formula

**Decision:** Use sine wave formula for asymmetric wave shape.

**Formula:** `y = amplitude * (1 + sin(2π * x / width)) / 2`

**Result:** Trough at left quarter, peak at right three-quarter, creating a dynamic asymmetric appearance.

**Alternative Considered:** Cosine formula `(1 - cos(2π))` created a symmetric bowl shape centered horizontally.

**Rationale:** The sine formula better matches the Figma design with the wave "cutting into" the header from below.

---

### Decision 14: HeaderBlock - Dynamic Character Spacing

**Decision:** Subtitle text dynamically calculates character spacing to fill ~90% of container width.

**Implementation:**
- `SizeChanged` and `Loaded` events trigger recalculation
- Character spacing = `(availableWidth - textWidth) / characterCount`
- Capped at maximum of 30 to prevent excessive spacing

**Rationale:** Creates a professional, balanced appearance matching the Figma mockups where subtitle text is evenly distributed.

---

### Decision 15: AppPageTemplate - Flattened Grid Structure

**Decision:** Use a single 7-row Grid instead of nested Grids.

**Structure:**
| Row | Height | Content |
|-----|--------|---------|
| 0 | Dynamic | Safe Area Top (status bar) |
| 1 | Auto | HeaderBlock |
| 2 | Calculated | Header Spacer (BrandMist breathing room) |
| 3 | Auto | SineWaveBlock |
| 4 | * | Body Content (scrollable or static) |
| 5 | Auto | Footer (optional buttons) |
| 6 | Dynamic | Safe Area Bottom (navigation bar) |

**Previous Approach:** Nested Grid with outer `Auto` row containing inner fixed-height Grid.

**Problem with Nesting:** Outer Grid's `Auto` row ignored inner Grid's `HeightRequest`, causing the header region to collapse.

**Rationale:** Flat structure gives direct control over all row heights without parent-child sizing conflicts.

---

### Decision 16: Safe Area Service - Platform Detection

**Decision:** Create ISafeAreaService with platform-specific implementations.

**Implementation:**
- Interface: `ISafeAreaService` with `TopInset`, `BottomInset`, `Refresh()`
- Service: `SafeAreaService` with `#if ANDROID` / `#if IOS` conditional compilation
- Registration: Singleton in `MauiProgram.cs`
- Usage: AppPageTemplate queries service on `Loaded` event

**Android Approach:**
```csharp
var windowInsets = ViewCompat.GetRootWindowInsets(decorView);
var systemBarsInsets = windowInsets.GetInsets(WindowInsetsCompat.Type.SystemBars());
TopInset = systemBarsInsets.Top / density;
```

**Rationale:** Platform-specific insets vary by device (notch, navigation bar style). Hardcoded values would fail on many devices.

---

### Decision 17: BoxView Implicit Style Conflict

**Problem:** Header spacer showed #B3CDD6 instead of expected BrandMist.

**Root Cause:** Controls.xaml had an implicit BoxView style setting `BackgroundColor` to `BorderDefault` (#B3CDD6).

**Solution Options:**
1. Remove implicit BoxView style (risky - might break dividers)
2. Set explicit `BackgroundColor="Transparent"` on spacer
3. Remove BoxView entirely - empty Grid row suffices

**Decision:** Option 3 - removed BoxView spacer. Empty `*` row in Grid automatically absorbs remaining space.

**Lesson Learned:** Implicit styles in Controls.xaml can unexpectedly affect components. Always verify background colors with Live Visual Tree.

---

### Decision 18: Hero Stage + Scrolling Behavior Refactor

**Date:** 2026-01-31

**Decision:** Refactor AppPageTemplate to use fixed header with scrollable hero+wave+body.

**Previous Approach:** 7-row Grid with fixed header, spacer, wave, and body rows. Spacer height calculated dynamically to achieve consistent wave position.

**Problems with Previous Approach:**
1. Complex spacer height calculation based on estimated component heights
2. Body ScrollView only scrolled the body content, not the wave transition
3. Wave position varied between devices due to dynamic calculations

**New Approach:** 5-row Grid with simpler structure:

| Row | Height | Content | Behavior |
|-----|--------|---------|----------|
| 0 | Dynamic | Safe Area Top | Fixed |
| 1 | Auto | HeaderBlock | Fixed |
| 2 | Star | ScrollView (hero+wave+body) | Scrolls |
| 3 | Auto | Footer | Fixed |
| 4 | Dynamic | Safe Area Bottom | Fixed |

**ScrollView Content:**
```
VerticalStackLayout
├── HeroStage (fixed height = HeroHeight token)
├── SineWaveBlock
└── BodyContentHost
```

**Key Changes:**
1. **HeroHeight token** replaces HeaderRegionHeight (400/460 Phone/Tablet)
2. **Wave inside ScrollView** - scrolls with content, not fixed in grid
3. **HeroContent slot** - new BindableProperty for hero area content (logo disc, etc.)
4. **Simplified code-behind** - removed ConfigureHeaderSpacerHeight(), simplified body content handling
5. **IsBodyScrollable** now toggles ScrollView.IsEnabled instead of swapping containers

**Rationale:**
- Wave starts at consistent Y-position (HeaderBlock height + HeroHeight) on every page
- Natural scrolling: hero + wave + body move together as users swipe
- Simpler mental model: "everything under the header scrolls"
- Footer remains fixed for navigation actions (Next/Back/Save)

**Trade-off:** Hero area now has a fixed height rather than flexible spacer. This is acceptable because we want consistent wave positioning across pages.

---

### Decision 19: HeroHeight Responsive Sizing - Width-Based Detection

**Date:** 2026-01-31

**Decision:** Use width-based screen detection in code-behind (not OnIdiom) for HeroHeight.

**Values:**
- SmallPhone (<360dp): 360dp
- Phone (360-599dp): 400dp
- Tablet (≥600dp): 460dp

**Implementation:**
```csharp
// Width breakpoints
private const double SmallPhoneMaxWidth = 360;
private const double TabletMinWidth = 600;

// HeroHeight values
private const double HeroHeight_SmallPhone = 360;
private const double HeroHeight_Phone = 400;
private const double HeroHeight_Tablet = 460;

// Applied via SizeChanged + ApplyResponsiveSizing()
```

**Why Not OnIdiom:**
- OnIdiom only distinguishes Phone/Tablet, not SmallPhone
- Width-based detection matches Archive approach (proven to work)
- More precise control over breakpoints

**Rationale:** 
- Smaller screens (320-360dp) get reduced hero height to maximize body content space
- Consistent with Archive codebase responsive sizing approach
- Ensures wave position is identical across devices of the same category

**Rationale:** Token-driven approach allows responsive adjustment while maintaining consistent proportions.

---

### Decision 20: Code Review Refactoring - Shared Helpers & Style Inheritance

**Date:** 2026-02-01

**Decision:** Refactor codebase to eliminate duplication and improve maintainability based on code review findings.

**Changes Made:**

1. **Button Style Inheritance:** `ButtonPrimary` is now the base definition; implicit `Button` style uses `BasedOn="{StaticResource ButtonPrimary}"` — eliminates ~35 lines of duplication.

2. **CharacterSpacingHelper:** Created `Helpers/CharacterSpacingHelper.cs` to centralize the character spacing calculation algorithm used by both `HeaderBlock` (subtitle) and `LogoDisc` (tagline).

3. **Removed Unused Tokens:**
   - `LogoDiscCharacterSpacing` — spacing is calculated dynamically, not from a token
   - `Primary` color alias — redundant duplicate of `BrandPrimary`

4. **Event Handler Lifecycle:** Added proper `OnHandlerChanged`/`OnHandlerChanging` overrides in `LogoDisc` and `ExerciseTile` to subscribe/unsubscribe `SizeChanged` events, preventing potential memory leaks.

5. **Magic Number Elimination:**
   - `LogoDisc.xaml`: `Spacing="2"` → `Spacing="{StaticResource Space2}"`
   - `ExerciseTile`: Tick size, corner radius, and font size now calculated proportionally in code-behind using `TickFontSizeRatio` constant
   - `HeaderBlock.xaml`: Hard-coded padding → `Padding="{StaticResource SpaceHeaderPadding}"`

**Rationale:**
- DRY principle: Single source of truth for button styling and spacing calculations
- Memory safety: Proper event cleanup prevents leaks in long-running apps
- Token-driven: All values derive from tokens, enabling future theming/scaling
- Maintainability: Changes to button styling only need to be made in one place

**Trade-off:** Slightly more complex button style structure (named style as base, implicit style inherits). Acceptable because it eliminates significant duplication.

---

### Decision 21: Component Event Subscription Pattern

**Date:** 2026-02-01

**Decision:** Use `OnHandlerChanged`/`OnHandlerChanging` lifecycle methods for event subscription management in components.

**Pattern:**
```csharp
protected override void OnHandlerChanged()
{
    base.OnHandlerChanged();
    if (Handler != null)
    {
        SizeChanged += OnSizeChanged;
    }
}

protected override void OnHandlerChanging(HandlerChangingEventArgs args)
{
    base.OnHandlerChanging(args);
    if (args.OldHandler != null)
    {
        SizeChanged -= OnSizeChanged;
    }
}
```

**Applied To:** `LogoDisc`, `ExerciseTile`

**Rationale:**
- Prevents memory leaks from orphaned event handlers
- Follows MAUI best practices for handler lifecycle
- Events are only subscribed when the control is actually in the visual tree
- Clean separation of construction vs. visual tree attachment

**Alternative Considered:** Subscribe in constructor, never unsubscribe. Rejected because it can cause memory leaks if controls are removed and recreated frequently (e.g., in CollectionView templates).

---

### Decision 22: Template Fixed Footer for Navigation Buttons

**Date:** 2026-02-04

**Decision:** Use AppPageTemplate's built-in footer (Row 3) for navigation buttons rather than inline buttons within page content.

**Properties Added:**
- `ShowFooter` - Whether to display the footer row
- `PrimaryButtonText` / `PrimaryCommand` - Right-side action button (e.g., "NEXT")
- `SecondaryButtonText` / `SecondaryCommand` - Left-side action button (e.g., "BACK")
- `PrimaryButtonIsEnabled` - Conditional enabling (e.g., for T&C acceptance)

**Rationale:**
- Buttons remain fixed at bottom of screen while content scrolls
- Consistent button placement across all pages
- For T&C pages specifically: fixed buttons are a UX trade-off, but we accepted this after considering that content still scrolls and users can read before accepting
- Centralized styling and positioning

**Trade-off:** For legal/compliance pages, fixed buttons might encourage skipping content. Mitigated by requiring toggle acceptance before NEXT is enabled.

**Pages Using This Pattern:** InformationPage, SettingsPage, TermsConditionsPage

---

### Decision 23: Centralized Loading Overlay in AppPageTemplate

**Date:** 2026-02-04

**Decision:** Add loading overlay with spinner as a built-in feature of AppPageTemplate rather than per-page implementation.

**Properties Added:**
- `ShowOverlay` (bool) - Bind to ViewModel's IsLoading property

**Implementation:**
- Outer Grid wraps the entire template
- Overlay Grid with LoadingSpinner sits on top when ShowOverlay=True
- Semi-transparent background (`OverlayLight` = #80FFFFFF) blocks interaction
- Generic "Loading..." message

**Rationale:**
- Eliminates per-page Grid wrappers and duplicate overlay code
- Consistent loading experience across all pages
- Easy to enable on any page via single property binding
- Spinner runs for duration of actual async operation (or placeholder delay)

**Usage:**
```xaml
<layout:AppPageTemplate
    ShowOverlay="{Binding IsLoading}"
    ... />
```

**Future Enhancement:** When MainExercisesPage has real data loading, replace `Task.Delay()` with actual async data fetch, tying spinner duration to actual load time.

---

### Decision 24: Typography H3 Size - 16pt (Phone) / 18pt (Tablet)

**Date:** 2026-02-04

**Decision:** HeadingH3 uses 16pt on Phone, 18pt on Tablet (reduced from original 18pt/20pt).

**Rationale:**
- Settings page headers appeared too large at 18pt
- 16pt provides better visual hierarchy with body text (15pt TextMd)
- Consistent with compact uppercase header style used across Settings, T&C, and Information pages

**Applied To:** All section headers in Settings, Terms & Conditions, and Information pages.

---

### Decision 25: Content Headers - Uppercase Style

**Date:** 2026-02-04

**Decision:** All section headers in content pages use ALL CAPS format.

**Examples:**
- Settings: `PROGRAM CODE`, `DOWNLOAD EXERCISE VIDEOS`, `NOTIFICATIONS`
- T&C: `TERMS OF SERVICE`, `PRIVACY POLICY`, `ACCEPTABLE USE POLICY`
- Information: `ABOUT THIS APP`, `PHYSIOTHERAPY`, `RECOGNISING PAIN`

**Rationale:**
- Consistent visual language across all pages
- Clear section delineation without relying on larger font sizes
- Matches the compact, professional aesthetic of the app

**Implementation:** Headers stored in AppContent.json as uppercase strings.

---

### Decision 26: Content Body Text - TextMuted Color

**Date:** 2026-02-04

**Decision:** Body text under section headers uses `TextMuted` color (#5A7A8A) rather than `TextPrimary`.

**Rationale:**
- Creates clear visual hierarchy between headers and body
- Reduces visual weight of long-form content
- Consistent across Information, T&C, and Settings description text

---

## Known Issues

### Issue 1: Settings Page Navigation Delay

**Date:** 2026-02-04  
**Status:** Parked for investigation

**Symptom:** Noticeable delay when navigating to SettingsPage compared to other pages. Occurs every navigation, not just first time. Observed on both emulator and physical device in debug mode.

**Possible Causes:**
1. Debug mode overhead (JIT compilation)
2. AppPageTemplate complexity (safe area calculations, responsive sizing, multiple layout layers)
3. Switch controls initialization
4. TimePicker control complexity

**Notes:**
- Other pages have slight delay but Settings is more pronounced
- May improve in Release build
- Consider profiling with dotnet-trace if issue persists in Release

**Action:** Defer investigation until Release build testing. If still slow, profile to identify bottleneck.

---

## Section 4.3: Authentication Pages

**Date:** 2026-02-05  
**Author:** Lead Engineer

---

### Decision 27: Auth Page Layout Pattern

**Date:** 2026-02-05

**Decision:** All authentication pages (Login, Registration, ForgottenPassword) follow a consistent layout pattern:

- **Hero area:** Form inputs (email, password) at 75% width using Grid columns `*,6*,*`
- **Body area:** Primary action button (LOGIN, REGISTER, SEND RESET)
- **Footer:** BACK button via AppPageTemplate
- **Entry styling:** BrandMist background with BrandPrimary border to match hero area
- **Error messages:** Fixed-height container (Space24) to prevent layout shift

**Rationale:**
- Consistent user experience across all auth flows
- 75% width prevents entries from stretching too wide on tablets
- Fixed error area height prevents jarring layout changes when errors appear/disappear
- BrandMist entry background blends with hero while BrandPrimary border provides definition

---

### Decision 28: ErrorOnMist Color Token

**Date:** 2026-02-05

**Decision:** Added `ErrorOnMist` color token (#D97706, warm amber) for error text on BrandMist backgrounds.

**Problem:** Standard `Error` token (#DC3545, red) has poor contrast against BrandMist (#D4E0E5) background.

**Solution:** Warm amber provides better visibility while remaining clearly "error-like" in appearance.

**Usage:** Error messages in hero area of Login, Registration, ForgottenPassword pages.

---

### Decision 29: ForgottenPassword Success State

**Date:** 2026-02-05

**Decision:** ForgottenPasswordPage shows success message by hiding form and displaying confirmation text.

**Behavior:**
1. User enters email, taps SEND RESET
2. Loading overlay shows during API call
3. On success: Form hides, "Password reset sent!" + "Please check your email." appears
4. User taps BACK to return to LoginPage

**Rationale:**
- Matches archive design pattern
- Clear visual feedback that action completed
- Prevents accidental re-submission

---

### Decision 30: Content Loading in OnAppearing

**Date:** 2026-02-06

**Decision:** Content loading (from AppContent.json) moved from constructor to `OnAppearing` lifecycle method.

**Problem:** On tablet devices, content was not displaying on InformationPage and TermsConditionsPage. Constructor-based async loading was racing with page rendering.

**Solution:**
- Move `LoadContentAsync()` call to `OnAppearing()` override
- Add `_contentLoaded` flag to prevent re-loading on subsequent appearances
- Use `MainThread.BeginInvokeOnMainThread` for collection updates

**Rationale:**
- `OnAppearing` is called when page is fully visible and ready for updates
- More reliable than constructor-based async void patterns
- Standard .NET MAUI pattern for data loading

---

### Decision 31: Registration Password Validation (Prototype Mode)

**Date:** 2026-02-06

**Decision:** Password minimum length validation temporarily disabled for prototype demos.

**Production Requirement:** Password must be at least 8 characters.

**Prototype Mode:** Validation commented out with TODO marker for re-enabling.

**Rationale:** Speeds up demo flow by allowing simple test passwords (e.g., "test", "1234").

---

## Section 4.4: MainExercisesPage & Data Services

**Date:** 2026-02-09  
**Author:** Lead Engineer

---

### Decision 32: PhysioContentService - JSON-Based Exercise Data

**Date:** 2026-02-09

**Decision:** Create `PhysioContentService` to load exercise data from `PhysioContent.json` with thread-safe caching.

**Implementation:**
- Service: `Services/PhysioContentService.cs`
- Models: `Models/PhysioContent.cs`, `ExerciseSet.cs`, `Exercise.cs`
- Data: `Resources/Raw/PhysioContent.json`

**Key Methods:**
- `LoadContentAsync()` - Loads and caches all content
- `GetFirstExerciseSetAsync()` - Returns first exercise set for MainExercisesPage
- `GetExerciseByKeyAsync(string key)` - Returns specific exercise for detail page

**Caching:** Uses `SemaphoreSlim` for thread-safe lazy initialization. Data loaded once per app session.

**Rationale:**
- Matches archive pattern for exercise data management
- Enables preloading on T&C page while spinner runs
- Separates data concerns from UI code

---

### Decision 33: MainExercisesPage Hero Layout - Instruction Card

**Date:** 2026-02-09

**Decision:** Move instruction card (description + progress bar + warning) into hero area instead of body.

**Previous Approach:** Empty hero with instruction card in body, wasting hero real estate.

**New Approach:**
- Hero contains instruction card with BrandMist background and BrandPrimary border
- Body contains only exercise tile grid
- Tiles scroll up under hero area, utilizing full screen height

**Card Styling:**
- Background: `BrandMist` (matches header)
- Border: `BrandPrimary` stroke (1px) to define card boundary
- Corner radius: `RadiusMd`

**Rationale:** Better use of hero space; creates cohesive visual flow from header through hero to body.

---

### Decision 34: Responsive Hero Content - IsTablet/IsPhone Properties

**Date:** 2026-02-09

**Decision:** Add `IsTablet` and `IsPhone` bindable properties to `AppPageTemplate` for responsive content switching.

**Properties:**
- `IsTablet` (bool) - true when screen width ≥ 600dp
- `IsPhone` (bool) - true when screen width < 600dp

**Implementation:** Properties updated in `ApplyResponsiveSizing()` method alongside hero height calculation.

**Usage Pattern:**
```xml
<Border IsVisible="{Binding Source={x:Reference pageTemplate}, Path=IsTablet}">
    <!-- Tablet layout with larger text -->
</Border>
<Border IsVisible="{Binding Source={x:Reference pageTemplate}, Path=IsPhone}">
    <!-- Phone layout with compact text -->
</Border>
```

**MainExercisesPage Responsive Values:**

| Element | Phone | Tablet |
|---------|-------|--------|
| Description | TextSmStyle | TextMdStyle |
| Progress % | TextXsStyle | TextSmStyle |
| Warning | TextXsStyle | TextSmStyle |
| Card padding | Space12 | SpaceCardPadding |
| Stack spacing | Space4 | SpaceStackSm |
| Progress bar | 10px | 12px |

**Rationale:**
- Fixed hero height means content must fit; smaller text needed on phones
- Tablets have more hero space; can use larger, more readable text
- Declarative XAML approach avoids complex code-behind logic

**Trade-off:** Duplicated XAML for tablet/phone layouts. Acceptable because it keeps styling declarative and easy to modify independently.

---

### Decision 35: T&C Preloading - Exercise Data Priming

**Date:** 2026-02-09

**Decision:** Preload MainExercisesPage data on TermsConditionsPage while spinner is showing.

**Previous Approach:** Fixed `Task.Delay(500)` as placeholder.

**New Approach:**
```csharp
await Task.WhenAll(
    _physioContentService.LoadContentAsync(),
    _contentLoader.GetWarningAsync("exerciseDisclaimer")
);
```

**Benefits:**
- Spinner runs for actual data load time (not arbitrary delay)
- MainExercisesPage loads instantly (data already cached)
- Parallel loading of both services maximizes efficiency

**Rationale:** Improves perceived performance; user sees purposeful spinner rather than artificial delay.

---

## Section 4.5: Code Review - MAUI & iOS Best Practices

**Date:** 2026-02-09  
**Author:** Lead Engineer

---

### Decision 36: iOS SafeAreaService - UIWindowScene API

**Date:** 2026-02-09

**Decision:** Update iOS safe area detection to use `UIWindowScene` API instead of deprecated `KeyWindow`.

**Problem:** `UIApplication.SharedApplication.KeyWindow` was deprecated in iOS 13+. Using deprecated APIs can cause warnings and potential future breakage.

**Solution:**
```csharp
#elif IOS || MACCATALYST
    UIKit.UIWindow? window = null;
    
    // iOS 15+ uses UIWindowScene API
    if (OperatingSystem.IsIOSVersionAtLeast(15))
    {
        var connectedScenes = UIKit.UIApplication.SharedApplication?.ConnectedScenes;
        var windowScene = connectedScenes?
            .OfType<UIKit.UIWindowScene>()
            .FirstOrDefault(s => s.ActivationState == UIKit.UISceneActivationState.ForegroundActive);
        window = windowScene?.Windows.FirstOrDefault(w => w.IsKeyWindow);
    }
    
    // Fallback for iOS 13-14
    window ??= UIKit.UIApplication.SharedApplication?.Windows?.FirstOrDefault(w => w.IsKeyWindow);
```

**Rationale:** Future-proofs the codebase for iOS updates; follows Apple's recommended approach.

---

### Decision 37: AppPageTemplate Event Lifecycle Management

**Date:** 2026-02-09

**Decision:** Move event subscriptions (`Loaded`, `SizeChanged`) from constructor to `OnHandlerChanged`/`OnHandlerChanging` lifecycle methods.

**Previous Approach:** Events subscribed in constructor, never unsubscribed.

**New Approach:**
```csharp
protected override void OnHandlerChanged()
{
    base.OnHandlerChanged();
    if (Handler != null)
    {
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
    }
}

protected override void OnHandlerChanging(HandlerChangingEventArgs args)
{
    base.OnHandlerChanging(args);
    if (args.OldHandler != null)
    {
        Loaded -= OnLoaded;
        SizeChanged -= OnSizeChanged;
    }
}
```

**Rationale:** Prevents memory leaks from orphaned event handlers. Consistent with Decision 21 pattern already used by `LogoDisc` and `ExerciseTile`.

---

### Decision 38: SafeAreaService Integration in AppPageTemplate

**Date:** 2026-02-09

**Decision:** Use `SafeAreaService` with fallback to tested defaults instead of hardcoded values only.

**Previous Approach:** Hardcoded values (27dp top, 24dp bottom) based on Samsung S22 testing.

**New Approach:**
```csharp
private void ConfigureSafeAreas()
{
    const double defaultTopInset = 27;
    const double defaultBottomInset = 24;
    
    double topInset = defaultTopInset;
    double bottomInset = defaultBottomInset;
    
    if (_safeAreaService != null)
    {
        _safeAreaService.Refresh();
        if (_safeAreaService.TopInset > 0 && _safeAreaService.TopInset <= MaxTopInset)
            topInset = _safeAreaService.TopInset;
        if (_safeAreaService.BottomInset > 0 && _safeAreaService.BottomInset <= MaxBottomInset)
            bottomInset = _safeAreaService.BottomInset;
    }
    
    safeAreaTopRow.Height = new GridLength(topInset);
    safeAreaBottomRow.Height = new GridLength(bottomInset);
}
```

**Rationale:** Enables proper safe area handling on iOS devices (notches, Dynamic Island) while maintaining tested fallbacks.

---

### Decision 39: Remove Shadowed INotifyPropertyChanged Events

**Date:** 2026-02-09

**Decision:** Remove `public new event PropertyChangedEventHandler? PropertyChanged;` declarations from all pages.

**Problem:** Pages were declaring a `new` event that shadowed the base `BindableObject.PropertyChanged` event. This created confusion and potential binding issues.

**Solution:** Remove the shadowing declarations. `ContentPage` inherits from `BindableObject` which already implements `INotifyPropertyChanged`. Call `base.OnPropertyChanged(propertyName)` directly in property setters.

**Pages Updated:**
- LoginPage
- RegistrationPage
- ForgottenPasswordPage
- MainExercisesPage
- SettingsPage
- TermsConditionsPage

**Rationale:** Cleaner code; prevents binding confusion; follows MAUI best practices.

---

### Decision 40: CancellationToken Support in PhysioContentService

**Date:** 2026-02-09

**Decision:** Add `CancellationToken` parameter to `LoadContentAsync()` method.

**Signature Change:**
```csharp
// Before
public async Task<PhysioContent> LoadContentAsync()

// After
public async Task<PhysioContent> LoadContentAsync(CancellationToken cancellationToken = default)
```

**Implementation:** Token passed to `_loadLock.WaitAsync()` and `JsonSerializer.DeserializeAsync()`.

**Rationale:** iOS-Safe Rule #10 requires cancellation support for long-running operations. iOS can suspend apps quickly; cancellation tokens allow graceful handling.

---

### Decision 41: Progress Bar Sizing Tokens

**Date:** 2026-02-09

**Decision:** Add `ProgressBarHeight` and `ProgressBarCornerRadius` tokens to Sizing.xaml.

**Values:**
| Token | Phone | Tablet |
|-------|-------|--------|
| ProgressBarHeight | 10 | 12 |
| ProgressBarCornerRadius | 5 | 6 |

**Usage:** MainExercisesPage hero card progress bar now uses these tokens instead of hardcoded `HeightRequest="10"` / `CornerRadius="5"`.

**Rationale:** Token-driven sizing per core principles; eliminates magic numbers; enables future theming.

---

## Section 4.2: ExerciseDetailPage Implementation

**Date:** 2026-02-09  
**Author:** Lead Engineer

---

### Decision 42: ExerciseDetailPage Layout Mode

**Decision:** Use `AppPageTemplate` with `ShowFullBody="True"` for full-screen immersive content.

**Alternatives Considered:**
1. Standard mode with header (inconsistent with exercise focus)
2. Custom layout bypassing template (violates template pattern)
3. FullBody mode (consistent with other content pages, immersive)

**Rationale:** FullBody mode provides edge-to-edge content, matches InformationPage pattern, and maintains template consistency.

---

### Decision 43: Video Player - CommunityToolkit.Maui.MediaElement

**Decision:** Use `CommunityToolkit.Maui.MediaElement` version 4.1.1 for video playback.

**Alternatives Considered:**
1. Platform-specific video controls (complex, requires multiple implementations)
2. WebView with embedded video (poor native controls)
3. CommunityToolkit.Maui.MediaElement (cross-platform, maintained, native controls)

**Rationale:** MediaElement provides native video controls on all platforms with a single API. Version 4.1.1 is compatible with .NET 10 MAUI's `$(MauiVersion)`.

**Configuration:**
```xml
<PackageReference Include="CommunityToolkit.Maui.MediaElement" Version="4.1.1" />
```

```csharp
builder.UseMauiCommunityToolkitMediaElement()
```

---

### Decision 44: Video Overlay Placement

**Decision:** Fullscreen video overlay sits **outside** `AppPageTemplate` content area.

**Implementation:** Video overlay is a separate `Grid` at the same level as `AppPageTemplate`, using `IsVisible` binding to toggle display.

**Rationale:** Overlay must cover the entire screen including any safe areas. Placing inside template would clip to content bounds.

---

### Decision 45: Sticky Footer Navigation

**Decision:** Implement back/done buttons as sticky footer, always visible at screen bottom.

**Implementation:** 
- Footer is last child in `VerticalStackLayout`, pushed to bottom via `VerticalOptions="End"` and content `VerticalOptions="FillAndExpand"`
- Buttons use full-width layout with proper spacing
- Safe area padding applied at footer level

**Rationale:** Consistent with mobile UX patterns; critical actions remain accessible during scroll.

---

### Decision 46: Responsive Tablet/Phone Layout

**Decision:** Use `IsTablet` and `IsPhone` boolean properties with `IsVisible` bindings.

**Implementation:**
```csharp
public bool IsTablet => DeviceInfo.Idiom == DeviceIdiom.Tablet;
public bool IsPhone => !IsTablet;
```

**Layout Differences:**
| Element | Phone | Tablet |
|---------|-------|--------|
| Content | Stacked vertical | Side-by-side (video left, details right) |
| Video thumbnail | Full width, AspectFill | Square, rounded corners |
| Instructions | Full width list | Contained in scrollable panel |

**Rationale:** Larger tablet screens benefit from horizontal layouts; phone layouts optimized for thumb reach and scrolling.

---

### Decision 47: Exercise Data Flow

**Decision:** Use Shell `QueryProperty` to pass `exerciseKey` string, then load from `PhysioContentService`.

**Implementation:**
```csharp
[QueryProperty(nameof(ExerciseKey), "exerciseKey")]
public partial class ExerciseDetailPage : ContentPage
```

**Navigation:**
```csharp
await Shell.Current.GoToAsync($"{nameof(ExerciseDetailPage)}?exerciseKey={exerciseKey}");
```

**Rationale:** 
- String key is lightweight for navigation
- Service already has caching; no redundant data passing
- Follows existing MainExercisesPage pattern

---

### Decision 48: InstructionStep Model

**Decision:** Create simple `InstructionStep` model with `Number` and `Text` properties.

**Location:** `Models/InstructionStep.cs`

**Usage:** Numbered instruction list built dynamically from `Exercise.Instructions` array.

**Rationale:** Separating number from text allows styled display (bold number, regular text) via `BindableLayout`.

---

### Decision 49: DisplayAlertAsync Migration

**Decision:** Use `DisplayAlertAsync` instead of deprecated `DisplayAlert`.

**Impact:** Updated all `DisplayAlert` calls in:
- `ExerciseDetailPage.xaml.cs`
- `MainExercisesPage.xaml.cs`

**Rationale:** `DisplayAlert` is obsolete in .NET 10 MAUI. `DisplayAlertAsync` is the recommended replacement.

---

### Decision 50: ExerciseProgressPage Entry Binding Type

**Date:** 2026-02-13

**Decision:** Use `string` properties for Entry field bindings, not `int?`.

**Context:** The reps/sets Entry fields on ExerciseProgressPage were initially bound to `int?` properties. This caused issues on Android emulators where the soft keyboard would not appear when tapping the fields.

**Root Cause:** The `Entry.Text` property expects a `string`. MAUI's default type conversion for nullable integers (`int?`) does not work reliably with Entry controls on Android, particularly affecting input focus and keyboard activation.

**Solution:** 
- Changed bound properties from `int?` to `string` (`RepsText`, `SetsText`)
- Added computed `int?` properties (`Reps`, `Sets`) that parse the string for validation
- Added `TapGestureRecognizer` on Border as additional fallback

**Known Issue:** Android emulator may still show "Choose input method" dialog instead of keyboard for numeric Entry fields in constrained layouts. This does NOT occur on physical devices (tested on S22 and tablets).

**Recommendation:** Always test Entry fields on physical devices. Emulator keyboard issues are a known Android emulator limitation.

---

### Decision 51: ExerciseProgressPage FullBodyContent Layout

**Date:** 2026-02-13

**Decision:** Use `ShowFullBody="True"` with `FullBodyContent` instead of separate `HeroContent`/`BodyContent`.

**Context:** Initial implementation placed form controls in `HeroContent` and comments in `BodyContent`. On smaller screens (S22), the INFO button overlapped the discomfort slider due to fixed hero height.

**Solution:** Converted to `FullBodyContent` mode where all form elements scroll together as a single unit.

**Trade-off:** Loses fixed hero/body visual separation but provides reliable layout on all screen sizes.

---

### Decision 52: ButtonSecondary Transparent Background

**Date:** 2026-02-13

**Decision:** Change `ButtonSecondary` style to use `Transparent` background instead of `Surface`.

**Location:** `Theming/Controls.xaml`

**Rationale:** Secondary buttons (like BACK, INFO) should blend with their container background rather than having a white fill. This provides better visual hierarchy with primary buttons and works correctly when placed over non-white backgrounds.

**Impact:** All buttons using `ButtonSecondary` style now have transparent backgrounds with visible borders.

---

### Decision 53: Exercise Completion Tracking via Shell Query Parameters

**Date:** 2026-02-13

**Decision:** Track exercise completion using a `HashSet<string>` in MainExercisesPage and pass completion status via Shell query parameters.

**Implementation:**
- `MainExercisesPage` maintains `_completedExerciseKeys` HashSet
- `ExerciseProgressPage` navigates back with `?completedExerciseKey={key}` on save
- `MainExercisesPage` receives key via `[QueryProperty]` and calls `MarkExerciseComplete()`
- Grid is rebuilt to show tick on completed exercise

**Rationale:**
- Simple in-memory tracking suitable for MVP/demo
- Shell query parameters are the standard MAUI way to pass data between pages
- No persistence layer needed yet (TODO for production)

**Trade-off:** Completion state is lost when app restarts. Future implementation should persist to Preferences or database.

---

### Decision 54: SVG Checkmark Icon Instead of Unicode Character

**Date:** 2026-02-13

**Decision:** Use an SVG image (`check_white.svg`) for completion tick instead of Unicode checkmark character.

**Context:** The exercise tile completion indicator was displaying as a green dot instead of a tick. Investigation revealed that the OpenSans font (the only font registered in the app) does not include the "✓" (U+2713) Unicode checkmark character.

**Root Cause:** When a glyph is missing from the specified font, Android renders it as blank/invisible. The Label content was not visible, leaving only the green circular Border background - appearing as a "green dot".

**Solution:**
- Created `Resources/Images/check_white.svg` with Material Design checkmark path
- Replaced `Label` with `Image` in ExerciseTile.xaml
- Updated code-behind to size icon instead of font

**Rationale:**
- SVG images render reliably across all platforms
- No font dependency issues
- Scales cleanly at any size
- Consistent appearance on Android, iOS, Windows

**Lesson Learned:** Always verify that Unicode symbols exist in the app's registered fonts. For icons, prefer SVG images or icon fonts (FluentUI, Material Icons) over Unicode characters.

---

## Section 4.6: SkipToNextSessionPage & Footer Refactoring

**Date:** 2026-02-16  
**Author:** Lead Engineer

---

### Decision 55: SkipToNextSessionPage Layout

**Date:** 2026-02-16

**Decision:** Create SkipToNextSessionPage using standard AppPageTemplate mode with hero card and body comments editor.

**Layout:**
- Hero: Bordered card (BrandMist background, BrandPrimary stroke) with instruction text
- Body: Comments editor with internal scrolling using `CommentsEditorHeight` token
- Footer: BACK + CONFIRM buttons via template footer

**Styling:** Hero card matches MainExercisesPage instruction card styling for visual consistency.

**Navigation:**
- BACK: `Shell.Current.GoToAsync("..")` - returns to previous page
- CONFIRM: Shows loading overlay, then navigates to `../../MainExercisesPage`

---

### Decision 56: Page Constructor Command Initialization Order

**Date:** 2026-02-16

**Decision:** Establish consistent constructor initialization order for all pages:

```csharp
public MyPage()
{
    // 1. Initialize commands FIRST
    BackCommand = new Command(async () => await NavigateBackAsync());
    
    // 2. Set BindingContext
    BindingContext = this;
    
    // 3. Initialize XAML
    InitializeComponent();
}
```

**Rationale:** Commands must be initialized before BindingContext is set, so XAML bindings can resolve to valid command objects.

**Pages Updated:** SkipToNextSessionPage (new), verified against InformationPage, SettingsPage, ExerciseProgressPage patterns.

---

### Decision 57: AppPageTemplate Footer/Safe Area Simplification

**Date:** 2026-02-16

**Decision:** Simplify AppPageTemplate from 5 rows to 4 rows by combining footer and safe area.

**Previous Structure (5 rows):**
- Row 0: Safe area top
- Row 1: Header
- Row 2: Content (ScrollView)
- Row 3: Footer buttons
- Row 4: Safe area bottom (BoxView with RowSpan for background)

**New Structure (4 rows):**
- Row 0: Safe area top
- Row 1: Header
- Row 2: Content (ScrollView)
- Row 3: VerticalStackLayout containing:
  - Footer buttons (Grid with padding)
  - Safe area spacer (BoxView with dynamic height)

**Benefits:**
- Single container with Surface background (no spanning BoxView)
- Cleaner XAML structure
- Safe area height set via `safeAreaBottomSpacer.HeightRequest` instead of row definition
- Easier to debug layout issues

---

### Decision 58: Android Bottom Safe Area = 0

**Date:** 2026-02-16

**Decision:** Set Android bottom safe area inset to 0 in SafeAreaService.

**Problem:** On Samsung S22 (and other Android devices), the app was adding bottom padding ABOVE the visible Android navigation bar, creating double spacing.

**Root Cause:** Even with `WindowCompat.SetDecorFitsSystemWindows(Window, false)` for edge-to-edge mode, the Android navigation bar (whether 3-button or gesture) still reserves its own space. Our safe area padding was duplicating this.

**Solution:**
```csharp
// In SafeAreaService.Refresh() for Android:
BottomInset = 0;
```

**Rationale:**
- With 3-button navigation: System reserves space; content doesn't draw under it
- With gesture navigation: Gesture bar is minimal; footer padding (8dp) is sufficient
- iOS still needs bottom safe area for home indicator

**Platform Behavior:**
- **Android:** BottomInset = 0 (nav bar handles its own space)
- **iOS:** BottomInset = 24-34dp (home indicator area)
- **Windows:** BottomInset = 0 (no safe area concerns)

---

### Decision 59: Footer Padding with Minimal Bottom Spacing

**Date:** 2026-02-16

**Decision:** Update `SpaceFooterPadding` to include 8dp bottom padding.

**Previous:** `Phone="16,12,16,0"` (no bottom padding, relying entirely on safe area)

**New:** `Phone="16,12,16,8"` (8dp bottom padding for visual breathing room)

**Rationale:** With Android bottom safe area set to 0, a small bottom padding provides comfortable spacing between buttons and screen edge without excessive white space.

**Token Value:**
```xml
<OnIdiom x:Key="SpaceFooterPadding" x:TypeArguments="Thickness" 
         Phone="16,12,16,8" 
         Tablet="24,16,24,8" 
         Default="16,12,16,8"/>
```

---

## Section 5: Google Play Deployment

### Decision 60: Disable IL Linker for Release Builds

**Date:** 2026-02-23

**Problem:** App worked perfectly in Debug mode but crashed immediately when navigating to Settings, Login, or Registration pages in the Release build deployed to Google Play Store.

**Root Cause Analysis:**
- Release builds use IL Linker (trimmer) to reduce APK size
- Linker removes types that appear "unused" at compile time
- Pages registered via DI and Shell routes use reflection at runtime
- Linker incorrectly stripped these "unused" types
- Result: `TypeLoadException` or silent crash when navigating

**Decision:** Disable the IL Linker and AOT compilation for Release builds.

**Configuration Added to .csproj:**
```xml
<!-- Release Linker Settings - Use partial trimming to prevent runtime crashes -->
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <TrimMode>partial</TrimMode>
    <RunAOTCompilation>false</RunAOTCompilation>
</PropertyGroup>

<!-- Android-specific: Disable R8/ProGuard code shrinking for Release -->
<PropertyGroup Condition="$(TargetFramework.Contains('-android')) and '$(Configuration)' == 'Release'">
    <AndroidLinkMode>None</AndroidLinkMode>
    <AndroidEnableProfiledAot>false</AndroidEnableProfiledAot>
</PropertyGroup>
```

**Trade-offs:**
| Aspect | Before (Linker On) | After (Linker Off) |
|--------|-------------------|-------------------|
| AAB Size | ~54 MB | ~86 MB |
| Stability | Crashes in Release | Stable |
| Startup Time | Slightly faster (AOT) | Standard |

**Alternatives Considered:**
1. **TrimmerRoots.xml** - Explicitly list types to preserve (complex, error-prone)
2. **[DynamicallyAccessedMembers] attributes** - Requires modifying many files
3. **Disable linker entirely** - Chosen for simplicity and demo timeline

**Future Optimization:** Once stable, could re-enable partial linking with proper TrimmerRoots.xml to preserve DI and navigation types while trimming unused library code.

---

### Decision 61: Android Signing Configuration

**Date:** 2026-02-23

**Decision:** Generate dedicated keystore for Google Play Store signing.

**Keystore Details:**
- **File:** `canine-physio.keystore` (stored in solution root)
- **Alias:** `caninephysio`
- **Validity:** 25 years
- **Key Algorithm:** RSA 2048-bit

**Configuration in .csproj:**
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

**Security Note:** For production, passwords should be stored in environment variables or CI/CD secrets, not in the .csproj file. Current approach is acceptable for demo/internal testing.

**Critical:** The keystore file must be preserved for all future updates. Losing it means unable to update the app on Play Store.

---

### Decision 62: AAB vs APK Format

**Date:** 2026-02-23

**Decision:** Use AAB (Android App Bundle) format for Google Play Store distribution.

**Rationale:**
- Google Play requires AAB for new apps (mandatory since August 2021)
- AAB allows Play Store to generate optimized APKs per device
- Results in smaller download sizes for users
- Supports Play Feature Delivery for future modularization

**Configuration:**
```xml
<AndroidPackageFormat>aab</AndroidPackageFormat>
```

**Note:** For direct device installation (sideloading), change to `apk` and rebuild.

---

### Decision 63: Application ID Format

**Date:** 2026-02-23

**Decision:** Use lowercase-only Application ID: `com.yourcompany.caninephysio`

**Previous (Invalid):** `com.yourcompany.canine-physio` (hyphens not allowed)

**Rationale:**
- Android package names must be lowercase
- Cannot contain hyphens (only letters, numbers, underscores, dots)
- Must be globally unique on Play Store
- Cannot be changed after first Play Store upload

**Current Value:** `com.yourcompany.caninephysio`

**Future:** Change to actual company domain before production release.

---

### Decision 64: Missing StaticResource Definitions Causing Release Crashes

**Date:** 2026-02-25

**Problem:** App crashed when navigating from HelloBuddyPage to Settings, Login, or Registration pages in Release mode ("Start Without Debugging"). InformationPage worked fine. With the debugger attached, all pages worked.

**Root Cause:** Three missing `StaticResource` definitions:
1. **`SpaceStackXs`** — referenced 10 times across 4 crashing pages (SettingsPage, LoginPage, RegistrationPage, ForgottenPasswordPage) but never defined in Spacing.xaml
2. **`TextH3Style`** — used in ForgottenPasswordPage but should be `HeadingH3`
3. **`TextBaseStyle`** — used in ForgottenPasswordPage but should be `TextMdStyle`

**Why Debug Worked:** Debug mode uses `DynamicResource`-like fallback behavior and doesn't throw on missing `StaticResource` references. Release mode with IL trimming aggressively resolves resources at build time and crashes on missing definitions.

**Why InformationPage Worked:** InformationPage did not reference `SpaceStackXs` or the incorrect style names.

**Fix Applied:**
1. Added `SpaceStackXs` to Spacing.xaml (Phone=4, Tablet=6, Default=4) — positioned before `SpaceStackSm` in the layout spacing section
2. Changed `TextH3Style` → `HeadingH3` in ForgottenPasswordPage.xaml
3. Changed `TextBaseStyle` → `TextMdStyle` in ForgottenPasswordPage.xaml

**Files Modified:**
- `Theming/Spacing.xaml` — Added `SpaceStackXs` OnIdiom Thickness definition
- `Views/ForgottenPasswordPage.xaml` — Corrected two style references

**Lesson Learned:** Always verify that every `StaticResource` reference has a corresponding definition. Missing resources may silently work in Debug but crash in Release. Consider adding a build-time check or style audit step.

---

### Decision 65: Disable Android Marshal Methods for .NET 10

**Date:** 2026-02-25

**Problem:** Signed AAB built for Google Play crashed immediately on startup with:
```
java.lang.UnsatisfiedLinkError: No implementation found for void
crc6488302ad6e9e4df1a.MauiApplication.n_onCreate()
```

**Root Cause:** .NET 10 Android introduced **marshal methods** — a feature that generates static JNI method registrations at build time instead of using dynamic registration at runtime. The ExoPlayer library (used by CommunityToolkit.Maui.MediaElement) had Boolean/Byte type mismatches in its Java bindings that caused marshal method generation to produce invalid native method stubs.

**Investigation Timeline:**
1. First fix attempt: `PublishTrimmed=false` — did NOT resolve the crash
2. Device verification via `adb shell dumpsys package` confirmed correct version was installed
3. Fresh `adb logcat` confirmed identical `UnsatisfiedLinkError` on `MauiApplication.n_onCreate()`
4. Identified 456 build warnings were ExoPlayer marshal method Boolean/Byte mismatch warnings
5. Added `AndroidEnableMarshalMethods=false` — warnings dropped from 456 to 166, confirming the fix

**Decision:** Disable marshal methods for Android Release builds.

**Configuration Added:**
```xml
<PropertyGroup Condition="$(TargetFramework.Contains('-android')) and '$(Configuration)' == 'Release'">
    <AndroidLinkMode>None</AndroidLinkMode>
    <AndroidEnableProfiledAot>false</AndroidEnableProfiledAot>
    <AndroidEnableMarshalMethods>false</AndroidEnableMarshalMethods>
</PropertyGroup>
```

**Also Added:**
```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <PublishTrimmed>false</PublishTrimmed>
    <RunAOTCompilation>false</RunAOTCompilation>
</PropertyGroup>
```

**Trade-offs:**
| Aspect | Marshal Methods On | Marshal Methods Off |
|--------|-------------------|--------------------|
| JNI Registration | Static (build-time) | Dynamic (runtime) |
| Startup Time | Slightly faster | Standard |
| Compatibility | Breaks with ExoPlayer bindings | Fully compatible |
| Build Warnings | 456 (ExoPlayer mismatches) | 166 (XamlC/binding only) |
| AAB Size | ~86 MB | ~86 MB |

**Key Evidence:** The 290 ExoPlayer marshal method warnings (456→166) disappeared when the setting was disabled, confirming ExoPlayer's Java bindings were the source of the JNI registration failure.

**Future:** Re-evaluate when CommunityToolkit.Maui.MediaElement updates its ExoPlayer bindings to be compatible with .NET 10 marshal methods.

---

## Live Issue Tracking

### Issue: Release Build Page Navigation Crashes

**Status:** ✅ RESOLVED (2026-02-25)

**Affected Pages:**
- SettingsPage ✅ Fixed
- LoginPage ✅ Fixed
- RegistrationPage ✅ Fixed
- ForgottenPasswordPage ✅ Fixed

**Working Pages (no issues):**
- HelloBuddyPage ✅
- InformationPage ✅

**Timeline:**
1. **2026-02-23 08:30** - Initial deployment to Google Play Internal Testing
2. **2026-02-23 09:00** - User reports crashes on S22 device
3. **2026-02-23 09:05** - Investigation began, compared working vs crashing pages
4. **2026-02-23 09:08** - Found invalid Thickness syntax in SettingsPage.xaml
5. **2026-02-23 09:10** - Fixed SettingsPage, rebuilt AAB (v1, 54MB)
6. **2026-02-23 09:12** - Hypothesis: This is a Release-only linker issue, not XAML syntax
7. **2026-02-23 09:13** - Disabled IL Linker in .csproj
8. **2026-02-23 09:18** - Rebuilt AAB with linker disabled (v2, 86MB)
9. **2026-02-25** - Found root cause: missing `SpaceStackXs` StaticResource + incorrect style names
10. **2026-02-25** - Fixed 3 missing/incorrect resource references (see Decision 64)
11. **2026-02-25** - User confirmed navigation fix working on device

**Root Cause:** Missing `SpaceStackXs` StaticResource definition in Spacing.xaml (referenced 10 times across 4 pages) and two incorrect style names in ForgottenPasswordPage. See Decision 64.

---

### Issue: AAB Startup Crash (UnsatisfiedLinkError)

**Status:** ✅ RESOLVED (2026-02-26)

**Symptom:** App crashed immediately on startup when installed from signed AAB. Error: `java.lang.UnsatisfiedLinkError: No implementation found for void MauiApplication.n_onCreate()`

**Timeline:**
1. **2026-02-25** - v1.0.3 AAB uploaded, crashed on startup
2. **2026-02-25** - First fix: `PublishTrimmed=false` (v1.0.4) — still crashed
3. **2026-02-25** - Verified v1.0.4 installed via `adb shell dumpsys package`
4. **2026-02-25** - Second fix: `AndroidEnableMarshalMethods=false` — build warnings dropped 456→166
5. **2026-02-26** - Built Release APK v1.0.5, installed directly on device
6. **2026-02-26** - User confirmed app working

**Root Cause:** .NET 10 marshal methods incompatible with ExoPlayer Java bindings (CommunityToolkit.Maui.MediaElement). See Decision 65.

---

## Section 5.1: Navigation UX Refinement — Header Info Icon & Button Cleanup

**Date:** 2026-02-26  
**Author:** Lead Engineer

---

### Decision 66: Replace In-Page INFO Buttons with Header Info Icon

**Date:** 2026-02-26

**Decision:** Remove all in-body/footer INFO buttons from exercise pages and replace with a persistent HeaderBlock info icon (top-right).

**Pages Updated:**
- **MainExercisesPage** — Removed INFO from footer (was `PrimaryButtonText="INFO"`), added `ShowHeaderIcon="True"` with `info_icon.svg`
- **ExerciseDetailPage** — Added `ShowHeaderIcon="True"` with `info_icon.svg` (previously had no INFO button)
- **ExerciseProgressPage** — Removed in-body `<Button Text="INFO">`, added `ShowHeaderIcon="True"` with `info_icon.svg`

**Rationale:**
- Consistent one-tap access to safety guidance across all exercise pages
- Removes duplicate navigation patterns (footer button + potential header icon)
- Industry-standard pattern: header icons for global actions, body/footer for contextual actions
- Reduces visual clutter on exercise pages

**Icon Approach:** Pre-colored SVG (`info_icon.svg`) using `BrandPrimaryDark` (#28404F) color baked into the vector path. Matches the project's existing SVG pattern (`check_white.svg`). MAUI's resizetizer compiles SVGs to PNGs at build time, preserving the embedded color.

---

### Decision 67: Remove SKIP from MainExercisesPage, Add to ExerciseProgressPage

**Date:** 2026-02-26

**Decision:** Relocate the SKIP action from MainExercisesPage's footer to ExerciseProgressPage's body.

**Previous Layout:**
- MainExercisesPage footer: SKIP (secondary) + INFO (primary)
- ExerciseProgressPage body: INFO button (right-aligned, secondary style)

**New Layout:**
- MainExercisesPage: No footer (`ShowFooter="False"`) — clean hub page
- ExerciseProgressPage body: SKIP button (right-aligned, secondary style, replaces INFO's position)

**Rationale:**
- SKIP is a contextual workflow action relevant only when logging progress
- Hub pages (MainExercisesPage) shouldn't lead with "skip" — users should engage with exercises
- Placing SKIP on ExerciseProgressPage gives users an exit ramp when they're mid-flow
- Footer template only supports 2 buttons (BACK + SAVE already occupy them), so SKIP is placed in-body

**Navigation:** SKIP navigates to `SkipToNextSessionPage` (same route, same destination).

---

### Decision 68: BrandPrimaryDark Icon Color Enforcement

**Date:** 2026-02-26

**Decision:** All icons in the HeaderBlock and Shell tab bar must use `BrandPrimaryDark` (#28404F).

**Implementation:**
1. **Header icon** — SVG asset (`info_icon.svg`) has BrandPrimaryDark color embedded in its vector paths (stroke="#28404F", fill="#28404F")
2. **Shell tab bar** — Updated `Controls.xaml` Shell style:
   - `Shell.TabBarForegroundColor` changed from `BrandPrimary` → `BrandPrimaryDark`
   - `Shell.TabBarTitleColor` changed from `BrandPrimary` → `BrandPrimaryDark`
   - `Shell.TabBarUnselectedColor` remains `TextMuted` (for contrast)

**Rationale:**
- `BrandPrimaryDark` (#28404F) has better contrast against `PageBackground`/`BrandMist` (light backgrounds) than `BrandPrimary` (#6392AE)
- Consistent dark icon tint creates a more professional, cohesive header appearance
- Pre-colored SVG approach avoids MAUI platform tinting inconsistencies

---

### Decision 69: Info Icon Asset — Pre-Colored SVG

**Date:** 2026-02-26

**Decision:** Use a pre-colored SVG file (`Resources/Images/info_icon.svg`) for the info icon rather than a FontImageSource or tinted PNG.

**Alternatives Considered:**
1. **FontImageSource** — Requires an icon font (FluentUI, Material Icons) not currently in the project. Would be the most flexible (color set via `Color` property) but adds a font dependency.
2. **Tinted PNG** — MAUI `ImageButton` has no built-in `TintColor` property. Would require custom handlers or behaviors.
3. **Pre-colored SVG** — Color baked into the SVG vector paths. Consistent with existing `check_white.svg` pattern.

**Chosen:** Option 3 (pre-colored SVG).

**SVG Design:** Circle with "i" letterform — stroke circle (r=10, stroke-width=2), vertical line (y 11–17), filled dot (r=1.25 at y=7.5). All paths use `#28404F` (BrandPrimaryDark).

**Trade-off:** Changing the icon color requires editing the SVG file rather than a XAML token binding. Acceptable because the BrandPrimaryDark color is a stable brand value unlikely to change frequently.

**Future Enhancement:** If/when an icon font is added to the project (e.g., for tab bar icons), header icons could migrate to FontImageSource for token-driven coloring.

---

## Section 5.2: Tab Navigation Architecture

**Date:** 2026-02-26
**Author:** Lead Engineer

---

### Decision 70: Post-Login Tab Bar — 3 Tabs via Shell TabBar

**Date:** 2026-02-26

**Decision:** Add a bottom tab bar for post-login navigation using MAUI Shell's built-in `TabBar > Tab > ShellContent` hierarchy.

**Tabs:**

| Tab | Icon | Page | Route |
|-----|------|------|-------|
| Exercises | `tab_exercises.svg` | MainExercisesPage | `MainExercises` |
| Progress | `tab_progress.svg` | ProgressTabPage | `ProgressTab` |
| Skip | `tab_skip.svg` | SkipToNextSessionPage | `SkipSession` |

**Shell Structure:**
```xml
<TabBar Route="PreLogin">          <!-- Single ShellContent = no tab bar -->
    <ShellContent Route="HelloBuddy" .../>
</TabBar>

<TabBar Route="PostLogin">          <!-- 3 Tabs = tab bar visible -->
    <Tab Title="Exercises" Icon="tab_exercises.svg">
        <ShellContent Route="MainExercises" .../>
    </Tab>
    <Tab Title="Progress" Icon="tab_progress.svg">
        <ShellContent Route="ProgressTab" .../>
    </Tab>
    <Tab Title="Skip" Icon="tab_skip.svg">
        <ShellContent Route="SkipSession" .../>
    </Tab>
</TabBar>
```

**Pre-Login:** Single `ShellContent` in PreLogin `TabBar` means MAUI renders no tab bar (need ≥2 items). StyleGuidePage moved to a registered route.

**Tab Styling (Controls.xaml):**
- `TabBarBackgroundColor` → `TabBarSurface` (#D9FFFFFF, 85% white opacity)
- `TabBarForegroundColor` / `TabBarTitleColor` → `TextMuted` (no active highlight distinction)
- `TabBarUnselectedColor` → `TextMuted`

**Rationale:**
- Tabs provide persistent navigation to key workflow destinations
- Hub (Exercises), Progress, and Skip are peer-level concerns
- No active/selected highlight — all tabs use `TextMuted` for a subtle, non-distracting appearance
- Pre-colored SVG tab icons follow the same pattern as header info icon

---

### Decision 71: Platform-Specific CustomShellRenderer — Tab Reselection

**Date:** 2026-02-26

**Decision:** Create platform-specific `CustomShellRenderer` classes to handle tab reselection (tapping the already-active tab).

**Problem:** MAUI Shell ignores taps on the currently selected tab. If a user navigates deep (e.g., Exercises → ExerciseDetail → ExerciseProgress) and taps the Exercises tab again, nothing happens.

**Solution:**
- **Android:** `CustomShellRenderer` extends `ShellRenderer`, overrides `OnTabReselected(ShellSection)` to call `PopToRootAsync()`.
- **iOS:** `CustomShellRenderer` extends `ShellRenderer`, uses `UITabBarControllerDelegate.ShouldSelectViewController` to detect same-tab taps and pop to root.

**Registration (MauiProgram.cs):**
```csharp
#if ANDROID
builder.ConfigureMauiHandlers(handlers =>
    handlers.AddHandler<Shell, Canine_Physio_App.Platforms.Android.CustomShellRenderer>());
#elif IOS
builder.ConfigureMauiHandlers(handlers =>
    handlers.AddHandler<Shell, Canine_Physio_App.Platforms.iOS.CustomShellRenderer>());
#endif
```

**Files Created:**
- `Platforms/Android/CustomShellRenderer.cs`
- `Platforms/iOS/CustomShellRenderer.cs`

**Rationale:** Standard UX expectation — tapping a tab should always return to its root page.

---

### Decision 72: Stack-Preserving Tab Back Navigation — CurrentItem vs GoToAsync

**Date:** 2026-02-26

**Decision:** Use `TabBar.CurrentItem = section` for BACK navigation from Skip/Progress tabs instead of `Shell.Current.GoToAsync(route)`.

**Problem:** `GoToAsync("//MainExercises")` navigates to the route, which always resets the target tab's navigation stack to root. If a user was on ExerciseDetail → Progress → BACK, they'd land on MainExercisesPage instead of ExerciseDetail.

**Solution:**
1. `AppShell.PreviousTabSection` (static `ShellSection?` property) stores the actual ShellSection reference when the user switches tabs.
2. BACK commands on ProgressTabPage and SkipToNextSessionPage set `tabBar.CurrentItem = PreviousTabSection` — this switches tabs without touching navigation stacks.
3. Fallback: If `PreviousTabSection` is null, falls back to `GoToAsync("//MainExercises")`.

**Stack Cleanup — Conditional:**
- `OnNavigated` only pops the previous tab's stack when the **destination** is the Exercises tab (`MainExercises`).
- Navigating to Skip or Progress preserves the source tab's stack.
- This means: ExerciseDetail → Progress → BACK → ExerciseDetail ✅

**Files Modified:**
- `AppShell.xaml.cs` — Added `PreviousTabSection` property, conditional stack cleanup
- `ProgressTabPage.xaml.cs` — `BackCommand` uses `CurrentItem` assignment
- `SkipToNextSessionPage.xaml.cs` — `NavigateBackAsync()` uses `CurrentItem` assignment

**Rationale:** Users expect BACK to return to their previous context, not reset to a tab root. Only Exercises (the hub) should reset stacks, since it's the primary destination.

---

### Decision 73: ProgressTabPage — Placeholder with BACK Button

**Date:** 2026-02-26

**Decision:** Create a placeholder ProgressTabPage as the second tab, with a BACK button for contextual return navigation.

**Current State:** Placeholder with centered "Progress tracking" heading and subtitle. Will be replaced with full progress overview in a future sprint.

**Features:**
- Header info icon → InformationPage
- Footer BACK button → Returns to source tab (via `PreviousTabSection`)
- No primary button (nothing to save/confirm yet)

**File:** `Views/ProgressTabPage.xaml` + `Views/ProgressTabPage.xaml.cs`

**Registration:** `builder.Services.AddTransient<ProgressTabPage>()` in MauiProgram.cs

---

### Decision 74: InformationPage — BACK Always Visible

**Date:** 2026-02-26

**Decision:** Always show the BACK button on InformationPage, regardless of login context.

**Previous Approach:** `ShowBackButton` property detected post-login route context and hid the BACK button (tabs handle navigation). `ShowFooter` was bound to `ShowBackButton`.

**Problem:** Users need to return from InformationPage regardless of which tab/page they arrived from. The info icon pushes InformationPage onto the current tab's navigation stack, so `GoToAsync("..")` correctly pops back to the calling page.

**New Approach:**
- `ShowFooter="True"` (hardcoded, not bound)
- Removed `ShowBackButton` property and post-login detection logic from `OnAppearing`
- `NavigateBackCommand` uses `GoToAsync("..")` — works from any tab or pre-login context

**Rationale:** Since InformationPage is always a pushed route (never a tab root), `GoToAsync("..")` always pops correctly. No special handling needed.

---

### Decision 75: SKIP Button Removed from ExerciseProgressPage

**Date:** 2026-02-26

**Decision:** Remove the SKIP button from ExerciseProgressPage body. The Skip tab in the tab bar now handles this navigation.

**Previous State:** ExerciseProgressPage had a right-aligned SKIP button in its body content (placed where the INFO button previously was).

**Change:** Removed SKIP `<Button>` from XAML and `SkipCommand` / `OnSkipAsync()` from code-behind.

**Rationale:** With a dedicated Skip tab always visible in the tab bar, a redundant in-page SKIP button creates duplicate navigation paths and visual clutter.

---

## Section 5.3: Skip Exercise Feature & Tab Bar Refinements

**Date:** 2026-03-10  
**Author:** Lead Engineer

---

### Decision 76: Skip Exercise via Tab Bar — Context-Aware SkipToNextSessionPage

**Date:** 2026-03-10

**Decision:** Reuse the existing `SkipToNextSessionPage` as a dual-mode page that handles both exercise-level and session-level skips, rather than creating a separate `SkipExercisePage`.

**Context Detection:**
- `SessionStateService.CurrentExerciseKey` is set when the user is viewing an exercise on `ExerciseDetailPage`
- If set → **Exercise skip mode**: skips only that exercise, marks it with a red X
- If null → **Session skip mode**: original behaviour, skips the entire session

**Implementation:**
```csharp
protected override async void OnAppearing()
{
    _exerciseKeyToSkip = _sessionStateService.CurrentExerciseKey;
    _isExerciseSkipMode = !string.IsNullOrEmpty(_exerciseKeyToSkip);

    if (_isExerciseSkipMode)
        await ConfigureExerciseSkipMode();
    else
        ConfigureSessionSkipMode();
}
```

**Exercise Skip Mode:** 
- Header title shows exercise name (e.g., "Baited Back Stretch")
- Hero text: "Skipping this exercise will mark it with a red cross..."
- Comments placeholder: "Why are you skipping this exercise?"
- CONFIRM navigates to `//MainExercises?skippedExerciseKey={key}`
- No revert button (exercise skips don't use SessionStateService persistence)

**Session Skip Mode:**
- Header title: "Skip to Next Session"
- Hero text: "Skipping this session will mark it as complete..."
- Comments placeholder: "Why are you skipping this session?"
- CONFIRM calls `SessionStateService.TrySkip()` then navigates to `//MainExercises`
- Revert button available when skip targets tomorrow

**Rationale:** Single page reduces maintenance burden. Mode detection via `CurrentExerciseKey` is reliable because it's set in `ExerciseDetailPage.OnAppearing` and cleared in `MainExercisesPage.OnAppearing`.

**Files Modified:**
- `Views/SkipToNextSessionPage.xaml.cs` — Added dual-mode logic, `_isExerciseSkipMode`, `_exerciseKeyToSkip`
- `Views/SkipToNextSessionPage.xaml` — Added `x:Name` references for `heroMainText` and `heroSubText`

---

### Decision 77: Dynamic Tab Icon Swapping — >> vs >

**Date:** 2026-03-10

**Decision:** Dynamically swap the Skip tab icon based on the current page context:
- `>>` (double chevron, `tab_skip.svg`) when on `MainExercisesPage` — indicates session skip
- `>` (single chevron, `tab_skip_exercise.svg`) when on `ExerciseDetailPage` — indicates exercise skip

**Implementation:**
```csharp
// In AppShell.OnNavigated
skipTab.Icon = isOnExerciseDetail
    ? "tab_skip_exercise.svg"
    : "tab_skip.svg";
```

**Skip Tab `x:Name`:** Added `x:Name="skipTab"` to the Skip `<Tab>` element in AppShell.xaml for programmatic access.

**SVG Assets Created:**
- `tab_skip.svg` — Double chevron `>>` (BrandPrimaryDark #28404F)
- `tab_skip_exercise.svg` — Single chevron `>` (BrandPrimaryDark #28404F)

**Rationale:** Visual indicator communicates the skip scope to the user without requiring additional UI text. Users learn that `>` skips the current exercise while `>>` skips the entire session.

**Files Modified:**
- `AppShell.xaml` — Added `x:Name="skipTab"` to Skip Tab
- `AppShell.xaml.cs` — Added icon swap logic in `OnNavigated`
- `Resources/Images/tab_skip_exercise.svg` — Created
- `Resources/Images/tab_skip.svg` — Updated to double chevron

---

### Decision 78: ExerciseTile IsSkipped — Red X Badge

**Date:** 2026-03-10

**Decision:** Add an `IsSkipped` bindable property to `ExerciseTile` that shows a red X badge (mirroring the green tick for `IsComplete`).

**Implementation:**
- New `IsSkippedProperty` bindable property (bool, default false)
- XAML: `<Border>` with `BackgroundColor="{StaticResource Error}"` containing `<Image Source="cross_white.svg" />`
- Positioned same as completion tick (top-right corner) with same sizing logic
- `cross_white.svg` created with white X paths

**Badge Sizing (same as tick):**
- Phone: 24dp circle
- Tablet: 32dp circle
- Icon: 60% of badge size

**Mutual Exclusivity:** `IsComplete` and `IsSkipped` use separate `IsVisible` bindings. If an exercise is subsequently completed after being skipped, `MainExercisesPage.MarkExerciseComplete()` sets `IsComplete=true` and `IsSkipped=false`, converting the red X to a green tick.

**Files Modified:**
- `Components/Controls/ExerciseTile.xaml` — Added `skippedCross` Border with `cross_white.svg`
- `Components/Controls/ExerciseTile.xaml.cs` — Added `IsSkippedProperty`, sizing in `ApplyResponsiveSizing()`
- `Resources/Images/cross_white.svg` — Created

---

### Decision 79: CurrentExerciseKey — Exercise-Level Skip Context

**Date:** 2026-03-10

**Decision:** Add a `CurrentExerciseKey` property to `SessionStateService` to track which exercise the user is currently viewing.

**Lifecycle:**
1. **Set** in `ExerciseDetailPage.OnAppearing()` — `_sessionStateService.CurrentExerciseKey = _exerciseKey`
2. **Read** in `SkipToNextSessionPage.OnAppearing()` — determines exercise vs session skip mode
3. **Cleared** in `MainExercisesPage.OnAppearing()` — `_sessionStateService.CurrentExerciseKey = null`
4. **Cleared** in `SkipToNextSessionPage.ConfirmSkipAsync()` (exercise mode only) — before navigating back

**Property Type:** `string?` (nullable). Null = no exercise context = session skip mode.

**Not Persisted:** This is an in-memory property only (not saved to `Preferences`). It represents transient navigation state, not persistent session state.

**Rationale:** Lightweight context passing between pages via a singleton service. Avoids complex Shell query parameters for tab navigation scenarios where `GoToAsync` query strings don't naturally apply.

**Files Modified:**
- `Services/SessionStateService.cs` — Added `CurrentExerciseKey` property with XML doc
- `Views/ExerciseDetailPage.xaml.cs` — Sets in `OnAppearing`, constructor takes `SessionStateService`
- `Views/MainExercisesPage.xaml.cs` — Clears in `OnAppearing`
- `Views/SkipToNextSessionPage.xaml.cs` — Reads in `OnAppearing`, clears on confirm

---

### Decision 80: Skipped Exercise Completion — Cross → Tick

**Date:** 2026-03-10

**Decision:** When a skipped exercise is subsequently completed, the red X badge converts to a green tick.

**Implementation in `MainExercisesPage.MarkExerciseComplete()`:**
```csharp
// Remove from skipped set if previously skipped (cross → tick)
_skippedExerciseKeys.Remove(completionKey);

// Update the view model
var exercise = _exercises.FirstOrDefault(e => e.Key == exerciseKey);
if (exercise != null)
{
    exercise.IsComplete = true;
    exercise.IsSkipped = false;
}
```

**Rationale:** Users should be able to change their mind. If they skip an exercise but later go back and complete it, the completion takes precedence. The exercise counts as completed (not skipped) for progress tracking and auto-advance logic.

**Auto-Advance Check:** Both `MarkExerciseComplete()` and `MarkExerciseSkipped()` check `_exercises.All(e => e.IsComplete || e.IsSkipped)` to trigger auto-advance. A completed exercise that was previously skipped still satisfies this condition.

---

### Decision 81: Tab Bar Hidden on ProgressTabPage and ExerciseProgressPage

**Date:** 2026-03-10

**Decision:** Hide the entire tab bar when the user is on `ProgressTabPage` (tab root) or `ExerciseProgressPage` (pushed page). The Skip tab is not appropriate on either page.

**Problem:** `Shell.TabBarIsVisible="False"` in XAML is unreliable on pushed pages in .NET MAUI. Setting it on the page element works for tab roots but not for pages pushed onto a tab's navigation stack.

**Solution — Multi-Layer Approach:**

1. **XAML:** `Shell.TabBarIsVisible="False"` on ProgressTabPage and ExerciseProgressPage ContentPage elements
2. **Programmatic (pushed pages):** `Shell.SetTabBarIsVisible(this, false)` in `ExerciseProgressPage.OnAppearing()`, restored in `OnDisappearing()`
3. **Centralized (AppShell.OnNavigated):** FlyoutItemIsVisible suppression for all tab items:
```csharp
if (isOnExerciseProgress || isOnProgressTab)
{
    foreach (var item in CurrentItem.Items)
        item.FlyoutItemIsVisible = false;
    Shell.SetTabBarIsVisible(CurrentPage, false);
}
else
{
    foreach (var item in CurrentItem.Items)
        item.FlyoutItemIsVisible = true;
}
```

**Why FlyoutItemIsVisible?** On some MAUI versions/platforms, `Shell.SetTabBarIsVisible` alone doesn't fully hide the tab bar. Setting `FlyoutItemIsVisible = false` on all `ShellSection` items removes them from the tab bar renderer entirely.

**Rationale:** Belt-and-braces approach ensures tab bar is hidden regardless of MAUI Shell rendering quirks. The central AppShell.OnNavigated handler catches all navigation events including tab switches.

**Files Modified:**
- `AppShell.xaml.cs` — Added `isOnProgressTab` and `isOnExerciseProgress` checks with FlyoutItemIsVisible toggling
- `Views/ExerciseProgressPage.xaml.cs` — Added `Shell.SetTabBarIsVisible(this, false/true)` in OnAppearing/OnDisappearing
- `Views/ProgressTabPage.xaml` — Added `Shell.TabBarIsVisible="False"` attribute

---

### Decision 82: Auto-Advance Includes Skipped Exercises

**Date:** 2026-03-10

**Decision:** Auto-advance to the next session triggers when ALL exercises in the current session are either completed or skipped, not just completed.

**Previous Behavior:** Auto-advance only triggered when `_exercises.All(e => e.IsComplete)`.

**New Behavior:** Auto-advance triggers when `_exercises.All(e => e.IsComplete || e.IsSkipped)`.

**Auto-Advance Flows:**
- **AM → PM (multi-session):** Shows "Session Complete — Well done! Loading your afternoon exercises."
- **PM → Done (multi-session) or Single → Done:** Shows "All Done! — Great work! You've completed all exercises for today."

**Rationale:** If a user skips all remaining exercises, they should advance to the next session rather than being stuck. Skipping is a deliberate decision to move forward, so it should be treated as "addressed" for session progression.

**Implementation:** Both `MarkExerciseComplete()` and `MarkExerciseSkipped()` include the auto-advance check at the end.

---

### Decision 83: AM Always Loads First — No Time-of-Day Logic

**Date:** 2026-03-10

**Decision:** `ResolveCurrentPeriod()` always returns the first session (AM) regardless of the current time of day.

**Previous Behavior:**
```csharp
private static string ResolveCurrentPeriod(List<DailySession> dailySessions)
{
    if (dailySessions.Count <= 1) return dailySessions[0].Period;
    var cutoff = new TimeSpan(12, 0, 0); // noon
    return DateTime.Now.TimeOfDay < cutoff
        ? dailySessions[0].Period   // AM before noon
        : dailySessions[1].Period;  // PM after noon
}
```

**New Behavior:**
```csharp
private static string ResolveCurrentPeriod(List<DailySession> dailySessions)
{
    return dailySessions.Count > 0 ? dailySessions[0].Period : string.Empty;
}
```

**Rationale:** The user might want to complete AM exercises late in the afternoon and then skip PM. With time-of-day logic, opening the app after noon would show PM exercises, preventing the user from completing AM first. The auto-advance mechanism handles AM → PM progression when all AM exercises are completed or skipped.

**Trade-off:** Users must always start with AM even if they only want to do PM. This is intentional — the physiotherapy programme is sequential.

**Files Modified:**
- `Services/SessionStateService.cs` — Simplified `ResolveCurrentPeriod()` to return first session always
```
