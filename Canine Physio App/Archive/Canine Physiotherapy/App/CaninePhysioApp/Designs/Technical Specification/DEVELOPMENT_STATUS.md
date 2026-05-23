# Canine Physiotherapy App - Development Status

**Last Updated:** November 7, 2025  
**Current Phase:** Ready to begin Phase 1 - Hello Buddy Welcome Screen Implementation  
**Repository:** https://github.com/nickg990/CaninePhysiotherapy  
**Branch:** main

---

## Project Overview

Building a .NET MAUI cross-platform canine physiotherapy application based on Figma designs and workflow specifications located in `Designs/Figma and workflow/`.

### Technology Stack
- **.NET MAUI** (net8.0) - Multi-platform: Android, iOS, Windows, macOS Catalyst
- **IDE:** Visual Studio 2022 (on DevBox for development)
- **Architecture:** MVVM pattern with dependency injection
- **Database:** SQLite + Entity Framework Core (migration framework v1.0)
- **Storage:** 
  - SecureStorage API for auth tokens (platform-specific encryption)
  - Preferences API for non-sensitive flags (termsAccepted, offlineEnabled)
  - SQLite for structured data
- **Backend:** Azure App Service (Free Tier) for mock API during MVP

---

## Design System Specifications

### Color Palette (EXACT HEX VALUES - DO NOT DEVIATE)
- **Background:** #D9D9D9 (light gray)
- **Primary (Wave/Buttons):** #3D81A9 (blue)
- **Header Background:** #FFFFFF (white)
- **Header Text "Hello Buddy":** #3D81A9 (blue)
- **Header Subtitle "Canine Physiotherapy":** #000000 (black)
- **Button Text:** Alternates between #FFFFFF (white) and #000000 (black)

### Wave Header Component
- **Reusable XAML component** (create once, use on all pages)
- **Height:** 50% of screen height
- **Shape:** Single cycle sine wave
- **Color:** #FFFFFF (white) on top of #3D81A9 (blue) background
- **Implementation:** Use Path with Bézier curves or sine wave geometry

### Button Styling
- **Width:** 70% of screen width
- **Centered horizontally**
- **Rounded corners**
- **Color Pattern (Hello Buddy page):**
  1. Information: #3D81A9 background, #FFFFFF text
  2. Settings: #FFFFFF background, #000000 text
  3. Start Physio: #3D81A9 background, #FFFFFF text
  4. Registration: #FFFFFF background, #000000 text

---

## Project Structure

```
CaninePhysiotherapy/
├── .gitignore                          ✅ CREATED - excludes .vs/, bin/, obj/
├── Designs/                            ✅ COMMITTED
│   ├── Figma and workflow/
│   │   ├── Canine Physio App Figma.png
│   │   └── Physio App - Main Workflow.svg
│   └── Technical Specification/
│       ├── workflow.md                 ✅ Mermaid diagram (fixed syntax)
│       ├── techspec.md                 ✅ Page-by-page specifications
│       ├── review_questions.md         ✅ 44 initial questions
│       ├── developer_clarifications.md ✅ First round answers
│       ├── followup_questions.md       ✅ 39 follow-up questions
│       ├── Followup Responses.md       ✅ Second round answers
│       ├── Final Clarifications.md     ✅ AUTHORITATIVE - corrected navigation flow
│       ├── final_clarification_review.md
│       └── development_ready_assessment.md
└── App/
    └── CaninePhysioApp/
        ├── CaninePhysioApp.sln
        └── CaninePhysioApp/
            ├── CaninePhysioApp.csproj
            ├── MainPage.xaml          ⚠️ DEFAULT TEMPLATE - TO BE REPLACED
            ├── MainPage.xaml.cs       ⚠️ DEFAULT COUNTER APP - TO BE REPLACED
            ├── App.xaml
            ├── AppShell.xaml
            └── MauiProgram.cs
```

---

## Critical Navigation Flow (from Final Clarifications.md)

**CORRECTED FLOW - Hello Buddy is the Central Hub:**

```
App Launch
    ↓
Hello Buddy Welcome Page (CENTRAL HUB)
    ├── Information → (View app info, back to Hello Buddy)
    ├── Settings → (Configure offline mode, back to Hello Buddy)
    ├── Start Physio → Login Required Check
    │                   ├── If logged in → Dog Selection
    │                   └── If not logged in → Login Page
    └── Registration → Registration Form → Email Verification → Hello Buddy
```

**NOT:** Information Page as hub (this was the original incorrect assumption)

---

## Hello Buddy Welcome Screen - Detailed Layout

**From Final Clarifications.md - This is the FIRST page to implement:**

