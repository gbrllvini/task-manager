# TaskManager Frontend (Angular 16)

## Prerequisites
- Node.js 16.x
- npm 8+

## Install
```bash
npm install
```

## Run (development)
```bash
npm start
```

The app starts on `http://localhost:4200` and proxies `/api` to `http://localhost:5134`.

## Structure
- `src/app/core/models`: task models and DTO contracts
- `src/app/core/services`: HTTP service for tasks
- `src/app/features/tasks/pages/task-list`: list/filter/pagination UI
- `src/app/features/tasks/pages/task-form`: create/edit UI
- `src/app/features/tasks/tasks.module.ts`: feature module
