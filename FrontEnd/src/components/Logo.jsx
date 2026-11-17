// Brand-mark: logo + produkttitel (Assets i /public).
// Størrelser er sat så brandet er tydeligt på både admin- og deltagersider.
const SIZES = {
  sm: { logo: 52, title: 40 },
  md: { logo: 72, title: 54 },
  lg: { logo: 108, title: 78 },
  xl: { logo: 128, title: 92 },
}

export default function Logo({ size = 'md', showTitle = true }) {
  const dims = SIZES[size] ?? SIZES.md

  return (
    <span className={`brand-mark brand-mark-${size}`} aria-label="ECJ EventQuiz">
      <img
        className="brand-mark-logo"
        src={`${import.meta.env.BASE_URL}BrandLogo.png`}
        alt=""
        style={{ height: dims.logo }}
      />
      {showTitle && (
        <img
          className="brand-mark-title"
          src={`${import.meta.env.BASE_URL}BrandTitle.png`}
          alt="ECJ EventQuiz"
          style={{ height: dims.title }}
        />
      )}
    </span>
  )
}
