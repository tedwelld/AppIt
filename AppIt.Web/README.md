# AppIt.Web

Angular SPA for the AppIt adventure and hospitality management platform.

## Development

```bash
npm install
npm start
```

The dev server proxies API calls to `http://localhost:5175` via `proxy.conf.json`.

## Build

```bash
npm run build
```

Output: `dist/AppIt.Web/browser`

## E2E

```bash
npm run e2e
```

Playwright starts the API and frontend automatically (see `playwright.config.ts`).

## Routes

- `/auth/login` — sign in and password reset
- `/admin/*` — staff workspace
- `/user/*` — guest portal
