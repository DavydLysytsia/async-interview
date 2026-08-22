import { Link, useLocation } from 'react-router-dom'
import { useAuth } from '../auth'

export default function Landing() {
  const { user, config, devLogin } = useAuth()
  const location = useLocation()
  const from = location.state?.from

  return (
    <section className="landing">
      <div className="hero">
        <h1>Your interview answers, recorded once, ready anywhere.</h1>
        <p>
          Build a reusable asynchronous interview profile: answer common interview
          questions on video, keep them on <strong>your own YouTube channel</strong>,
          and review everything in one clean page.
        </p>

        {user ? (
          <Link className="button-primary" to="/dashboard">Go to your dashboard</Link>
        ) : (
          <div className="signin-box">
            {from && <p role="alert">Please sign in to open that page.</p>}
            {config.googleEnabled ? (
              <a className="button-primary" href="/api/auth/google">Sign in with Google</a>
            ) : config.devFakeAuth ? (
              <button type="button" className="button-primary" onClick={devLogin}>
                Sign in (local demo mode)
              </button>
            ) : (
              <p role="alert">
                Sign-in is not configured yet — the server is missing Google credentials.
              </p>
            )}
            <p className="hint">
              One Google account does everything: it signs you in and (with your separate
              permission) uploads your answers to your YouTube channel. No passwords to remember.
            </p>
          </div>
        )}
      </div>

      <div className="feature-grid">
        <article>
          <h2>1. Complete your profile</h2>
          <p>Name, headline, short biography, skills and links — the basics a recruiter reads first.</p>
        </article>
        <article>
          <h2>2. Answer on video</h2>
          <p>Upload a video answer for each standard interview question, at your own pace.</p>
        </article>
        <article>
          <h2>3. Preview and reuse</h2>
          <p>See your whole interview profile on one page, with your videos embedded.</p>
        </article>
      </div>
    </section>
  )
}
