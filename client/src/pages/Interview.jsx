import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { api, uploadVideo } from '../api'

function QuestionCard({ question, response, youtubeReady, onChanged }) {
  const [file, setFile] = useState(null)
  const [progress, setProgress] = useState(null) // null | 0-100
  const [error, setError] = useState('')
  const fileInputRef = useRef(null)

  const status = response?.status
  const busy = progress !== null

  async function handleUpload() {
    if (!file) {
      setError('Choose a video file first.')
      return
    }
    setError('')
    setProgress(0)
    try {
      await uploadVideo(question.id, file, setProgress)
      setFile(null)
      if (fileInputRef.current) fileInputRef.current.value = ''
      onChanged()
    } catch (err) {
      setError(err.message)
    } finally {
      setProgress(null)
    }
  }

  async function handleDelete() {
    setError('')
    try {
      await api.del(`/api/responses/${response.id}`)
      onChanged()
    } catch (err) {
      setError(err.message)
    }
  }

  return (
    <article className="card question-card">
      <h2>
        <span className="question-number">Q{question.displayOrder}</span> {question.text}
      </h2>

      {status === 'completed' && (
        <p className="status-ok">
          ✓ Answered — video <code>{response.youTubeVideoId}</code> on your YouTube channel
          ({response.privacyStatus}).{' '}
          <a
            href={`https://www.youtube.com/watch?v=${response.youTubeVideoId}`}
            target="_blank"
            rel="noreferrer"
          >
            Open on YouTube
          </a>
        </p>
      )}
      {status === 'failed' && (
        <p role="alert" className="form-error">Last upload failed: {response.errorMessage}</p>
      )}
      {status === 'uploading' && !busy && (
        <p className="hint">An upload for this question did not finish. Upload again below.</p>
      )}

      <div className="upload-row">
        <label className="file-label" htmlFor={`file-${question.id}`}>
          {status === 'completed' ? 'Replace with a different video:' : 'Your video answer:'}
        </label>
        <input
          id={`file-${question.id}`}
          ref={fileInputRef}
          type="file"
          accept="video/*,.mp4,.webm,.mov,.m4v"
          disabled={busy || !youtubeReady}
          onChange={(e) => { setFile(e.target.files[0] || null); setError('') }}
        />
        <button type="button" disabled={busy || !youtubeReady || !file} onClick={handleUpload}>
          {busy ? `Uploading… ${progress}%` : status === 'completed' ? 'Replace' : 'Upload'}
        </button>
        {status === 'completed' && !busy && (
          <button type="button" className="button-danger" onClick={handleDelete}>
            Remove answer
          </button>
        )}
      </div>
      {busy && (
        <progress max="100" value={progress} aria-label="Upload progress" />
      )}
      {error && <p role="alert" className="form-error">{error}</p>}
    </article>
  )
}

export default function Interview() {
  const [questions, setQuestions] = useState(null)
  const [responses, setResponses] = useState([])
  const [youtube, setYoutube] = useState(null)
  const [error, setError] = useState('')

  async function load() {
    try {
      const [qs, rs, yt] = await Promise.all([
        api.get('/api/questions'),
        api.get('/api/responses'),
        api.get('/api/youtube/status'),
      ])
      setQuestions(qs)
      setResponses(rs)
      setYoutube(yt)
    } catch {
      setError('Could not load the interview questions. Refresh to try again.')
    }
  }

  useEffect(() => { load() }, [])

  if (error) return <p role="alert" className="page-status">{error}</p>
  if (!questions) return <p className="page-status">Loading questions…</p>

  const youtubeReady = youtube?.connected && youtube?.hasChannel && !youtube?.needsReconnect
  const byQuestion = Object.fromEntries(responses.map((r) => [r.questionId, r]))
  const completed = responses.filter((r) => r.status === 'completed').length

  return (
    <section>
      <h1>Interview questions</h1>
      <p className="hint">
        {completed} of {questions.length} answered. Record each answer with any camera app,
        then upload the file here — it goes straight to your own YouTube channel.
      </p>

      {!youtubeReady && (
        <p role="alert" className="notice">
          {youtube?.needsReconnect
            ? 'Your YouTube authorization expired.'
            : youtube?.connected && !youtube?.hasChannel
              ? 'Your Google account has no YouTube channel yet.'
              : 'YouTube is not connected yet.'}{' '}
          <Link to="/connect">Fix this on the connection page</Link> before uploading.
        </p>
      )}

      {questions.map((q) => (
        <QuestionCard
          key={q.id}
          question={q}
          response={byQuestion[q.id]}
          youtubeReady={youtubeReady}
          onChanged={load}
        />
      ))}
    </section>
  )
}
