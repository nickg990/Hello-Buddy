# Canine Physio App — Developer Clarifications (Audit Record)

This document consolidates all developer questions and corresponding responses. It includes initial interpretations, agreed specifications, and Nick’s final clarifications as of this version.

---

## 🔐 1. Authentication & Session

**Q1.1 – Token Refresh**  
Manual re-login for MVP. Automatic refresh to be added in final release.

**Q1.2 – Storage Locations**  
Use .NET MAUI `SecureStorage` for tokens and sensitive data. Use `Preferences` for simple flags (`termsAccepted`, `offlineEnabled`).

**Q1.3 – Local User Profile**  
Store `{ userId, email, programmeCode, termsAccepted }` only. Programme details are fetched dynamically.

---

## 📜 2. Terms & Conditions

**Q2.2 – Offline Acceptance Handling**  
Silent retry if offline, without warning user. If PATCH fails while online, send text message to support number.

**Q2.3 – Version Control**  
Track Terms version (e.g., v1.0). Require re-acceptance if newer version detected.

---

## 🧩 3. Data Model

**Q3.1 – `/programme/today` Fields**  
Response example:
```json
{
  "sessionId": "abc123",
  "dayNumber": 5,
  "exercises": [
    { "id": "E01", "name": "Sit to Stand", "description": "...", "mediaUrl": "https://...", "status": "pending" }
  ]
}
```

**Q3.2 – Exercise Status Values**  
`pending`, `in_progress`, `completed`, `abandoned`.

**Q3.3 – Parameters**  
Reps and sets only (integers 0–999).

**Q3.4 – Progress Chart**  
Shows completed vs target per day (stacked bar style).

---

## ⚙️ 4. Offline Strategy

**Q4.1 – Cache Retention**  
Keep last 7 days, purge older sessions.

**Q4.2 – Pending Write Expiry**  
Keep pending writes 30 days, then purge.

**Q4.3 – Retry Policy**  
Continuous retry logic: check connection first.  
If server offline, send error message to admin, wait 15 minutes, retry. Never permanently stop retries.

---

## 🌊 5. Wave Header

**Q5.1 – Curve Definition**  
Single sine wave (`y = A * sin(kx)`), amplitude ≈ 8–10% of header height.

**Q5.2 – Animation**  
Static for MVP. Optional slow animation later.

---

## 🎥 6. Exercise Media

**Q6.1 – Source**  
Fetched dynamically from API/CDN, cached offline.

**Q6.2 – Formats**  
Supports JPG, PNG, MP4.

**Q6.3 – Fallback**  
Show placeholder icon if media fails.

---

## 🧭 7. Navigation & UX

**Q7.1 – “Back” on Information Page**  
Returns to the Welcome Page (not exit).

**Q7.3 – Session Completion**  
Automatically mark session complete and unlock next day.

---

## ⚙️ 8. Settings

**Q8.1 – Offline Caching Toggle**  
Enables caching; defaults to enabled.  
If disabled, offline mode unavailable.

**Q8.2 – Notifications**  
Local reminders only (push deferred).

**Q8.3 – Dark Mode**  
Future feature.

---

## 🚨 9. Error Handling

**Q9.1 – Network Drop During Sync**  
Continuous silent retry. If offline, work in offline mode.  
Inform user only if progress cannot continue offline.

**Q9.2 – Non-Critical Errors**  
Log silently unless user must act.

---

## 💻 10. Platform & Layout

**Q10.2 – Cross-Platform Design**  
Same layout and styling on iOS/Android.

**Q10.3 – Tablet Layout**  
3-column grid on tablets; maintain proportional spacing.

---

## 🧱 12. Database Schema

**Q12.1 – Tables**  
`Users`, `Exercises`, `Programme`, `Completions`, `PendingQueue`.

**Q12.2 – Migrations**  
Include basic migration from v1.0.

---

## 🔄 13. Sync & Background Processing

**Q13.1 – Sync Frequency**  
Trigger sync: on resume, every 5 minutes while active, and on exit.

**Q13.3 – Sync Indicator**  
Show toast: “Syncing…” → green checkmark when done.

---

## 📄 17–20. Content & UX

**Q17 – Information Page Text**  
Option A: Nick will supply content.

**Q18 – Terms & Conditions Source**  
Option A: Local Markdown file (`terms_v1.md`).

**Q19 – Message Tone**  
Friendly tone for MVP. Adjustable later.

**Q20 – Loading Style**  
Shimmer skeletons for lists and grids.

---

## 🎨 Design Note – Home Page Digital Dog Disc

- The Home Page (“Hello Buddy”) features a **circular disc (100–120dp)** overlapping the wave header and body background.  
- Contains **digital dog logo** with gradient (light cyan → mid-blue) and soft shadow.  
- Only the home page includes this element; others begin below the header.  
- Acts as the visual identity anchor for the app.

---

## ✅ Summary

This file provides a complete audit of developer clarifications and final agreed behaviour.  
All rules and visuals are definitive for MVP build.
