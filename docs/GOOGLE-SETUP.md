# Google Cloud setup (one-time, ~10 minutes, must be done by Davyd)

The app needs OAuth 2.0 web credentials before Google sign-in or YouTube upload can work.
Until then, the app runs locally with `DEV_FAKE_AUTH=true` (fake sign-in, uploads disabled).

## Steps

1. Go to https://console.cloud.google.com/ and sign in with the Google account whose
   YouTube channel will receive the test uploads.
2. Create a new project, e.g. `async-interview` (top bar → project picker → New Project).
3. **Enable the API:** APIs & Services → Library → search **"YouTube Data API v3"** → Enable.
4. **Consent screen:** APIs & Services → OAuth consent screen →
   - User type: **External**, Publishing status stays **Testing** (do NOT publish/verify).
   - App name `Async Interview Profile`, your email for both contact fields.
   - Scopes: you can skip adding scopes here (the app requests them at runtime).
   - **Test users:** add your own Gmail address (and the instructor's if he should try it live).
5. **Credentials:** APIs & Services → Credentials → Create credentials → **OAuth client ID** →
   Application type **Web application**, name `async-interview-web`.
   - Authorized redirect URIs (add all):
     - `http://localhost:5240/signin-google`
     - `http://localhost:5240/api/youtube/callback`
     - (after deploy) `https://<yourapp>.azurewebsites.net/signin-google`
     - (after deploy) `https://<yourapp>.azurewebsites.net/api/youtube/callback`
6. Copy `server/AsyncInterview.Api/.env.example` to `.env` (same folder) and fill in the
   **Client ID** and **Client secret**:

   ```
   GOOGLE_CLIENT_ID=xxxxxxxx.apps.googleusercontent.com
   GOOGLE_CLIENT_SECRET=GOCSPX-xxxxxxxx
   DEV_FAKE_AUTH=false
   ```

7. Restart the API. The landing page button becomes real Google sign-in, and
   Dashboard → "Connect YouTube" starts the upload authorization.

## Notes / gotchas (already accounted for in the code & plan)

- Testing mode: only listed **test users** can sign in; refresh tokens expire after **7 days**
  (the app then shows "reconnect YouTube").
- Default quota = ~**6 video uploads per day** (videos.insert costs 1,600 of 10,000 units).
  Use short clips for testing.
- Uploads from unverified API projects are forced **private** regardless of the requested
  privacy status. The app records what YouTube actually applied. Don't fight this — it's
  documented as a known limitation.
- The account must have a **YouTube channel** (youtube.com → create channel) or uploads fail
  with "channel not found" — the app detects and explains this.
