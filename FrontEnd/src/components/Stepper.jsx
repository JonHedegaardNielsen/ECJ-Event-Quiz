// Trin-indikator til "Opret quiz" (1 Grundlæggende · 2 Spørgsmål · 3 Gennemgang).
import { IconCheck } from './icons'

const STEPS = ['Grundlæggende', 'Spørgsmål', 'Gennemgang']

export default function Stepper({ current }) {
  return (
    <ol className="stepper" aria-label="Trin">
      {STEPS.map((label, i) => {
        const cls = i === current ? 'active' : i < current ? 'done' : ''
        return (
          <li key={label} className={`step ${cls}`.trim()}>
            <span className="step-dot">{i < current ? <IconCheck size={14} /> : i + 1}</span>
            <span className="step-label">{label}</span>
          </li>
        )
      })}
    </ol>
  )
}
