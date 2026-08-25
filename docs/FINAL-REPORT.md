# Final Report — Asynchronous Video Interview Web Application

> **DRAFT for Davyd to edit.** Facts and dates below are accurate to the repo history;
> rewrite any wording into your own voice before submitting, and fill the hours from
> ACTIVITY-LOG.md once you've completed it.

**Student:** Davyd Lysytsia · **Project:** Supervised internship extra project
**Repo:** github.com/DavydLysytsia/async-interview · **Live:** async-interview-davyd.azurewebsites.net

## 1. What was built

A full-stack prototype where a job candidate builds a reusable asynchronous interview
profile: they sign in with Google, complete a text profile, and upload a video answer for
each of eight seeded interview questions. Videos are uploaded through the YouTube Data API
to the candidate's **own** YouTube channel; the application stores only the returned video
id and privacy status, and embeds the answers on a preview page. All ten minimum
functional requirements from the project PDF are implemented, including response
replace/remove, per-field validation, upload progress, and friendly handling of the
required failure cases (denied consent, missing channel, expired authorization,
unsupported file, missing fields).

**Stack:** ASP.NET Core (.NET 10) Web API · EF Core + SQLite · React 19 (Vite) SPA served
from the API's `wwwroot` · Google OAuth 2.0 (`Microsoft.AspNetCore.Authentication.Google`)
· YouTube Data API v3 (`Google.Apis.YouTube.v3`) · Azure App Service (free tier) · GitHub.

## 2. Key technical decisions

1. **Google sign-in only, no local passwords.** Every user needs a Google account anyway
   for YouTube; the requirements allow an external login identifier in place of a
   password hash. This removed an entire class of security work (hashing, reset flows)
   and the associated attack surface, at the cost of the app being unusable without a
   Google account — acceptable for the scenario.
2. **Two-step consent (least privilege).** Sign-in requests only `openid email profile`;
   YouTube's `upload` + `readonly` scopes are a separate, explicit "Connect YouTube" step.
   The readonly scope exists solely to detect the "account has no channel" failure case.
3. **One deployable unit.** The React build is copied into `wwwroot`, so a single free
   App Service hosts everything — one origin, one HTTPS OAuth redirect, no CORS.
4. **SQLite + EF Core** (explicitly permitted) — zero external services; the DB file
   lives on the App Service's persistent storage.
5. **Tokens server-side.** OAuth tokens are stored in the database through a custom
   EF-backed `IDataStore` for the Google auth library — never in the browser, never in
   the repo. Secrets are environment variables with a committed `.env.example`.

## 3. Challenges and how they were resolved

- **YouTube API constraints discovered up front:** unverified API projects can have
  uploads forced private; default quota is ~6 uploads/day (`videos.insert` = 1,600 of
  10,000 units); testing-mode refresh tokens expire after ~7 days. Designed for all
  three (status recorded per upload and labelled in the UI; short test clips; a
  "reconnect" state). In live testing the *unlisted* status was honored, so the
  forced-private mitigation stayed a documented fallback.
- **A trailing space in a pasted client secret** produced a puzzling OAuth failure risk;
  fixed by trimming configuration values and documenting the gotcha.
- **HTTPS behind Azure's proxy:** OAuth redirect URIs were initially built as `http://`
  behind the App Service front end; solved with forwarded-headers middleware
  (`X-Forwarded-Proto`) so the Google handler emits correct `https://` URIs.
- **Azure for Students free-tier quota:** the subscription only actually runs two
  Free-tier apps; additional free apps are created but immediately force-stopped with
  `QuotaExceeded` regardless of plan or region. Diagnosed via the sites/usages API
  (rising `WPStopRequests` with zero CPU use), resolved by stopping an old project's app
  and reusing its (Windows) Free plan — which also meant switching the runtime id and
  the SQLite path to Windows conventions. The deployment is scripted in `deploy.ps1`.
- **False "bug" during verification:** a wedged browser session made the deployed
  sign-in look broken while the server's OAuth challenge was provably correct (verified
  by inspecting the 302 and `redirect_uri` directly). Lesson: verify at the protocol
  level before debugging application code.

## 4. Testing

Manual test evidence with dates and observed results is in `docs/TEST-PLAN.md` (22
cases: environment/config, auth and route protection, profile validation and drafts,
YouTube connect/status, upload happy path and failure cases, preview embedding,
deployment checks). Highlights: full end-to-end flow verified locally on 2026-08-24
(sign-in → consent → connect → upload → embedded playback of video `3p81Yj_mhu8`), and
the deployed app verified for health, configuration, SPA serving, and OAuth challenge
correctness the same day.

## 5. Lessons learned

- Read the external API's operational limits *before* designing around it — quota,
  verification status, and token lifetime shaped the whole test strategy.
- Free hosting tiers have real, undocumented-feeling constraints; scripting the
  deployment made each relocation cheap.
- Building a dev fallback (fake sign-in mode) kept the whole app testable before any
  credentials existed, and it remains useful for UI work.

## 6. Future improvements

In-browser recording (MediaRecorder), named collections of answers with a shareable
public link, automated integration tests around the controllers, an AI-generated resume
PDF from the profile + answers, and Google verification if the app ever needed real
users.

## 7. Hours

See `docs/ACTIVITY-LOG.md` (dates, tasks, hours, evidence per work period), with the
repository's commit history as corroborating evidence.
