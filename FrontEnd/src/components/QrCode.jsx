// QR-kode der åbner deltagersiden med Room-ID'et udfyldt.
import { QRCodeCanvas } from "qrcode.react";

function joinUrl(roomId) {
  return `${window.location.origin}/#/deltag?code=${encodeURIComponent(roomId)}`
}

export default function QrCode({ seed = '0000', size = 78 }) {
  return (
    <QRCodeCanvas value={joinUrl(seed)} size={size} />
  )
}
