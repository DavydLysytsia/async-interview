import { useEffect, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { api } from '../api'

const CALLBACK_MESSAGES = {
  denied: 'You declined the authorization on Google. Connect again when you are ready.',
  state: 'The authorization could not be verified (state mismatch). Please try again.',
  exchange: 'Google did not accept the authorization. Please try again.',
}

export default function ConnectYouTube() {
  const [status, setStatus] = useState(null)
  const [error, setError] = useState('')
  const [params] = useSearchParams()
  const callbackError = params.get('error')
  const justConnected = params.get('connected') === '1'

  async function load() {
    try {
      setStatus(await api.get('/api/youtube/status'))
    } catch {
      setError('Could not check the YouTube connection. Refresh to try again.')
    }
  }

  useEffect(() => { load() }, [])

  async function handleDisconnect() {
    await api.post('/api/youtube/disconnect')
    load()
  }

  if (error) return <p role="alert" className="page-status">{error}</p>
  if (!status) return <p className="page-status">Checking your YouTube connection…</p>

  return (
    <section className="narrow">
      <h1>YouTube connection</h1>

      {justConnected && <p role="status" className="status-ok">✓ YouTube connected.</p>}
      {callbackError && (
        <p role="alert" className="form-error">
          {CALLBACK_MESSAGES[callbackError] || 'The authorization failed. Please try again.'}
        </p>
      )}

      {!status.configured ? (
        <p role="alert" className="notice">
          The server has no Google API credentials yet, so connecting is disabled.
          (See docs/GOOGLE-SETUP.md in the repository.)
        </p>
      ) : status.connected ? (
        <>
          <p className="status-ok">
            ✓ Connected{status.channelTitle ? <> to channel <strong>{status.channelTitle}</strong></> : null}.
          </p>
          {!status.hasChannel && (
            <p role="alert" className="notice">
              This Google account has no YouTube channel, so uploads will fail. Create one at{' '}
              <a href="https://www.youtube.com/create_channel" target="_blank" rel="noreferrer">
                youtube.com
              </a>{' '}
              first, then refresh this page.
            </p>
          )}
          <button type="button" className="button-danger" onClick={handleDisconnect}>
            Disconnect YouTube
          </button>
        </>
      ) : (
        <>
          {status.needsReconnect && (
            <p role="alert" className="notice">
              Your previous authorization expired (test-mode Google apps expire tokens after about
              7 days). Reconnect below.
            </p>
          )}
          <p>
            To upload interview answers, allow this app to upload videos to{' '}
            <strong>your own YouTube channel</strong>. You will be sent to Google to approve it —
            this app never sees your Google password.
          </p>
          <a className="button-primary" href="/api/youtube/connect">Connect YouTube</a>
        </>
      )}

      <h2>What you are agreeing to</h2>
      <ul className="plain-list">
        <li>Videos are uploaded as <strong>unlisted</strong> where possible. YouTube forces
          uploads from unverified test apps to <strong>private</strong> — either way they are
          not publicly listed.</li>
        <li>The app only asks for permission to upload videos and read your channel info —
          nothing else.</li>
        <li>You can disconnect here at any time, and manage or delete the videos in your own
          YouTube Studio.</li>
      </ul>
    </section>
  )
}
