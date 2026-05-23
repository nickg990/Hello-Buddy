# Follow-up Questions & Clarifications Review
**Date:** November 3, 2025  
**Status:** Review of developer_clarifications.md  

---

## ✅ **Clear & Ready to Implement**

The following areas are well-defined and ready for development:
- Authentication flow and storage strategy
- Terms & Conditions handling with version control
- Exercise data model and status values
- Wave header specifications
- Media handling and formats
- Navigation patterns
- Database schema structure
- Sync frequency and indicators

---

## ❓ **Follow-up Questions Requiring Clarification**

### 1. **Authentication & API Integration**

**Q1.A:** What is the base URL for the API endpoints? (e.g., `https://api.caninephysio.com/v1`)

**Q1.B:** For Q1.3, you mention storing `programmeCode` - where does this come from? Is it:
- Returned from login/register API?
- Part of the registration "Code" field?
- A unique identifier per user's assigned programme?

**Q1.C:** What HTTP status codes should we handle beyond 200/201? (401 unauthorized, 403 forbidden, 500 server error, etc.)

---

### 2. **Terms & Conditions**

**Q2.A:** Re: "send text message to support number" - should the app:
- Call a `/support/notify` endpoint on your backend?
- Use a third-party SMS API directly from the app?
- Display a phone number for the user to call?

**Q2.B:** Where is the terms version checked? 
- In the login response `{ token, user, termsAccepted, termsVersion }`?
- Separate endpoint `GET /terms/latest`?

**Q2.C:** You mention `terms_v1.md` - should I create a placeholder file, or will you provide it?

---

### 3. **Data Model & API Contracts**

**Q3.A:** For the `/programme/today` response:
- What if there are no exercises for today? Empty array or specific message?
- Should the app show exercises from the next available day?

**Q3.B:** You removed "Seconds" parameter - confirmed we only need Reps and Sets? No duration tracking at all?

**Q3.C:** What's the complete structure for POST `/exercise/complete`? Should it be:
```json
{
  "exerciseId": "E01",
  "sessionId": "abc123",
  "reps": 10,
  "sets": 3,
  "notes": "Optional comment",
  "timestamp": "2025-11-03T14:30:00Z",
  "uuid": "local-generated-uuid"
}
```

**Q3.D:** Progress chart shows "completed vs target" - where does the "target" value come from?
- Is it in the exercise data from `/programme/today`?
- A separate field like `targetReps` and `targetSets`?

---

### 4. **Offline & Sync Strategy**

**Q4.A:** Re: "Continuous retry logic: check connection first. If server offline, send error message to admin"
- How do we distinguish between "no internet connection" vs "server is down"?
- Should we ping the server to check health before assuming it's down?

**Q4.B:** Who is "admin" and how do we send them messages?
- API endpoint?
- Email service?
- Push notification to admin app?

**Q4.C:** For the 15-minute wait between retries when server is offline:
- Should the app continue to function normally during this wait?
- Should we show any indicator to the user that sync is paused?

**Q4.D:** You mentioned "Never permanently stop retries" - what about when pending writes are purged after 30 days? Do those get one final attempt before purge?

---

### 5. **Exercise Media & Assets**

**Q5.A:** For media caching:
- Should we preload all media when user opens Main Dashboard?
- Load on-demand when user opens Exercise Detail?
- Background download when network available?

**Q5.B:** What's the max file size for cached media? Any limits on total cache size?

**Q5.C:** Do you have actual exercise media files/URLs, or should I use placeholders for now?

---

### 6. **Navigation & Welcome Page**

**Q6.A:** You mention "Back" on Information Page returns to "Welcome Page" - but the workflow shows:
- App Launch → Information Page (if no token) → Home
- There's no "Welcome Page" in the workflow diagram

Should this be:
- A new Welcome/Splash screen before Information?
- Just return to Home?
- Exit the app?

**Q6.B:** When session completes and "unlocks next day" - should we:
- Show a congratulations message?
- Automatically navigate somewhere?
- Just update the data silently?

---

### 7. **Settings & Preferences**

**Q7.A:** If Offline Caching is disabled:
- Should we delete existing cached data?
- Just stop downloading new data?
- Prevent app launch without connection?

**Q7.B:** Local notifications/reminders - what should trigger them?
- Daily reminder at specific time?
- Reminder if user hasn't completed today's exercises?
- User-configurable schedule?

---

### 8. **Error Handling & User Feedback**

**Q8.A:** Re: "Inform user only if progress cannot continue offline" - examples of when to inform:
- No cached data and no internet?
- User tries to skip session offline (needs to sync)?
- Anything else?

**Q8.B:** For validation errors (e.g., invalid email format), should we:
- Show inline errors under each field?
- Toast message at bottom?
- Alert dialog?

