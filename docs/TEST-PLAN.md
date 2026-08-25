# Test Plan & Evidence — Async Interview Profile

Manual test table (PDF deliverable #5). Every "Pass" row records what was actually
observed on the date given, not what should happen. Uploads are tested sparingly on
purpose: the default YouTube API quota allows ~6 `videos.insert` calls per day.

Environments:

- **L-dev** — local, `DEV_FAKE_AUTH=true` (no Google credentials), Windows, http://localhost:5240
- **L-real** — local with real Google OAuth credentials (testing mode, test user `lysytsiad@gmail.com`)
- **PROD** — deployed Azure App Service (see README/Deployment)

| # | Area | Steps | Expected | Actual | Env / date | Status |
|---|------|-------|----------|--------|-----------|--------|
| 1 | Health | GET `/api/health` | `{"ok":true}` | As expected | L-dev 08-22, PROD 08-24 | Pass |
| 2 | Auth config | GET `/api/auth/config` | Flags mirror server env | `googleEnabled:false, devFakeAuth:true` before creds; `googleEnabled:true, devFakeAuth:false` after | L-dev + L-real 08-22/24 | Pass |
| 3 | Dev sign-in | POST `/api/auth/dev-login`, then `/api/auth/me` | Demo session cookie; user returned | Demo Candidate created (user id 1) | L-dev 08-22 | Pass |
| 4 | Route protection | GET `/api/profile` with no cookie | 401, no data leak | 401 returned | L-dev 08-22 | Pass |
| 5 | Unknown API route | GET `/api/nope` | JSON 404, not the SPA page | `{"error":"Not found."}` 404 | L-dev 08-22 | Pass |
| 6 | SPA serving | Open `/`, navigate client routes | React app served, routing works, 404 page on junk URL | Landing/dashboard/interview render | L-dev 08-22, L-real 08-24 | Pass |
| 7 | Google sign-in | Click "Sign in with Google" | Google flow; app user created; dashboard shows identity | User id 2 created for `lysytsiad@gmail.com`, "Welcome, Davyd Lysytsia" | L-real 08-24 | Pass |
| 8 | First-login consent | Sign in first time; accept notice; revisit | Notice blocks until accepted; never shown again | Accepted 12 s after first sign-in (DB `ConsentAcceptedAt` 18:14:29); absent on later visits | L-real 08-24 | Pass |
| 9 | Profile validation | PUT `/api/profile` without `fullName` | Field error message, nothing saved | `{"errors":{"fullName":"Full name is required."}}` (client shows same rule) | L-dev 08-22 | Pass |
| 10 | Profile save | Fill profile, save, open preview | Data persists and shows on preview | Saved via API; preview returned same values | L-dev 08-22 | Pass |
| 11 | Draft autosave | Type in profile form, reload page without saving | Draft restored from localStorage | Typed headline text survived a full reload, unsaved | L-real 08-24 | Pass |
| 12 | Connect YouTube | Connect → Google consent (both scopes) → callback | Status shows connected + channel name | "✓ Connected to channel Davyd Lysytsia"; testing-mode warning shown as documented | L-real 08-24 | Pass |
| 13 | Channel detection | `/api/youtube/status` after connect | `hasChannel:true`, channel title | Channel found via `channels.list(mine)` | L-real 08-24 | Pass |
| 14 | Video upload | Upload 6 s mp4 for Q1 | Uploaded to own channel; video id + privacy stored; card shows ✓ | Video `3p81Yj_mhu8`, privacy **unlisted** (honored, not forced private) | L-real 08-24 | Pass |
| 15 | Preview embeds | Open `/preview` after upload | Question + playable embedded video | Embed renders and plays | L-real 08-24 | Pass |
| 16 | Replace / remove answer | Replace video; remove answer record | Old id replaced / record deleted (YouTube video untouched) | — | — | Not yet tested |
| 17 | Failure: consent denied | Cancel on Google consent screen | Friendly message, retry offered | — | — | Not yet tested |
| 18 | Failure: unsupported file | Upload a .txt | Rejected server-side with a friendly message, no crash | "Unsupported file type. Upload a video file (mp4, webm, mov...)." shown on the card | L-real 08-24 | Pass |
| 19 | Failure: expired token | Wait ~7 days (testing-mode expiry) | Status shows "reconnect needed"; upload prompts reconnect | Expected around 08-31; will document | — | Pending |
| 20a | PROD health/config | GET `/api/health`, `/api/auth/config` on the Azure URL | 200s; `googleEnabled:true, devFakeAuth:false` | As expected | PROD 08-24 | Pass |
| 20b | PROD SPA + routing | Open the site root | Landing renders over HTTPS | As expected | PROD 08-24 | Pass |
| 20c | PROD OAuth challenge | GET `/api/auth/google`, inspect 302 | Redirect to accounts.google.com with `https://` redirect_uri (forwarded headers working) | Exact registered URI + correct scopes observed | PROD 08-24 | Pass |
| 20d | PROD sign-in round trip | Complete Google sign-in in a browser | Dashboard, session cookie | Blocked *on the dev machine only*: local security software intercepts browser traffic to the new domain (curl succeeds; google.com fine). Verify from another device (e.g. phone) | PROD 08-24 | Blocked / pending |
| 21 | Responsive layout | Core pages at ~375 px width | Usable, no horizontal scroll | — | — | Not yet tested |
| 22 | Accessibility basics | Keyboard-only pass; labels; focus visibility | All controls reachable; visible focus; labelled inputs | — | — | Not yet tested |

Notes:

- 08-24: a trailing space pasted into `GOOGLE_CLIENT_SECRET` was caught before it caused
  `invalid_client`; the server now trims env values (commit a184eda).
- Automated tests: candidates are controller tests with the EF in-memory/SQLite provider;
  the manual table above is the required minimum and stays the source of truth.
