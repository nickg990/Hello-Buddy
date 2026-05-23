# Canine Physio App — Technical Specification (Developer-Ready)

**Design Fidelity:** Matches your Figma. **Wave Header Block** is a rectangular block whose **bottom edge is a sine wave** (single cycle). **Navigation is button-only** (no persistent tabs). **Option B**: the **entire page scrolls together** (including wave) when content exceeds viewport height.

---

## 0. Global Design System

### 0.1 Colours (tune to exact Figma hex if different)
- Header (Wave Block): `#7FAFC9`; optional gradient top `#A9C8DA` → bottom `#7FAFC9`
- Body Background: `#E9F3F9`
- Accent/Dark Text: `#2C3E50`
- Primary Button: fill `#2F5D80`, text `#FFFFFF`
- Ghost/Secondary: border `#2F5D80`, text `#2F5D80`
- Success `#3CB371` • Error `#D9534F` • Info `#4C86A8`
- Card `#FFFFFF` • Divider `#D8E5EE`

### 0.2 Typography
- Header Title: 20–22sp, semi-bold
- Section Title: 16–18sp, medium
- Body: 14–16sp, regular
- Caption: 12–13sp

### 0.3 Wave Header Block
- Height: 24–30% vh (page-specific)
- Bottom border: single sine-like curve (amplitude ~6–10% of header height)
- Title centered in upper half; optional logo centered beneath
- Implementation: reusable component (SVG/XAML Path) with adjustable amplitude & height
- Scrolling: **Wave scrolls with page**

### 0.4 Buttons & Inputs
- Buttons: 12–16dp radius, 44–52dp height, 12–16dp spacing
- Inputs: 6–8dp radius, border `#D8E5EE`, focus `#2F5D80`; inline validation

### 0.5 Offline
- SQLite for programme/exercises/completions/skips; Preferences for token, terms, settings
- Pending write queue with `uuid` and background sync

---
## 1. Information Page
**Purpose:** Welcome/overview of programme and how to begin.

**Wave Header**
- Height ~28% vh; title “Information”; single sine bottom.

**Layout**
- Stacked text sections (16dp margins)
- Bottom row: **Back** (ghost, left) • **Home** (primary, right)

**Navigation**
- Home → **Hello Buddy (Home)**

---
## 2. Hello Buddy (Home)
**Purpose:** Main hub.

**Wave Header**
- Height ~30% vh; title “Hello Buddy”; circular dog logo under title.

**Layout**
- Center column (max 80% width): **Login**, **Register**, **Start Physio**, **Settings** (if present)

**Navigation**
- Login → **Login**
- Register → **Registration**
- Start Physio → **Terms & Conditions**
- Settings → **Settings**

---
## 3. Login
**Wave Header:** ~26% vh; title “Login”.

**Layout**
- Inputs: **Email**, **Password**
- Buttons: **Login** (primary), **Forgot Password** (text)
- Inline error area under password

**Data Input**
- Email (email keyboard), Password (secure)

**System**
- Validate → POST `/auth/login` → store `{token, user, termsAccepted}`

**Navigation**
- Success → **Terms** (if not accepted) else **Main Dashboard**
- Forgot → **Forgotten Password**
- Back → **Home**

---
## 4. Registration
**Wave Header:** ~26% vh; title “Registration”.

**Layout**
- Inputs: **Code** (if present), **Email**, **Password**, **Confirm Password**
- Button: **Register** (primary)
- Inline validation/success message

**System**
- Validate → POST `/auth/register` → auto-login

**Navigation**
- Success → **Terms & Conditions**

---
## 5. Forgotten Password
**Wave Header:** ~24% vh; title “Forgotten Password”.

**Layout**
- Input: **Email**
- Button: **Send Reset**
- Inline confirmation on success

**System**
- POST `/auth/forgot`

**Navigation**
- Back → **Login**

---
## 6. Terms & Conditions
**Wave Header:** ~24–26% vh; title “Terms & Conditions”.

**Layout**
- Terms text (page scrolls with header if long)
- **Accept Terms** switch (right), description (left)
- Bottom buttons: **Back** (ghost), **Next** (primary, disabled until accepted)

**System**
- Persist locally; PATCH `/users/me/terms {accepted:true}` when online

**Navigation**
- Accept → **Main Exercises Dashboard**
- Back → **Home**

---
## 7. Main Exercises Dashboard
**Wave Header:** ~22–24% vh; title “Main Exercises”.

**Layout**
- Helper text above grid
- 2-column **exercise grid**; tiles show icon, name, **status dot** (top-right)
- Action row: **Skip Session** (secondary) • **Progress** (primary)

**Data**
- `/programme/today` or cache

**Navigation**
- Tap tile → **Exercise Detail**
- Skip → **Skip Session**
- Progress → **Exercise Progress**

---
## 8. Exercise Detail (e.g., “Lateral Stepping”)
**Wave Header:** ~26% vh; title = exercise name.

**Layout**
- **Hero image/video**
- **Instructions** text
- **Parameters**: Reps, Sets, Seconds
- **Comments** multiline (variant)
- Bottom: **Complete** (primary), **Save** (ghost when comments shown)

**System**
- Local persist → POST `/exercise/complete` with `{exerciseId, reps, sets, seconds, notes, timestamp, uuid}`

**Navigation**
- Complete → **Exercise Progress**
- Back → **Main Exercises**

---
## 9. Exercise Progress
**Wave Header:** ~22% vh; title “Exercise Progress”.

**Layout**
- **Bar chart** in card with legend (two series)
- Bottom mini buttons: **Back**, **Home** (if present)

**System**
- GET `/progress/summary`; merge local pending for chart

**Navigation**
- Back → **Main Exercises**
- Home → **Home**

---
## 10. Skip Session
**Wave Header:** ~24% vh; title “Skip to Next Session”.

**Layout**
- **Comments** multiline
- Bottom: **Back** (ghost), **Confirm Skip** (primary)

**System**
- Local persist; POST `/session/skip` when online

**Navigation**
- Submit → **Main Exercises**

---
## 11. Settings
**Wave Header:** ~24% vh; title “Settings”.

**Layout**
- Toggles: **Enable Offline Caching**, **Notifications**, **Dark Mode** (if present)
- Descriptive captions per row
- Bottom **Back** button

**System**
- Update local preferences; optionally POST `/users/me/prefs`

**Navigation**
- Back → **Home**

---
## 12. API (Conceptual)
- `POST /auth/login` → `{ token, user, termsAccepted }`
- `POST /auth/register` → `{ token }`
- `POST /auth/forgot`
- `PATCH /users/me/terms`
- `GET /programme/today`
- `POST /exercise/complete`
- `GET /progress/summary`
- `POST /session/skip`
- `POST /sync/bulk`

**Auth:** Bearer token. Refresh via re-login for MVP.

---
## 13. Offline Strategy
- Reads: network-first, cache fallback
- Writes: pending queue with `uuid`; background sync on resume/timer; exponential backoff

---
## 14. Accessibility & Measurements
- Contrast ≥ 4.5:1; focus indicators; screen reader labels
- Header height 24–30% vh; grid tiles min width ~44% with 6–8dp gutter
- Buttons 44–52dp high; 12–16dp spacing; cards 16dp padding

---
## 15. Reusable Components
- WaveHeader(title, height%, logo?)
- ExerciseTile(name, icon, statusDot)
- ChartProgress(seriesA, seriesB, labels)

---
## 16. Navigation Summary
- Button-only navigation
- Terms as hard gate
- Back/Home actions exist per page (per Figma)
