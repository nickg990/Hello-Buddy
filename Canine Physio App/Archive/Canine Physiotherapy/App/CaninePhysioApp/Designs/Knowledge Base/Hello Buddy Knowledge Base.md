# Hello Buddy Knowledge Base - Design Summary

## Overview
The Hello Buddy page is the central hub/welcome screen for the Canine Physiotherapy app built in .NET MAUI (.NET 10).

---

## Page Structure (Top to Bottom)

### 1. Header Block (`HeaderBlock` control)
- **Background**: White (#FFFFFF) - uses `SafeArea` resource
- **Title**: "Hello Buddy" - Color: #3D81A9 (`HeaderTitle`), Font: MontserratSemiBold, Size: 36
- **Subtitle**: "C A N I N E   P H Y S I O T H E R A P Y" (spaced letters) - Color: Black, Font: MontserratSemiBold, Size: 10

### 2. Logo Disc (`LogoDisc` control)
- **Size**: 260x260 (outer), 250x250 (inner blue disc)
- **Shadow**: #33000000, offset 3px right and down
- **Border**: White ellipse
- **Background**: #3D81A9 (`HeaderTitle`)
- **Logo Image**: `hellobuddy_logo.png`, 170x170, offset TranslationX="-30", TranslationY="-25"
- **Text**: "GETTING BETTER" at bottom, Color: #D8CDA3, Font: MontserratSemiBold, Size: 10
- **Z-Index**: Set to 1 in PageTemplate to appear above the sine wave

### 3. Sine Wave Block (`SineWaveBlock` control)
- **Block Height**: 410
- **Wave Amplitude**: 50
- **Color**: #3D81A9 (`HeaderTitle`)

### 4. Navigation Buttons (4x `StyledButton` controls)
- **Container**: StackLayout, Width: 280, Spacing: 15, centered, VerticalOptions: End
- **Button Height**: 48 (default)
- **Corner Radius**: 12
- **Font**: MontserratSemiBold, Size: 10
- **Shadow**: #33000000, offset 2px right and down
- **Text Style**: Uppercase with 2 spaces between letters

#### Button Order & Colors:
| Button | Background | Text Color | Text | Command |
|--------|------------|------------|------|---------|
| 1. Information | #D9D9D9 | Black | I  N  F  O  R  M  A  T  I  O  N | `NavigateToInformationCommand` |
| 2. Settings | #3D81A9 | White | S  E  T  T  I  N  G  S | `NavigateToSettingsCommand` |
| 3. Start Physio | #D9D9D9 | Black | S  T  A  R  T     P  H  Y  S  I  O | `NavigateToLoginCommand` |
| 4. Registration | #3D81A9 | White | R  E  G  I  S  T  R  A  T  I  O  N | `NavigateToRegistrationCommand` |

---

## Key Color Resources (from Colors.xaml)
| Resource Name | Hex Value | Usage |
|---------------|-----------|-------|
| `Primary` | #3D81A9 | App primary color |
| `PageBackground` | #D9D9D9 | Light gray background, secondary buttons |
| `HeaderTitle` | #3D81A9 | Blue - wave, disc, primary buttons, input borders |
| `SafeArea` | #FFFFFF | White - header block background |
| `Error` | #D9534F | Error message text |
| `Success` | #3CB371 | Success message text, completion ticks |
| `GhostButton` | #D7F2FD | Light blue for Back buttons |
| `Disabled` | #CCCCCC | Disabled button background |
| `BodyText` | #555555 | Secondary body text (e.g., T&C content) |
| `Divider` | #BBBBBB | Subtle divider lines |

---

## Reusable Controls

### PageTemplate
- Provides overall page layout with Grid rows
- **Row 0-1**: HeaderBlock (title, subtitle)
- **Row 2**: AboveWaveContent slot (e.g., LogoDisc) - has `ZIndex="1"`
- **Row 3**: SineWaveBlock + WaveContent slot - **Margin="20,0,20,20"**
- **Row 4**: Bottom safe area (30px)

### HeaderBlock
- White background
- Bindable: `Title`, `Subtitle`, `ShowSubtitle`

### SineWaveBlock
- Blue wave with configurable `BlockHeight` and `WaveAmplitude`

### LogoDisc
- Circular disc with shadow, white border, blue background
- Contains `hellobuddy_logo.png` image and "GETTING BETTER" text

### StyledButton
Reusable button with shadow effect and press feedback.

#### Properties:
| Property | Default | Description |
|----------|---------|-------------|
| `Text` | empty | Button text |
| `ButtonBackgroundColor` | #3D81A9 | Background color |
| `ButtonTextColor` | White | Text color |
| `ButtonWidth` | -1 (fill) | Width in px, or -1 to fill parent |
| `ButtonHeight` | 48 | Height in px |
| `ButtonHorizontalAlignment` | Fill | Layout alignment |
| `ButtonCornerRadius` | 12 | Corner radius |
| `ButtonFontSize` | 10 | Font size |
| `ButtonFontFamily` | MontserratSemiBold | Font family |
| `ShadowColor` | #33000000 | Shadow color |
| `ShadowOffsetX` | 2 | Shadow X offset |
| `ShadowOffsetY` | 2 | Shadow Y offset |
| `Command` | null | Click command |
| `ButtonBorderColor` | Transparent | Border color |
| `ButtonBorderWidth` | 0 | Border width |

#### StyledButton Variants:
| Variant | Background | Text | Shadow | Use Case |
|---------|------------|------|--------|----------|
| Primary | #3D81A9 | White | Default | Main actions (Login, Register, Next) |
| Ghost | #D7F2FD | Black | Default | Back buttons |
| Text-style | Transparent | #3D81A9 | Transparent | Secondary links (Forgot Password) |
| Disabled | #CCCCCC | White | Default | Disabled state |

#### Visual Feedback:
- **Pressed**: Button scales to 0.96 and opacity reduces to 0.7
- **Released**: Button scales back to 1.0 and opacity returns to 1.0

### StyledInputBox
Reusable input field with border styling (no underline).

#### Properties:
| Property | Default | Description |
|----------|---------|-------------|
| `Text` | empty | Two-way bound input text |
| `Placeholder` | empty | Grey placeholder text |
| `PlaceholderColor` | #919191 | Placeholder text color |
| `TextColor` | Black | Input text color |
| `BorderColor` | #3D81A9 | Border color |
| `BorderThickness` | 1.5 | Border width |
| `InputBackgroundColor` | White | Background color |
| `InputHeight` | 48 | Height of input box |
| `IsPassword` | false | Mask input for passwords |
| `Keyboard` | Default | Keyboard type (Email, Numeric, etc.) |
| `FontFamily` | MontserratRegular | Font family |
| `FontSize` | 14 | Font size |

### LoadingSpinner
Reusable loading indicator with optional message text.

#### Properties:
| Property | Default | Description |
|----------|---------|-------------|
| `IsLoading` | false | Controls visibility and animation |
| `Message` | Loading... | Text shown below spinner |
| `ShowMessage` | true | Whether to show message text |
| `SpinnerColor` | #3D81A9 | Spinner color |
| `SpinnerSize` | 40 | Width/height of spinner |
| `MessageColor` | #3CB371 | Message text color |
| `MessageFontFamily` | MontserratSemiBold | Font family |
| `MessageFontSize` | 14 | Font size |

#### LoadingSpinner Notes:
- Entire control hidden when `IsLoading` is false (via `IsVisible` on root ContentView)
- Uses `ActivityIndicator` for the spinning animation

### ExerciseTile
Reusable exercise tile for the Main Exercises grid.

#### Properties:
| Property | Default | Description |
|----------|---------|-------------|
| `ExerciseKey` | empty | Unique key for the exercise |
| `ExerciseName` | empty | Display name below tile |
| `ImageSource` | null | Thumbnail image source |
| `IsComplete` | false | Shows green tick when true |
| `TileWidth` | 150 | Tile width in px |
| `TileHeight` | 100 | Tile height in px |
| `Command` | null | Tap command (receives ExerciseKey) |

#### Visual Features:
- **Image**: Uses `Aspect="AspectFill"` to fill without distortion
- **Border**: #3D81A9, 2px, rounded corners (8px)
- **Shadow**: #33000000, offset 2px right and down
- **Completion tick**: Green (#3CB371) circle with white "✓", top-right corner
- **Tick visibility**: Only shown when `IsComplete` is true
- **Exercise name**: MontserratSemiBold, FontSize 11, centered, max 2 lines

---

## File Locations

### Views
| File | Status |
|------|--------|
| `Views/HelloBuddyPage.xaml` (.cs) | ✅ COMPLETED |
| `Views/RegistrationPage.xaml` (.cs) | ✅ COMPLETED |
| `Views/LoginPage.xaml` (.cs) | ✅ COMPLETED |
| `Views/ForgottenPasswordPage.xaml` (.cs) | ✅ COMPLETED |
| `Views/TermsConditionsPage.xaml` (.cs) | ✅ COMPLETED |
| `Views/InformationPage.xaml` (.cs) | ✅ COMPLETED |
| `Views/SettingsPage.xaml` (.cs) | ✅ COMPLETED |
| `Views/MainExercisesPage.xaml` (.cs) | ✅ COMPLETED |
| `Views/ExerciseDetailPage.xaml` (.cs) | ✅ COMPLETED |

### Controls
| File | Status |
|------|--------|
| `Controls/PageTemplate.xaml` (.cs) | ✅ |
| `Controls/HeaderBlock.xaml` (.cs) | ✅ |
| `Controls/SineWaveBlock.xaml` (.cs) | ✅ |
| `Controls/LogoDisc.xaml` (.cs) | ✅ |
| `Controls/StyledButton.xaml` (.cs) | ✅ |
| `Controls/StyledInputBox.xaml` (.cs) | ✅ |
| `Controls/LoadingSpinner.xaml` (.cs) | ✅ |
| `Controls/ExerciseTile.xaml` (.cs) | ✅ |

### Models
| File | Description |
|------|-------------|
| `Models/ContentSection.cs` | Section model with `Key`, `Header`, `Body` properties |
| `Models/AppContent.cs` | Root model containing `Information`, `TermsConditions`, and `Warnings` |
| `Models/Exercise.cs` | Exercise model with full properties (see below) |
| `Models/ExerciseSet.cs` | Exercise set with `Key`, `Description`, and `Exercises` list |
| `Models/PhysioContent.cs` | Root model containing `ExerciseSets` list |
| `Models/InstructionStep.cs` | Instruction step with `Number` and `Text` properties |

### Services
| File | Description |
|------|-------------|
| `Services/TextContentService.cs` | Loads and caches text content from AppContent.json |
| `Services/PhysioContentService.cs` | Loads and caches exercise content from PhysioContent.json |

### Resources
| File | Description |
|------|-------------|
| `Resources/Styles/Colors.xaml` | App color definitions |
| `Resources/Raw/AppContent.json` | App text content (Information, T&C, Warnings) |
| `Resources/Raw/PhysioContent.json` | Exercise content (exercise sets and exercises) |
| `Resources/Images/` | Exercise thumbnails (baited_back_stretch.jpg, etc.) |
| `Resources/Raw/` | Exercise videos (.mp4 format) |

---

## Exercise Model (Updated)

The Exercise model contains all data for an exercise:

    public class Exercise
    {
        public string Key { get; set; }           // Unique identifier (e.g., "raisedPoles")
        public string Title { get; set; }         // Display title (e.g., "Raised poles")
        public string Image { get; set; }         // Image filename without extension
        public string Summary { get; set; }       // Brief description shown above video
        public List<string> Instructions { get; set; }  // Numbered steps array
        public int Reps { get; set; }             // Number of repetitions
        public int Sets { get; set; }             // Number of sets
        public string VideoName { get; set; }     // Video filename (e.g., "raised_poles_video.mp4")
        
        // Derived property - Title Case version of Title
        public string Name => ToTitleCase(Title);
    }

### InstructionStep Model

    public class InstructionStep
    {
        public int Number { get; set; }   // Step number (1, 2, 3...)
        public string Text { get; set; }  // Instruction text
    }

---

## PhysioContent.json Structure (Updated)

    {
      "exerciseSets": [
        {
          "key": "hindlimbCore",
          "description": "These canine physiotherapy videos explain how to strengthen...",
          "exercises": [
            {
              "key": "baitedBackStretch",
              "title": "Baited back stretch",
              "image": "baited_back_stretch",
              "summary": "Neck and spine stretches on block – 2-3 stretches per direction...",
              "instructions": [
                "Raise your dog's forelimbs onto a flat, stable platform...",
                "Place your hand in front of your dog's hindlimbs...",
                "Take your hand from your dog's nose to the side..."
              ],
              "reps": 3,
              "sets": 1,
              "videoName": "baited_back_stretch_video.mp4"
            }
          ]
        }
      ]
    }

---

## PhysioContentService API (Updated)

    // Get the first exercise set
    var exerciseSet = await _physioService.GetFirstExerciseSetAsync();

    // Get a specific exercise set by key
    var exerciseSet = await _physioService.GetExerciseSetAsync("hindlimbCore");

    // Get a specific exercise within a set
    var exercise = await _physioService.GetExerciseAsync("hindlimbCore", "baitedBackStretch");

    // Get any exercise by key (searches all sets)
    var exercise = await _physioService.GetExerciseByKeyAsync("baitedBackStretch");

---

## Text Content System

### Overview
All static text content is stored in JSON files and loaded via content services. This provides:
- Single source of truth for all text
- Easy content updates without code changes
- Caching for performance (loaded once, reused)
- Extensible for future pages

### AppContent.json Structure

    {
      "information": [
        { "key": "about", "header": "About this app", "body": "..." },
        { "key": "physiotherapy", "header": "Physiotherapy", "body": "..." },
        { "key": "pain", "header": "Recognising pain", "body": "..." }
      ],
      "termsConditions": [
        { "key": "termsOfService", "header": "T  E  R  M  S     O  F     S  E  R  V  I  C  E", "body": "..." },
        { "key": "privacyPolicy", "header": "P  R  I  V  A  C  Y     P  O  L  I  C  Y", "body": "..." },
        { "key": "acceptableUse", "header": "A  C  C  E  P  T  A  B  L  E     U  S  E     P  O  L  I  C  Y", "body": "..." }
      ],
      "warnings": {
        "exerciseDisclaimer": "If you are concerned about pain or discomfort, stop immediately and contact your practitioner."
      }
    }

### TextContentService API

    // Get all sections for a page
    var sections = await _contentService.GetSectionsAsync("information");
    var sections = await _contentService.GetSectionsAsync("termsconditions");

    // Get a specific section by key
    var section = await _contentService.GetSectionAsync("termsconditions", "privacyPolicy");

    // Get a warning message
    var warning = await _contentService.GetWarningAsync("exerciseDisclaimer");

---

## Dependency Injection

### Registered Services (MauiProgram.cs)

    // Services
    builder.Services.AddSingleton<TextContentService>();
    builder.Services.AddSingleton<PhysioContentService>();

    // Pages (for constructor injection)
    builder.Services.AddTransient<InformationPage>();
    builder.Services.AddTransient<TermsConditionsPage>();
    builder.Services.AddTransient<MainExercisesPage>();
    builder.Services.AddTransient<ExerciseDetailPage>();

### MauiProgram.cs MediaElement Setup

    builder
        .UseMauiApp<App>()
        .UseMauiCommunityToolkitMediaElement()  // Required for video playback
        .ConfigureFonts(...)

### Required NuGet Packages

    <PackageReference Include="CommunityToolkit.Maui.MediaElement" Version="4.1.1" />
    
    <!-- Android-specific (conditional) -->
    <ItemGroup Condition="$(TargetFramework.Contains('-android'))">
        <PackageReference Include="Xamarin.AndroidX.LocalBroadcastManager" Version="1.1.0.10" />
    </ItemGroup>

---

## Navigation Map

    Hello Buddy (Home)
    ├── Information → Back → Hello Buddy
    ├── Settings → Back → Hello Buddy
    ├── Start Physio → Login
    │   ├── Forgot Password → Forgotten Password → Back → Login
    │   ├── Back → Hello Buddy
    │   └── Success → LoadingSpinner → Terms & Conditions
    │       ├── Back → Login
    │       └── Next (when accepted) → Main Exercises
    │           ├── Info → Information → Back → Main Exercises
    │           ├── Skip → Skip Session (TBD)
    │           ├── Progress Bar → Progress (TBD)
    │           └── Exercise Tile → Exercise Detail
    │               ├── Back → Main Exercises
    │               ├── Play Video → Fullscreen Video Overlay → Close → Exercise Detail
    │               └── Done → Alert → Main Exercises
    └── Registration → Back → Hello Buddy
        └── Success → Terms & Conditions (TODO)

### Route Registration (AppShell.xaml.cs)

    Routing.RegisterRoute(nameof(InformationPage), typeof(InformationPage));
    Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
    Routing.RegisterRoute(nameof(RegistrationPage), typeof(RegistrationPage));
    Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
    Routing.RegisterRoute(nameof(ForgottenPasswordPage), typeof(ForgottenPasswordPage));
    Routing.RegisterRoute(nameof(TermsConditionsPage), typeof(TermsConditionsPage));
    Routing.RegisterRoute(nameof(MainExercisesPage), typeof(MainExercisesPage));
    Routing.RegisterRoute(nameof(ExerciseDetailPage), typeof(ExerciseDetailPage));

---

## Page Implementations

### Exercise Detail Page ✅ COMPLETED
Dynamic page that displays exercise details based on selected tile from Main Exercises.

#### Navigation:
- **From**: MainExercisesPage (tile tap)
- **Route**: `ExerciseDetailPage?exerciseKey={key}`
- **Back**: Returns to MainExercisesPage
- **Done**: Shows alert, returns to MainExercisesPage

#### Layout Structure:
| Area | Content |
|------|---------|
| Header | Exercise title (Title Case) + subtitle |
| AboveWaveContent | Summary text + Video thumbnail |
| WaveContent | Numbered instructions (scrollable) + Sticky footer |

#### Video Thumbnail:
- **Container**: Border with #3D81A9 stroke, 2px, rounded 8px
- **Height**: 180px
- **Image**: Exercise thumbnail (`{image}.jpg`) with AspectFill
- **Play overlay**: Semi-transparent circle (#80000000) with white "▶" icon
- **Tap behavior**: Opens fullscreen video overlay

#### Fullscreen Video Overlay:
- **Background**: Black, covers entire screen (ZIndex="100")
- **Close button**: White "✕" in top-right corner
- **MediaElement**: Full width/height, AspectFit, auto-plays
- **Controls**: Built-in playback controls visible

#### Numbered Instructions:
- **Layout**: BindableLayout with StackLayout (Spacing="12")
- **Step number**: Column 0, width 28px, #3D81A9, right-aligned, MontserratSemiBold
- **Step text**: Column 1, black, MontserratRegular, FontSize 14

#### Sticky Footer:
| Element | Styling |
|---------|---------|
| Divider | #D8E5EE, 2px height |
| Reps/Sets | Side-by-side, "Reps: X" / "Sets: X", values in #3D81A9 |
| Buttons | Back (ghost #D7F2FD) left, Done (primary #3D81A9) right, both 140px |

#### Video Playback Notes:
- **Format**: MP4 required (H.264 + AAC codec)
- **.mov files**: Must be converted to .mp4 for Android compatibility
- **Source loading**: Use `MediaSource.FromResource(filename)` for local files
- **File location**: `Resources/Raw/` with MauiAsset build action

#### Code-Behind Properties:
| Property | Type | Description |
|----------|------|-------------|
| `ExerciseKey` | string | Query parameter, triggers data load |
| `ExerciseTitle` | string | Title Case exercise name |
| `Summary` | string | Brief description |
| `ThumbnailImage` | string | Image filename with extension |
| `InstructionSteps` | ObservableCollection | Numbered instruction list |
| `Reps` | int | Number of repetitions |
| `Sets` | int | Number of sets |
| `IsVideoPlaying` | bool | Controls overlay visibility |

#### Commands:
| Command | Action |
|---------|--------|
| `PlayVideoCommand` | Shows video overlay, loads and plays video |
| `CloseVideoCommand` | Stops video, hides overlay |
| `NavigateBackCommand` | Navigates to previous page |
| `MarkCompleteCommand` | Shows completion alert, navigates back |

### Main Exercises Page ✅ COMPLETED (Updated)
- **Loading Spinner**: Added overlay with spinner when navigating to exercise
- **IsNavigating property**: Controls spinner visibility
- **Spinner overlay**: Semi-transparent dark background (#80000000)
- **Reset**: `IsNavigating = false` in `OnAppearing()`

#### Navigation to Exercise Detail:

    private async Task OnNavigateToExercise(string exerciseKey)
    {
        IsNavigating = true;
        try
        {
            await Shell.Current.GoToAsync($"{nameof(ExerciseDetailPage)}?exerciseKey={exerciseKey}");
        }
        finally
        {
            // Spinner hidden in OnAppearing when returning
        }
    }

### Registration Page ✅ COMPLETED
- **Inputs**: Code, Email, Password, Confirm Password (4 fields)
- **Primary button**: Register (Margin="0,130,0,0")
- **Validation**: Email format, password length (8+), password match

### Login Page ✅ COMPLETED
- **Inputs**: Email, Password (2 fields)
- **Primary button**: Login (Margin="0,70,0,0")
- **Secondary button**: Forgot Password (text-style: transparent bg, #3D81A9 text, no shadow)
- **Loading state**: Shows LoadingSpinner with "Redirecting..." after successful login
- **Navigation**: Success → Terms & Conditions (always, no terms check in login)

### Forgotten Password Page ✅ COMPLETED
- **Inputs**: Email (1 field)
- **Primary button**: Send Reset (Margin="0,50,0,0")
- **Success message**: Centered in wave area with Margin="0,60,0,0" offset
- **Form stays visible** on success

### Terms & Conditions Page ✅ COMPLETED
- **Agreement switch**: Top of wave content, text left (FontSize="15"), switch right
- **Switch styling**: OnColor="#D7F2FD", ThumbColor="#3D81A9"
- **Collapsible sections**: 3 sections with +/− icons
- **Navigation**: Next → Main Exercises (when accepted)

### Information Page ✅ COMPLETED
- **Content**: Scrollable text sections loaded from JSON

### Settings Page ✅ COMPLETED
- **Items**: Program Code, Download Videos, Offline Caching, Notifications, Notification Time
- **Persistence**: MAUI Preferences API

---

## Video Implementation Details

### File Requirements:
- **Format**: MP4 (H.264 video + AAC audio)
- **Location**: `Resources/Raw/`
- **Build Action**: MauiAsset (automatic via wildcard in .csproj)
- **Naming**: Match `videoName` in PhysioContent.json

### .csproj Configuration:

    <!-- Raw Assets - includes videos and JSON files -->
    <MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />

    <!-- Android-specific dependencies for MediaElement -->
    <ItemGroup Condition="$(TargetFramework.Contains('-android'))">
        <PackageReference Include="Xamarin.AndroidX.LocalBroadcastManager" Version="1.1.0.10" />
    </ItemGroup>

### Loading Video Source:

    // Correct way to load local video files
    VideoPlayer.Source = MediaSource.FromResource(exercise.VideoName);
    
    // NOT this (won't work):
    // VideoPlayer.Source = exercise.VideoName;

### Converting .mov to .mp4:
Android cannot play .mov files. Convert using FFmpeg:

    ffmpeg -i video.mov -c:v libx264 -c:a aac -movflags +faststart video.mp4

Or use online converters like CloudConvert or VLC Media Player.

---

## Common Page Patterns

### Standard Form Page Structure (XAML)

    <controls:PageTemplate Title="Page Title" Subtitle="..." ShowSubtitle="True">
        <controls:PageTemplate.AboveWaveContent>
            <Grid HeightRequest="25" />
        </controls:PageTemplate.AboveWaveContent>
        
        <controls:PageTemplate.WaveContent>
            <Grid RowDefinitions="Auto,20,Auto,*,Auto,Auto">
                <!-- Row 0: Input fields grid -->
                <!-- Row 1: Error message area (20px fixed) -->
                <!-- Row 2: Primary action button -->
                <!-- Row 3: Loading spinner / flexible spacer -->
                <!-- Row 4: Secondary button (e.g., Forgot Password) -->
                <!-- Row 5: Back button -->
            </Grid>
        </controls:PageTemplate.WaveContent>
    </controls:PageTemplate>

### Page with Overlay Pattern (for fullscreen video, loading, etc.)

    <ContentPage>
        <Grid>
            <!-- Main content -->
            <controls:PageTemplate>
                ...
            </controls:PageTemplate>
            
            <!-- Overlay (shown conditionally) -->
            <Grid IsVisible="{Binding IsOverlayVisible}"
                  BackgroundColor="#80000000"
                  ZIndex="100">
                <!-- Overlay content -->
            </Grid>
        </Grid>
    </ContentPage>

### Tap-to-Action Pattern with Transparent BoxView

    <Grid>
        <Image Source="..." />
        <Border><!-- Overlay icon --></Border>
        
        <!-- Invisible tap layer on top captures all taps -->
        <BoxView Color="Transparent">
            <BoxView.GestureRecognizers>
                <TapGestureRecognizer Command="{Binding TapCommand}" />
            </BoxView.GestureRecognizers>
        </BoxView>
    </Grid>

### Numbered List with BindableLayout

    <StackLayout Spacing="12"
                 BindableLayout.ItemsSource="{Binding Items}">
        <BindableLayout.ItemTemplate>
            <DataTemplate>
                <Grid ColumnDefinitions="28,*" ColumnSpacing="8">
                    <Label Grid.Column="0"
                           Text="{Binding Number, StringFormat='{0}.'}"
                           HorizontalTextAlignment="End" />
                    <Label Grid.Column="1"
                           Text="{Binding Text}" />
                </Grid>
            </DataTemplate>
        </BindableLayout.ItemTemplate>
    </StackLayout>

---

## Switch Styling (Consistent across app)

    <Switch IsToggled="{Binding PropertyName}"
            OnColor="#D7F2FD"
            ThumbColor="#3D81A9"
            VerticalOptions="Center" />

---

## Remaining Pages

### Progress Page (TBD)
- Overall progress visualization
- Exercise completion history

### Skip Session Page (TBD)
- Confirm skip action
- Navigate to next session

---

## Tips for Future Agents

1. Always use `nameof()` for navigation routes
2. Check existing pages for patterns before creating new ones
3. Input grid columns are fixed: `4,Auto,8,260`
4. Button text uses 2 spaces between letters: `L  O  G  I  N`
5. Error color: #D9534F, Success color: #3CB371
6. Ghost button color: #D7F2FD with black text
7. Disabled button color: #CCCCCC with white text
8. Standard back navigation: `Shell.Current.GoToAsync("..")`
9. Message area is always 20px fixed height
10. StyledButton supports transparent background for text-style links
11. All pages use self-binding - page is its own BindingContext
12. LoadingSpinner - use for redirect states with IsLoading binding
13. Switch styling: OnColor="#D7F2FD", ThumbColor="#3D81A9"
14. Collapsible sections: Use +/− icons with TapGestureRecognizer
15. Side-by-side buttons: Use Grid ColumnDefinitions="Auto,*,Auto"
16. Terms persistence: Use Preferences.Set("TermsAccepted", true)
17. Divider lines: HeightRequest="1.5", BackgroundColor="#BBBBBB"
18. Body text in collapsibles: FontSize="14", TextColor="#555555"
19. ExerciseTile defaults: 150x100, use AspectFill for images
20. Image filenames use underscores: `baited_back_stretch.jpg`
21. Completion indicator: Green tick (#3CB371) with white "✓", not dots
22. Exercise content: Load from PhysioContent.json via PhysioContentService
23. Warning text: Load from AppContent.json warnings section
24. Login navigation: Always goes to Terms & Conditions (no terms check)
25. Terms Next button: Use `ChangeCanExecute()` to refresh enabled state
26. Video files: Must be MP4 format for Android compatibility
27. Video loading: Use `MediaSource.FromResource()` for local files
28. Video overlay: Use Grid with ZIndex="100" for fullscreen
29. Tap capture: Use transparent BoxView on top for reliable tap detection
30. Android MediaElement: Requires Xamarin.AndroidX.LocalBroadcastManager package
31. Instructions array: Store as List<string> in JSON, display as numbered steps
32. Title Case: Use `CultureInfo.CurrentCulture.TextInfo.ToTitleCase()` for exercise titles
33. Loading overlay on MainExercises: Use IsNavigating property with semi-transparent Grid