# Demo video script (target 6–8 minutes, one take, no editing needed)

**Setup before recording:** app open at https://async-interview-davyd.azurewebsites.net
(or `http://localhost:5240` — equally valid), signed out; have a 60–90 s video file ready
to upload; second tab open at the GitHub repo; YouTube Studio in a third tab.
Record with Win+G (Game Bar) or OBS, mic on, 1080p.

| ~Time | On screen | Say roughly |
|------|-----------|-------------|
| 0:00 | Landing page | "This is Async Interview Profile, my full-stack internship project: a candidate records answers to standard interview questions once, keeps them on their own YouTube channel, and reuses them. ASP.NET Core API, EF Core with SQLite, React front end, deployed on Azure." |
| 0:40 | Click **Sign in with Google**, account chooser, unverified-app warning | "There are no local passwords — sign-in is Google OAuth. The app is in Google's testing mode, so this warning is expected; only registered test users can sign in." |
| 1:20 | First-login consent (if fresh account) or point at footer | "The app states clearly that videos live on the user's own YouTube account — that's also in the privacy policy page." |
| 1:40 | Dashboard | "The dashboard shows profile completion, interview progress, and the YouTube connection state." |
| 2:00 | Profile page: edit a field, save; then reload mid-edit once | "Standard validated CRUD. One UX detail: the form keeps a local draft — if I close the page accidentally, nothing is lost." |
| 2:50 | Connect page → **Connect YouTube** → Google consent with the two scopes → back, connected | "Uploading needs YouTube permission, requested separately with the minimum scopes: upload plus read-only. The app detects my channel; if the account had none, it explains how to fix that." |
| 3:50 | Interview page → pick file for a question → **Upload**, progress bar → ✓ Answered | "Each question takes one video answer. The file streams through my API to the YouTube Data API as a resumable upload; the app stores only the returned video id and the privacy status YouTube actually applied." |
| 5:00 | Click **Open on YouTube** or show YouTube Studio tab | "And here's the video on my own channel — the app never stores video files itself." |
| 5:30 | Preview page | "The preview page is the reusable result: profile plus all answers embedded, in question order." |
| 6:00 | Error handling: upload a .txt to a question | "Failures are handled with real messages — unsupported files, denied authorization, expired tokens, missing channel, quota." |
| 6:30 | Repo tab: README, docs folder, commit history; test table | "Everything is documented in the repo: plan, decisions, Google setup guide, test evidence, activity log, and the commit history shows the progress." |
| 7:15 | Back on preview | "Known limitations are documented: testing-mode tokens expire weekly, quota allows about six uploads a day, and unverified API projects may have uploads forced private — in my testing, unlisted was honored. Thanks for watching." |

**If a step misbehaves on camera:** say what it should do and move on — the test table
in `docs/TEST-PLAN.md` carries the evidence; don't restart the take.
