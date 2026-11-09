# EventQuiz API

Backend til ECJ Event Quiz. Lavet i ASP.NET Core (.NET 10) med Entity Framework Core og en SQLite-database.

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

Databasen (`eventquiz.db`) bliver oprettet automatisk første gang API'et starter.

## Mapper

- `Models/` – klasserne fra UML-diagrammet
- `Data/AppDbContext.cs` – databasen (tabeller og relationer fra ER-diagrammet)
- `Controllers/` – API-endpoints
- `Services/` – udregning af point og leaderboard
- `DTOs/` – de objekter API'et modtager og sender
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

## Ændrer du i en model?

Lav en ny migration bagefter:

```bash
dotnet tool install --global dotnet-ef   # kun første gang
cd BackEnd/EventQuiz.Api
dotnet ef migrations add NavnPåÆndring
```

Den nye migration bliver kørt automatisk, næste gang API'et starter.
