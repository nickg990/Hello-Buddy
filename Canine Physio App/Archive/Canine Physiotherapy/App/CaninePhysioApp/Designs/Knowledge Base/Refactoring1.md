You are refactoring .NET MAUI XAML pages to be responsive across Android devices (Pixel 6 emulator API 35, Samsung S22, Samsung S9) with consistent spacing and typography. 

Scope: Refactor only these XAML Views (no behavioural changes; keep bindings/commands the same):
- TermsConditionsPage.xaml
- ExerciseDetailPage.xaml
- ForgottenPasswordPage.xaml
- HelloBuddyPage.xaml
- InformationPage.xaml
- LoginPage.xaml
- MainExercisesPage.xaml
- RegistrationPage.xaml
- SettingsPage.xaml

Constraints:
- Do NOT change ViewModel property/command names or bindings.
- Keep custom controls as-is: controls:PageTemplate, controls:StyledButton, controls:StyledInputBox, controls:LoadingSpinner, controls:LogoDisc.
- Keep PageTemplate slots (AboveWaveContent, WaveContent) but make the content inside them responsive.
- Preserve visual intent (same hierarchy/sections), but remove “emulator-tweaks” that depend on fixed pixels.

Primary goals:
1) Remove fixed layout sizing that causes cross-device drift:
   - Replace ColumnDefinitions like "4,Auto,8,260" with responsive grids.
   - Avoid fixed WidthRequest/HeightRequest and big margins used to position content (e.g., Margin="0,135,0,0").
   - Replace ButtonWidth="140" usage where possible with grid star sizing / fill behaviour.
2) Centralise colours, typography, and spacing into shared resources:
   - Move repeated hex colours (#3D81A9, #D7F2FD, #D9534F, #3CB371, #555555, #666666, #CCCCCC, #D9D9D9, overlays) into a Colors resource dictionary.
   - Move repeated FontSize values into Styles (H1/H2/Body/Caption/SectionHeader etc), using your Montserrat fonts.
   - Introduce spacing resources (XS/S/M/L/XL) and use them for Padding/Margin/RowSpacing/ColumnSpacing.
3) Ensure pages behave well under font scaling and small screens:
   - Ensure no clipped/overlapping text at Android Accessibility font size “Large”.
   - Use ScrollView where content can exceed vertical space.
   - Prefer Grid with Auto/* rows and avoid “nudge margins”.

Implementation plan (do these steps):
A) Add resource dictionaries (or extend existing ones) and merge in App.xaml:
   - Resources/Styles/Colors.xaml (Primary, Surface, Background, TextPrimary, TextSecondary, Divider, Error, Success, Overlay, CardBackground).
   - Resources/Styles/Spacing.xaml (Thickness & doubles: SpacingXS=4, S=8, M=12, L=16, XL=24, PagePadding=20, etc).
   - Resources/Styles/Typography.xaml (Styles for Label: H1, H2, Body, Caption, SectionLabel, Link; set FontFamily and FontSize consistently).
   Use StaticResource/DynamicResource in all pages, remove inline hex colours and most inline FontSize.

B) Refactor form grids (LoginPage, RegistrationPage, ForgottenPasswordPage):
   - Replace the fixed-width input grid:
     From: ColumnDefinitions="4,Auto,8,260" HorizontalOptions="Start"
     To:   ColumnDefinitions="Auto,*" ColumnSpacing="{StaticResource SpacingM}"
           HorizontalOptions="Fill" (or FillAndExpand)
           Set input control to HorizontalOptions="FillAndExpand".
   - Keep labels in column 0 and input in column 1.
   - Use consistent row spacing via resources.
   - Remove unnecessary HorizontalOptions="Start" that prevents stretching.

C) Refactor action button rows (many pages):
   - Where there are Back/Done or Back/Login buttons, use a Grid:
     ColumnDefinitions="*,*" (or "Auto,*" depending on design)
     Set StyledButton HorizontalOptions="Fill" and remove ButtonWidth when possible.
   - If StyledButton requires a width for consistent look, prefer MinimumWidthRequest rather than fixed width; if that’s not available, keep ButtonWidth but allow the grid to space/align properly and avoid hard-coded margins.

D) Refactor MainExercisesPage layout:
   - Remove ScrollView Margin="0,135,0,0" and instead place the exercise grid in its own Grid.Row with RowDefinitions like:
     RowDefinitions="Auto,Auto,Auto,Auto,*"
     Put helper text, progress, CTA buttons, etc in rows 0-3, then put ScrollView/Grid in row 4 with * height.
   - Ensure ExerciseGrid uses HorizontalOptions="Fill" and responsive spacing resources.

E) Refactor ExerciseDetailPage:
   - Replace fixed image/video sizing (HeightRequest="180", WidthRequest="70", etc) with:
     - Grid rows (Auto/*) or OnIdiom for height where needed.
     - Make the button row responsive (no fixed widths).
   - Ensure content is scrollable if needed.
   - Keep the overlay close button but ensure it aligns with safe areas and doesn’t rely on fixed margins alone.

F) General clean-ups:
   - Prefer VerticalStackLayout/HorizontalStackLayout over StackLayout where appropriate (MAUI performance).
   - Replace divider BoxView height requests with a shared Divider style.
   - Standardise page background usage (avoid per-page hard-coded BackgroundColor unless necessary).
   - Add AutomationId to key inputs/buttons if not present (optional, but useful for UI testing).

Acceptance criteria:
- Each page renders correctly on S22 and emulator without manual “nudge” margins.
- No text overlaps/cuts off at system font scale Large.
- Forms fill width gracefully; inputs do not clip.
- Buttons align consistently and don’t look cramped on smaller screens.
- Colours and font sizes are controlled via resources/styles, not inline values.

Deliverables:
- Updated XAML pages.
- New/updated shared resource dictionaries + App.xaml merge updates.
- A short summary in PR notes of what changed and why (responsive layout + centralised styles).
