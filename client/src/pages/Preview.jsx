import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api'

// The "clean page" from the requirements: profile + embedded video answers,
// the way a reviewer would see them.
export default function Preview() {
  const [data, setData] = useState(null)
  const [error, setError] = useState('')

  useEffect(() => {
    let cancelled = false
    api.get('/api/preview')
      .then((d) => { if (!cancelled) setData(d) })
      .catch(() => { if (!cancelled) setError('Could not load the preview. Refresh to try again.') })
    return () => { cancelled = true }
  }, [])

  if (error) return <p role="alert" className="page-status">{error}</p>
  if (!data) return <p className="page-status">Building your preview…</p>

  const { profile, answers } = data
  const skills = profile.skills.split('\n').map((s) => s.trim()).filter(Boolean)
  const links = profile.contactLinks.split('\n').map((s) => s.trim()).filter(Boolean)

  return (
    <section className="preview">
      <header className="preview-header">
        <h1>{profile.fullName || 'Unnamed candidate'}</h1>
        {profile.headline && <p className="headline">{profile.headline}</p>}
        {profile.biography && <p className="bio">{profile.biography}</p>}
        {skills.length > 0 && (
          <ul className="skill-list" aria-label="Skills">
            {skills.map((s) => <li key={s}>{s}</li>)}
          </ul>
        )}
        {links.length > 0 && (
          <ul className="plain-list" aria-label="Links">
            {links.map((l) => (
              <li key={l}><a href={l} target="_blank" rel="noreferrer">{l}</a></li>
            ))}
          </ul>
        )}
      </header>

      <h2>Interview answers</h2>
      {answers.length === 0 ? (
        <p>
          No completed video answers yet. <Link to="/interview">Answer your first question</Link>{' '}
          and it will appear here.
        </p>
      ) : (
        answers.map((a) => (
          <article key={a.youTubeVideoId} className="answer">
            <h3>{a.text}</h3>
            <div className="video-frame">
              <iframe
                src={`https://www.youtube.com/embed/${a.youTubeVideoId}`}
                title={`Video answer: ${a.text}`}
                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
                allowFullScreen
              />
            </div>
            {a.privacyStatus === 'private' && (
              <p className="hint">
                This video is <strong>private</strong> on YouTube (test-app limitation), so the
                embed may say "unavailable" for other viewers.{' '}
                <a
                  href={`https://www.youtube.com/watch?v=${a.youTubeVideoId}`}
                  target="_blank"
                  rel="noreferrer"
                >
                  Open it on YouTube
                </a>{' '}
                while signed in to your account.
              </p>
            )}
          </article>
        ))
      )}
    </section>
  )
}
