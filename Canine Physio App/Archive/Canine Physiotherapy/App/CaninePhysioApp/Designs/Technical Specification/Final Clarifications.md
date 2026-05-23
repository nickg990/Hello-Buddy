### 1. **Navigation Flow Clarification**
**Q1.A:** You mentioned "Hello Buddy welcome page before information" but the current workflow shows:
- App Launch → Information Page (if no token) → Hello Buddy (Home)

The workflow is incorrect. It should be Hello Buddy welcome page with options to navigate to: Information, Registration, Settings and Login. Information page navigates back to the previous page. Settings and registration are only accessible from the welcome page and navigate back to it. The login page (which only needs to be used once per device) can access the forgotten password page or the T&C page, or navigate back to the welcome page. T&C navigates to main exercises, which can navigate to progress, skip to next session and (via the image tiles) to the specific exercise page. The specific exercise page can navigate back to the main exercise page or the complete page (via the complete button). The save action on the complete page navigates back to the main exercise page. When an exercise is complete the image on the main exercise page is updated with a green tick. The main exercise page has buttons for skipping or info. Let me know if you need more details on what should be n each page.


Should the flow be:
- App Launch → **Hello Buddy (Welcome)** → Information Page → Hello Buddy (Home)?
- Or is "Hello Buddy" both the welcome screen AND the main hub?

See response above.

### 2. **Dog Disc Positioning**
**Q2.A:** You clarified the disc sits "above the top of the wave" - so the layout from top to bottom is:
1. "Hello Buddy" title text
2. Dog disc (in white space)
3. Wave header starts below the disc?

The layout is as follows: The background colour is #D9D9D9. A white block #FFFFFF is displayed at the top of the screen with the text Hello Buddy and Canine Physiotherapy below. The colour of the Hello Buddy text is #3D81A9 and the colour of the Canine Physiotherapy text is #000000. The wave is formed from a rectangular block with a flattened sinewave on top. The top of the sinewave plus the rectangle uses 50% of the real estate. Overlayed on the wave and rectangle are 4 buttons that are centred and 70% of the width of the page. The first button (from top to bottom) is Info #D9D9D9 with black text, the next is Settings #3D81A9 with white text, then Start Physio ##D9D9D9 with black text and finally Registration #3D81A9. The Logo is displayed inside a disc which has a thick white border and a background of #3D81A9. The white boarder has an outer and inner thin black boarder that is offset to make it look like the disc has depth. 



**Q2.B:** When do I get the dog logo asset? Should I start with a placeholder and you'll provide it later?

Lets get the layout and colours correct for the welcome page and then we can add the logo and use the welcome page as a template for the other pages.

### 3. **Color Scheme Specification**
**Q3.A:** You asked me to list colors and hex values. From the techspec.md, here are the current values to confirm:

| Component | Current Hex | Needs Confirmation? |
|-----------|-------------|-------------------|
| Wave Header | `#7FAFC9` | ✅ |
| Body Background | `#E9F3F9` | ✅ |
| Accent/Dark Text | `#2C3E50` | ✅ |
| Primary Button Fill | `#2F5D80` | ✅ |
| Primary Button Text | `#FFFFFF` | ✅ |
| Ghost Button Border | `#2F5D80` | ✅ |
| Ghost Button Text | `#2F5D80` | ✅ |
| Success | `#3CB371` | ✅ |
| Error | `#D9534F` | ✅ |
| Info | `#4C86A8` | ✅ |
| Card Background | `#FFFFFF` | ✅ |
| Divider | `#D8E5EE` | ✅ |
| **Dog Disc Gradient** | **❓ MISSING** | **❓** |

**Need:** Light cyan → mid-blue gradient hex values for the dog disc.

For now use the colours I’ve given above. We can fine tune that and then use it as a template for the other pages

### 4. **Exercise Grid Layout**
**Q4.A:** You mentioned "image in each tile, clicking image takes to exercise page" - should the **entire tile** be clickable, or **only the image** within the tile?

Entire tile.

### 5. **Session Scrolling Behavior**
**Q5.A:** You said users can "scroll through all exercises past and future" but it's "online only." Should this be:
- A separate "Browse All Exercises" page/mode?
- Extended functionality within the Main Dashboard?
- A calendar/date picker to select different days?

Lets try a calendar / date picker and see how it looks.

### 6. **Notifications Setup**
**Q6.A:** For user-configurable notifications:
- Should this be in the Settings page?
Yes
- What time options? (dropdown, time picker, preset times)
Time picker
- Default reminder time if user doesn't configure?
Default is off

### 7. **Account Suspension Message**
**Q7.A:** When an account is disabled after 30-day sync failure:
- What should the exact message say?
- Should we provide contact information for the administrator?
- Should this be a full-screen message or a popup?

A popup with the contact email and phone of the admin. Use placeholder data for now. 
