# ECJ EventQuiz

Fælles solution til skoleprojektet (live quiz via SignalR).

## Projekter

| Projekt | Rolle |
|---------|--------|
| `ECJ.EventQuiz.Core` | Modeller, seed-data, session-service, `QuizHub` (.NET 10) |
| `ECJ.EventQuiz.ServerUI` | WPF + WebView2 – starter/stopper Kestrel + SignalR (.NET 10) |
| `FrontEnd` | Jons React/Vite-app (solution folder, køres med npm) |

## Hurtig start

1. Åbn `ECJ.EventQuiz.sln`
2. Kør `ECJ.EventQuiz.ServerUI` → tryk **Start server**
3. Hub: `http://localhost:5188/hubs/quiz`
4. Frontend: `cd FrontEnd && npm install && npm run dev`
5. I `FrontEnd/src/game/GameContext.jsx`: sæt `USE_SIGNALR = true` for live-tilstand

## Tests

```bash
dotnet test ECJ.EventQuiz.Tests\ECJ.EventQuiz.Tests.csproj
```

- **Unit** – ScoreCalculator, LeaderBoard, SeedData, QuizSessionService, AppSettings, NetworkAddresses, QuizServerHost
- **Integration** – SignalR hub-events over rigtig Kestrel-host
- **Acceptance** – fulde scenarier (arrangør + deltagere gennem live quiz)

## SignalR (kort)

- Klient → server: `CreateSession`, `JoinLobby`, `StartQuiz`, `OpenQuestion`, `SubmitAnswer`, `CloseQuestion`, `NextQuestion`, `EndQuiz`
- Server → klient: `LobbyUpdated`, `QuizStarted`, `QuestionChanged`, `AnswerCountsUpdated`, `QuestionClosed`, `QuizEnded`
- JS-hjælper: `FrontEnd/src/lib/signalrClient.js`

## Indtil Mads er klar

Sessioner og quizzer kører **in-memory** (`SeedData` + `QuizSessionService`). EF/SQLite kan erstatte det senere uden at ændre hub-kontrakten særligt meget.
