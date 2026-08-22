// Small fetch wrapper. All API errors become ApiError with a friendly
// message the pages can show directly.

export class ApiError extends Error {
  constructor(status, data) {
    super(data?.error || 'Something went wrong. Please try again.')
    this.status = status
    this.data = data || {}
  }
}

async function request(path, { method = 'GET', body } = {}) {
  const init = { method, credentials: 'same-origin' }
  if (body !== undefined) {
    init.headers = { 'Content-Type': 'application/json' }
    init.body = JSON.stringify(body)
  }
  const res = await fetch(path, init)
  let data = null
  try {
    data = await res.json()
  } catch {
    // empty or non-JSON body
  }
  if (!res.ok) throw new ApiError(res.status, data)
  return data
}

export const api = {
  get: (path) => request(path),
  post: (path, body) => request(path, { method: 'POST', body }),
  put: (path, body) => request(path, { method: 'PUT', body }),
  del: (path) => request(path, { method: 'DELETE' }),
}

// Upload with progress needs XMLHttpRequest — fetch can't report progress.
export function uploadVideo(questionId, file, onProgress) {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest()
    xhr.open('POST', `/api/responses/${questionId}/video`)
    xhr.withCredentials = true
    xhr.responseType = 'json'
    xhr.upload.onprogress = (e) => {
      if (e.lengthComputable && onProgress) onProgress(Math.round((e.loaded / e.total) * 100))
    }
    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) resolve(xhr.response)
      else reject(new ApiError(xhr.status, xhr.response))
    }
    xhr.onerror = () => reject(new ApiError(0, { error: 'Network error during upload.' }))
    const form = new FormData()
    form.append('file', file)
    xhr.send(form)
  })
}
