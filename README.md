# MockMind AI

Full stack AI-powered interview preparation platform.

## 1. Create The Project In Visual Studio

Prerequisites:

- Visual Studio 2022 or Visual Studio Code.
- .NET 8 SDK/runtime.
- Node.js 20 or newer.
- SQL Server or SQL Server Express.
- Gemini API key.
- For Docker SQL Server, expose port `1433` and keep your `SA` password ready.

1. Open Visual Studio.
2. Select **Create a new project**.
3. Choose **Blank Solution** and name it `MockMindAI`.
4. Create these folders in the solution root:
   - `backend`
   - `frontend`
5. Add an **ASP.NET Core Web API** project inside `backend` named `MockMindAI.Api`.
6. Use `.NET 8`, enable controllers, and keep Swagger enabled.

This repository already contains the completed structure:

```text
MockMindAI/
  MockMindAI.slnx
  backend/
    MockMindAI.Api/
      Controllers/
      Data/
      Dtos/
      Models/
      Options/
      Services/
  frontend/
    src/
      api/
      components/
      context/
      pages/
```

## 2. Backend Packages

Install these packages in `backend/MockMindAI.Api`:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.6
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.6
dotnet add package BCrypt.Net-Next --version 4.0.3
```

## 3. Backend Configuration

Do not commit real database passwords, JWT secrets, or Gemini API keys.

For local development, either copy the example file:

```bash
cp backend/MockMindAI.Api/appsettings.Development.example.json backend/MockMindAI.Api/appsettings.Development.json
```

Then edit `appsettings.Development.json` locally. This file is ignored by Git.

Alternatively, use .NET user secrets:

```bash
cd backend/MockMindAI.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=MockMindAI;User Id=SA;Password=YOUR_SA_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
dotnet user-secrets set "Jwt:Key" "YOUR_32_PLUS_CHARACTER_JWT_SECRET"
dotnet user-secrets set "Gemini:ApiKey" "YOUR_GEMINI_API_KEY"
```

The committed `backend/MockMindAI.Api/appsettings.json` intentionally contains no secrets:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Key": "",
    "Issuer": "MockMindAI",
    "Audience": "MockMindAIUsers",
    "ExpiryMinutes": 120
  },
  "Gemini": {
    "ApiKey": "",
    "Model": "gemini-2.5-flash"
  },
  "Frontend": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://127.0.0.1:5173"
    ]
  }
}
```

For deployment, set these as environment variables/secrets in your hosting provider:

```text
ConnectionStrings__DefaultConnection
Jwt__Key
Gemini__ApiKey
Frontend__AllowedOrigins__0
```

For frontend deployment, set:

```text
VITE_API_URL=https://your-backend-domain.com/api
```

## 4. Database

Create the SQL Server database with EF migrations:

```bash
cd backend/MockMindAI.Api
dotnet tool install --global dotnet-ef
dotnet ef database update
```

The backend also applies pending migrations automatically in Development when it starts.

If your existing local database is missing the newer dashboard/auth columns, you can also run
`backend/MockMindAI.Api/Scripts/repair-feature-expansion.sql` in Azure Data Studio.

## 5. Frontend Setup

Install and run the React app:

```bash
cd frontend
npm install
npm run dev
```

The frontend opens at `http://localhost:5173`.

## 6. Run The Backend

From Visual Studio, set `MockMindAI.Api` as the startup project and run the `https` profile.

Or use:

```bash
dotnet run --project backend/MockMindAI.Api --launch-profile https
```

Swagger opens at `https://localhost:7230/swagger`.

## 7. Main Features Implemented

- Register and login with JWT authentication.
- BCrypt password hashing.
- Protected React routes using token stored in `localStorage`.
- Dashboard with profile, total interviews, average score, recent attempts, and complete history modal.
- Profile avatar choices and a custom browser favicon.
- Interview setup dropdowns for role, difficulty, experience, and interview type.
- 20-minute timed interview mode with countdown and auto-submit.
- Gemini question generation with exactly 5 questions.
- One-question-at-a-time interview session.
- Gemini answer evaluation.
- Result page with score, strengths, weaknesses, improved answer, feedback, and Chart.js graph.
- Skill heatmap, achievement badges, leaderboard, richer history search/filter/sort, and admin user management.

After pulling feature updates that add database fields, run:

```bash
cd /Users/srivarshini/Documents/MockMindAI/backend/MockMindAI.Api
dotnet ef database update
```
