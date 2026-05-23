# Responsive Refactoring Test Matrix

## Overview
This document provides acceptance criteria and a test matrix for validating the responsive refactoring work across target Android devices.

## Target Devices
| Device | Screen Size | Resolution | Density |
|--------|-------------|------------|---------|
| Pixel 6 Emulator (API 35) | 6.4" | 1080 x 2400 | 411 DPI |
| Samsung Galaxy S22 | 6.1" | 1080 x 2340 | 425 DPI |
| Samsung Galaxy S9 | 5.8" | 1440 x 2960 | 570 DPI |

---

## Test Matrix

### 1. HelloBuddyPage
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| Logo disc centered and proportional | ☐ | ☐ | ☐ | Should scale based on DiscSize=260 |
| All 4 navigation buttons visible | ☐ | ☐ | ☐ | No clipping at bottom |
| Button spacing consistent | ☐ | ☐ | ☐ | Using SpacingL (16dp) |
| No horizontal scrolling | ☐ | ☐ | ☐ | Content fits width |

### 2. LoginPage
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| Logo disc visible above wave | ☐ | ☐ | ☐ | Proportional sizing |
| Form fields expand to fill width | ☐ | ☐ | ☐ | ColumnDefinitions="Auto,*" |
| Error labels appear correctly | ☐ | ☐ | ☐ | ErrorTextStyle applied |
| Login button centered | ☐ | ☐ | ☐ | HorizontalOptions="Center" |
| Forgotten Password and Register links work | ☐ | ☐ | ☐ | |

### 3. RegistrationPage
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| All form fields visible | ☐ | ☐ | ☐ | Email, Password, Confirm |
| Fields expand responsively | ☐ | ☐ | ☐ | No fixed ColumnDefinitions |
| Register button visible without scrolling | ☐ | ☐ | ☐ | Proper grid layout |
| Terms toggle working | ☐ | ☐ | ☐ | |

### 4. ForgottenPasswordPage
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| Email field expands to width | ☐ | ☐ | ☐ | |
| Send button properly positioned | ☐ | ☐ | ☐ | |
| Back navigation works | ☐ | ☐ | ☐ | |

### 5. TermsConditionsPage
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| Agreement toggle styled correctly | ☐ | ☐ | ☐ | Primary/Surface colors |
| Collapsible sections work | ☐ | ☐ | ☐ | Terms, Privacy, Acceptable Use |
| Scrollable content when expanded | ☐ | ☐ | ☐ | |
| Back/Next buttons visible | ☐ | ☐ | ☐ | No clipping |

### 6. MainExercisesPage
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| Header displays correctly | ☐ | ☐ | ☐ | Title "Physio Exercises" |
| Progress bar visible | ☐ | ☐ | ☐ | With percentage |
| ScrollView starts below header | ☐ | ☐ | ☐ | **CRITICAL: No Margin hack** |
| Exercise tiles laid out in 2 columns | ☐ | ☐ | ☐ | FlexLayout wrap |
| All tiles tappable | ☐ | ☐ | ☐ | |
| Bottom padding for navigation | ☐ | ☐ | ☐ | |

### 7. ExerciseDetailPage
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| Summary text wraps correctly | ☐ | ☐ | ☐ | BodyStyle |
| Video thumbnail proportional | ☐ | ☐ | ☐ | MinimumHeightRequest=160 |
| Play button overlay centered | ☐ | ☐ | ☐ | |
| Instruction steps scrollable | ☐ | ☐ | ☐ | |
| Reps/Sets display correct | ☐ | ☐ | ☐ | ParameterLabel/ValueStyle |
| Back/Done buttons visible | ☐ | ☐ | ☐ | |
| Video fullscreen works | ☐ | ☐ | ☐ | Overlay with close button |

### 8. InformationPage
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| Content sections display | ☐ | ☐ | ☐ | Header + Body |
| Scrollable if content exceeds screen | ☐ | ☐ | ☐ | |
| Back button positioned correctly | ☐ | ☐ | ☐ | Bottom-left |

### 9. SettingsPage
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| Program code displays | ☐ | ☐ | ☐ | Border with code |
| Toggle switches work | ☐ | ☐ | ☐ | Download, Offline, Notifications |
| Time picker appears when enabled | ☐ | ☐ | ☐ | |
| All settings labels readable | ☐ | ☐ | ☐ | SectionLabelStyle |

---

## Custom Controls Test Cases

