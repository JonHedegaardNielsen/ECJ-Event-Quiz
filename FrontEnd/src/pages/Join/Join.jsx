// 5. Deltagelse (mobil/PC): indtast Room-ID eller scan QR-kode.
// ?code=1234 (fra QR-koden i lobbyen) udfylder Room-ID'et og fortsætter direkte
// til navne-siden, så der kun skal tastes navn efter et scan.
import { useEffect, useRef, useState } from 'react'
import Logo from '../../components/Logo'
import { IconCamera, IconQr } from '../../components/icons'
import { useGame } from '../../game/useGame'
import { Link, useSearchParams } from 'react-router-dom'
import './Join.css'

export default function Join() {
  const { startJoin } = useGame()
  const [tab, setTab] = useState('id') // 'id' | 'qr'
  const [params] = useSearchParams()
  const [digits, setDigits] = useState(() => {
    const code = (params.get('code') ?? '').replace(/\D/g, '').slice(0, 4)
    return code.padEnd(4).split('').map((ch) => ch.trim())
  })
  const inputs = useRef([])
  const [error, setError] = useState(null)
  const [joining, setJoining] = useState(false)
  // Auto-join ved ?code må kun køre én gang (React StrictMode kører effects to gange i dev)
  const autoJoinRef = useRef(false)

  const code = digits.join('')
  const ready = code.length === 4

  const setDigit = (i, value) => {
    const v = value.replace(/\D/g, '').slice(-1)
    setDigits((d) => d.map((x, j) => (j === i ? v : x)))
    if (v && inputs.current[i + 1]) inputs.current[i + 1].focus()
  }

  const onKey = (i, e) => {
    if (e.key === 'Backspace' && !digits[i] && inputs.current[i - 1]) {
      inputs.current[i - 1].focus()
      setDigits((d) => d.map((x, j) => (j === i - 1 ? '' : x)))
    }
  }

  const onPaste = (e) => {
    const text = (e.clipboardData.getData('text') || '').replace(/\D/g, '').slice(0, 4)
    if (!text) return
    e.preventDefault()
    setDigits((d) => text.padEnd(4).slice(0, 4).split('').map((ch, i) => (ch ? ch : d[i])))
    if (text.length >= 4 && inputs.current[3]) inputs.current[3].focus()
  }

  const doJoin = async () => {
    if (!ready) return
    setJoining(true)
    setError(await startJoin(code) ?? null)
    setJoining(false)
  }

  const submit = (e) => {
    e.preventDefault()
    doJoin()
  }

  // QR-/deep-link: med et fuldt Room-ID i ?code forbindes direkte,
  // så spilleren lander på navne-siden med det samme.
  useEffect(() => {
    if (autoJoinRef.current) return
    const code = (params.get('code') ?? '').replace(/\D/g, '').slice(0, 4)
    if (code.length === 4) {
      autoJoinRef.current = true
      // startJoin navigerer selv til /deltag/navn ved succes
      startJoin(code).then((err) => {
        if (err) setError(err)
      })
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return (
    <main className="page-join">
      <Logo size="xl" />
      <div className="join-card">
        {tab === 'id' ? (
          <form className="join-form" onSubmit={submit}>
            <h1 className="join-title">Deltag i quizzen</h1>
            <p className="join-sub">Indtast Room-ID eller scan QR-kode</p>

            <div className="code-row" onPaste={onPaste}>
              {digits.map((d, i) => (
                <input
                  key={i}
                  ref={(el) => {
                    inputs.current[i] = el
                  }}
                  className="code-box"
                  inputMode="numeric"
                  maxLength={1}
                  value={d}
                  autoFocus={i === 0}
                  onChange={(e) => setDigit(i, e.target.value)}
                  onKeyDown={(e) => onKey(i, e)}
                  aria-label={`Ciffer ${i + 1}`}
                />
              ))}
            </div>

            {error && <p className="join-error" role="alert" style={{ color: "var(--danger-soft)", margin: 0 }}>{error}</p>}
            <button type="submit" className="btn btn-block btn-lg" disabled={!ready || joining}>
              {joining ? 'Forbinder…' : 'Fortsæt'}
            </button>
          </form>
        ) : (
          <div className="join-qr-pane">
            <div className="qr-panel">
              <span className="qr-panel-icon">
                <IconCamera size={22} />
              </span>
              <p>
                Peg kameraet mod værten QR-kode.
                <br />
                <em>(Kamera-apis og fjern-server kobles på, når API'et er klar.)</em>
              </p>
            </div>
            <button type="button" className="btn btn-block btn-scan" onClick={() => setTab('id')}>
              Skriv Room-ID i stedet
            </button>
          </div>
        )}
      </div>
      <div className="join-footer-links">
        <Link className="join-admin-link" to="/">
          Se åbne rum
        </Link>
        <Link className="join-admin-link" to="/login">
          Arrangør-login
        </Link>
      </div>
    </main>
  )
}
