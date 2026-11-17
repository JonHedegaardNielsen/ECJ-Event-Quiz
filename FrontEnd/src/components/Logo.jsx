// Brand-mark: logo + produkttitel (Assets i /public).
const SIZES = {
  sm: { logo: 40, title: 30 },
  md: { logo: 56, title: 42 },
  lg: { logo: 88, title: 64 },
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
