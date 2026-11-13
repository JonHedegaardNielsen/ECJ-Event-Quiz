# ECJ EventQuiz

Fælles solution til skoleprojektet (live quiz via SignalR).

## Projekter

| Projekt | Rolle |
|---------|--------|
| `ECJ.EventQuiz.Core` | Modeller, database (`AppDbContext` + migrationer), seed-data, session-service, `QuizHub` (.NET 10) |
| `ECJ.EventQuiz.ServerUI` | WPF + WebView2 – starter/stopper Kestrel + SignalR (.NET 10) |
| `BackEnd/EventQuiz.Api` | REST API (login, quizzer, sessioner, leaderboard) – se `BackEnd/README.md` |
| `FrontEnd` | Jons React/Vite-app (solution folder, køres med npm) |

## Hurtig start

Programmet består af tre dele, der skal køre samtidig:

| Del | Adresse |
|---|---|
| API (`BackEnd/EventQuiz.Api`) | http://localhost:5218 |
| SignalR (`ECJ.EventQuiz.ServerUI`, kun Windows) | http://localhost:5188/hubs/quiz |
| Frontend (`FrontEnd`) | http://localhost:5173 |

**VS Code**

1. Første gang: `cd FrontEnd && npm install`
2. Tryk **F5** og vælg **Hele systemet (API + SignalR + Frontend)**
3. Tryk **Start server** i ServerUI-vinduet

**Terminal** (tre vinduer)

```bash
dotnet run --project BackEnd/EventQuiz.Api
dotnet run --project ECJ.EventQuiz.ServerUI   # tryk Start server
cd FrontEnd && npm run dev
```

**Spil en quiz**

1. Åbn http://localhost:5173 og log ind med `demo` / `demo123`
2. Tryk **Start** på en quiz – lobbyen viser et 4-cifret Room-ID
3. Deltagere åbner http://localhost:5173/#/deltag (andet browservindue/inkognito), skriver Room-ID og navn
4. Tryk **Start quiz**

Uden server kan frontenden køre i mock-tilstand med `VITE_USE_SIGNALR=false`.

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

## Database

API og SignalR deler SQLite-databasen `eventquiz.db` i roden af repoet. Quizzer hentes fra databasen, og sessioner, deltagere og svar (med point) gemmes der. Live-tilstand (fase, forbindelser, timer) ligger kun i hukommelsen.

Ved start oprettes demo-brugeren `demo` / `demo123` med tre testquizzer (id 1–3).

Point beregnes efter Kahoot-systemet (`ScoreCalculator`): maks. 1000 point, svar under 0,5 sek. giver altid 1000, og et rigtigt svar giver mindst 500.
