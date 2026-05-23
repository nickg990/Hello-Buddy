# Technical Specification Review & Questions
**Date:** October 29, 2025  
**Reviewer:** GitHub Copilot  
**Purpose:** Pre-development review of workflow and technical specification

---

## 🔍 Overall Assessment

The specification is comprehensive and well-structured. The workflow diagram clearly shows the user journey and system interactions. However, there are several areas that need clarification before development begins.

---

## ❓ Critical Questions Requiring Clarification

### 1. **Authentication & Session Management**

**Q1.1:** What should be the token expiration policy? The spec mentions "refresh via re-login for MVP" - should we implement automatic token refresh or force manual re-login?

**Q1.2:** Where exactly is the session token stored? SecureStorage, Preferences, or SQLite? What about the user profile data?

**Q1.3:** What constitutes the "user profile" that gets stored after login? Just basic info or full user details?

**Q1.4:** In the registration flow, what is the "Code" field for? Is this an invitation code, verification code, or something else?

### 2. **Terms & Conditions Flow**

**Q2.1:** Can users decline terms and still use the app in some limited capacity, or does declining terms kick them back to Home permanently?

**Q2.2:** What happens if terms are accepted offline but the PATCH request fails? Do we show an error or silently retry?

**Q2.3:** Are terms version-controlled? What happens when terms are updated - do users need to re-accept?

### 3. **Data Models & Structure**

**Q3.1:** What does the `/programme/today` response structure look like? We need the complete JSON schema for:
- Exercise objects (id, name, description, instructions, parameters, media)
- Programme structure
- Session information

**Q3.2:** What are the possible values for exercise "status dot"? (Completed, In Progress, Pending, etc.)

**Q3.3:** For exercise completion, what are the exact parameter types?
- Reps: Integer?
- Sets: Integer?
- Seconds: Integer or decimal?
- Notes: String with max length?

**Q3.4:** What does "two series" mean for the progress chart? What are the two data series being displayed?

### 4. **Offline Strategy Implementation**

**Q4.1:** What's the maximum size limit for the offline cache? Should we implement cache cleanup/rotation?

**Q4.2:** How long should pending writes be kept in the queue before giving up?

**Q4.3:** What's the exponential backoff strategy? Starting interval, max interval, multiplier?

**Q4.4:** Should the app work completely offline on first install, or does it require an initial online session to download programme data?

### 5. **Wave Header Implementation**

**Q5.1:** Do you have the exact mathematical formula for the sine wave? Or should I create a standard sine wave with configurable amplitude?

**Q5.2:** Should the wave be animated (subtle movement) or completely static?

**Q5.3:** When you say "single cycle" - does this mean exactly one complete sine wave across the width, or something else?

### 6. **Exercise Media & Assets**

**Q6.1:** Where are exercise images/videos stored? In the app bundle, downloaded from API, or cached locally?

**Q6.2:** What are the supported media formats? (PNG, JPG for images; MP4, etc. for videos?)

**Q6.3:** What's the fallback if media fails to load?

### 7. **Navigation & User Experience**

**Q7.1:** In the Information Page, what is the "Back" button for? Back to where if this might be the first page a user sees?

**Q7.2:** Should there be any global navigation elements (like a hamburger menu) or is it strictly button-based as specified?

**Q7.3:** What happens when a user completes all exercises in a session? Do they automatically advance to the next day/session?

### 8. **Settings & Configuration**

**Q8.1:** What exactly does "Enable Offline Caching" toggle do? Does it prevent/allow downloading programme data?

**Q8.2:** Are notifications push notifications? What triggers them?

**Q8.3:** Is Dark Mode a future feature or should it be implemented now?

### 9. **Error Handling & Edge Cases**

**Q9.1:** What should happen if the user loses network connection mid-sync?

**Q9.2:** How should we handle API errors beyond the basic success/failure shown in the workflow?

**Q9.3:** What if a user tries to skip a session but hasn't started any exercises?