---

### 9. **Database & Migration**

**Q9.A:** You mention basic migration from v1.0 - does this mean:
- We're starting at v1.0, so just set up migration framework?
- There's an existing v1.0 we need to migrate from?

**Q9.B:** Should the PendingQueue table include:
- Original request payload?
- Number of retry attempts?
- Last retry timestamp?
- Error messages from failed attempts?

---

### 10. **Digital Dog Disc on Home Page**

**Q10.A:** The digital dog disc overlapping the wave header:
- Should it be positioned exactly at the wave curve line?
- Centered horizontally?
- Any specific z-index/elevation shadow specs?

**Q10.B:** Do you have the dog logo asset, or should I create a placeholder?

**Q10.C:** What are the exact gradient colors for "light cyan → mid-blue"?

---

### 11. **Progress Chart Details**

**Q11.A:** The progress chart shows "completed vs target per day":
- How many days should be visible? Last 7 days? Last 30?
- Should it scroll horizontally if more than fits on screen?
- What happens on Day 1 when there's no history?

**Q11.B:** Chart legend shows "two series" - are these:
- Completed (actual) vs Target (goal)?
- Today vs Previous days average?
- Current week vs Last week?

---

### 12. **Registration Flow**

**Q12.A:** The Registration "Code" field - is this:
- Required or optional?
- An invitation/access code?
- A programme selection code?
- Validated against a list of valid codes?

**Q12.B:** What validation rules for passwords?
- Minimum length?
- Complexity requirements (uppercase, numbers, special chars)?
- Maximum length?

---

### 13. **Skip Session Workflow**

**Q13.A:** When user skips a session:
- Does it count as completed for progression purposes?
- Does it affect the progress chart?
- Can they go back and complete skipped exercises later?

**Q13.B:** If skip fails to sync for 30 days, what happens?
- Session remains skipped locally?
- Session becomes available again?
- Any notification to user or admin?

---

### 14. **Loading & Shimmer Effects**

**Q14.A:** Shimmer skeletons for lists/grids:
- Should we show approximate number of items (e.g., 6 exercise tiles)?
- Generic rectangular shapes or match actual component shapes?

---

### 15. **Tablet-Specific Questions**

**Q15.A:** 3-column grid on tablets:
- What's the breakpoint? Screen width > 600dp?
- Should we also adjust button sizes, fonts, spacing?
- Any max width constraint for very large tablets?

---

## 🎯 **Priority Follow-up Questions**

These need answers before we can start coding:

### **CRITICAL (Must Answer First):**
1. **Q1.A** - API base URL
2. **Q1.B** - What is programmeCode and where does it come from?
3. **Q3.C** - Complete POST `/exercise/complete` payload structure
4. **Q3.D** - Where do target values come from for progress chart?
5. **Q6.A** - What is the "Welcome Page"?

### **HIGH PRIORITY (Needed Early):**
6. **Q2.A** - How to send support notifications?
7. **Q2.B** - Where is terms version provided?
8. **Q4.A/B** - How to detect server vs network issues and notify admin?
9. **Q12.A** - Registration code purpose and validation

### **MEDIUM PRIORITY (Needed Before Full Implementation):**
10. **Q5.A/B** - Media caching strategy and limits
11. **Q7.B** - Notification trigger logic
12. **Q11.A/B** - Progress chart data range and series definitions
13. **Q10.B/C** - Dog logo asset and gradient colors

### **LOW PRIORITY (Can Use Placeholders Initially):**
14. Content files (Q2.C, Q5.C)
15. Exact measurements and styling (Q10.A, Q14.A, Q15.A)

---

## 💡 **Assumptions I'll Make If Not Clarified**

To unblock development, I'll proceed with these assumptions unless you specify otherwise:

1. **API Base URL:** Will use configurable placeholder `https://api.placeholder.dev`
2. **ProgrammeCode:** Will assume it's returned in login response
3. **Welcome Page:** Will treat Information Page as the welcome screen
4. **Target Values:** Will include `targetReps` and `targetSets` in exercise data
5. **Support Notifications:** Will log errors locally and add TODO for integration
6. **Dog Logo:** Will use placeholder circular icon with gradient
7. **Chart Range:** Will show last 7 days
8. **Registration Code:** Will make it optional with basic validation

---

## ✅ **Next Steps**

Please review the follow-up questions above, especially the **CRITICAL** and **HIGH PRIORITY** ones. Once those are clarified, I can:

1. Create complete data models and API contracts
2. Set up the project architecture
3. Implement mock data services
4. Begin building the UI pages
5. Implement offline-first data layer

**Total New Questions:** 39 follow-up questions across 15 categories