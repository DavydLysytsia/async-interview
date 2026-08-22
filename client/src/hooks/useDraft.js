import { useEffect, useRef } from 'react'

// Keeps a draft of form values in localStorage so accidentally closing the
// page loses nothing (sketch requirement). Restores once on mount, saves on
// every change, and the caller clears it after a successful submit.
export function useDraft(key, values, setValues) {
  const restored = useRef(false)

  useEffect(() => {
    if (restored.current) return
    restored.current = true
    try {
      const raw = localStorage.getItem(key)
      if (raw) setValues((current) => ({ ...current, ...JSON.parse(raw) }))
    } catch {
      // corrupt draft — ignore it
    }
  }, [key, setValues])

  useEffect(() => {
    if (!restored.current) return
    try {
      localStorage.setItem(key, JSON.stringify(values))
    } catch {
      // storage full/blocked — drafts are best-effort
    }
  }, [key, values])

  return { clearDraft: () => localStorage.removeItem(key) }
}
