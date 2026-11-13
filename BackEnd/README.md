# EventQuiz API

Backend til ECJ Event Quiz. Lavet i ASP.NET Core (.NET 10) med Entity Framework Core og en SQLite-database.

Projektet ligger i den fælles solution `ECJ.EventQuiz.sln` i roden af repoet (under mappen `BackEnd`).

## Kør projektet

**Krav:** [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Visual Studio Code

1. Åbn hele repo-mappen (`ECJ-Event-Quiz`) i VS Code.
2. Installer de anbefalede extensions, når VS Code spørger (C# Dev Kit og REST Client).
3. Tryk **F5** og vælg **Backend (API)**. Swagger åbner automatisk i browseren.

### Terminal

```bash
cd BackEnd/EventQuiz.Api
dotnet run
```

Åbn http://localhost:5218/swagger for at se og teste alle endpoints.

Databasen (`eventquiz.db`) ligger i roden af repoet og bliver oprettet automatisk. API'et og SignalR-serveren (ServerUI) bruger den samme fil. Slet filen, hvis du vil starte forfra.

Ved start oprettes en demo-bruger med tre testquizzer (Firmafest 2026, Teambuilding og Sommerfest):

- Brugernavn: `demo`
- Adgangskode: `demo123`

## Mapper

I API'et (`BackEnd/EventQuiz.Api`):

- `Controllers/` – API-endpoints
- `DTOs/` – de objekter API'et modtager og sender
- `Data/QuizSessionQueries.cs` – hjælpe-queries til sessioner

Delt med SignalR (`ECJ.EventQuiz.Core`):

- `Models/` – klasserne (`User`, `Quiz`, `Question` …)
- `Data/AppDbContext.cs` – databasen (tabeller og relationer)
- `Data/SeedData.cs` – demo-bruger og testquizzer
- `Services/ScoreCalculator.cs` og `LeaderBoardCalculator.cs` – point og leaderboard
- `Migrations/` – genereres af Entity Framework

## Endpoints

| Metode | Route | Beskrivelse |
|---|---|---|
| POST | `/api/auth/register` | Opret bruger |
| POST | `/api/auth/login` | Login |
| GET | `/api/quiz` | Hent alle quizzer (`?creatorId=1` for én brugers) |
| GET | `/api/quiz/{id}` | Hent én quiz med spørgsmål |
| POST | `/api/quiz` | Opret quiz |
| DELETE | `/api/quiz/{id}` | Slet quiz |
| POST | `/api/quizsession` | Start en session (opretter et rum) |
| GET | `/api/quizsession/{roomId}` | Hent session og deltagere |
| GET | `/api/quizsession/{roomId}/current-question` | Hent det aktuelle spørgsmål |
| GET | `/api/quizsession/{roomId}/leaderboard` | Hent leaderboard |
| POST | `/api/quizsession/{roomId}/end` | Afslut session |

Eksempler på requests står i `EventQuiz.Api/EventQuiz.Api.http`. Du kan køre dem direkte i VS Code med REST Client.

## Point (Kahoot)

`ScoreCalculator` (i `ECJ.EventQuiz.Core`) giver op til 1000 point for et rigtigt svar:

- Svar under 0,5 sekund giver altid 1000 point.
- Ellers: `1000 × (1 − (svartid ÷ tidsgrænse) ÷ 2)`, rundet til nærmeste hele tal. Fx 2 sek. på et 30-sek. spørgsmål giver 967 point.
- Forkert svar giver 0 point.

Hvert spørgsmål har en tidsgrænse (`timeLimitSeconds`, 5–240 sek., standard 20).

## Ændrer du i en model?

Modellerne ligger i `ECJ.EventQuiz.Core/Models`. Lav en ny migration bagefter:

```bash
dotnet tool install --global dotnet-ef   # kun første gang
cd BackEnd/EventQuiz.Api
dotnet ef migrations add NavnPåÆndring --project ../../ECJ.EventQuiz.Core
```

Den nye migration bliver kørt automatisk, næste gang API'et starter.
