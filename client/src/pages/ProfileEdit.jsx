import { useEffect, useState } from 'react'
import { api } from '../api'
import { useAuth } from '../auth'
import { useDraft } from '../hooks/useDraft'

const EMPTY = { fullName: '', headline: '', biography: '', skills: '', contactLinks: '' }

export default function ProfileEdit() {
  const { user } = useAuth()
  const [values, setValues] = useState(EMPTY)
  const [errors, setErrors] = useState({})
  const [status, setStatus] = useState('loading') // loading | ready | saving | saved
  const [loadError, setLoadError] = useState('')
  const { clearDraft } = useDraft(`profile-draft-${user.id}`, values, setValues)

  useEffect(() => {
    let cancelled = false
    api.get('/api/profile')
      .then((profile) => {
        if (cancelled) return
        // Server values win over an older local draft only for empty fields,
        // so an unsaved draft isn't silently thrown away.
        setValues((draft) => {
          const merged = { ...profile }
          for (const key of Object.keys(EMPTY)) {
            if (draft[key] && draft[key] !== profile[key]) merged[key] = draft[key]
          }
          return merged
        })
        setStatus('ready')
      })
      .catch(() => {
        if (!cancelled) {
          setLoadError('Could not load your profile. Refresh to try again.')
          setStatus('ready')
        }
      })
    return () => { cancelled = true }
  }, [])

  function setField(name, value) {
    setValues((v) => ({ ...v, [name]: value }))
    setErrors((e) => ({ ...e, [name]: undefined }))
    setStatus('ready')
  }

  async function handleSubmit(e) {
    e.preventDefault()
    const clientErrors = {}
    if (!values.fullName.trim()) clientErrors.fullName = 'Full name is required.'
    if (Object.keys(clientErrors).length > 0) {
      setErrors(clientErrors)
      return
    }

    setStatus('saving')
    try {
      await api.put('/api/profile', values)
      clearDraft()
      setErrors({})
      setStatus('saved')
    } catch (err) {
      setErrors(err.data?.errors || {})
      if (!err.data?.errors) setErrors({ form: err.message })
      setStatus('ready')
    }
  }

  if (status === 'loading') return <p className="page-status">Loading your profile…</p>

  return (
    <section className="narrow">
      <h1>Your profile</h1>
      <p className="hint">
        This is the text a reviewer sees next to your videos. A draft is kept in your
        browser automatically, so closing the page by accident loses nothing.
      </p>
      {loadError && <p role="alert" className="form-error">{loadError}</p>}

      <form onSubmit={handleSubmit} noValidate>
        <div className="field">
          <label htmlFor="fullName">Full name *</label>
          <input
            id="fullName"
            autoFocus
            value={values.fullName}
            onChange={(e) => setField('fullName', e.target.value)}
            aria-invalid={!!errors.fullName}
          />
          {errors.fullName && <p role="alert" className="form-error">{errors.fullName}</p>}
        </div>

        <div className="field">
          <label htmlFor="headline">Professional headline</label>
          <input
            id="headline"
            placeholder="e.g. Junior full-stack developer (ASP.NET / React)"
            value={values.headline}
            onChange={(e) => setField('headline', e.target.value)}
            aria-invalid={!!errors.headline}
          />
          {errors.headline && <p role="alert" className="form-error">{errors.headline}</p>}
        </div>

        <div className="field">
          <label htmlFor="biography">Short biography</label>
          <textarea
            id="biography"
            rows="5"
            value={values.biography}
            onChange={(e) => setField('biography', e.target.value)}
            aria-invalid={!!errors.biography}
          />
          {errors.biography && <p role="alert" className="form-error">{errors.biography}</p>}
        </div>

        <div className="field">
          <label htmlFor="skills">Skills (one per line)</label>
          <textarea
            id="skills"
            rows="4"
            placeholder={'C# / ASP.NET Core\nReact\nSQL'}
            value={values.skills}
            onChange={(e) => setField('skills', e.target.value)}
            aria-invalid={!!errors.skills}
          />
          {errors.skills && <p role="alert" className="form-error">{errors.skills}</p>}
        </div>

        <div className="field">
          <label htmlFor="contactLinks">Contact links (one per line)</label>
          <textarea
            id="contactLinks"
            rows="3"
            placeholder={'https://www.linkedin.com/in/you\nhttps://github.com/you'}
            value={values.contactLinks}
            onChange={(e) => setField('contactLinks', e.target.value)}
            aria-invalid={!!errors.contactLinks}
          />
          {errors.contactLinks && <p role="alert" className="form-error">{errors.contactLinks}</p>}
        </div>

        {errors.form && <p role="alert" className="form-error">{errors.form}</p>}
        <div className="form-actions">
          <button type="submit" disabled={status === 'saving'}>
            {status === 'saving' ? 'Saving…' : 'Save profile'}
          </button>
          {status === 'saved' && <span role="status" className="save-ok">Saved ✓</span>}
        </div>
      </form>
    </section>
  )
}
