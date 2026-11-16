// QR-kode der åbner deltagersiden med Room-ID'et udfyldt.
// Join-URL'en bygges i lib/qr.js (LAN-IP i dev-mode).
import { QRCodeCanvas } from "qrcode.react";
import { joinUrl } from '../lib/qr'

export default function QrCode({ seed = '0000', size = 78 }) {
  return (
    <QRCodeCanvas value={joinUrl(seed)} size={size} />
  )
}
