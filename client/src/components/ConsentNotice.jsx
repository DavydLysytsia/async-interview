import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth'

// First-login consent (sketch: privacy policy, terms of use, statement of
// truthfulness; PDF: must state clearly that videos live on YouTube).
export default function ConsentNotice() {
  const { acceptConsent } = useAuth()
  const [checked, setChecked] = useState(false)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  async function handleAccept() {
    setSaving(true)
    setError('')
    try {
      await acceptConsent()
    } catch {
      setError('Could not save your consent. Please try again.')
      setSaving(false)
    }
  }

  return (
    <div className="consent-notice" role="region" aria-label="Consent required">
      <div>
        <h2>Before you start</h2>
        <p>
          Your interview videos are uploaded to <strong>your own YouTube channel</strong> and
          are subject to your Google/YouTube account settings. This app stores only your
          profile text and the YouTube video links. Read the{' '}
          <Link to="/privacy">privacy policy</Link> and <Link to="/terms">terms of use</Link>.
        </p>
        <label className="checkbox-row">
          <input
            type="checkbox"
            checked={checked}
            onChange={(e) => setChecked(e.target.checked)}
          />
          I agree, and I confirm the information and videos I provide are truthful.
        </label>
        {error && <p role="alert" className="form-error">{error}</p>}
        <button type="button" disabled={!checked || saving} onClick={handleAccept}>
          {saving ? 'Saving…' : 'I agree — continue'}
        </button>
      </div>
    </div>
  )
}
