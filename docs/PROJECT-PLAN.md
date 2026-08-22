# Project Plan — Asynchronous Video Interview Web Application

**Course context:** Supervised Internship — extra project (individual, pass/fail on documented hours, ~40–60 h)
**Student:** Davyd Lysytsia
**Repo:** github.com/DavydLysytsia/async-interview
**Sources:** instructor requirements PDF (`Asynchronous_Interview_Web_Project_Requirements 1.pdf`), instructor whiteboard sketch, instructor email (2026-08).

---

## 1. What the app is

A job candidate builds a **reusable asynchronous interview profile**: they sign in, fill in a
basic profile (name, headline, bio, skills, links), and answer a standard set of at least five
interview questions by uploading a video for each. Videos are uploaded **to the candidate's own
YouTube channel** through Google OAuth 2.0 + the YouTube Data API; the app stores only the
returned YouTube video ID and embeds the videos in a clean preview page.

Explicitly out of scope (per PDF §10): recruiter portal, AI scoring/transcription, payments,
video storage/transcoding infrastructure, production OAuth verification.

## 2. Stack decision

| Layer | Choice | Justification |
|---|---|---|
| Backend | **ASP.NET Core (.NET 10) Web API** | Same family as the SODV program's class stack; Davyd deployed an ASP.NET Core API to Azure App Service two weeks ago, so setup/deploy knowledge is fresh and demonstrable. Official Google API client (`Google.Apis.YouTube.v3`) is well supported. |
| ORM / DB | **EF Core + SQLite** | PDF §5 explicitly allows SQLite for a prototype. Zero external services; DB file persists in `/home` on Azure App Service. |
| Frontend | **React (Vite) SPA** | React was the class frontend framework (React 19). Vite instead of the deprecated CRA. SPA is built into the API's `wwwroot`, so the whole app deploys as **one** Azure App Service — one URL, one HTTPS OAuth redirect origin, no CORS. |
| Auth | **Google sign-in only** (no local passwords) | See §3. |
| Hosting | **Azure App Service (Azure for Students, free F1 Linux)** | Proven path (bvc-group-2-backend deploy). Free HTTPS domain satisfies the "HTTPS for deployed OAuth callback" requirement. |
| Source control | Git + GitHub (private repo on DavydLysytsia) | Required deliverable; commit history documents progress. |

## 3. Requirement adjustments (to be confirmed with instructor — see INSTRUCTOR-NOTE.md)

The sketch predates the email/PDF; the instructor already said auth moved to
**OAuth + YouTube API** instead of a custom pipeline. Concrete adjustments:

1. **Sign-in = "Sign in with Google" only.** The PDF's data model allows "password hash **or
   external login identifier**". Since every user needs a Google account anyway (to upload to
   YouTube), local email/password registration, password recovery, and password hashing are
   dropped. Register/Login/Recover-password pages from the sketch collapse into one Google
   sign-in action. Sign-out and server-side route protection remain.
2. **Two-step Google authorization (least privilege).** Sign-in requests only `openid email
   profile`. YouTube scopes (`youtube.upload` + `youtube.readonly`) are requested in a separate
   explicit **"Connect YouTube"** step, matching PDF §6 "request only the permissions required".
   `youtube.readonly` is needed to detect the "no YouTube channel available" failure case (PDF §5).
3. **Video workflow: file upload first, in-browser recording as stretch.** PDF §2.4 explicitly
   allows the simpler file-selection workflow. MediaRecorder-based in-browser capture is a
   stretch goal if hours remain.
4. **Sketch features deferred to stretch scope:** named video *collections*, public share links
   with URL shortener, and the "Phase 2" AI-generated resume PDF. Core PDF requirements don't
   need them; they are listed in §8 so they can be picked up if the hour budget allows.
5. **Consent content:** privacy policy / terms / statement-of-truthfulness from the sketch are
   kept as lightweight static pages + a first-login consent notice, including the PDF-required
   statement that videos are stored on YouTube under the candidate's own account settings.

## 4. Architecture

```
Browser (React SPA)
   │  fetch /api/*  (cookie auth)
   ▼
ASP.NET Core  ── serves SPA from wwwroot (fallback to index.html)
   ├─ AuthController        /api/auth/google | /me | /logout | /dev-login (dev only)
   ├─ ProfileController     GET/PUT /api/profile
   ├─ QuestionsController   GET /api/questions   (seeded, ≥5)
   ├─ ResponsesController   GET/POST/DELETE /api/responses  (+ multipart video upload)
   ├─ YouTubeController     /api/youtube/status | /connect | /callback
   ├─ EF Core (SQLite app.db)
   └─ YouTubeUploadService  → Google OAuth token store (DB) → YouTube Data API videos.insert
```

