// 5. Deltagelse (mobil/PC): indtast Room-ID eller scan QR-kode.
import { useRef, useState } from 'react'
import Logo from '../../components/Logo'
import { IconCamera, IconQr } from '../../components/icons'
import { useGame } from '../../game/useGame'
import './Join.css'

export default function Join() {
  const { startJoin } = useGame()
  const [tab, setTab] = useState('id') // 'id' | 'qr'
  const [digits, setDigits] = useState(['', '', '', ''])
  const inputs = useRef([])

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

  const submit = (e) => {
    e.preventDefault()
    if (!ready) return
    // Demo: ukendt kode giver en demo-quiz. API'et kobles på senere.
    startJoin(code)
  }

  return (
    <main className="page-join">
      <Logo size="lg" />
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

            <button type="submit" className="btn btn-block btn-lg" disabled={!ready}>
              Fortsæt
            </button>

            <div className="join-or" aria-hidden="true">
              eller
            </div>

            <button type="button" className="btn btn-block btn-scan" onClick={() => setTab('qr')}>
              <IconQr size={19} />
              Scan QR-kode
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
      <a className="join-admin-link" href="#/login">
        Gå til admin
      </a>
    </main>
  )
}