### LogoDisc
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| Proportional scaling works | ☐ | ☐ | ☐ | DiscSize property |
| Outer ring visible | ☐ | ☐ | ☐ | Primary color |
| Inner disc visible | ☐ | ☐ | ☐ | Surface color |
| Dog image centered | ☐ | ☐ | ☐ | |
| Shadow offset proportional | ☐ | ☐ | ☐ | |
| "Hello Buddy" text visible | ☐ | ☐ | ☐ | LogoDiscTextStyle |

### StyledButton
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| Minimum touch target 48dp | ☐ | ☐ | ☐ | MinButtonHeight resource |
| ButtonWidth as minimum, not fixed | ☐ | ☐ | ☐ | MinimumWidthRequest |
| Text centered | ☐ | ☐ | ☐ | |
| Corner radius consistent | ☐ | ☐ | ☐ | ControlCornerRadius resource |

### StyledInputBox
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| Expands to fill available width | ☐ | ☐ | ☐ | ColumnDefinitions="*,Auto" |
| Minimum height respected | ☐ | ☐ | ☐ | MinimumHeightRequest |
| Border styled correctly | ☐ | ☐ | ☐ | InputBorder color |

### ExerciseTile
| Test Case | Pixel 6 | S22 | S9 | Notes |
|-----------|---------|-----|-----|-------|
| Tiles fit in 2-column layout | ☐ | ☐ | ☐ | FlexBasis 50% |
| Title wraps correctly | ☐ | ☐ | ☐ | TileTitleStyle |
| Completion tick visible when done | ☐ | ☐ | ☐ | CompletionTick color |
| Touch target adequate | ☐ | ☐ | ☐ | MinimumHeightRequest |

---

## Resource Validation

### Colors.xaml
| Resource | Used In | Validated |
|----------|---------|-----------|
| Primary (#3D81A9) | Buttons, accents | ☐ |
| Surface (#D7F2FD) | Wave, switch background | ☐ |
| TextPrimary (Black) | Body text | ☐ |
| TextSecondary (#555555) | Secondary labels | ☐ |
| TextMuted (#666666) | Captions, hints | ☐ |
| Error (#CC0000) | Validation messages | ☐ |
| ButtonPrimary (#3D81A9) | Primary buttons | ☐ |
| ButtonSecondary (#D7F2FD) | Secondary buttons | ☐ |

### Spacing.xaml
| Resource | Value | Used In | Validated |
|----------|-------|---------|-----------|
| SpacingXS | 4 | Tight spacing | ☐ |
| SpacingS | 8 | Label-field gaps | ☐ |
| SpacingM | 12 | Section spacing | ☐ |
| SpacingL | 16 | Button groups | ☐ |
| SpacingXL | 24 | Major sections | ☐ |
| PagePadding | 20,0,20,20 | Page margins | ☐ |
| MinButtonHeight | 48 | Touch targets | ☐ |

### Typography.xaml
| Style | FontSize | Used In | Validated |
|-------|----------|---------|-----------|
| PageTitleStyle | 32 | Page headers | ☐ |
| H1Style | 28 | Major headings | ☐ |
| H2Style | 18 | Section headings | ☐ |
| BodyStyle | 14 | Body text | ☐ |
| CaptionStyle | 12 | Hints, labels | ☐ |
| ButtonTextStyle | 11 | Button labels | ☐ |
| ErrorTextStyle | 14 | Validation errors | ☐ |

---

## No Regression Checklist

| Functionality | Status | Notes |
|---------------|--------|-------|
| All bindings still work | ☐ | Test Command bindings |
| Navigation between pages | ☐ | Test all navigation paths |
| Form validation | ☐ | Login, Registration |
| Video playback | ☐ | ExerciseDetailPage |
| Exercise completion tracking | ☐ | Tiles update |
| Settings persistence | ☐ | Toggle states saved |
| Offline behavior | ☐ | If applicable |

---

## Sign-off

| Device | Tester | Date | Pass/Fail |
|--------|--------|------|-----------|
| Pixel 6 Emulator | | | |
| Samsung S22 | | | |
| Samsung S9 | | | |

---

## Notes
- All fixed `WidthRequest`/`HeightRequest` values have been converted to `MinimumWidthRequest`/`MinimumHeightRequest` where appropriate
- All inline hex colors replaced with resource references
- All `StackLayout` converted to `VerticalStackLayout`/`HorizontalStackLayout` where touched
- The critical `Margin="0,135,0,0"` hack in MainExercisesPage has been removed and replaced with proper Grid row structure
