# React + Vite

## Local development

By default, the frontend sends relative API requests and relies on the Vite dev proxy.

- Start the ASP.NET backend locally on `https://localhost:7017`.
- If HTTPS requests fail, trust the local ASP.NET development certificate first.
- If you need a different backend address, create `frontend/.env.local` with `VITE_API_BASE_URL=...`.

The default dev setup proxies only requests under `/api`, so browser routes like `/projects/testproject/issues` stay in the React app and are no longer mistaken for backend endpoints.

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend using TypeScript with type-aware lint rules enabled. Check out the [TS template](https://github.com/vitejs/vite/tree/main/packages/create-vite/template-react-ts) for information on how to integrate TypeScript and [`typescript-eslint`](https://typescript-eslint.io) in your project.
