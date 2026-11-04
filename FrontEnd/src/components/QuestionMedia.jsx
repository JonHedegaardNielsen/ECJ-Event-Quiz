// Spørgsmåls-billede: bruger <img> hvis der findes en billed-url,
// ellers et mock-"foto" (gradient + emoji) svarende til pakkens billed-indhold.
export default function QuestionMedia({ question }) {
  if (!question) return null
  if (question.image) {
    return (
      <div className="qmedia">
        <img src={question.image} alt={question.text || 'Spørgsmålsbillede'} />
      </div>
    )
  }
  const art = question.art
  if (!art) return null
  return (
    <div className="qmedia" style={{ background: art.gradient }}>
      <span className="qmedia-emoji" aria-hidden="true">
        {art.emoji}
      </span>
    </div>
  )
}
