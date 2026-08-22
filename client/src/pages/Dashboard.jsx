import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../auth'

export default function Dashboard() {
  const { user } = useAuth()
  const [data, setData] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    let cancelled = false
    async function load() {
      try {
        const [profile, questions, responses, youtube] = await Promise.all([
          api.get('/api/profile'),
          api.get('/api/questions'),
          api.get('/api/responses'),
          api.get('/api/youtube/status'),
        ])
        if (!cancelled) setData({ profile, questions, responses, youtube })
      } catch {
        if (!cancelled) setError('Could not load your dashboard. Refresh to try again.')
      }
    }
    load()
    return () => { cancelled = true }
  }, [])

  if (error) return <p role="alert" className="page-status">{error}</p>
  if (!data) return <p className="page-status">Loading your dashboard…</p>

  const { profile, questions, responses, youtube } = data
  const profileFields = [profile.fullName, profile.headline, profile.biography, profile.skills]
  const filledFields = profileFields.filter((f) => f && f.trim() !== '').length
  const profilePercent = Math.round((filledFields / profileFields.length) * 100)
  const completed = responses.filter((r) => r.status === 'completed').length

  return (
    <section>
      <h1>Welcome, {user.displayName}</h1>
      <p className="hint">Signed in as {user.email}</p>

      <div className="card-grid">
        <article className="card">
          <h2>Profile</h2>
          <p className="big-number">{profilePercent}%</p>
          <p>of the basic fields are filled in.</p>
          <Link className="button-secondary" to="/profile">
            {profilePercent === 100 ? 'Edit profile' : 'Complete your profile'}
          </Link>
        </article>

        <article className="card">
          <h2>Interview</h2>
          <p className="big-number">{completed}/{questions.length}</p>
          <p>questions answered with a video.</p>
          <Link className="button-secondary" to="/interview">
            {completed === 0 ? 'Start answering' : 'Continue answering'}
          </Link>
        </article>

        <article className="card">
          <h2>YouTube</h2>
          {youtube.connected && youtube.hasChannel ? (
            <p>
              Connected to <strong>{youtube.channelTitle || 'your channel'}</strong>. Uploads go
              to your own account.
            </p>
          ) : youtube.needsReconnect ? (
            <p role="alert">Your YouTube authorization expired — please reconnect.</p>
          ) : youtube.connected && !youtube.hasChannel ? (
            <p role="alert">Connected, but this Google account has no YouTube channel yet.</p>
          ) : (
            <p>Not connected yet. Connect before uploading your first answer.</p>
          )}
          <Link className="button-secondary" to="/connect">Manage connection</Link>
        </article>

        <article className="card">
          <h2>Preview</h2>
          <p>See your profile the way a recruiter would: one page, videos embedded.</p>
          <Link className="button-secondary" to="/preview">Open preview</Link>
        </article>
      </div>
    </section>
  )
}
