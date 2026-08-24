# Note to instructor (copy, adjust greeting, send from Davyd's email)

> Subject: Async interview project — deployed, scope decisions
>
> Hi [instructor name],
>
> The asynchronous interview project is up and running:
>
> - **Live app:** https://async-interview-davyd.azurewebsites.net (Azure App Service)
> - **Repo:** https://github.com/DavydLysytsia/async-interview (public — plan, activity
>   log, test evidence and setup docs are in `docs/`)
>
> The core workflow already works end to end: Google sign-in, a separate "Connect
> YouTube" authorization, uploading a video answer to my own YouTube channel through the
> YouTube Data API, and a preview page with the answers embedded. As you suggested, here
> are the justifications for the adjustments I made instead of asking permission:
>
> 1. **Sign-in is "Sign in with Google" only — no local email/password.** Every user
>    needs a Google account anyway to upload to YouTube, and the requirements allow an
>    "external login identifier" instead of a password hash. This removes the
>    register/password/recover pages from the original sketch and the risk of storing
>    passwords. Sign-out and server-side protection of private routes remain.
> 2. **YouTube permissions are a separate consent step** (upload + read-only scopes
>    only), following the "request only the permissions required" guideline.
> 3. **Video answers are file uploads** (allowed by requirement #4); in-browser
>    recording is a stretch goal. From the sketch, collections / share links / the AI
>    resume are deferred as stretch scope; the sketch's UX details (shared
>    header/footer, first-input focus, tab order, local form drafts) are implemented.
> 4. **Stack:** ASP.NET Core + EF Core (SQLite) + React, one Azure App Service —
>    consistent with my program stack and my class-project deployment.
> 5. **Notes on the live demo:** the OAuth app is in Google "testing" mode (no
>    verification, as the requirements allow), so only listed test users can sign in —
>    send me the Gmail address you'd like added if you want to try it live; otherwise
>    the demo video will cover the workflow. Uploads are quota-limited to ~6/day, and
>    in testing the videos land as *unlisted* on the candidate's own channel.
>
> Hours are logged in `docs/ACTIVITY-LOG.md` in the repo, with commits as evidence.
>
> Thanks!
> Davyd
