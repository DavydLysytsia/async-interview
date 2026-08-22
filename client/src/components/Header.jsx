import { Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth'

// Shared header on every page (sketch requirement).
export default function Header() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  async function handleLogout() {
    await logout()
    navigate('/')
  }

  return (
    <header className="site-header">
      <Link to={user ? '/dashboard' : '/'} className="brand">
        <span aria-hidden="true">🎬</span> Async Interview Profile
      </Link>
      <nav aria-label="Main navigation">
        {user ? (
          <>
            <NavLink to="/dashboard">Dashboard</NavLink>
            <NavLink to="/interview">Interview</NavLink>
            <NavLink to="/preview">Preview</NavLink>
            <button type="button" className="link-button" onClick={handleLogout}>
              Sign out
            </button>
          </>
        ) : (
          <NavLink to="/">Sign in</NavLink>
        )}
      </nav>
    </header>
  )
}
