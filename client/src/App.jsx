import { Route, Routes } from 'react-router-dom'
import Header from './components/Header'
import Footer from './components/Footer'
import ConsentNotice from './components/ConsentNotice'
import { RequireAuth, useAuth } from './auth'
import Landing from './pages/Landing'
import Dashboard from './pages/Dashboard'
import ProfileEdit from './pages/ProfileEdit'
import Interview from './pages/Interview'
import ConnectYouTube from './pages/ConnectYouTube'
import Preview from './pages/Preview'
import { Contact, Privacy, Terms } from './pages/StaticPages'
import NotFound from './pages/NotFound'

export default function App() {
  const { user } = useAuth()

  return (
    <div className="app-shell">
      <Header />
      {user && !user.consentAccepted && <ConsentNotice />}
      <main id="main">
        <Routes>
          <Route path="/" element={<Landing />} />
          <Route path="/dashboard" element={<RequireAuth><Dashboard /></RequireAuth>} />
          <Route path="/profile" element={<RequireAuth><ProfileEdit /></RequireAuth>} />
          <Route path="/interview" element={<RequireAuth><Interview /></RequireAuth>} />
          <Route path="/connect" element={<RequireAuth><ConnectYouTube /></RequireAuth>} />
          <Route path="/preview" element={<RequireAuth><Preview /></RequireAuth>} />
          <Route path="/privacy" element={<Privacy />} />
          <Route path="/terms" element={<Terms />} />
          <Route path="/contact" element={<Contact />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </main>
      <Footer />
    </div>
  )
}
