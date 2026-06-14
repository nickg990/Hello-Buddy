# Canine Physio Admin - Workflow Diagram

## Main Admin Application Workflow

```mermaid
graph TD
    Start([User Visits App]) --> CheckAuth{Authenticated?}
    CheckAuth -->|No| Login[Account/Login Page]
    Login --> LoginForm[Enter Credentials + Honeypot Check]
    LoginForm --> AuthCheck{Auth Success?}
    AuthCheck -->|Invalid| LoginError[Show Error Message]
    LoginError --> Login
    AuthCheck -->|MustChangePassword| ChangePwd[Account/MustChangePassword]
    ChangePwd --> ChangeForm[Enter New Password]
    ChangeForm --> ChangeSuccess[Password Updated]
    ChangeSuccess --> Home
    AuthCheck -->|Locked| LockError[Account Locked 15 min]
    LockError --> Login
    AuthCheck -->|Inactive| InactiveError[Account Inactive]
    InactiveError --> Login
    AuthCheck -->|Success| Home[Home/Index - Dashboard]
    CheckAuth -->|Yes| Home
    
    Home --> NavChoice{Select Action}
    
    %% Cases Workflow
    NavChoice -->|View Cases| CasesList[Cases/Index<br/>List All Cases]
    CasesList --> CaseAction{Case Action?}
    CaseAction -->|View Details| CaseDetail[Cases/id<br/>View Case Detail]
    CaseAction -->|Create New| CaseCreate[Cases/Create<br/>Create New Case]
    CaseCreate --> CaseForm[Select Pet + Enter Details]
    CaseForm --> SaveCase{Valid?}
    SaveCase -->|No| CaseForm
    SaveCase -->|Yes| CaseSaved[Redirect to CaseDetail]
    CaseAction -->|Edit| CaseEdit[Cases/id/Edit<br/>Edit Case Info]
    CaseEdit --> CaseForm
    
    %% Case Detail Workflow
    CaseDetail --> CaseDetailVm[Display Case Info + Programmes]
    CaseDetailVm --> CaseDetailAction{Action?}
    CaseDetailAction -->|Add Note| AddNote[POST Cases/id/Notes]
    AddNote --> NoteForm[Enter Note Text]
    NoteForm --> SaveNote{Valid?}
    SaveNote -->|No| NoteForm
    SaveNote -->|Yes| NoteSaved[Display Updated Case]
    NoteSaved --> CaseDetail
    CaseDetailAction -->|Manage Programme| ProgAction
    CaseDetailAction -->|View Assigned Programmes| ProgList
    CaseDetailAction -->|Back| CasesList
    
    %% Programme Workflow
    ProgAction{Programme Action?} -->|Create New| ProgCreate[Create Programme]
    ProgCreate --> ProgForm[Set Programme Details]
    ProgForm --> SaveProg{Valid?}
    SaveProg -->|No| ProgForm
    SaveProg -->|Yes| ProgSaved[Redirect to Builder]
    
    ProgAction -->|Edit Existing| ProgList[Programmes/id/History<br/>View Programme Versions]
    ProgList --> SelectVersion{Select Version?}
    SelectVersion -->|Published| PubView[View Published]
    SelectVersion -->|Draft| DraftEdit[Edit Draft]
    
    %% Programme Builder Workflow
    DraftEdit --> BuilderHome[Programmes/id/Builder<br/>Programme Builder Main]
    ProgSaved --> BuilderHome
    BuilderHome --> BuilderUI[Display:<br/>- Structure Panel<br/>- Editor Panel<br/>- Preview Pane]
    BuilderUI --> EditChoice{Edit What?}
    
    EditChoice -->|Session Structure| StructEdit[Edit Dates + Session Count]
    StructEdit --> StructForm[Set Start/End Dates + Sessions]
    StructForm --> StructSave[POST Structure]
    StructSave --> StructResult{Success?}
    StructResult -->|No| StructError[Show Error Message]
    StructError --> StructEdit
    StructResult -->|Yes| StructOk[Structure Updated]
    StructOk --> BuilderUI
    
    EditChoice -->|Exercises| ExerciseEdit[Edit Exercise Sessions]
    ExerciseEdit --> ExerciseForm[Update Reps/Sets/Hold/Notes]
    ExerciseForm --> ExerciseSave[POST Builder]
    ExerciseSave --> ExerciseResult{Success?}
    ExerciseResult -->|Published Block| PublishedError[Programme Immutable Error]
    PublishedError --> ExerciseEdit
    ExerciseResult -->|Success| ExerciseOk[Exercises Updated]
    ExerciseOk --> BuilderUI
    
    EditChoice -->|Preview| PreviewAction[Live Preview<br/>View Rendered Document]
    PreviewAction --> BuilderUI
    
    BuilderUI --> Publish{Ready to Publish?}
    Publish -->|Yes| ProgrammePreview[Programmes/id/Preview<br/>Final Review Before Publish]
    Publish -->|No| BuilderUI
    
    %% Preview & Publish Workflow
    ProgrammePreview --> PreviewDoc[Display Full Programme<br/>as Client Will See]
    PreviewDoc --> PublishDecision{Approve?}
    PublishDecision -->|Cancel| BackToBuilder[Return to Builder]
    BackToBuilder --> BuilderUI
    PublishDecision -->|Confirm Publish| DoPublish[POST Programmes/id/Publish]
    DoPublish --> GeneratePDF[Trigger PDF Generation]
    GeneratePDF --> CreateVersion[Create Published Version]
    CreateVersion --> PublishOk[Programme Published]
    PublishOk --> PublishConfirm[Return to Case Detail]
    PublishConfirm --> CaseDetail
    
    %% Supporting Data Workflows
    NavChoice -->|Manage Pets| PetList[Pets/Index]
    PetList --> PetAction{Pet Action?}
    PetAction -->|View| PetDetail[Pets/Details]
    PetAction -->|Add New| PetCreate[Pets/Index]
    PetAction -->|Edit| PetEdit[Pets/Edit]
    PetEdit --> PetForm[Enter Pet Details]
    PetForm --> SavePet[POST Edit]
    SavePet --> PetList
    
    NavChoice -->|Manage Owners| OwnerList[Owners/Index]
    OwnerList --> OwnerAction{Owner Action?}
    OwnerAction -->|View| OwnerDetail[Owners/Details]
    OwnerAction -->|Add New| OwnerCreate[Owners/Index]
    OwnerAction -->|Edit| OwnerEdit[Owners/Edit]
    OwnerEdit --> OwnerForm[Enter Owner Details]
    OwnerForm --> SaveOwner[POST Edit]
    SaveOwner --> OwnerList
    
    NavChoice -->|Manage Exercises| ExerciseList[Exercises/Index]
    ExerciseList --> ExerciseListAction{Exercise Action?}
    ExerciseListAction -->|View| ExerciseDetail[Exercises/Details]
    ExerciseListAction -->|Add New| ExerciseCreate[Exercises/Index]
    ExerciseListAction -->|Edit| ExerciseListEdit[Exercises/Edit]
    ExerciseListEdit --> ExerciseListForm[Enter Exercise Details]
    ExerciseListForm --> SaveExercise[POST Edit]
    SaveExercise --> ExerciseList
    
    %% Admin Workflow
    NavChoice -->|Admin Functions| AdminPanel[Admin/Practitioners]
    AdminPanel --> AdminAction{Admin Action?}
    AdminAction -->|View Practitioners| PractList[List All Practitioners]
    AdminAction -->|Add Practitioner| AddPract[Admin/Add]
    AddPract --> PractForm[Enter Name + Email]
    PractForm --> SavePract[POST Add]
    SavePract --> SetPassword[Admin/SetPassword]
    SetPassword --> PwdForm[Generate Temp Password]
    PwdForm --> PwdSent[Password Set]
    PwdSent --> PractList
    AdminAction -->|Edit Practitioner| EditPract[Admin/Edit]
    EditPract --> EditForm[Update Details]
    EditForm --> SaveEdit[POST Edit]
    SaveEdit --> PractList
    AdminAction -->|Manage Practitioner Password| ManagePwd[Admin/ChangePassword]
    ManagePwd --> PwdForm
    AdminAction -->|GDPR Data Control| DataControl[Admin/DataControl]
    DataControl --> GDPRAction{GDPR Action?}
    GDPRAction -->|Export| DataExport[Export Practitioner Data]
    GDPRAction -->|Delete| DataDelete[Delete Practitioner Account + Data]
    
    %% Navigation
    CasesList -.->|Always Available| NavBar[Top Navigation]
    NavBar -.->|Home| Home
    NavBar -.->|Cases| CasesList
    NavBar -.->|Admin| AdminPanel
    NavBar -.->|Logout| Logout[Sign Out]
    Logout --> SignedOut([Redirected to Home])
    
    style Login fill:#e1f5ff
    style Home fill:#c8e6c9
    style CasesList fill:#fff9c4
    style CaseDetail fill:#fff9c4
    style BuilderHome fill:#f8bbd0
    style ProgrammePreview fill:#f8bbd0
    style AdminPanel fill:#d1c4e9
    style Logout fill:#ffccbc
```

