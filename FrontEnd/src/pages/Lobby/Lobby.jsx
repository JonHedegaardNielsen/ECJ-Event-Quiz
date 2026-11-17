// 4. Lobby (arrangør): Room ID, join-link, QR-kode og deltagere.
// LIVE: deltagere kommer fra serveren via LobbyUpdated. MOCK: simulerede deltagere.
import { useEffect, useState } from 'react'
import Logo from '../../components/Logo'
import QrCode from '../../components/QrCode'
import { joinUrl } from '../../lib/qr'
import Avatar from '../../components/Avatar'
import { IconArrowLeft, IconX } from '../../components/icons'
import { useGame } from '../../game/useGame'
import { packageInfo, getQuiz } from '../../data/mock'
import { pickBots } from '../../lib/utils'
import './Lobby.css'

export default function Lobby() {
  const { room, quizzes, addParticipant, removeParticipant, launchQuiz } = useGame()
  const quiz = getQuiz(quizzes ?? [], room?.quizId)
  const isLive = Boolean(room?.live)
  const [copied, setCopied] = useState(false)

  // Demo: mock-deltagere tilslutter sig automatisk. I LIVE-mode kommer de via SignalR.
  useEffect(() => {
    if (!room || isLive) return undefined
    const names = pickBots(6)
    const timers = names.map((name, i) => setTimeout(() => addParticipant(name), i * 950 + 500))
    return () => timers.forEach(clearTimeout)
  }, [addParticipant, isLive, room])

  if (!room || !quiz) return null

  const pkg = packageInfo(quiz.packageId)
  const canStart = room.participants.length > 0
  const url = joinUrl(room.id)
  const isLocalHost = ['localhost', '127.0.0.1'].includes(new URL(url).hostname)

  const copyJoinLink = async () => {
    try {
      await navigator.clipboard.writeText(url)
      setCopied(true)
      setTimeout(() => setCopied(false), 1400)
    } catch {
      setCopied(false)
    }
  }

  return (
    <main className="page-lobby">
      <div className="lobby-wrap">
        <div className="lobby-top">
          <Logo size="sm" />
          <button type="button" className="link-ghost" onClick={() => window.history.back()}>
            <IconArrowLeft size={16} />
            Tilbage
          </button>
        </div>

        <div className="lobby-head">
          <h1>{quiz.title}</h1>
          <p>Lobby</p>
        </div>

        <section className="lobby-card room-card">
          <div className="room-id">
            <span className="room-label">Room ID</span>
            <span className="room-digits">{room.id}</span>
          </div>
          <div className="lobby-qr">
            <span className="qr-box">
              <QrCode seed={room.id} />
            </span>
            <span className="qr-caption">Scan med telefon</span>
          </div>
        </section>

        <section className="lobby-card join-pc-card">
          <span className="room-label">Deltag fra PC</span>
          <p className="join-pc-help">
            Åbn linket i et andet browservindue (gerne inkognito) eller på en anden enhed — ikke i samme fane som værten.
            Room ID er allerede udfyldt; der skal kun tastes et spillernavn.
          </p>
          <div className="join-link-row">
            <code className="join-link">{url}</code>
            <button type="button" className="btn-copy" onClick={copyJoinLink}>
              {copied ? 'Kopieret' : 'Kopiér'}
            </button>
          </div>
          <p className="join-pc-alt">
            Eller gå til <strong>/#/deltag</strong> og indtast Room ID <strong>{room.id}</strong> manuelt.
          </p>
          {isLocalHost && (
            <p className="lobby-hint lobby-hint-warn">
              Telefoner kan ikke bruge localhost. Vælg LAN-IP i ServerUI, eller åbn quizzen via computerens netværksadresse.
            </p>
          )}
        </section>

        <section className="lobby-card">
          <div className="p-head">
            <span className="dot-live" aria-hidden="true" />
            Deltagere ({room.participants.length}/{pkg.cap})
          </div>
          <div className="p-list">
            {room.participants.map((name) => (
              <div className="p-row" key={name}>
                <Avatar size={28} />
                <span>{name}</span>
                {!isLive && (
                  <button
                    type="button"
                    className="p-x"
                    aria-label={`Fjern ${name}`}
                    onClick={() => removeParticipant(name)}
                  >
                    <IconX size={14} />
                  </button>
                )}
              </div>
            ))}
            {room.participants.length === 0 && <p className="p-empty">Venter på deltagere…</p>}
          </div>
        </section>

        <button type="button" className="btn btn-green btn-lg btn-block" onClick={launchQuiz} disabled={!canStart}>
          Start quiz
        </button>
        {!canStart && (
          <p className="lobby-hint">
            PC: kopiér linket ovenfor. Telefon: scan QR-koden. Begge dele bruger Room ID {room.id}.
          </p>
        )}
      </div>
    </main>
  )
}
