# ECJ EventQuiz

Fælles solution til skoleprojektet (live quiz via SignalR).

## Projekter

| Projekt | Rolle |
|---------|--------|
| `ECJ.EventQuiz.Core` | Modeller, quiz-repository, session-service, `QuizHub` (.NET 10) |
| `ECJ.EventQuiz.ServerUI` | WPF + WebView2 – starter/stopper Kestrel + SignalR (.NET 10) |
| `FrontEnd` | Jons React/Vite-app (solution folder, køres med npm) |

## Hurtig start

1. Åbn `ECJ.EventQuiz.sln` (og start `BackEnd/EventQuiz.Api` én gang, så databasen og dine quizzer findes)
2. Kør `ECJ.EventQuiz.ServerUI` → tryk **Start server**
3. Hub: `http://localhost:5188/hubs/quiz`
4. Frontend: `cd FrontEnd && npm install && npm run dev`
5. I `FrontEnd/src/game/GameContext.jsx`: sæt `USE_SIGNALR = true` for live-tilstand

## Tests

```bash
dotnet test ECJ.EventQuiz.Tests\ECJ.EventQuiz.Tests.csproj
```

- **Unit** – ScoreCalculator, LeaderBoard, SqliteQuizRepository, QuizSessionService, AppSettings, NetworkAddresses, QuizServerHost
- **Integration** – SignalR hub-events over rigtig Kestrel-host
- **Acceptance** – fulde scenarier (arrangør + deltagere gennem live quiz)

## SignalR (kort)

- Klient → server: `CreateSession`, `JoinLobby`, `StartQuiz`, `OpenQuestion`, `SubmitAnswer`, `CloseQuestion`, `NextQuestion`, `EndQuiz`
- Server → klient: `LobbyUpdated`, `QuizStarted`, `QuestionChanged`, `AnswerCountsUpdated`, `QuestionClosed`, `QuizEnded`
- JS-hjælper: `FrontEnd/src/lib/signalrClient.js`

## Hvor kommer spørgsmålene fra?

Quizzer og spørgsmål oprettes via `EventQuiz.Api` og gemmes i `BackEnd/EventQuiz.Api/eventquiz.db`.
Den live SignalR-server læser **samme SQLite-database** via `SqliteQuizRepository`, så det er de
quizzer, brugeren har oprettet i frontend – ikke hardkodede spørgsmål.

- Serveren leder automatisk efter `BackEnd/EventQuiz.Api/eventquiz.db` opad fra executable-mappen.
- Sæt miljøvariablen `ECJ_QUIZ_DB` for at pege på en anden databasefil.
- Live-sessioner (rum, deltagere, svar og point) kører stadig **in-memory** i `QuizSessionService`.
- `GET /api/quiz-database` på live-serveren viser hvilken databasefil der bruges.
