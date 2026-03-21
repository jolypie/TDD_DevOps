const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5189';

export const api = {
  get: (path: string) =>
    fetch(`${BASE_URL}${path}`).then((r) => r.json()),

  post: (path: string, body: unknown) =>
    fetch(`${BASE_URL}${path}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    }),

  patch: (path: string, body: unknown) =>
    fetch(`${BASE_URL}${path}`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    }),

  delete: (path: string) =>
    fetch(`${BASE_URL}${path}`, { method: 'DELETE' }),
};