### Structure (Top to Bottom)
1. **White Header Block** (top section above wave)
   - Background: #FFFFFF
   - "Hello Buddy" text: #3D81A9, large, bold
   - "Canine Physiotherapy" subtitle: #000000, smaller

2. **Logo Disc** (overlapping wave)
   - Circular placeholder
   - White border
   - Black drop shadow for depth
   - Background: #3D81A9
   - Positioned where wave crests

3. **Wave Rectangle**
   - Takes 50% of screen height
   - #3D81A9 background
   - White wave shape on top edge

4. **Background Below Wave**
   - #D9D9D9 (light gray)
   - Contains the 4 navigation buttons

5. **Four Buttons** (70% width, centered, stacked vertically)
   - **Information:** #3D81A9 bg, #FFFFFF text → Navigate to Information Page
   - **Settings:** #FFFFFF bg, #000000 text → Navigate to Settings Page
   - **Start Physio:** #3D81A9 bg, #FFFFFF text → Check auth → Dog Selection or Login
   - **Registration:** #FFFFFF bg, #000000 text → Navigate to Registration Page

---

## Development Approach

### Iterative Strategy
1. **Start with Hello Buddy welcome screen** (this document focuses on Phase 1)
2. **Fine-tune and test** each page before moving to the next
3. **Windows target first** (easiest platform for rapid testing)
4. **User reviews and approves** each iteration before proceeding
5. **Add complexity incrementally** (navigation → data → offline sync → API)

### Phase 1 Tasks (READY TO START)

#### 1. Create Wave Header Component
```
Location: App/CaninePhysioApp/CaninePhysioApp/Controls/WaveHeader.xaml
```
- Create custom ContentView
- Implement sine wave path (50% height, single cycle)
- Make reusable with bindable properties (Title, Subtitle)
- Colors: #FFFFFF wave on #3D81A9 background

#### 2. Update AppShell Navigation
```
Location: App/CaninePhysioApp/CaninePhysioApp/AppShell.xaml
```
- Register routes for all pages:
  - `HelloBuddy` (main/home)
  - `Information`
  - `Settings`
  - `StartPhysio`
  - `Registration`
  - `Login`
  - `DogSelection`

#### 3. Create Hello Buddy Page
```
Location: App/CaninePhysioApp/CaninePhysioApp/Views/HelloBuddyPage.xaml
```
- Replace MainPage.xaml or create new HelloBuddyPage
- Use WaveHeader component
- Implement layout per specifications above
- Add 4 navigation buttons with exact colors
- Set as startup page in AppShell

#### 4. Create ViewModel (MVVM)
```
Location: App/CaninePhysioApp/CaninePhysioApp/ViewModels/HelloBuddyViewModel.cs
```
- Implement INotifyPropertyChanged
- Create Command properties for each button
- Navigation logic using Shell.Current.GoToAsync()
- Check auth state for "Start Physio" button

#### 5. Create Placeholder Pages
```
Create basic pages for navigation testing:
- Views/InformationPage.xaml
- Views/SettingsPage.xaml
- Views/LoginPage.xaml
- Views/RegistrationPage.xaml
- Views/DogSelectionPage.xaml
```
Each should just have WaveHeader and basic content to confirm navigation works.

---

## All Page Specifications (Reference techspec.md)

1. **Hello Buddy (Welcome)** - Phase 1 ⬅️ START HERE
2. **Information** - Placeholder in Phase 1
3. **Registration** - Placeholder in Phase 1
4. **Email Verification** - Later phase
5. **Login** - Placeholder in Phase 1
6. **Settings** - Placeholder in Phase 1
7. **Dog Selection (Choose Your Dog)** - Placeholder in Phase 1
8. **Select Week** - Later phase
9. **Exercise List (Week View)** - Later phase
10. **Exercise Details** - Later phase
11. **Complete Exercise** - Later phase
12. **Exercise Completion Confirmation** - Later phase
13. **View Exercise Video** - Later phase
14. **View Progress** - Later phase (7-day chart, streak tracking)
15. **Contact Physiotherapist** - Later phase
16. **Sync Management** - Later phase (offline queue, pending uploads)

---

## Key Implementation Notes

### Authentication Flow
- **SecureStorage** for JWT tokens: `auth_token`
- **Preferences** for flags: `termsAccepted`, `offlineEnabled`
- Check token existence before navigating to Dog Selection from "Start Physio"
- If no token → navigate to Login
- If token exists → navigate to DogSelection

### Offline-First Architecture (Later Phases)
- **Network-first reads** with cache fallback
- **Pending write queue** with UUID tracking
- **Background sync** with exponential backoff retry
- **Conflict resolution:** Server wins (simple for MVP)

