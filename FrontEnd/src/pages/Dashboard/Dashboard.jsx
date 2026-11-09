// 2. Dashboard (arrangør): quizliste med start/menupunkter.
import { useEffect, useState } from 'react'
import Sidebar from '../../components/Sidebar'
import Avatar from '../../components/Avatar'
import { IconDots, IconArrowRight, IconPlusCircle, IconX } from '../../components/icons'
import { useGame } from '../../game/useGame'
import { packageInfo } from '../../data/mock'
import './Dashboard.css'

export default function Dashboard() {
  const { user, quizzes, quizzesLoading, quizzesError, refreshQuizzes, startSession, openEditor, startQuizDraft, duplicateQuiz, deleteQuiz } = useGame()
  const [menuFor, setMenuFor] = useState(null)

  useEffect(() => {
    if (!menuFor) return undefined
    const close = (e) => {
      if (e.target.closest('.quiz-menu') || e.target.closest('.quiz-kebab')) return
      setMenuFor(null)
    }
    window.addEventListener('click', close)
    return () => window.removeEventListener('click', close)
  }, [menuFor])

  const list = quizzes ?? []

  return (
    <div className="page-dash">
      <Sidebar />
      <section className="dash-main">
        <header className="dash-top">
          <Avatar size={34} />
          <span>{user?.name ?? 'Arrangør'}</span>
        </header>
        <div className="dash-body">
          <div className="dash-head">
            <h1>Mine quizzer</h1>
            <button type="button" className="btn" onClick={startQuizDraft}>
              <IconPlusCircle size={18} />
              Opret ny quiz
            </button>
          </div>

          {quizzesError && (
            <div className="dash-error">
              <p>{quizzesError}</p>
              <button type="button" className="btn" onClick={refreshQuizzes}>
                Prøv igen
              </button>
            </div>
          )}

          {quizzesLoading && list.length === 0 && <p className="dash-loading">Henter dine quizzer…</p>}

          {!quizzesError && !quizzesLoading && list.length === 0 && (
            <div className="dash-empty">
              <p>Ingen quizzer endnu.</p>
              <button type="button" className="btn" onClick={startQuizDraft}>
                Opret din første quiz
              </button>
            </div>
          )}

          {list.map((quiz) => {
            const pkg = packageInfo(quiz.packageId)
            return (
              <article className="quiz-row" key={quiz.id}>
                <span className="quiz-ico" style={{ background: quiz.tile }} aria-hidden="true">
                  {quiz.icon}
                </span>
                <div className="quiz-info">
                  <h3 className="quiz-title">{quiz.title}</h3>
                  <p className="quiz-meta">
                    {quiz.questions.length} spørgsmål · {pkg.features}
                  </p>
                </div>
                <button
                  type="button"
                  className="btn quiz-start"
                  onClick={() => startSession(quiz)}
                  disabled={quiz.questions.length === 0}
                  title={quiz.questions.length === 0 ? 'Quizzen har ingen spørgsmål' : `Start "${quiz.title}"`}
                >
                  Start
                </button>
                <button
                  type="button"
                  className="quiz-kebab"
                  aria-label="Quizmenu"
                  onClick={() => setMenuFor((id) => (id === quiz.id ? null : quiz.id))}
                >
                  <IconDots />
                </button>
                {menuFor === quiz.id && (
                  <div className="quiz-menu" role="menu">
                    <button type="button" role="menuitem" onClick={() => { setMenuFor(null); openEditor(quiz.id) }}>
                      Rediger
                    </button>
                    <button type="button" role="menuitem" onClick={() => { setMenuFor(null); duplicateQuiz(quiz.id) }}>
                      Dupliker
                    </button>
                    <button
                      type="button"
                      role="menuitem"
                      className="danger"
                      onClick={() => {
                        setMenuFor(null)
                        if (window.confirm(`Slet quizzen "${quiz.title}"?`)) deleteQuiz(quiz.id)
                      }}
                    >
                      <IconX size={14} />
                      Slet
                    </button>
                  </div>
                )}
              </article>
            )
          })}
        </div>
      </section>
    </div>
  )
}
