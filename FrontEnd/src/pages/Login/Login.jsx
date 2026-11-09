// 1. Login (arrangør) – valider mod EventQuiz.Api (/api/auth/login).
import { useState } from 'react'
import Logo from '../../components/Logo'
import { IconUser, IconLock, IconEye, IconEyeOff } from '../../components/icons'
import { useGame } from '../../game/useGame'
import { registerUser } from '../../lib/api'
import { Link } from 'react-router-dom'
import './Login.css'

export default function Login() {
  const { login } = useGame()
  const [mode, setMode] = useState('login') // 'login' | 'register'
  const [name, setName] = useState('')
  const [password, setPassword] = useState('')
  const [showPw, setShowPw] = useState(false)
  const [busy, setBusy] = useState(false)
  const [err, setErr] = useState('')

  const isRegister = mode === 'register'

  const onSubmit = async (e) => {
    e.preventDefault()
    const username = name.trim()
    if (!username || !password) {
      setErr('Udfyld både brugernavn og adgangskode.')
      return
    }
    setErr('')

    // Client-side validation matching the API rules
    if (isRegister && (username.length < 3 || password.length < 6)) {
      setErr('Brugernavn min. 3 tegn og adgangskode min. 6 tegn.')
      return
    }

    setBusy(true)
    try {
      if (isRegister) await registerUser(username, password)
      await login(username, password)
    } catch (ex) {
      setErr(ex.message || 'Log ind mislykkedes')
    } finally {
      setBusy(false)
    }
  }

  return (
    <main className="page-login">
      <div className="login-card">
        <Logo size="lg" />
        <form className="login-form" onSubmit={onSubmit}>
          <h1>{isRegister ? 'Opret arrangørkonto' : 'Velkommen tilbage'}</h1>
          <p>{isRegister ? 'Opret en konto for at administrere dine quizzer' : 'Log ind for at administrere dine quizzer'}</p>
          <label className="field">
            <IconUser size={19} />
            <input
              type="text"
              placeholder="Brugernavn"
              value={name}
              onChange={(e) => setName(e.target.value)}
              maxLength={255}
              autoComplete="username"
              autoFocus
            />
          </label>
          <label className="field">
            <IconLock size={19} />
            <input
              type={showPw ? 'text' : 'password'}
              placeholder={isRegister ? 'Adgangskode (min. 6 tegn)' : 'Adgangskode'}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              maxLength={255}
              autoComplete={isRegister ? 'new-password' : 'current-password'}
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
          <button type="submit" className="btn btn-block" disabled={busy}>
            {busy ? 'Vent…' : isRegister ? 'Opret konto' : 'Log ind'}
          </button>
          {err && !busy && <p className="login-error" role="alert">{err}</p>}
          <button type="button" className="link-ghost login-switch" onClick={() => {
            setMode(isRegister ? 'login' : 'register')
            setErr('')
          }}>
            {isRegister ? 'Har du allerede en konto? Log ind' : 'Har du ikke en konto? Opret en'}
          </button>
        </form>
      </div>
      <Link className="login-player-link" to="/deltag">
        Skal du deltage i en quiz? Gå til deltagersiden
      </Link>
    </main>
  )
}
