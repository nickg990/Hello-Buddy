### 1. **Authentication & API Integration**

**Q1.A:** What is the base URL for the API endpoints? 
The admin system hasn’t been built yet, so this functionality needs to be mocked up. Let’s create the mock-up as an API and implement it in an Azure free-tier App Service. We will implement API endpoints in the mock-up as we develop. Once we have implemented the first iteration of the mock-up then we will use it’s URL and so forth.

**Q1.B:** For Q1.3, you mention storing `programmeCode` - where does this come from?
Ignore programmeCode for the MVP.

**Q1.C:** What HTTP status codes should we handle beyond 200/201? (401 unauthorized, 403 forbidden, 500 server error, etc.)
We need to trap all non 200 return codes and display the code in a generic error message. 

### 2. **Terms & Conditions**

**Q2.A:** Re: "send text message to support number" - should the app:
Lets change this to log the error in the admin system and cache on the device. If offline then send the error as soon as back online. 

**Q2.B:** Where is the terms version checked? 
This is a good question. We had considered logging acceptance of T&Cs in the admin system but I think that’s going to be too much of an overhead to manage. I think we should log T&C acceptive against the user profile in admin against the T&C version number. Then only log it again when T&Cs change. However, for the workflow perspective the user must tick the acceptance box before progression. 

**Q2.C:** You mention `terms_v1.md` - should I create a placeholder file, or will you provide it?
Create a placeholder for now (for the MVP) and I’ll provide the detail later. 

### 3. **Data Model & API Contracts**

**Q3.A:** For the `/programme/today` response:
- What if there are no exercises for today? Empty array or specific message?
Specific message. Use a placeholder message for the MVP and we will clarify later.
- Should the app show exercises from the next available day?
Yes, the user should be scroll through all of the exercises past and future. I think this should be an online only function (so, scrolling on available offline). 

**Q3.B:** You removed "Seconds" parameter - confirmed we only need Reps and Sets? No duration tracking at all?
For the MVP yes, but we may decide to add seconds in the future.

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
This looks good as a starter and we will extend as necessary.

**Q3.D:** Progress chart shows "completed vs target" - where does the "target" value come from?
- Is it in the exercise data from `/programme/today`?
- A separate field like `targetReps` and `targetSets`?
In the exercise data.

### 4. **Offline & Sync Strategy**

**Q4.A:** Re: "Continuous retry logic: check connection first. If server offline, send error message to admin"
- How do we distinguish between "no internet connection" vs "server is down"?
- Should we ping the server to check health before assuming it's down?
Yes, ping. 

**Q4.B:** Who is "admin" and how do we send them messages?
- API endpoint?
- Email service?
- Push notification to admin app?
Admin is the backend desktop application that will probably run on Azure, so an API endpoint. If Admin needs to send notifications we will handle in the endpoint code. 

**Q4.C:** For the 15-minute wait between retries when server is offline:
- Should the app continue to function normally during this wait?
- Should we show any indicator to the user that sync is paused?
Show an indicator that the app is in offline or connected mode. In the information section we can explain what can’t be done in offline mode vs connected.  

**Q4.D:** You mentioned "Never permanently stop retries" - what about when pending writes are purged after 30 days? Do those get one final attempt before purge?
Yes

### 5. **Exercise Media & Assets**

**Q5.A:** For media caching:
- Should we preload all media when user opens Main Dashboard?
- Load on-demand when user opens Exercise Detail?
- Background download when network available?

Background download to facilitate offline working.

**Q5.B:** What's the max file size for cached media? Any limits on total cache size?
Assume 100MB per video clip.

**Q5.C:** Do you have actual exercise media files/URLs, or should I use placeholders for now?
Use placeholders for now.

### 6. **Navigation & Welcome Page**

**Q6.A:** You mention "Back" on Information Page returns to "Welcome Page" - but the workflow shows:
- App Launch → Information Page (if no token) → Home
- There's no "Welcome Page" in the workflow diagram

Should this be:
- A new Welcome/Splash screen before Information?
- Just return to Home?
- Exit the app?

There is a main ‘Hello Buddy’ welcome page before information. This is the page with the Hello Buddy logo inside the disc. The description was added in the last set of responses. We can have a further discussion about this if needed.

**Q6.B:** When session completes and "unlocks next day" - should we:
- Show a congratulations message?
- Automatically navigate somewhere?
- Just update the data silently?

