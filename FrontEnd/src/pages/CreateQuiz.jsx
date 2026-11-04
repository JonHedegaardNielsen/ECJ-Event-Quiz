// 3. Opret quiz (arrangør): 3 trin – grundlæggende, spørgsmål, gennemgang.
import { useEffect, useState } from 'react'
import Stepper from '../components/Stepper'
import Logo from '../components/Logo'
import { IconArrowLeft, IconArrowRight, IconCheck, IconX, IconPlusCircle } from '../components/icons'
import { useGame } from '../game/useGame'
import { ANSWER_COLORS, LETTERS, PACKAGES, emptyQuestion } from '../data/mock'
import { clone } from '../lib/utils'
import { navigate } from '../lib/router'
import './CreateQuiz.css'

export default function CreateQuiz() {
  const { quizzes, draft, clearDraft, saveQuiz } = useGame()
  const [step, setStep] = useState(0)
  const [err, setErr] = useState('')
  const [expanded, setExpanded] = useState(null)
  const [quiz, setQuiz] = useState(() => {
    const src = draft && quizzes ? quizzes.find((q) => q.id === draft) : null
    return src ? clone(src) : { title: '', description: '', packageId: 'stor', questions: [] }
  })

  // Ryd editor-draft når skærmen forlades
  useEffect(() => () => clearDraft(), [clearDraft])

  const update = (patch) => setQuiz((q) => ({ ...q, ...patch }))
  const updateQuestion = (i, patch) =>
    setQuiz((q) => ({ ...q, questions: q.questions.map((qq, j) => (j === i ? { ...qq, ...patch } : qq)) }))

  const nextStep = () => {
    setErr('')
    if (step === 0 && !quiz.title.trim()) {
      setErr('Giv din quiz et navn.')
      return
    }
    setStep((s) => s + 1)
  }

  const addQuestion = () => {
    const empty = emptyQuestion()
    setQuiz((q) => ({ ...q, questions: [...q.questions, empty] }))
    setExpanded(quiz.questions.length)
  }

  const removeQuestion = (i) => {
    setQuiz((q) => ({ ...q, questions: q.questions.filter((_, j) => j !== i) }))
  }

  const publish = () => {
    setErr('')
    if (!quiz.title.trim()) {
      setErr('Giv din quiz et navn.')
      setStep(0)
      return
    }
    if (quiz.questions.length === 0) {
      setErr('Tilføj mindst ét spørgsmål.')
      setStep(1)
      return
    }
    saveQuiz(quiz)
  }

  const goBack = () => {
    if (step > 0) setStep((s) => s - 1)
    else {
      clearDraft()
      navigate('/dashboard')
    }
  }

  return (
    <div className="page-create">
      <div className="create-top">
        <Logo size="sm" tone="light" />
      </div>
      <div className="create-card">
        <button type="button" className="create-back" onClick={goBack}>
          <IconArrowLeft size={16} />
          Tilbage
        </button>
        <h1 className="create-title">Opret ny quiz</h1>
        <Stepper current={step} />

        {step === 0 && (
          <div className="u-fields">
            <label className="u-field">
              <span className="u-label">Quiz navn</span>
              <input
                className="u-input"
                placeholder="Fx Firmafest 2026"
                value={quiz.title}
                onChange={(e) => update({ title: e.target.value })}
                autoFocus
              />
            </label>
            <label className="u-field">
              <span className="u-label">Beskrivelse (valgfri)</span>
              <textarea
                className="u-input u-textarea"
                placeholder="En sjov quiz til firmafesten."
                value={quiz.description}
                onChange={(e) => update({ description: e.target.value })}
              />
            </label>
            <div className="u-field">
              <span className="u-label">Vælg pakke</span>
              <div className="pkg-grid" role="radiogroup" aria-label="Pakke">
                {PACKAGES.map((p) => {
                  const selected = quiz.packageId === p.id
                  return (
                    <button
                      key={p.id}
                      type="button"
                      role="radio"
                      aria-checked={selected}
                      className={`pkg-card${selected ? ' selected' : ''}`}
                      onClick={() => update({ packageId: p.id })}
                    >
                      <b>{p.name}</b>
                      <span>{p.players}</span>
                      <span>{p.features}</span>
                      {selected && (
                        <span className="pkg-check">
                          <IconCheck size={12} />
                        </span>
                      )}
                    </button>
                  )
                })}
              </div>
            </div>
          </div>
        )}

        {step === 1 && (
          <div>
            {quiz.questions.map((qq, i) => (
              <div className="q-item" key={i}>
                <button
                  type="button"
                  className="q-item-head"
                  onClick={() => setExpanded((e) => (e === i ? null : i))}
                >
                  <span className="q-num">{i + 1}.</span>
                  <span className="q-preview">{qq.text || 'Nyt spørgsmål – klik for at redigere'}</span>
                  <span className="q-time">{qq.time} sek.</span>
                  <span
                    role="button"
                    tabIndex={0}
                    className="q-delete"
                    aria-label="Slet spørgsmål"
                    onClick={(e) => {
                      e.stopPropagation()
                      removeQuestion(i)
                    }}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter' || e.key === ' ') {
                        e.stopPropagation()
                        removeQuestion(i)
                      }
                    }}
                  >
                    <IconX size={15} />
                  </span>
                </button>
                {expanded === i && (
                  <div className="q-edit">
                    <input
                      className="u-input"
                      placeholder="Skriv dit spørgsmål…"
                      value={qq.text}
                      onChange={(e) => updateQuestion(i, { text: e.target.value })}
                    />
                    <div className="opt-grid">
                      {qq.options.map((opt, j) => (
                        <div className="opt" key={j} style={{ '--ans': ANSWER_COLORS[j] }}>
                          <span className="opt-letter">{LETTERS[j]}</span>
                          <input
                            className="u-input"
                            placeholder={`Svarmulighed ${LETTERS[j]}`}
                            value={opt}
                            onChange={(e) => updateQuestion(i, { options: qq.options.map((o, k) => (k === j ? e.target.value : o)) })}
                          />
                        </div>
                      ))}
                    </div>
                    <div className="q-edit-row">
                      <span className="u-label">Rigtigt svar</span>
                      <div className="corr-group">
                        {LETTERS.map((l, j) => (
                          <button
                            key={l}
                            type="button"
                            className={`corr-btn${qq.correct === j ? ' selected' : ''}`}
                            style={{ '--ans': ANSWER_COLORS[j] }}
                            onClick={() => updateQuestion(i, { correct: j })}
                            aria-pressed={qq.correct === j}
                          >
                            {l}
                          </button>
                        ))}
                      </div>
                      <span className="u-label">Tid</span>
                      <select
                        className="u-input u-select"
                        value={qq.time}
                        onChange={(e) => updateQuestion(i, { time: Number(e.target.value) })}
                      >
                        <option value={15}>15 sek.</option>
                        <option value={20}>20 sek.</option>
                        <option value={30}>30 sek.</option>
                        <option value={60}>60 sek.</option>
                      </select>
                    </div>
                    <input
                      className="u-input"
                      placeholder="Billede-URL (valgfri)"
                      value={qq.image ?? ''}
                      onChange={(e) => updateQuestion(i, { image: e.target.value })}
                    />
                  </div>
                )}
              </div>
            ))}
            <button type="button" className="add-q" onClick={addQuestion}>
              <IconPlusCircle size={18} />
              Tilføj spørgsmål
            </button>
          </div>
        )}

        {step === 2 && (
          <div>
            <dl className="summary">
              <div className="summary-row">
                <dt>Navn</dt>
                <dd>{quiz.title}</dd>
              </div>
              <div className="summary-row">
                <dt>Beskrivelse</dt>
                <dd>{quiz.description || '–'}</dd>
              </div>
              <div className="summary-row">
                <dt>Pakke</dt>
                <dd>{PACKAGES.find((p) => p.id === quiz.packageId)?.name}</dd>
              </div>
              <div className="summary-row">
                <dt>Spørgsmål</dt>
                <dd>{quiz.questions.length}</dd>
              </div>
            </dl>
            <p className="summary-note">Din quiz er klar til at blive spillet.</p>
          </div>
        )}

        {err && <p className="u-error">{err}</p>}

        <div className="create-foot">
          {step < 2 ? (
            <button type="button" className="btn" onClick={nextStep}>
              Næste
              <IconArrowRight size={17} />
            </button>
          ) : (
            <button type="button" className="btn" onClick={publish}>
              Udgiv quiz
            </button>
          )}
        </div>
      </div>
    </div>
  )
}
