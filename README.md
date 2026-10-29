# Dishwise

A full-stack recipe manager. Users can create, browse, edit and delete recipes, organise them into categories, and import recipes from a web URL using OpenAI to extract the ingredients and steps.

## Tech stack

- **Backend:** ASP.NET Core (.NET 9) minimal APIs, Entity Framework Core (SQL Server), JWT authentication, BCrypt password hashing, AutoMapper, Serilog, OpenAI for recipe import
- **Frontend:** React 19, TypeScript, Vite, React Router, Bootstrap / React-Bootstrap, Axios
- **Ops:** Docker, Render (deployment), GitHub Actions (build, test, deploy)

## Project layout

```
backend/
  Dishwise.Api             HTTP endpoints, auth setup, Program.cs
  Dishwise.Application     Services, DTOs, interfaces
  Dishwise.Domain          Entities
  Dishwise.Infrastructure  EF Core DbContext, repositories, migrations
  Dishwise.Tests           xUnit tests (Moq, FluentAssertions)
frontend/                       React + TypeScript single-page app
.github/workflows/              CI/CD pipeline
```

## Getting started

### Prerequisites

- .NET 9 SDK
- Node.js 20+
- A SQL Server database (local SQL Server / LocalDB works)

### Backend

Secrets are never stored in the repo. Set them with user secrets:

```bash
cd backend/Dishwise.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<at least 32 random bytes, base64-encoded>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your SQL Server connection string>"
dotnet user-secrets set "OpenAI:ApiKey" "<your OpenAI key>"   # only needed for URL import
dotnet run
```

Database migrations are applied automatically on startup. The API listens on `http://localhost:5001`, and Swagger is available in the Development environment.

### Frontend

```bash
cd frontend
npm install
npm run dev
```

The app runs at `http://localhost:5173`. To point it at a different backend, copy `frontend/.env.example` to `frontend/.env` and set `VITE_API_URL`.

## Testing

```bash
cd backend
dotnet test

cd ../frontend
npm run lint
npm run build
```

## Configuration reference

| Key | Purpose |
|---|---|
| `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience`, `Jwt:ExpireMinutes` | Token signing and validation |
| `ConnectionStrings:DefaultConnection` | SQL Server connection |
| `ConnectionStrings:AzureBlobStorage` | Optional blob storage for recipe images |
| `OpenAI:ApiKey`, `OpenAI:Model` | Recipe import via OpenAI |
| `FRONTEND_URL` | Extra allowed CORS origin in deployed environments |

On Render these are supplied as environment variables using the `Section__Key` form.

## Author

Built by **Maria**.
