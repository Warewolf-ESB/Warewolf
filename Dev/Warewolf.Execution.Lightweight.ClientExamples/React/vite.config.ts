import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// Vite dev server defaults to http://localhost:5173.
// This origin MUST be registered as a SPA redirect URI on the Entra ID
// app registration AND allowed by the Function App CORS policy.
// See README.md -> "CORS and SPA redirect-URI setup".
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
  },
  preview: {
    port: 5173,
    strictPort: true,
  },
});
