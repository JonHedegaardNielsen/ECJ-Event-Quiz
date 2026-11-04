import { useContext } from 'react'
import { GameContext } from './gameContext'

export function useGame() {
  return useContext(GameContext)
}
