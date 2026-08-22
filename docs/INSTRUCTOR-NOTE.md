# Draft note to instructor (send before/with first progress update)

> Subject: Async interview project — scope decisions
>
> Hi [instructor name],
>
> I've started the asynchronous interview project (repo:
> https://github.com/DavydLysytsia/async-interview). As you suggested, I'm sending
> justifications for the adjustments I made instead of asking permission:
>
> 1. **Sign-in is "Sign in with Google" only — no local email/password.** Every user
>    already needs a Google account to upload to their own YouTube channel, and the
>    requirements allow an "external login identifier" instead of a password hash. This
>    removes the register/password/recover pages from the original sketch and the security
>    risk of storing passwords. Sign-out and server-side protection of private routes stay.
> 2. **YouTube permissions are requested separately from sign-in** (a "Connect YouTube"
>    step asking only for upload + read-only scopes), to follow the "request only the
>    permissions required" guideline.
> 3. **Video responses start as a file-upload workflow** (allowed by requirement #4);
>    in-browser recording is a stretch goal if hours allow.
> 4. **From the sketch, video collections / share links / the AI resume ("Phase 2") are
>    deferred as stretch scope** — they aren't needed by the minimum requirements. I kept
>    the sketch's UX notes: shared header/footer, first-input focus + tab order on action
>    pages, and forms that keep a local draft if the page closes.
> 5. **Stack:** ASP.NET Core + EF Core (SQLite) + React, deployed as a single Azure App
>    Service — consistent with my program stack and my recent class-project deployment.
> 6. **Known API limitation I'll document:** uploads from unverified API projects are
>    forced to *private* by YouTube, so the "unlisted" default may be overridden and
>    embedded playback may be limited; the app records/report what YouTube actually
>    applied, and the preview page falls back to direct YouTube links. Also the default
>    quota allows roughly 6 uploads/day, and testing-mode refresh tokens expire weekly
>    (the app treats that as "reconnect needed").
>
> I'm logging dates/tasks/hours in docs/ACTIVITY-LOG.md in the repo, with the commit
> history as evidence. I'll reach out if I hit a wall with the YouTube API.
>
> Thanks!
> Davyd