**Q9.4:** Should there be data validation on exercise parameters (min/max values for reps, sets, time)?

### 10. **Platform-Specific Considerations**

**Q10.1:** Are there different behaviors needed for iOS vs Android vs Windows?

**Q10.2:** Should we implement platform-specific UI patterns or maintain consistent design across all platforms?

**Q10.3:** Any special considerations for tablet layouts vs phone layouts?

---

## 🔧 Technical Implementation Questions

### 11. **Architecture & Patterns**

**Q11.1:** Should we use MVVM pattern throughout? Any preference for dependency injection framework?

**Q11.2:** For the reusable components (WaveHeader, ExerciseTile, ChartProgress) - should these be UserControls, ContentViews, or custom renderers?

**Q11.3:** Should we implement a state management pattern (like Redux) or keep state local to ViewModels?

### 12. **Database Schema**

**Q12.1:** Can you provide or should I design the SQLite schema for:
- Users/Sessions
- Programmes 
- Exercises
- Completions
- Pending sync queue

**Q12.2:** Should we implement database migrations from the start?

### 13. **Sync & Background Processing**

**Q13.1:** Should background sync run on a timer, or only on app resume/network change events?

**Q13.2:** How should we handle conflicts if server data has changed while user was offline?

**Q13.3:** Should we show sync status to users (loading indicators, "syncing..." messages)?

---

## 🎨 Design & UI Questions

### 14. **Responsive Design**

**Q14.1:** What are the target screen sizes? Should we optimize for specific breakpoints?

**Q14.2:** How should the 2-column exercise grid behave on very wide or very narrow screens?

### 15. **Accessibility**

**Q15.1:** Should we implement screen reader descriptions for the progress charts?

**Q15.2:** Any specific accessibility requirements beyond the basic contrast/focus indicators mentioned?

### 16. **Assets & Resources**

**Q16.1:** Do you have the dog logo asset, or should I create a placeholder?

**Q16.2:** Are there specific exercise icon assets, or should I use generic icons initially?

---

## 📋 Missing Specifications

### 17. **Information Page Content**
- What specific text/content should be displayed on the Information page?
- Is this static content or dynamic/configurable?

### 18. **Terms & Conditions Content**
- Should this be embedded text, loaded from server, or a web view?
- Do you have the actual terms content?

### 19. **Error Messages**
- Should I create standard error messages for network failures, validation errors, etc.?
- Any specific tone/style guidelines for user-facing messages?

### 20. **Loading States**
- How should we handle loading states for API calls?
- Shimmer effects, spinners, progress bars?

---

## 🚀 Development Approach Questions

### 21. **Development Phases**
**Q21.1:** Should we build this incrementally? Suggested phases:
1. Basic navigation and offline-first UI
2. Authentication flows
3. Exercise data and offline sync
4. Progress tracking and charts

**Q21.2:** Should we start with mock data services before implementing real API calls?

### 22. **Testing Strategy**
**Q22.1:** What level of testing do you want? Unit tests, integration tests, UI tests?

**Q22.2:** Should we implement automated testing for offline/online scenarios?

---

## 📝 Recommendations

1. **Start with mock data services** to unblock UI development while API questions are resolved
2. **Implement wave header component first** as it's used across all pages
3. **Build offline-first** from the beginning rather than retrofitting
4. **Create a comprehensive error handling strategy** before implementing individual features
5. **Design the data models carefully** as they'll impact both local storage and API integration

---

## ✅ Next Steps

Once these questions are answered, I recommend:

1. **Finalize data models and API contracts**
2. **Create mock data services**
3. **Build core infrastructure** (navigation, storage, sync engine)
4. **Implement pages incrementally** starting with authentication flow
5. **Add real API integration**
6. **Polish and optimize**

---

**Total Questions:** 44 specific questions across 22 categories  
**Priority:** Please address Q1-Q10 (Critical Questions) first, as these will significantly impact the foundational architecture.