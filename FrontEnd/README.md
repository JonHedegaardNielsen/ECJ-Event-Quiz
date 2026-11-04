# ECJ EventQuiz – FrontEnd

Kahoot-inspireret quiz-app bygget fra mockuperne (Quiz-Mockups.png).

## Teknologi

- React 19 + Vite 8 (ingen ekstra dependencies)
- Hash-routing (`src/lib/router.js`) – ingen serverkonfiguration nødvendig
- Global spilstyring i `src/game/GameContext.jsx` – ét sted at koble API'et på senere
- Mock-data i `src/data/` (quizzer + deltagere simuleres lokalt)

## Screens (svarende til mockuperne)

| # | Screen | Route |
|---|--------|-------|
| 1 | Login (arrangør) | `#/login` |
| 2 | Dashboard / Mine quizzer | `#/dashboard` |
| 3 | Opret quiz (3 trin) | `#/opret` |
| 4 | Lobby med Room-ID + QR | `#/lobby` |
| 5 | Deltagelse (Room-ID / QR) | `#/deltag` |
| 5b | Indtast navn + "du deltager nu i" | `#/deltag/navn` |
| 6 | Live spørgsmål (værtsvisning) | `#/live` |
| 7 | Resultatliste med konfetti | `#/resultat` |
| 7b | Deltagerens live-spil (mobil/PC) | `#/spil` |

## Demo-flow

**Vært:** Log ind → Start en quiz fra Dashboardet → Lobbyen, hvor testdeltagerne tilslutter sig → **Start quiz** → værten ser spørgsmålene og antallet af svar pr. mulighed, og når alle har svaret, går den videre automatisk → slutresultat med rangliste.

**Deltager:** Tryk "Skal du deltage i en quiz?" på login-siden (eller gå til `#/deltag`) → indtast et Room-ID (fx `4729` eller koden fra lobbyen) → skriv dit navn → svar på spørgsmålene mod mockdeltagere → se sin placering på ranglisten.

Quizzer oprettet i "Opret quiz" gemmes i browseren (localStorage) sammen med den loggede bruger.

## Scripts

```bash
npm run dev      # udviklingsserver
npm run build    # produktionsbuild
npm run lint     # ESLint
npm run preview  # serve dist/
```

## Når API'et er klar

- `src/game/GameContext.jsx` er det eneste sted, der skal skiftes: indlæs quizzer fra API, opret rum, send svar over WebSocket osv.
- Mockdeltagere i Lobby/Spil udskiftes med rigtige websocket-events.
- Login-skærmen sender nu kun `name`/`password` videre til `login()` i contexten.