Show a message.

### 7. **Settings & Preferences**

**Q7.A:** If Offline Caching is disabled:
- Should we delete existing cached data?
- Just stop downloading new data?
- Prevent app launch without connection?

Stop downloading new data.

**Q7.B:** Local notifications/reminders - what should trigger them?
- Daily reminder at specific time?
- Reminder if user hasn't completed today's exercises?
- User-configurable schedule?

User configurable, which should include an option for a reminder if user hasn’t completed exercises today. 

### 8. **Error Handling & User Feedback**

**Q8.A:** Re: "Inform user only if progress cannot continue offline" - examples of when to inform:
- No cached data and no internet?
- User tries to skip session offline (needs to sync)?
- Anything else?

For now, no cached data and no internet only. Allow them to skip sessions and then sync when the internet is available. 

**Q8.B:** For validation errors (e.g., invalid email format), should we:
- Show inline errors under each field?
- Toast message at bottom?
- Alert dialog?

Lets try toast messages at the bottom and consider changing if it results in confusion.

### 9. **Database & Migration**

**Q9.A:** You mention basic migration from v1.0 - does this mean:
- We're starting at v1.0, so just set up migration framework?
- There's an existing v1.0 we need to migrate from?

Starting at v1.0 this is a new application, so nothing to migrate from. 

**Q9.B:** Should the PendingQueue table include:
- Original request payload?
- Number of retry attempts?
- Last retry timestamp?
- Error messages from failed attempts?

All of the above

### 10. **Digital Dog Disc on Home Page**

**Q10.A:** The digital dog disc overlapping the wave header:
- Should it be positioned exactly at the wave curve line?
- Centered horizontally?
- Any specific z-index/elevation shadow specs?

The disc doesn’t overlap the wave it sits below the hello buddy canine physiotherapy text in white space, and above the top of the wave.

**Q10.B:** Do you have the dog logo asset, or should I create a placeholder?
I have the asset.

**Q10.C:** What are the exact gradient colors for "light cyan → mid-blue"?

Please list the colours and hex values you have for each component (background, buttons, wave etc) and I will provide the hex values.

### 11. **Progress Chart Details**

**Q11.A:** The progress chart shows "completed vs target per day":
- How many days should be visible? Last 7 days? Last 30?
Last 7 days.
- Should it scroll horizontally if more than fits on screen?
No, just display the last 7 days
- What happens on Day 1 when there's no history?
Blank chart with a no data message

**Q11.B:** Chart legend shows "two series" - are these:
- Completed (actual) vs Target (goal)?
- Today vs Previous days average?
- Current week vs Last week?
Completed vs target.

### 12. **Registration Flow**

**Q12.A:** The Registration "Code" field - is this:
- Required or optional?
- An invitation/access code?
- A programme selection code?
- Validated against a list of valid codes?

This is required for new customers to be able to get access to the main exercises and will be provided separately via an email from the admin system.

**Q12.B:** What validation rules for passwords?
- Minimum length?
- Complexity requirements (uppercase, numbers, special chars)?
- Maximum length?

No validation rules for the MVP

### 13. **Skip Session Workflow**

**Q13.A:** When user skips a session:
- Does it count as completed for progression purposes?
No
- Does it affect the progress chart?
Yes no progress against the target
- Can they go back and complete skipped exercises later?
Yes

**Q13.B:** If skip fails to sync for 30 days, what happens?
- Session remains skipped locally?
- Session becomes available again?
- Any notification to user or admin?

If a skip fails to sync for 30 days disable the account and provide a message if they try to log in, that the account has been disabled and to contact the administrator. 

### 14. **Loading & Shimmer Effects**

**Q14.A:** Shimmer skeletons for lists/grids:
- Should we show approximate number of items (e.g., 6 exercise tiles)?
- Generic rectangular shapes or match actual component shapes?

Generic rectangular shapes is a must. There should be an image in each tile, and clicking the image will take them to the specific exercise page with the video and text. 


### 15. **Tablet-Specific Questions**

**Q15.A:** 3-column grid on tablets:
- What's the breakpoint? Screen width > 600dp?
- Should we also adjust button sizes, fonts, spacing?
- Any max width constraint for very large tablets?

Not sure as I hadn’t considered tablet. Lets build a few pages for the mobile and then look at tablet layout.  
