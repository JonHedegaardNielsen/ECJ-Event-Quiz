// 5b. Navn + "du deltager nu"-bekræftelse (svarer til andet panel i mockup 5).
import { useState } from 'react'
import Logo from '../components/Logo'
import Avatar from '../components/Avatar'
import { useGame } from '../game/useGame'
import { getQuiz } from '../data/mock'
import './Join.css'

export default function JoinName() {
  const { player, quizzes, setPlayerName } = useGame()
  const quiz = getQuiz(quizzes ?? [], player?.quizId)
  const [name, setName] = useState('')

  if (!player) return null

  const submit = (e) => {
    e.preventDefault()
    if (!name.trim()) return
    setPlayerName(name)
  }

  return (
    <main className="page-join">
      <Logo size="lg" />
      <form className="join-card join-form" onSubmit={submit}>
        <h1 className="join-title">Indtast dit navn</h1>
        <input
          className="name-input"
          placeholder="Dit navn"
          value={name}
          onChange={(e) => setName(e.target.value)}
          maxLength={24}
          autoFocus
        />
        <button type="submit" className="btn btn-block btn-lg" disabled={!name.trim()}>
          Deltag
        </button>

        <div className="joined">
          <Avatar size={76} />
          <div>
            <p className="joined-sub">Du deltager nu i</p>
            <p className="joined-title">{quiz?.title ?? 'Quizzen'}</p>
          </div>
        </div>
      </form>
    </main>
  )
}
