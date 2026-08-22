import { Link } from 'react-router-dom'

// Shared footer on every page (sketch requirement).
export default function Footer() {
  return (
    <footer className="site-footer">
      <p>
        © {new Date().getFullYear()} Async Interview Profile — student project.
        Videos are uploaded to <strong>your own YouTube channel</strong> and follow
        your Google/YouTube account settings.
      </p>
      <nav aria-label="Footer">
        <Link to="/privacy">Privacy policy</Link>
        <Link to="/terms">Terms of use</Link>
        <Link to="/contact">Contact</Link>
      </nav>
    </footer>
  )
}
