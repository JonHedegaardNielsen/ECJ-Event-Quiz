// 1. Login (arrangør)
import { useState } from 'react'
import Logo from '../components/Logo'
import { IconUser, IconLock, IconEye, IconEyeOff } from '../components/icons'
import { useGame } from '../game/useGame'
import './Login.css'

export default function Login() {
  const { login } = useGame()
  const [name, setName] = useState('')
  const [password, setPassword] = useState('')
  const [showPw, setShowPw] = useState(false)

  const onSubmit = (e) => {
    e.preventDefault()
    // Demo: alle kombinationer accepteres. API-validering kobles på senere.
    login(name.trim() || 'Arrangør')
  }

  return (
    <main className="page-login">
      <div className="login-card">
        <Logo size="lg" />
        <form className="login-form" onSubmit={onSubmit}>
          <h1>Velkommen tilbage</h1>
          <p>Log ind for at administrere dine quizzer</p>
          <label className="field">
            <IconUser size={19} />
            <input
              type="text"
              placeholder="Brugernavn"
              value={name}
              onChange={(e) => setName(e.target.value)}
              autoComplete="username"
              autoFocus
            />
          </label>
          <label className="field">
            <IconLock size={19} />
            <input
              type={showPw ? 'text' : 'password'}
              placeholder="Adgangskode"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
            />
            <button
              type="button"
              className="field-eye"
              onClick={() => setShowPw((s) => !s)}
              aria-label={showPw ? 'Skjul adgangskode' : 'Vis adgangskode'}
            >
              {showPw ? <IconEyeOff size={19} /> : <IconEye size={19} />}
            </button>
          </label>
          <button type="submit" className="btn btn-block">
            Log ind
          </button>
          <p className="login-noaccount">
            Har du ikke en konto? <a href="mailto:hej@ecj.dk">Kontakt ECJ</a>
          </p>
        </form>
      </div>
      <a className="login-player-link" href="#/deltag">
        Skal du deltage i en quiz? Gå til deltagersiden
      </a>
    </main>
  )
}
