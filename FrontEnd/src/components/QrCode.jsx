// Deterministisk "QR-kode" (mock – rigtig QR genereres senere fra API/URL).
import { QRCodeCanvas } from "qrcode.react";

export default function QrCode({ seed = '0000', size = 78 }) {
  const rootUrl = `${window.location.origin}`;
  return (
    <QRCodeCanvas value={`${rootUrl}/deltag/${seed}`} size={size} />
  )
}