**Data model** (PDF §4 mapping):

- `AppUser` — Id, Email, DisplayName, GoogleSubject (external login id), ConsentAcceptedAt, CreatedAt
- `CandidateProfile` — UserId (1:1), FullName, Headline, Biography, Skills, ContactLinks
- `InterviewQuestion` — Id, Text, DisplayOrder, IsActive (seeded with 8)
- `VideoResponse` — Id, UserId, QuestionId, YouTubeVideoId, PrivacyStatus, UploadStatus
  (None/Uploading/Completed/Failed), CreatedAt, UpdatedAt
- `YouTubeToken` — UserId, Google `TokenResponse` JSON (access+refresh), stored server-side only

**Secrets:** `server/.env` (gitignored) holds `GOOGLE_CLIENT_ID`, `GOOGLE_CLIENT_SECRET`;
`.env.example` committed. Tokens live in the DB, DB file gitignored.

## 5. Known external-API risks (documented up front, per instructor note in PDF)

1. **Unverified API projects → uploads locked private.** Since July 2020, videos uploaded via
   the YouTube Data API by API projects that have not completed a YouTube API audit are forced
   to **private** and cannot be switched to unlisted/public. Our project stays in testing mode
   (no audit), so `privacyStatus: unlisted` in the request will likely be overridden to private.
   *Mitigation:* request unlisted, record what YouTube actually returns, display it honestly in
   the UI, and document the limitation in the README (PDF §2.7 already requires explaining the
   chosen privacy setting). Embedded playback of private videos may show "Video unavailable"
   even for the owner; fallback is a direct watch/Studio link (owner can always view) — an
   explicitly documented "reasonable alternative" as the PDF's instructor note allows.
2. **Quota:** default YouTube Data API quota is 10,000 units/day; `videos.insert` costs ~1,600
   → **~6 uploads/day**. Test with tiny clips; don't burn quota on re-runs.
3. **Testing-mode refresh tokens expire after 7 days** (external consent screen in Testing).
   Users must periodically re-connect YouTube; the UI treats "token invalid" as
   "reconnect needed". Acceptable for a prototype; documented.
4. **Test users:** while in testing mode only listed test users can authorize — Davyd's Google
   account (and optionally the instructor's) must be added on the consent screen.

## 6. Required failure handling (PDF §5)

- Authorization denied → friendly message on the connect page, retry action.
- No YouTube channel on the account → detected via `channels.list(mine=true)`, explained with a
  "create a channel on YouTube first" link.
- Upload failure → response record marked Failed with a retry option; message shown.
- Unsupported file → client-side accept filter + server-side content-type/size validation.
- Missing required fields → per-field validation messages on all forms.

## 7. Milestones (mapped to the PDF's suggested hours)

| # | Milestone | PDF hours | Status |
|---|---|---|---|
| 1 | Planning & setup: requirements review, plan, repo, scaffold, DB plan | 4–6 | **done 2026-08-22** (this document + scaffold) |
| 2 | Auth & profile: Google sign-in, protected routes, profile CRUD | 8–12 | scaffolded; needs Google credentials to go live |
| 3 | Interview workflow: questions, response states, file selection | 8–12 | scaffolded skeleton |
| 4 | Google OAuth + YouTube upload (the risky one — spike EARLY) | 10–16 | service written; blocked on Google Cloud setup (GOOGLE-SETUP.md) |
| 5 | Preview page, validation, responsive layout, accessibility | 5–8 | |
| 6 | Testing evidence, README, final report, demo video, activity log | 5–8 | activity log started |

Sketch requirements carried into the build: same header/footer everywhere; first input
auto-focused on action pages and sane tab order; forms keep a draft in localStorage so an
accidental page close loses nothing.

## 8. Stretch scope (only if hours remain)

In-browser recording (MediaRecorder) → collections of responses → shareable public preview link
(short URL) → AI-generated resume PDF (sketch "Phase 2").

## 9. Deliverables checklist (PDF §8)

- [x] Git repo with meaningful history (started 2026-08-22)
- [ ] README (living document — grows with features)
- [x] `.env.example` safe config template
- [x] Schema via EF Core model + seed data
- [ ] Test plan + evidence (manual test table minimum; xUnit where cheap)
- [ ] 5–10 min demo video or live demo
- [ ] Final report (2–4 pages)
- [ ] Activity log (`docs/ACTIVITY-LOG.md`) — dates, tasks, hours, evidence
