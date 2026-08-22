export function Privacy() {
  return (
    <section className="narrow">
      <h1>Privacy policy</h1>
      <p>
        This is an academic prototype built for a supervised internship project. Use fictional
        or personal demonstration data only.
      </p>
      <h2>What this app stores</h2>
      <ul>
        <li>Your name, email and Google account identifier (to sign you in).</li>
        <li>The profile text you enter (name, headline, biography, skills, links).</li>
        <li>The YouTube video IDs of the answers you upload, and their privacy status.</li>
        <li>OAuth tokens that let the app upload to your channel — stored server-side and
          removable at any time via "Disconnect YouTube".</li>
      </ul>
      <h2>What this app does not store</h2>
      <ul>
        <li>Your videos. They are uploaded to <strong>your own YouTube channel</strong> and are
          governed by your Google/YouTube account settings — delete them any time in
          YouTube Studio.</li>
        <li>Your Google password. Sign-in and uploads use Google OAuth; the password never
          touches this app.</li>
      </ul>
    </section>
  )
}

export function Terms() {
  return (
    <section className="narrow">
      <h1>Terms of use</h1>
      <ul>
        <li>This is a student project provided as-is, with no warranty or uptime promise.</li>
        <li>It is a prototype — do not rely on it for real recruitment decisions.</li>
        <li>You confirm that the information and videos you provide are truthful and your own.</li>
        <li>Do not upload content that violates YouTube's terms of service — uploads land on
          your own channel under your responsibility.</li>
      </ul>
    </section>
  )
}

export function Contact() {
  return (
    <section className="narrow">
      <h1>Contact</h1>
      <p>
        Built by Davyd Lysytsia for a Bow Valley College supervised internship project.
      </p>
      <p>
        Email: <a href="mailto:l.lysytsia184@mybvc.ca">l.lysytsia184@mybvc.ca</a><br />
        GitHub: <a href="https://github.com/DavydLysytsia" target="_blank" rel="noreferrer">DavydLysytsia</a>
      </p>
    </section>
  )
}
