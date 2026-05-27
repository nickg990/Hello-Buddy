# Reusable Planning Approach for a Canine Physiotherapy App

This guide summarises the approach used across the Life Balance planning documents so it can be reused on a different project. The method is simple: move from user needs, to requirements, to site structure, to interface decisions, and keep each stage traceable to the one before it.

## 1. Overall method

The four documents work as one chain:

1. Users and tasks identifies the main user groups and the highest-priority tasks they need to complete.
2. Requirements turns those tasks into clear user-centred must/should statements.
3. Sitemap turns the highest-priority requirements into a small set of pages and navigation labels.
4. Interface plan explains how page structure and interface sections support the requirements and tasks.

The key principle is traceability. Each planning decision should be explainable in plain language:

- Which user is this for?
- Which task does it help them complete?
- Which requirement does it satisfy?
- Which page or interface element delivers it?

## 2. What each document should do

### Users and tasks

Purpose:

- Define 2 to 4 core user types.
- List the main things each user is trying to do.
- Mark the most important tasks so they are prioritised later.

Approach used in Life Balance:

- User groups were defined by intent, not demographics.
- Tasks focused on what users needed to understand, trust, or do next.
- The highest-value tasks were the ones tied to conversion, confidence, and clarity.

How to reuse this for a canine physiotherapy app:

- Define user types such as first-time dog owner, returning client, and vet/referrer.
- Focus on tasks like understanding treatment options, checking therapist credibility, knowing what happens at an appointment, booking, and finding rehabilitation advice.
- Mark the tasks that must be effortless on mobile.

Suggested output shape:

- User type
- Key tasks
- Top-priority tasks
- Short note on what the site must make easy

### Requirements

Purpose:

- Convert user tasks into actionable website requirements.
- Keep them specific, prioritised, and user-centred.

Approach used in Life Balance:

- Each requirement was written as a clear must/should statement.
- Each one linked back to a user type and a specific task.
- Accessibility, trust, and next-step clarity were included as first-class requirements, not optional extras.

How to reuse this for a canine physiotherapy app:

- Write requirements such as making treatment services easy to understand, making booking straightforward, showing credentials and outcomes clearly, explaining the process before the first appointment, and ensuring accessibility across devices.
- Include both conversion tasks and reassurance tasks.
- Keep the list short enough to drive design decisions.

Suggested output shape:

- Priority
- Requirement
- User type
- Task supported
- Note explaining why it matters

### Sitemap

Purpose:

- Turn requirements into a clear, minimal information architecture.
- Make main tasks easy to find from the homepage and navigation.

Approach used in Life Balance:

- The sitemap stayed small and task-led.
- Navigation labels were plain language.
- Separate destinations were created for services, trust/credibility, useful content, and contact/booking.

How to reuse this for a canine physiotherapy app:

- Build navigation around what users need most, for example: Home, Treatments, About the Therapist, Advice, Book, Contact.
- Give trust content a clear destination if credibility is critical.
- Keep booking visible as a direct path, not buried in general contact content.

Suggested output shape:

- Main navigation
- Page hierarchy
- Short explanation of why those pages exist
- Short note on how the sitemap improves on any earlier sketches

### Interface plan

Purpose:

- Explain how the chosen page structure becomes a usable interface.
- Show how homepage sections and navigation choices support the top tasks.

Approach used in Life Balance:

- The homepage was treated as the main entry point.
- The highest-priority tasks were surfaced early in the page.
- Requirement traceability was stated directly, section by section.
- User/task traceability was also included to justify the layout.

How to reuse this for a canine physiotherapy app:

- Plan a homepage that quickly answers: what canine physiotherapy is offered, why the practitioner is trustworthy, what happens next, and how to book.
- Include sections such as hero, treatment overview, benefits or conditions treated, trust signals, how it works, advice/resources, and booking CTA.
- State exactly which requirements each section supports.

Suggested output shape:

- Main planning choices
- Improvements from earlier wireframes
- Requirement traceability
- User and task traceability

## 3. Transferable design logic

These planning choices are the real method behind the documents:

- Prioritise intent over broad audience description.
- Make top tasks visible early, especially on the homepage.
- Treat trust and reassurance as core UX work, not just marketing content.
- Use plain-language labels in navigation and calls to action.
- Make the next step obvious, especially for first-time users.
- Keep the structure small unless a larger structure clearly supports a user task.
- Show accessibility and mobile use as explicit requirements.
- Keep every design choice traceable back to a user task.

## 4. Canine physiotherapy adaptation

For the canine physiotherapy app, the same logic can be reframed like this:

- Users need to understand whether the service is right for their dog.
- They need confidence in the practitioner, treatment approach, and expected outcomes.
- They need to know what conditions can be treated and what happens during assessment and rehabilitation.
- They need a low-friction route to book, enquire, or get referred.
- Returning users may also need exercise plans, aftercare guidance, progress tracking, or appointment follow-up.

That means the planning work should probably emphasise:

- service clarity
- therapist credibility
- treatment process transparency
- booking and referral flow
- practical rehabilitation guidance

## 5. Prompt-ready brief for Claude Opus 4.7

Use or adapt this prompt on the new project:

```text
Create four planning documents for a canine physiotherapy app: Users and Tasks, Requirements, Sitemap, and Interface Plan.

Use this method:

1. Start with 2 to 4 main user types defined by intent, for example first-time dog owner, returning client, and vet/referrer.
2. For each user type, list the key tasks they need to complete.
3. Mark the highest-priority tasks that the app must support clearly and quickly.
4. Turn those tasks into a short set of user-centred requirements written as must/should statements.
5. For each requirement, include priority, user type, task supported, and a short note.
6. Create a sitemap with plain-language navigation labels and a minimal structure driven by the highest-priority requirements.
7. Write an interface plan explaining how the homepage or main app entry screen supports the top tasks first.
8. Include requirement traceability and user/task traceability in the interface plan.

Planning priorities:

- Make treatments and conditions treated easy to understand.
- Make therapist credibility and trust signals easy to find.
- Explain what happens before, during, and after appointments.
- Make booking, enquiry, and referral routes obvious.
- Consider accessibility, mobile use, and reassurance for anxious first-time users.

Write in clear plain English. Keep the structure practical and concise. Avoid generic UX language unless it directly supports a planning decision.
```

## 6. Simple checklist

Use this checklist to judge whether Claude has followed the approach properly:

- Are the user groups based on real intent and behaviour?
- Are the top tasks clearly prioritised?
- Does each requirement clearly link to a user and task?
- Does the sitemap reflect the highest-priority requirements?
- Does the interface plan explain why sections appear in that order?
- Is trust/credibility handled explicitly?
- Is accessibility included explicitly?
- Is booking or the next step clear?
- Can each interface decision be traced back to a user need?
