# ToDoApp Frontend (React + TypeScript)

This repository contains the frontend client for the ToDoApp. It is a small React + TypeScript application built with Vite.

Purpose
-------
- Provide a simple UI to manage tasks backed by the .NET server in this workspace.
- Demonstrates authentication with JWT, protected API calls, and basic CRUD flows (create, read, update, delete).

Quick overview
--------------
- Login: POST `/api/login` with JSON `{ Username, Password }`. Server returns a JSON token `{ JwtToken, ExpiresIn }`.
- Tasks: GET/POST/PUT/DELETE under `/api/tasks`. All task endpoints require Authorization: Bearer <token>.
- The client stores the JWT in `localStorage` after successful login and attaches it to requests.

Features
--------
- Login form with username/password.
- Create task: provide Name, Status (NotStarted/InProgress/Completed), and Priority.
- Edit task: change Status and Priority (PUT /api/tasks/{id}).
- Delete task: allowed only when Status is `Completed` (DELETE /api/tasks/{id}). The UI disables the Delete button otherwise.
- Error handling: shows login errors, surfaces server messages for failed deletes, and logs out on 401 responses.

Development
-----------
Prerequisites:
- Node.js (16+)
- npm

Install dependencies and run the dev server:

```powershell
cd todoapp.client
npm install --legacy-peer-deps
npm run dev
```

Notes:
- Use `--legacy-peer-deps` if you encounter peer dependency conflicts with test/dev dependencies.

Configuration
-------------
- Backend base URL is set in `src/App.tsx` via the `API_BASE` constant. Change it if your server runs on a different address.

Running tests
-------------
Unit and integration tests use Jest + React Testing Library.

```powershell
cd todoapp.client
npm test
```

Tests cover login, create, update and delete flows and several error cases (login failure, 401 on tasks, server-side delete errors).

Integration with backend
-----------------------
Expected server endpoints and payloads:

- POST `/api/login` — request `{ Username, Password }`, response `{ JwtToken: string, ExpiresIn: number }`.
- GET `/api/tasks` — returns an array of tasks `{ id: string, name: string, status: string, priority: number }`.
- POST `/api/tasks` — accepts `{ Name, Status, Priority }` and returns created object or 201.
- PUT `/api/tasks/{id}` — accepts `{ Id, Name, Status, Priority }`.
- DELETE `/api/tasks/{id}` — deletes the task; server may reject deletion (400/403) if business rules disallow it.

Make sure the backend enables CORS for the frontend origin during development (e.g. `https://localhost:5173`) and that `app.UseCors(...)` is applied before authentication/authorization in the middleware pipeline.

Authentication & token expiry
-----------------------------
- The client stores JWT in `localStorage` under `jwt_token` after login.
- The token is attached to requests via the `Authorization` header.
- If the server returns `401` for any request the client calls `logout()` which:
  - clears the token in state and `localStorage`
  - clears the loaded tasks
  - shows the login form

Troubleshooting
---------------
- CORS errors: confirm backend CORS policy allows the frontend origin and that middleware ordering is correct.
- Tests failing due to TypeScript/CSS imports: the test setup includes `ts-jest` and a small `custom.d.ts` to allow style imports; run `npm install --legacy-peer-deps` if dependencies are missing.
- Dev HTTPS cert: if your backend uses HTTPS with a dev cert, ensure the browser trusts it or use `curl -k` for quick testing.

Files of interest
-----------------
- `src/App.tsx` — main UI and fetch logic
- `src/App.css` — styling
- `src/App.test.tsx` — tests for flows and error cases
- `jest.config.cjs`, `tsconfig.jest.json` — test runner config

Next improvements
-----------------
- Centralize fetch calls into an `api` module that automatically attaches the token and handles 401s.
- Implement refresh-token flow on the server and client to avoid forcing frequent re-login when token expires.
- Replace custom test fetch mocks with `msw` (Mock Service Worker) for realistic network tests.