## User Workflows by Role

### Practitioner Workflow (Main User)
1. **Login** → Account/Login
2. **View Dashboard** → Home/Index
3. **Select Case** → Cases/Index → Select Case
4. **View Case Details** → Cases/{id} → View Programme Status
5. **Build Programme** → Programmes/{id}/Builder
   - Edit Structure (dates/sessions)
   - Edit Exercises (reps/sets/duration)
   - Live Preview
6. **Publish Programme** → Programmes/{id}/Preview → Publish
7. **Add Case Notes** → Cases/{id}/Notes
8. **Manage Supporting Data** → Pets/Owners/Exercises as needed

### Administrator Workflow
1. **Login** → Account/Login
2. **Navigate to Admin** → Admin/Practitioners
3. **Manage Practitioners**
   - Add new practitioner → Admin/Add
   - Set password → Admin/SetPassword
   - Edit practitioner → Admin/Edit
   - Change password → Admin/ChangePassword
4. **GDPR Data Control** → Admin/DataControl
   - Export practitioner data
   - Delete practitioner account & associated data

### New Practitioner Onboarding
1. Receive email with temporary password (Admin/SetPassword)
2. Login → Account/Login
3. Forced password change → Account/MustChangePassword
4. Access main dashboard → Home/Index

## Key Data Dependencies

