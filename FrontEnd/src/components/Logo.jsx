// "ECJ EventQuiz" wordmark – bruges på alle skærme.
export default function Logo({ size = 'md', tone = 'dark' }) {
  return (
    <span className={`logo logo-${size} logo-${tone}`} aria-label="ECJ EventQuiz">
      <span className="logo-ecj">ECJ</span>
      <span className="logo-event">Event</span>
      <span className="logo-quiz">Quiz</span>
    </span>
  )
}
