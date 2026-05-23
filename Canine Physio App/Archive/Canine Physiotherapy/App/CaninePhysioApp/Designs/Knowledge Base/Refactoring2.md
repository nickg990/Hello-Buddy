You are refactoring .NET MAUI custom controls to be responsive across Android devices (Pixel 6 emulator API 35, Samsung S22, Samsung S9). 

Scope (only these controls):
- Controls/StyledInputBox.xaml
- Controls/StyledButton.xaml
- Controls/ExerciseTile.xaml
- Controls/HeaderBlock.xaml
- Controls/LoadingSpinner.xaml
- Controls/LogoDisc.xaml
- Controls/PageTemplate.xaml
- Controls/SineWaveBlock.xaml

Hard constraints:
- Do NOT change public bindable property names or remove them.
- Do NOT change the meaning of existing bindings/commands.
- Preserve current visual intent, but remove fixed sizing/margins that were tuned for emulator.
- Prefer XAML-only changes. Code-behind is allowed ONLY where XAML cannot express proportional sizing (LogoDisc is the likely exception).

Primary goals:
1) Remove fixed pixel sizing that causes cross-device drift:
   - Avoid fixed WidthRequest/HeightRequest defaults that force layout.
   - Replace layout “nudges” (big margins/translations) with proportional layout where possible.
2) Centralise typography/colours/spacing:
   - Move inline hex colours and FontSize values into app resources (Colors, Typography, Spacing) and use StaticResource/DynamicResource.
3) Ensure controls tolerate:
   - different screen densities
   - different font scaling (Android Accessibility font size Large)
   - narrow and wide layouts

Deliverables:
- Updated component XAML.
- If necessary, minimal code-behind only for proportional drawing/layout (e.g., LogoDisc sizing).
- Short summary of key changes.

Component-specific instructions:

A) StyledButton.xaml
- Current: uses WidthRequest and HeightRequest bindable properties.
- Refactor:
  - Do not hard-force WidthRequest unless explicitly set by consumer. Default should be "Auto" sizing.
  - Prefer MinimumHeightRequest + Padding for consistent touch targets.
  - Keep ButtonHeight property but treat it as a MINIMUM rather than absolute if possible.
  - Remove inline FontSize; use a Typography Style (e.g., ButtonText).
  - Ensure HorizontalOptions defaults to Fill (but still works in Auto layouts).
- Acceptance:
  - Buttons look consistent on S22/S9 without requiring ButtonWidth everywhere.

B) StyledInputBox.xaml
- Current: Border has Padding="12,0" and HeightRequest bound to InputHeight; Grid hosts an Entry and optional Button.
- Refactor:
  - Avoid forcing HeightRequest if it causes text clipping when font scale is Large.
  - Prefer MinimumHeightRequest; use VerticalOptions Center and Padding that scales.
  - Ensure internal grid columns allow Entry to expand: ColumnDefinitions="*,Auto" (or "*", if no button).
  - Ensure Entry respects text scaling and does not clip vertically.
  - Move colours/strokes to resources; keep bindable properties but provide resource defaults.
- Acceptance:
  - Entry text is vertically centred and not clipped on large font settings; input fills width.

C) ExerciseTile.xaml
- Current: TileHeight/TileWidth are bindable, but inner elements include fixed 20x20 icon sizes and fixed FontSize 12/11, plus margins.
- Refactor:
  - Do not require TileWidth/TileHeight to be set; let tiles scale within parent grid/collection.
  - Replace fixed icon sizes with resource-driven sizes (e.g., IconSize=20) and allow override.
  - Replace fixed FontSize values with styles (TileTitle, TileSubtitle).
  - Ensure layout uses Grid with Auto/* rows and consistent spacing resources.
- Acceptance:
  - Tiles remain readable and aligned on small screens and don’t look “off” on high-density devices.

D) HeaderBlock.xaml
- Current: FontSize="36" and "10" inline; StackLayout Padding="20,5,20,20".
- Refactor:
  - Replace fixed FontSizes with Typography styles (HeaderTitle, HeaderSubtitle).
  - Replace padding with spacing resources (PagePadding, SpacingS/L).
  - Ensure text wraps gracefully where needed.
- Acceptance:
  - Header doesn’t dominate on small screens and scales cleanly with font size changes.

E) LoadingSpinner.xaml
- Current: SpinnerSize and MessageFontSize bindables.
- Refactor:
  - Keep SpinnerSize but avoid forcing WidthRequest/HeightRequest unless required; prefer setting ActivityIndicator scale or container sizing.
  - Use Typography style for message by default; keep MessageFontSize as an override.
  - Ensure overlay/backdrop colours come from resources.
- Acceptance:
  - Spinner looks consistent and centered; message doesn’t clip.

F) LogoDisc.xaml  (high priority)
- Current: lots of fixed WidthRequest/HeightRequest (240/260/etc) and translations and a hard-coded label margin.
- Refactor plan:
  - Introduce a single bindable property (if one already exists, use it; if not, add DiscSize) OR derive from control Width/Height.
  - Make every ellipse/image size proportional to DiscSize (e.g., 1.0x, 1.08x, 0.71x).
  - Replace fixed TranslationX/Y and label bottom margin with proportional offsets derived from DiscSize.
  - If proportional binding is too hard in XAML, implement minimal code-behind:
      - Handle SizeChanged / OnSizeAllocated
      - Compute and set sizes/translations via element references
  - Move colours to resources.
- Acceptance:
  - LogoDisc renders identically (relative proportions) on S22 and emulator; no “floating” text or misalignment.

G) PageTemplate.xaml
- Current: outer Grid with row defs; margin "20,0,20,20"; SineWaveBlock HeightRequest="30".
- Refactor:
  - Replace hard-coded margins with PagePadding resource.
  - Ensure content areas (AboveWaveContent, WaveContent) do not rely on fixed spacing.
  - Ensure it behaves correctly with safe areas and different aspect ratios.
  - Keep wave height configurable via resource/bindable property.
- Acceptance:
  - All pages using PageTemplate get consistent padding/spacing across devices.

H) SineWaveBlock.xaml
- Current: HeightRequest bindable BlockHeight.
- Refactor:
  - Keep BlockHeight, but provide a sensible default via resource (e.g., WaveHeight).
  - Ensure it stretches horizontally and doesn’t assume a fixed width.
- Acceptance:
  - Wave block looks consistent; height can be tuned centrally.

Global acceptance criteria:
- No text clipping at Android Accessibility “Large” font.
- No reliance on fixed pixel nudges for positioning.
- Colours, typography, and spacing come from shared resources by default.
- Components remain backwards-compatible for existing pages (bindings/props still work).