- **Cases** depend on **Pets** (PetId)
- **Pets** depend on **Owners** (OwnerId)
- **Programmes** depend on **Cases** (CaseId)
- **Programmes** contain **Exercises** and **Sessions**
- **Case Notes** are attached to **Cases**
- **Practitioners** are isolated per practitioner (X-Practitioner-Id header)

## Security Features

- **Authentication**: Cookie-based after login
- **Authorization**: [Authorize] attribute on protected routes
- **Anti-forgery**: [ValidateAntiForgeryToken] on POST actions
- **Honeypot**: Bot detection on login form
- **Account Lockout**: 15-minute lockout after failed attempts
- **Practitioner Isolation**: API enforces X-Practitioner-Id header (via middleware)
- **GDPR Compliance**: Data export/deletion via Admin/DataControl

## View Structure

```
Views/
├── Home/
│   ├── Index.cshtml (Dashboard)
│   └── AccessDenied.cshtml
├── Account/
│   ├── Login.cshtml
│   └── MustChangePassword.cshtml
├── Cases/
│   ├── Index.cshtml (List)
│   └── Edit.cshtml (Create/Edit)
├── CaseDetail/
│   └── Index.cshtml (Detail + Notes)
├── Programmes/
│   ├── Builder.cshtml (Main builder)
│   ├── History.cshtml (Version history)
│   ├── Preview.cshtml (Final preview)
│   ├── _BuilderEditor.cshtml (Editor panel)
│   ├── _BuilderPreviewPane.cshtml (Live preview)
│   └── _ProgrammePreviewDocument.cshtml (Document template)
├── Owners/
│   ├── Index.cshtml
│   ├── Details.cshtml
│   └── Edit.cshtml
├── Pets/
│   ├── Index.cshtml
│   ├── Details.cshtml
│   └── Edit.cshtml
├── Exercises/
│   ├── Index.cshtml
│   ├── Details.cshtml
│   └── Edit.cshtml
└── Admin/
    ├── Practitioners.cshtml (List)
    ├── Add.cshtml
    ├── Edit.cshtml
    ├── SetPassword.cshtml
    ├── ChangePassword.cshtml
    └── DataControl.cshtml (GDPR)
```
