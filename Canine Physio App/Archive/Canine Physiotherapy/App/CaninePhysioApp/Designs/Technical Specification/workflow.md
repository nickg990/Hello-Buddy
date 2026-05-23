```mermaid
flowchart TD
    A0([App Launch]) --> A1{Has local session token?}
    A1 -- No --> A2[Information Page]
    A1 -- Yes --> A3{Terms accepted?}
    A3 -- No --> E0[Terms & Conditions]
    A3 -- Yes --> H0[Hello Buddy - Home]
    A2 --> H0
    H0 -->|Login| L0[Login Page]
    H0 -->|Register| R0[Registration Page]
    H0 -->|Start Physio| E0
    H0 -->|Settings| S0[Settings Page]
    L0 --> L1{Tap Login}
    L1 --> L2[Client-side validation]
    L2 --> L3{Network available?}
    L3 -- No --> L4[Show offline message]
    L4 --> L5[Offer cached Info Only mode]
    L5 --> H0
    L3 -- Yes --> L6[POST /auth/login]
    L6 --> L7{200 OK?}
    L7 -- Yes --> L8[Store token + profile securely]
    L8 --> E0
    L7 -- No --> L9[Show error & allow retry]
    R0 --> R1{Tap Register}
    R1 --> R2[Client-side validation]
    R2 --> R3{Network available?}
    R3 -- No --> R4[Show offline message]
    R4 --> R0
    R3 -- Yes --> R5[POST /auth/register]
    R5 --> R6{201 Created?}
    R6 -- Yes --> R7[Auto-login & store token]
    R7 --> E0
    R6 -- No --> R8[Show field errors]
    E0 --> E1{Accept toggle ON?}
    E1 -- Yes --> M0[Main Exercises Dashboard]
    E1 -- No --> H0
    M0 --> M1{Network available?}
    M1 -- Yes --> M2[GET /programme/today]
    M2 --> M3[Cache programme locally]
    M1 -- No --> M4[Load programme from cache]
    M4 --> M5{Cache exists?}
    M5 -- No --> M6[Show offline-empty state]
    M6 --> H0
    M5 -- Yes --> M7[Render cached programme]
    M3 --> M7
    M7 -->|Select Exercise| X0[Exercise Detail]
    M7 -->|Skip Session| SK0[Skip Session]
    M7 -->|View Progress| P0[Exercise Progress]
    X0 --> X1{Tap Complete}
    X1 --> X2[Collect reps/sets/time/notes]
    X2 --> X3[Persist locally - pending sync]
    X3 --> X4{Network available?}
    X4 -- Yes --> X5[POST /exercise/complete]
    X5 --> X6{200 OK?}
    X6 -- Yes --> X7[Mark synced]
    X7 --> P0
    X6 -- No --> X8[Keep pending & notify]
    X8 --> P0
    X4 -- No --> X9[Queue for background sync]
    X9 --> P0
    P0 --> P1{Network available?}
    P1 -- Yes --> P2[GET /progress/summary]
    P2 --> P3[Merge with local pending]
    P1 -- No --> P4[Build from cache + pending]
    P3 --> P5[Render chart]
    P4 --> P5
    P5 --> M0
    SK0 --> SK1[Enter comments - optional]
    SK1 --> SK2[Persist locally]
    SK2 --> SK3{Network available?}
    SK3 -- Yes --> SK4[POST /session/skip]
    SK4 --> SK5{200 OK?}
    SK5 -- Yes --> SK6[Return to Main Dashboard]
    SK5 -- No --> SK7[Keep pending & notify]
    SK7 --> M0
    SK3 -- No --> SK8[Queue for sync]
    SK8 --> M0
    S0 --> S1{Toggle Offline Caching?}
    S1 --> S2[Update local preferences]
    S2 --> H0
    
    subgraph "Background Sync"
        BS0[On app resume / timer] --> BS1{Has pending writes?}
        BS1 -- Yes --> BS2{Network available?}
        BS2 -- Yes --> BS3[POST /sync/bulk]
        BS3 --> BS4{200 OK?}
        BS4 -- Yes --> BS5[Mark items synced]
        BS4 -- No --> BS6[Keep queue; backoff]
        BS2 -- No --> BS6
        BS1 -- No --> BS7[No-op]
    end
```

---

# End-to-End Workflow (Narrative)

1) **Launch & Session Gate** → If no token, user sees **Information** then **Home**. If token exists but Terms not accepted, route to **Terms**.
2) **Home** → Entry to **Login**, **Registration**, **Start Physio** (Terms), **Settings**.
3) **Auth** → Validate → POST login/register → store token → **Terms** if required.
4) **Terms** → Toggle **Accept** → proceed to **Main Dashboard**.
5) **Dashboard** → Fetch/Cache programme (or use cached offline) → open **Exercise Detail**, **Progress**, or **Skip**.
6) **Exercise Detail** → User completes parameters → Local persist → POST now or queue if offline.
7) **Progress** → Merge server data with pending local items → render chart.
8) **Skip** → Local persist → POST now or queue.
9) **Background Sync** → Flush pending writes on resume/timer with backoff.