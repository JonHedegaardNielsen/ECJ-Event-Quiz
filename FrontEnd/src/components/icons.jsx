// Smalle inline-SVG ikoner (stroke = currentColor).
const Base = ({ children, size = 18, ...rest }) => (
  <svg
    width={size}
    height={size}
    viewBox="0 0 24 24"
    fill="none"
    stroke="currentColor"
    strokeWidth="1.8"
    strokeLinecap="round"
    strokeLinejoin="round"
    aria-hidden="true"
    {...rest}
  >
    {children}
  </svg>
)

export const IconUser = (p) => (
  <Base {...p}>
    <circle cx="12" cy="8" r="3.6" />
    <path d="M4.7 19.8c1.5-3.2 4.2-4.7 7.3-4.7s5.8 1.5 7.3 4.7" />
  </Base>
)

export const IconLock = (p) => (
  <Base {...p}>
    <rect x="5" y="10.5" width="14" height="9.5" rx="2.2" />
    <path d="M8.2 10.5V8a3.8 3.8 0 0 1 7.6 0v2.5" />
  </Base>
)

export const IconEye = (p) => (
  <Base {...p}>
    <path d="M2.8 12S6.4 5.8 12 5.8 21.2 12 21.2 12 17.6 18.2 12 18.2 2.8 12 2.8 12Z" />
    <circle cx="12" cy="12" r="2.7" />
  </Base>
)

export const IconEyeOff = (p) => (
  <Base {...p}>
    <path d="M4 4l16 16" />
    <path d="M10.6 6.1c.5-.2.9-.3 1.4-.3 5.6 0 9.2 6.2 9.2 6.2s-1 1.8-2.9 3.4M7 7.4C4.5 9 2.8 12 2.8 12s3.6 6.2 9.2 6.2c1.2 0 2.3-.3 3.3-.7" />
    <path d="M9.9 9.9a3 3 0 0 0 4.2 4.2" />
  </Base>
)

export const IconHome = (p) => (
  <Base {...p}>
    <path d="M4.4 10.7 12 4.2l7.6 6.5" />
    <path d="M6.3 9.5V19a1 1 0 0 0 1 1h9.4a1 1 0 0 0 1-1V9.5" />
  </Base>
)

export const IconList = (p) => (
  <Base {...p}>
    <circle cx="4.6" cy="6.4" r="1" fill="currentColor" stroke="none" />
    <circle cx="4.6" cy="12" r="1" fill="currentColor" stroke="none" />
    <circle cx="4.6" cy="17.6" r="1" fill="currentColor" stroke="none" />
    <path d="M8.6 6.4h10.8M8.6 12h10.8M8.6 17.6h10.8" />
  </Base>
)

export const IconPlusCircle = (p) => (
  <Base {...p}>
    <circle cx="12" cy="12" r="8.6" />
    <path d="M12 8.3v7.4M8.3 12h7.4" />
  </Base>
)

export const IconGear = (p) => (
  <Base {...p}>
    <circle cx="12" cy="12" r="3.1" />
    <path d="M12 2.9v2.5M12 18.6v2.5M21.1 12h-2.5M5.4 12H2.9M18.4 5.6l-1.8 1.8M7.4 16.6l-1.8 1.8M18.4 18.4l-1.8-1.8M7.4 7.4 5.6 5.6" />
  </Base>
)

export const IconLogout = (p) => (
  <Base {...p}>
    <path d="M14 4.6h4.4a1 1 0 0 1 1 1v12.8a1 1 0 0 1-1 1H14" />
    <path d="M10 8.4 6.4 12l3.6 3.6" />
    <path d="M6.6 12H15" />
  </Base>
)

export const IconDots = (p) => (
  <Base {...p} size={20}>
    <circle cx="12" cy="5.4" r="1.7" fill="currentColor" stroke="none" />
    <circle cx="12" cy="12" r="1.7" fill="currentColor" stroke="none" />
    <circle cx="12" cy="18.6" r="1.7" fill="currentColor" stroke="none" />
  </Base>
)

export const IconArrowLeft = (p) => (
  <Base {...p}>
    <path d="M19 12H5" />
    <path d="m11 6-6 6 6 6" />
  </Base>
)

export const IconArrowRight = (p) => (
  <Base {...p}>
    <path d="M5 12h14" />
    <path d="m13 6 6 6-6 6" />
  </Base>
)

export const IconClock = (p) => (
  <Base {...p}>
    <circle cx="12" cy="12" r="8.6" />
    <path d="M12 7.4V12l3 1.9" />
  </Base>
)

export const IconQr = (p) => (
  <Base {...p}>
    <path d="M4.5 4.5h5v5h-5zM14.5 4.5h5v5h-5zM4.5 14.5h5v5h-5z" />
    <path d="M14.2 14.4h2.6v2.6h-2.6zM17.8 17.9h2.4v2.4h-2.4z" fill="currentColor" stroke="none" />
    <path d="M17.7 14.5h2.2M14.2 18v2.1" />
  </Base>
)

export const IconCheck = (p) => (
  <Base {...p}>
    <path d="m5.5 12.6 4.2 4.2 9-9.2" />
  </Base>
)

export const IconX = (p) => (
  <Base {...p}>
    <path d="m6 6 12 12M18 6 6 18" />
  </Base>
)

export const IconCamera = (p) => (
  <Base {...p}>
    <path d="M4.6 7.8h2.9l1.6-2.2h5.8l1.6 2.2h2.9a1 1 0 0 1 1 1v8.6a1 1 0 0 1-1 1H4.6a1 1 0 0 1-1-1V8.8a1 1 0 0 1 1-1Z" />
    <circle cx="12" cy="12.8" r="3.3" />
  </Base>
)
