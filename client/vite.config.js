import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// `npm run build` (or `npm run watch`) outputs straight into the API's
// wwwroot, so the whole app is served from http://localhost:5240.
// `npm run dev` gives hot reload on :5173 and proxies API calls — use it
// together with DEV_FAKE_AUTH, since Google OAuth redirects target :5240.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5240'
    }
  },
  build: {
    outDir: '../server/AsyncInterview.Api/wwwroot',
    emptyOutDir: true
  }
})
