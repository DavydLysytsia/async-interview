import { Link } from 'react-router-dom'

export default function NotFound() {
  return (
    <section className="narrow page-status">
      <h1>Page not found</h1>
      <p>That page doesn't exist (or moved).</p>
      <Link className="button-secondary" to="/">Back to the start</Link>
    </section>
  )
}
