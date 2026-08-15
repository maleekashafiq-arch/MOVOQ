# MOVOQ Admin

The back-office web app for MOVOQ. React 19 + TypeScript + Vite, Tailwind v4, react-router, TanStack Query,
and Recharts. It talks to the `/api/v1/admin` endpoints of the .NET API in `../backend`.

## Running it

```bash
npm install
cp .env.example .env.local   # optional; the defaults work against a local API
npm run dev
```

Start the API first so the dev proxy has something to talk to:

```bash
cd ../backend
dotnet run --project MOVOQ.Api --launch-profile http
```

The dev server runs on <http://localhost:5173> and proxies `/api` to `http://localhost:5014`, so the browser
sees a single origin and the API needs no CORS entry during development. Point `VITE_API_PROXY_TARGET` at a
different port if you run the API elsewhere.

| Command | What it does |
| --- | --- |
| `npm run dev` | Dev server with hot reload |
| `npm run build` | Typecheck, then a production bundle in `dist/` |
| `npm run preview` | Serve the built bundle |
| `npm run typecheck` | Types only, no output |

## Signing in

Operators sign in with Firebase email and password. The API then checks that the verified email exists in
`admin_users` and is Active. Adding someone to Firebase is not enough on its own — a SuperAdmin must add
their email under Roles & Permissions, or list it in `Admin:BootstrapEmails`.

In development the sign-in screen also offers **Continue as local developer**, which sends the API's local
test token instead of a Firebase one. It is compiled out of production builds regardless of configuration,
and can be turned off locally with `VITE_ALLOW_DEV_SIGN_IN=false`. The API rejects that token outside
Development.

## Live screens and previews

Seven screens read and write real data: Dashboard, Users, Activity, Points Wallet, Fraud Monitoring, Audit
Trail, and Settings.

Five more are live for the marketplace and draws: Tickets, Rewards, Lucky Draws, Prizes & Fulfillment, and
Sponsors.

Roles & Permissions is live for SuperAdmins: it is the allowlist of Firebase emails that may open this app.

The rest — teams, AI Coach, ads, campaigns, announcements, support, analytics, and content — are marked
**Preview** in the sidebar.
