import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    port: 5188,
    strictPort: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5088',
        changeOrigin: true,
      },
    },
  },
})