### Database Schema (Later Phases)
```sql
Dogs (DogId, Name, DateOfBirth, Condition, IsActive)
Exercises (ExerciseId, Name, Description, VideoUrl, Sets, Reps)
Weeks (WeekId, DogId, StartDate, EndDate)
WeekExercises (WeekExerciseId, WeekId, ExerciseId, DayOfWeek, SortOrder)
CompletedExercises (CompletionId, WeekExerciseId, CompletedAt, Notes, SyncStatus)
PendingSync (QueueId, EntityType, EntityId, Action, Payload, CreatedAt, RetryCount)
```

### Migration Framework
- **Version tracking** in Preferences: `db_version` (starts at 1.0)
- **Sequential migrations** in `Migrations/` folder
- Check and apply on app startup

---

## Testing Strategy

### Phase 1 Testing
1. **Windows target** (F5 in Visual Studio 2022)
2. **Visual inspection:**
   - Wave renders correctly at 50% height
   - Colors match EXACT hex values
   - Buttons are 70% width and centered
   - Logo disc overlaps wave properly
3. **Navigation testing:**
   - Each button navigates to correct placeholder page
   - Back navigation returns to Hello Buddy
   - "Start Physio" checks auth state (should go to Login when not authenticated)

### Later Phase Testing
- Android Emulator
- iOS Simulator (if on Mac or using remote Mac)
- Offline mode testing
- Sync queue behavior

---

## Resources and Documentation

### Essential Files to Read First
1. `Designs/Technical Specification/Final Clarifications.md` - AUTHORITATIVE design source
2. `Designs/Technical Specification/techspec.md` - Page-by-page specifications
3. `Designs/Technical Specification/workflow.md` - Mermaid navigation diagram
4. `Designs/Figma and workflow/Canine Physio App Figma.png` - Visual reference
5. `Designs/Figma and workflow/Physio App - Main Workflow.svg` - Workflow diagram

### Q&A History (All Questions Resolved)
- 44 initial questions → `developer_clarifications.md`
- 39 follow-up questions → `Followup Responses.md`
- Final corrections → `Final Clarifications.md`

---

## Git Status

### Last Commit
```
"Add .gitignore and remove build artifacts from tracking"
```

### Repository State
- ✅ Clean working directory (build artifacts excluded)
- ✅ All design documentation committed
- ✅ Pushed to GitHub origin/main
- ✅ .gitignore properly excludes .vs/, bin/, obj/

### Important Git Notes
- **NEVER commit:** .vs/, bin/, obj/ folders (already in .gitignore)
- Close Visual Studio 2022 before Git operations if permission errors occur
- Use VS Code or command line for Git operations

---

## Ready to Start - Phase 1 Checklist

- [x] Design specifications complete and reviewed
- [x] All clarification questions answered
- [x] Navigation flow corrected and confirmed
- [x] Color palette finalized (#D9D9D9, #3D81A9, #FFFFFF, #000000)
- [x] Development environment tested (VS 2022 on DevBox)
- [x] Git repository clean and pushed
- [ ] Create WaveHeader component ⬅️ **START HERE**
- [ ] Create HelloBuddyPage with exact layout
- [ ] Create HelloBuddyViewModel with navigation commands
- [ ] Update AppShell with route registrations
- [ ] Create placeholder pages (Information, Settings, Login, Registration, DogSelection)
- [ ] Test navigation flow on Windows target
- [ ] User review and approval before Phase 2

---

## Next Steps for New Copilot Instance

1. **Read this document completely**
2. **Review `Designs/Technical Specification/Final Clarifications.md`** for Hello Buddy layout details
3. **Open the solution** in Visual Studio 2022: `App/CaninePhysioApp/CaninePhysioApp.sln`
4. **Start with WaveHeader component creation** as outlined in Phase 1 Tasks above
5. **Use Folder View** in Solution Explorer to see the Designs/ folder
6. **Test on Windows target** after each component is created
7. **Commit frequently** with descriptive messages

---

## Contact Information

- **Repository Owner:** nickg990
- **Development Machine:** DevBox (higher resources than local machine)
- **Development Style:** Iterative with user review at each phase

---

## Important Reminders

⚠️ **DO NOT deviate from hex color values** - use EXACT values specified above  
⚠️ **DO NOT assume Information page is the hub** - Hello Buddy is the central hub  
⚠️ **DO NOT start with complex features** - begin with Hello Buddy welcome screen  
⚠️ **DO NOT skip the wave header component** - it's reusable across all pages  
⚠️ **DO follow MVVM pattern** - ViewModels for all pages with INotifyPropertyChanged  

---

**Ready to build! Start with Phase 1: Wave Header Component** 🚀
