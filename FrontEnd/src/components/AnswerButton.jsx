// Svar-knap i Kahoot-farver.
// count: antal svar (værts-visning). state: null | 'picked' | 'correct' | 'wrong'.
import { LETTERS } from '../data/mock'

export default function AnswerButton({ option, index, color, count = null, state = null, clickable = false, onClick }) {
  const cls = ['answer', state ? `is-${state}` : '', clickable ? 'is-clickable' : ''].filter(Boolean).join(' ')
  return (
    <button
      type="button"
      className={cls}
      style={{ '--ans': color }}
      disabled={!clickable}
      onClick={clickable ? onClick : undefined}
    >
      <span className="answer-letter" aria-hidden="true">
        {LETTERS[index]}
      </span>
      <span className="answer-text">{option}</span>
      {count != null && count > 0 && <span className="answer-count">{count}</span>}
    </button>
  )
}
