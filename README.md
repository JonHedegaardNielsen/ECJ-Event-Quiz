# ECJ EventQuiz

Fælles solution til skoleprojektet (live quiz via SignalR).

## Projekter

| Projekt | Rolle |
|---------|--------|
| `ECJ.EventQuiz.ServerUI` | WPF + WebView2 – hoster API, SignalR og React-SPA på én port (.NET 10) |
| `ECJ.EventQuiz.Core` | Modeller, database, seed-data, session-service, `QuizHub` |
| `BackEnd/EventQuiz.Api` | REST API-controllers (bruges af ServerUI; kan også køres separat til Swagger/dev) |
| `FrontEnd` | Jons React/Vite-app |

## Installer (Windows 11)

Til udlevering på en anden PC **uden** .NET SDK, MSBuild eller Node:

1. På udviklingsmaskinen (kræver .NET 10 SDK, Node og [Inno Setup 6](https://jrsoftware.org/isinfo.php)):

```bat
Build-Installer.bat
```

2. Installer-filen lander i `dist\ECJ-EventQuiz-Setup.exe`
3. Kør setup på mål-PC → Start-menu → **ECJ EventQuiz**
4. Tryk **Start server** → **Åbn quiz**
5. Login: `demo` / `demo123`

Databasen ligger i `%LocalAppData%\ECJ.EventQuiz\eventquiz.db` når programmet er installeret.

## Hurtig start

**Anbefalet (som installeret app)**

```bat
Start-All.bat
```

Det bygger frontend ind i ServerUI og starter **kun** `ECJ.EventQuiz.ServerUI.exe` (splash + WPF). Ingen ekstra konsolvinduer.

1. Splash vises → ServerUI åbner  
2. Tryk **Start server** → **Åbn quiz**  
3. Login: `demo` / `demo123`  
4. Alt kører på http://localhost:5188 (API + SignalR + React)

**Udvikling med Vite (valgfrit)**

Til frontend-hot-reload kan du stadig køre API + ServerUI + `npm run dev` hver for sig, eller bruge VS Code F5 → **Hele systemet**.

| Del | Adresse |
|---|---|
| Samlet ServerUI (anbefalet) | http://localhost:5188 |
| API alene (dev/Swagger) | http://localhost:5218 |
| Frontend alene (Vite) | http://localhost:5173 |

Uden server kan frontenden køre i mock-tilstand med `VITE_USE_SIGNALR=false`.

## Tests

```bash
dotnet test ECJ.EventQuiz.Tests\ECJ.EventQuiz.Tests.csproj
```

## SignalR (kort)

- Klient → server: `CreateSession`, `JoinLobby`, `StartQuiz`, `OpenQuestion`, `SubmitAnswer`, `CloseQuestion`, `NextQuestion`, `EndQuiz`
- Server → klient: `LobbyUpdated`, `QuizStarted`, `QuestionChanged`, `AnswerCountsUpdated`, `QuestionClosed`, `QuizEnded`
- JS-hjælper: `FrontEnd/src/lib/signalrClient.js`

## Database

I udvikling ligger `eventquiz.db` i repo-roden. Ved installeret app bruges `%LocalAppData%\ECJ.EventQuiz\`.

Ved start oprettes demo-brugeren `demo` / `demo123` med tre testquizzer.

Point beregnes efter Kahoot-systemet (`ScoreCalculator`): maks. 1000 point, svar under 0,5 sek. giver altid 1000, og et rigtigt svar giver mindst 500.
