# Async Interview Profile

A full-stack web application where a job candidate builds a **reusable asynchronous
interview profile**: basic profile information plus video answers to standard interview
questions. Videos are uploaded to the **candidate's own YouTube channel** through Google
OAuth 2.0 and the YouTube Data API — the app stores only the returned video IDs and embeds
the videos in a clean preview page.

Built by **Davyd Lysytsia** (Bow Valley College, supervised internship extra project).
Planning docs, decisions, and the activity log live in [`docs/`](docs/).

## Features

- **Sign in with Google** — no local passwords; the Google account is the login
  (an app session cookie protects all private routes server-side).
- **Candidate profile** — name, headline, biography, skills, contact links, with
  validation and an automatic local draft (accidentally closing the page loses nothing).
- **8 seeded interview questions** with a per-question video answer workflow:
  choose a file → upload with progress → replace or remove later.
- **Connect YouTube** as a separate, least-privilege consent step
  (`youtube.upload` + `youtube.readonly` only), with connection status, channel
  detection, and disconnect.
- **Uploads go to the candidate's own channel** (requested as *unlisted* — see
  Known limitations), only the video ID/privacy status are stored.
- **Candidate preview page** — profile + embedded answers in question order.
- Friendly handling of the likely failures: authorization denied, no YouTube channel,
  expired authorization, upload failure, unsupported file, missing fields.
- Responsive layout, semantic headings/labels, keyboard-accessible controls, visible
  focus states, first input auto-focused on action pages.

## Tech stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core (.NET 10) Web API, cookie auth + Google OAuth |
| Database | SQLite via EF Core (schema created + questions seeded on startup) |
| YouTube | Google.Apis.YouTube.v3 (resumable upload), tokens stored server-side in the DB |
| Frontend | React 19 (Vite) SPA, react-router — built into the API's `wwwroot` |
| Hosting | One Azure App Service serves both API and SPA (planned) |

## Getting started

Prerequisites: .NET 10 SDK, Node.js 20+.

```bash
# 1. Build the frontend into the API's wwwroot
cd client
npm install
npm run build

# 2. Configure the server
cd ../server/AsyncInterview.Api
# copy .env.example to .env — with no Google credentials yet, set DEV_FAKE_AUTH=true
# to use a local fake sign-in (uploads stay disabled until Google is configured)

# 3. Run
dotnet run
# open http://localhost:5240
```

The SQLite database (`app.db`) is created and seeded automatically on first run.

**Real Google sign-in + YouTube uploads** need OAuth credentials — the one-time Google
Cloud setup (~10 min) is described in [`docs/GOOGLE-SETUP.md`](docs/GOOGLE-SETUP.md).

### Environment variables (`server/AsyncInterview.Api/.env`)

| Variable | Purpose |
|---|---|
| `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET` | OAuth web credentials from Google Cloud Console |
| `APP_BASE_URL` | Public base URL, used for the YouTube OAuth callback (default `http://localhost:5240`) |
| `DEV_FAKE_AUTH` | `true` enables a local fake sign-in for development/demo without Google credentials |
| `DB_PATH` | SQLite file path (default `app.db`; use `/home/data/app.db` on Azure) |

A safe template is committed as `server/AsyncInterview.Api/.env.example`. No real secrets
are ever committed; OAuth tokens live in the (gitignored) database.

### Frontend development loop

- `npm run watch` in `client/` rebuilds into `wwwroot` on change (everything on :5240 —
  required when testing real Google flows), or
- `npm run dev` for hot reload on :5173 with `/api` proxied — use with `DEV_FAKE_AUTH`.

## Known limitations (documented on purpose)

1. **Privacy status:** the app requests `unlisted`, but YouTube forces uploads from
   unverified API projects (any project still in testing mode) to **private** and the
   preview embed may show "unavailable" to anyone but the owner. The app records the
   status YouTube actually applied, labels it in the UI, and links to the watch page.
2. **Quota:** the default YouTube Data API quota (10,000 units/day) allows roughly
   **6 uploads per day** (`videos.insert` ≈ 1,600 units).
3. **Testing-mode tokens:** Google expires refresh tokens for testing-mode consent
   screens after ~7 days; the app detects this and asks the user to reconnect.
4. Only listed **test users** can sign in while the OAuth consent screen is in testing
   mode (this project does not pursue Google verification).
5. Deleting an answer removes the app's record; the video itself stays in the owner's
   YouTube Studio (deleting there is optional, per requirements).
