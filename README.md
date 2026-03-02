# Task Manager

Aplicação fullstack de gerenciamento de tarefas com autenticacao JWT + refresh token.

## Funcionalidades

- Cadastro e login de usuário
- Logout da sessão atual
- Refresh token com rotação
- CRUD de tarefas
- Ownership: cada usuario acessa apenas as próprias tarefas

## Stack

- Backend: ASP.NET Core 8 + EF Core + PostgreSQL
- Frontend: Angular 16
- Auth: ASP.NET Identity + JWT

## Estrutura

```text
task-manager/
|- backend/
|  |- TaskManager.API
|  |- TaskManager.Application
|  |- TaskManager.Domain
|  |- TaskManager.Infrastructure
|  `- TaskManager.Tests
|- frontend/
|- database/
|  `- taskmanager_schema.sql
|- scripts/
|  `- bootstrap.ps1
`- docker-compose.yml
```

## Script SQL do banco

O script SQL está em:

- `database/taskmanager_schema.sql`

Ele é idempotente (gerado a partir das migrations do EF Core) e inclui:

- tabela `tasks`
- tabelas do Identity (`AspNetUsers`, `AspNetRoles`, etc.)
- tabela `refresh_tokens`

## Requisitos

- .NET SDK 8
- Node.js 18
- PostgreSQL local (modo local)
- Docker Desktop (modo docker)

## Rodar local

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\bootstrap.ps1
```

URLs:

- Frontend: `http://localhost:4200`
- API/Swagger: `http://localhost:5134/swagger`

## Rodar com Docker

API + banco:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\bootstrap.ps1 -Mode docker
```

API + banco + frontend:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\bootstrap.ps1 -Mode docker -WithFrontend -Detached
```

## Importante: local x docker

- Local e Docker podem usar bancos diferentes.
- Não rode local e docker ao mesmo tempo nas portas `4200` e `5134`.
- Antes de voltar para local, finalize os containers:

```powershell
docker compose down
```

## Testes

Backend:

```powershell
dotnet test .\backend\TaskManager.sln
```

Frontend (build):

```powershell
cd .\frontend
npm run build
```

## Endpoints principais

Auth:

- `POST /api/auth/cadastro`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/auth/me`

Tasks (autenticado):

- `GET /api/tasks`
- `GET /api/tasks/{id}`
- `POST /api/tasks`
- `PUT /api/tasks/{id}`
- `DELETE /api/tasks/{id}`

## CI

Workflow: `.github/workflows/ci.yml`

- Backend: build + test
- Frontend: npm ci + build
