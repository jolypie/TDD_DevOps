> **Note:** This repository is maintained from a machine with multiple GitHub accounts configured (jolypie — personal/university, viktoriagar — work).
> Due to IDE configuration, some commits may appear under the wrong account. All work was done by the same author.

# Book Library App

A reading tracker application built with .NET 8 (backend) and React + TypeScript (frontend), developed using Test-Driven Development (TDD) and a full DevOps pipeline.

---

## Domain

Users can manage their personal reading list. Each user can add books, track reading status, log progress in pages, and rate finished books.

**Entities:**
- `User` — a reader with name and email
- `Book` — a book with title, author and total pages
- `ReadingEntry` — a relation between User and Book, with status, progress and rating

**Business Rules:**
1. A user cannot add the same book twice to their reading list
2. Status cannot skip `Reading` — `WantToRead → Finished` directly is not allowed
3. Rating (1–5) can only be set when status is `Finished`
4. Pages read cannot exceed the book's total pages
5. Start date cannot be set to a future date
6. A book cannot be marked as `Finished` without a start date set
7. Finish date cannot be set to a future date
8. Finish date cannot be earlier than start date (validated in both directions)

---

## Architecture

React frontend talks to the .NET API over HTTP, the API uses EF Core to talk to PostgreSQL.

```
React (frontend)  →  .NET 8 API (backend)  →  PostgreSQL
```

The backend is split into a few folders:
- `Controllers` — handles HTTP requests, validates input, returns proper status codes
- `Services` — where the actual business logic lives
- `Interfaces` — repository abstractions so services can be tested without a real database
- `Models` — the three domain entities (User, Book, ReadingEntry)
- `Data` — EF Core DbContext and the repository implementation
- `Exceptions` — custom exceptions for each business rule violation

---

## Running Locally

### With Docker Compose (recommended)

```bash
cp .env.example .env
# edit .env and set your DB_PASSWORD
docker compose up --build
```

- Frontend: http://localhost:80
- Backend API: http://localhost:8080
- Swagger: http://localhost:8080/swagger

### Without Docker

**Backend:**
```bash
cd backend
dotnet restore
dotnet run
```

**Frontend:**
```bash
cd frontend
npm install
npm run dev
```

> Requires PostgreSQL running locally. Update connection string in `backend/appsettings.json`.

### Running Tests

```bash
dotnet test backend.Tests/BookLibraryApp.Tests.csproj
```

With coverage:
```bash
dotnet test backend.Tests/BookLibraryApp.Tests.csproj \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=./coverage.xml
```

---

## Testing Strategy

| Type | What is tested | Tools |
|------|---------------|-------|
| Unit | Business rules, domain logic, edge cases | xUnit, FluentAssertions, Moq |
| Integration | Service + Repository + InMemory DB together | xUnit, EF Core InMemory |

**What is mocked and why:**
- `IReadingEntryRepository` is mocked in unit tests — isolates service logic from database, tests run fast without infrastructure
- Real `EfReadingEntryRepository` is used in integration tests with an InMemory database — verifies the full stack works together

**Coverage targets:** The project targets ≥90% branch coverage on the business logic layer (`ReadingEntryService`). Overall line coverage is lower (~22%) due to infrastructure code (controllers, EF Core configuration, Program.cs) which contains no domain logic and is intentionally excluded from the coverage target. Branch coverage across the business logic layer reaches ~93%, measured via coverlet and published as a CI artifact.

---

## CI/CD Pipeline

CI runs automatically on every push and pull request via GitHub Actions (`.github/workflows/ci.yml`):

| Step | Description |
|------|-------------|
| Build | `dotnet build` |
| Unit + Integration Tests | `dotnet test` with coverlet |
| Coverage Report | Published as artifact |
| Static Analysis | Build with nullable warnings, frontend `tsc` |
| Docker Build & Push | Backend + frontend images pushed to ghcr.io (main branch only) |

---

## Environments

| | Staging | Production |
|--|---------|------------|
| Namespace | `staging` | `production` |
| Replicas (backend) | 1 | 2 |
| Replicas (frontend) | 1 | 2 |
| ASPNETCORE_ENVIRONMENT | Staging | Production |
| Host | booklibrary.staging.local | booklibrary.production.local |

---

## Kubernetes Deployment

Manifests are located in `k8s/staging/` and `k8s/production/`.

**Apply staging:**
```bash
kubectl create namespace staging
kubectl apply -f k8s/staging/
```

**Apply production:**
```bash
kubectl create namespace production
kubectl apply -f k8s/production/
```

Manifests include: `Deployment`, `Service`, `ConfigMap`, `Secret`, `Ingress` with resource limits and requests.

Both environments have been verified locally using minikube. All pods start successfully:
- Staging: 1 replica each for backend, frontend and postgres
- Production: 2 replicas for backend and frontend (high availability), 1 postgres

**Accessing locally via port-forward:**
```bash
# Staging
kubectl port-forward -n staging service/frontend-service 8081:80
# open http://localhost:8081

# Production
kubectl port-forward -n production service/frontend-service 8082:80
# open http://localhost:8082
```

> Secrets in `secret.yaml` contain placeholder values (`REPLACE_ME`). Apply real credentials with:
> ```bash
> kubectl create secret generic booklibrary-secret \
>   --namespace=staging \
>   --from-literal=DB_USER=postgres \
>   --from-literal=DB_PASSWORD=your_password
> ```

---

## Project Structure

```
BookLibraryApp/
├── backend/                  # .NET 8 Web API
│   ├── Controllers/
│   ├── Services/
│   ├── Interfaces/
│   ├── Models/
│   ├── Data/
│   ├── Exceptions/
│   └── Dockerfile
├── backend.Tests/            # xUnit test project
│   ├── ReadingEntryServiceTests.cs   # unit tests
│   └── Integration/
│       └── ReadingEntryIntegrationTests.cs
├── frontend/                 # React + Vite + TypeScript
│   ├── src/
│   │   ├── api/
│   │   └── pages/
│   ├── nginx.conf
│   └── Dockerfile
├── k8s/
│   ├── staging/
│   └── production/
├── docker-compose.yml
└── .github/workflows/ci.yml
```
