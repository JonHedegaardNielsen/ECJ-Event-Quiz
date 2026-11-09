// ECJ EventQuiz – app-rod: provider + hash-router.
import { useEffect } from 'react'
import { GameProvider } from './game/GameContext'
import { useGame } from './game/useGame'
import { navigate, useRoute } from './lib/router'
import Login from './pages/Login/Login'
import Dashboard from './pages/Dashboard/Dashboard'
import CreateQuiz from './pages/CreateQuiz/CreateQuiz'
import Lobby from './pages/Lobby/Lobby'
import HostLive from './pages/Live/HostLive'
import Results from './pages/Results/Results'
import Join from './pages/Join/Join'
import JoinName from './pages/Join/JoinName'
import PlayerLive from './pages/Live/PlayerLive'

function Redirect({ to }) {
  useEffect(() => {
    navigate(to)
  }, [to])
  return null
}

function Router() {
  const path = useRoute()
  const { user, room, player } = useGame()

  // deltagerflow (altid tilgængelig)
  if (path === '/deltag') return <Join />
  if (path === '/deltag/navn') return player ? <JoinName /> : <Redirect to="/deltag" />
  if (path === '/spil') return player ? <PlayerLive /> : <Redirect to="/deltag" />
  if (path === '/resultat') return room || player ? <Results /> : <Redirect to="/" />

  // vært-flow (kræver login)
  if (path === '/' || path === '/login') return user ? <Redirect to="/dashboard" /> : <Login />
  if (path === '/dashboard') return user ? <Dashboard /> : <Redirect to="/login" />
  if (path === '/opret') return user ? <CreateQuiz /> : <Redirect to="/login" />
  if (!user) return <Redirect to="/login" />
  if (path === '/lobby') return room ? <Lobby /> : <Redirect to="/dashboard" />
  if (path === '/live') return room ? <HostLive /> : <Redirect to="/dashboard" />

  return <Redirect to="/" />
}

export default function App() {
  return (
    <GameProvider>
      <Router />
    </GameProvider>
  )
}
